﻿// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using System.Text.RegularExpressions;

namespace VirtualRadar.Library.BaseStation
{
    /// <summary>
    /// The default implementation of <see cref="IBaseStationAircraftList"/>.
    /// </summary>
    sealed class BaseStationAircraftList : IBaseStationAircraftList
    {
        #region Private Class - TrackCalculationParameters
        /// <summary>
        /// A private class that records parameters necessary for calculating tracks.
        /// </summary>
        class TrackCalculationParameters
        {
            /// <summary>
            /// Gets or sets the latitude used for the last track calculation.
            /// </summary>
            public double LastLatitude { get; set; }

            /// <summary>
            /// Gets or sets the longitude used for the last track calculation.
            /// </summary>
            public double LastLongitude { get; set; }

            /// <summary>
            /// Gets or sets the track last transmitted for the aircraft.
            /// </summary>
            public float? LastTransmittedTrack { get; set; }

            /// <summary>
            /// Gets or sets a value indicating that the transmitted track on ground appears to
            /// have locked to the track as it was when the aircraft was first started up.
            /// </summary>
            /// <remarks>
            /// This problem appears to affect 757-200s. When going from airborne to surface the
            /// SurfacePosition tracks are correct, but when the aircraft is started the tracks
            /// in SurfacePositions lock to the heading the aircraft was in on startup and never
            /// report the correct track until after the aircraft has taken off and landed.
            /// </remarks>
            public bool TrackFrozen { get; set; }

            /// <summary>
            /// Gets or sets the time at UTC when the track was considered to be frozen. Frozen
            /// tracks are expired - some operators continue to transmit messages for many hours
            /// while the aircraft is on the ground; because the track after landing was correct
            /// it will still be considered to be correct once the aircraft taxis to takeoff,
            /// this reset prevents that.
            /// </summary>
            public DateTime TrackFrozenAt { get; set; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// Number of ticks in a second.
        /// </summary>
        private const long TicksPerSecond = 10000000L;

        /// <summary>
        /// The number of seconds to wait before retrying a failed air pressure lookup.
        /// </summary>
        private const int SecondsBeforeRetryAirPressureLookup = 5;

        /// <summary>
        /// The number of seconds to wait before refreshing a successful air pressure lookup.
        /// </summary>
        private const int SecondsBeforeRefreshAirPressureLookup = 120;

        /// <summary>
        /// The object that abstracts away the clock for us.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// True once <see cref="Start"/> has been called. This indicates that all properties are in a good
        /// state. Although properties such as the listener and database can be changed at any time the intention
        /// is that they are configured once and then remain constant over the lifetime of the aircraft list.
        /// </summary>
        private bool _Started;

        /// <summary>
        /// The last DataVersion applied to an aircraft.
        /// </summary>
        private long _DataVersion;

        /// <summary>
        /// A map of unique identifiers to aircraft objects. Do not reference this directly unless you have a lock on
        /// _AircraftListLock - instead you need to take a reference to the dictionary and use that instead, without
        /// a lock.
        /// </summary>
        private Dictionary<int, IAircraft> _AircraftMap = new Dictionary<int, IAircraft>();

        /// <summary>
        /// The object that fetches aircraft details for us.
        /// </summary>
        private IAircraftDetailFetcher _AircraftDetailFetcher;

        /// <summary>
        /// The object that fetches callsign routes for us.
        /// </summary>
        private ICallsignRouteFetcher _CallsignRouteFetcher;

        /// <summary>
        /// The object that handles air pressure lookups.
        /// </summary>
        private IAirPressureManager _AirPressureManager;

        /// <summary>
        /// The object that synchronises changes to <see cref="_AircraftMap"/> and <see cref="_CalculatedTrackCoordinates"/>.
        /// </summary>
        private object _AircraftListLock = new object();

        /// <summary>
        /// The object that synchronises access to the fields that are copied from the current configuration.
        /// </summary>
        private object _ConfigurationLock = new object();

        /// <summary>
        /// The number of seconds of coordinates that are held in the ShortCoordinates list for aircraft.
        /// </summary>
        private int _ShortTrailLengthSeconds;

        /// <summary>
        /// The number of seconds that has to elapse since the last message for an aircraft before <see cref="TakeSnapshot"/>
        /// suppresses it from the returned list.
        /// </summary>
        private int _SnapshotTimeoutSecondsModeS;

        /// <summary>
        /// The number of minutes that have to elapse before SatCom aircraft are suppressed by <see cref="TakeSnapshot"/>.
        /// </summary>
        private int _SnapshotTimeoutMinutesSatcom;

        /// <summary>
        /// The number of seconds that has to elapse after the last Mode-S message before old aircraft are removed from the list.
        /// </summary>
        private int _TrackingTimeoutSecondsModeS;

        /// <summary>
        /// The number of seconds that has to elapse after the last SatCom message before old aircraft are removed from the list.
        /// </summary>
        private int _TrackingTimeoutMinutesSatcom;

        /// <summary>
        /// A map of aircraft identifiers to the parameters used for calculating its track. This is a parallel list to
        /// <see cref="_AircraftMap"/> and is locked using <see cref="_AircraftListLock"/>.  Do not reference this directly
        /// unless you have a lock on _AircraftListLock - take a reference to the dictionary instead and use that (without
        /// a lock).
        /// </summary>
        private Dictionary<int, TrackCalculationParameters> _CalculatedTrackCoordinates = new Dictionary<int,TrackCalculationParameters>();

        /// <summary>
        /// The time that the last removal of old aircraft from the list was performed.
        /// </summary>
        private DateTime _LastRemoveOldAircraftTime;

        /// <summary>
        /// A copy of the prefer IATA codes setting from the configuration.
        /// </summary>
        private bool _PreferIataAirportCodes;

        /// <summary>
        /// An empty BaseStationAircraft object.
        /// </summary>
        private BaseStationAircraft _EmptyBaseStationAircraft = new BaseStationAircraft();

        /// <summary>
        /// An object that can detect bad altitudes and positions.
        /// </summary>
        private IAircraftSanityChecker _SanityChecker;

        /// <summary>
        /// True if the listener has had its events hooked.
        /// </summary>
        private bool _Port30003ListenerHooked;
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public AircraftListSource Source { get { return AircraftListSource.BaseStation; } }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Count
        {
            get
            {
                var aircraftMap = _AircraftMap;
                return aircraftMap.Count;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsTracking { get; private set; }

        IListener _Port30003Listener;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IListener Listener
        {
            get { return _Port30003Listener; }
            set
            {
                if(_Port30003Listener != value) {
                    UnhookListener();
                    _Port30003Listener = value;
                    if(IsTracking) {
                        HookListener();
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IStandingDataManager StandingDataManager { get; set; }

        /// <summary>
        /// See interface docs. Be careful with this property - it can be nulled out by another thread while
        /// it's in use, always take a local copy before use.
        /// </summary>
        public IPolarPlotter PolarPlotter { get; set; }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>. Note that the class is sealed, hence this is private instead of protected virtual.
        /// </summary>
        /// <param name="args"></param>
        private void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CountChanged;

        /// <summary>
        /// Raises <see cref="CountChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnCountChanged(EventArgs args)
        {
            EventHelper.Raise(CountChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler TrackingStateChanged;

        /// <summary>
        /// Raises <see cref="TrackingStateChanged"/>. Note that the class is sealed.
        /// </summary>
        /// <param name="args"></param>
        private void OnTrackingStateChanged(EventArgs args)
        {
            EventHelper.Raise(TrackingStateChanged, this, args);
        }
        #endregion

        #region Constructor and Finaliser
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public BaseStationAircraftList()
        {
            _Clock = Factory.Resolve<IClock>();
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~BaseStationAircraftList()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Finalises or disposes of the object. Note that this class is sealed.
        /// </summary>
        /// <param name="disposing"></param>
        private void Dispose(bool disposing)
        {
            if(disposing) {
                if(_Port30003Listener != null) _Port30003Listener.Port30003MessageReceived -= BaseStationListener_MessageReceived;
                if(_AircraftDetailFetcher != null) _AircraftDetailFetcher.Fetched -= AircraftDetailFetcher_Fetched;
                if(_CallsignRouteFetcher != null) _CallsignRouteFetcher.Fetched -= CallsignRouteFetcher_Fetched;
                if(_SanityChecker != null) _SanityChecker.Dispose();
            }
        }
        #endregion

        #region Start, Stop, LoadConfiguration
        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Start()
        {
            if(!_Started) {
                if(Listener == null) throw new InvalidOperationException("You must supply a Port30003 listener before the aircraft list can be started");
                if(StandingDataManager == null) throw new InvalidOperationException("You must supply a standing data manager before the aircraft list can be started");

                LoadConfiguration();

                var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
                configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;

                _SanityChecker = Factory.Resolve<IAircraftSanityChecker>();

                Factory.ResolveSingleton<IHeartbeatService>().SlowTick += Heartbeat_SlowTick;
                Factory.ResolveSingleton<IStandingDataManager>().LoadCompleted += StandingDataManager_LoadCompleted;

                _AircraftDetailFetcher = Factory.ResolveSingleton<IAircraftDetailFetcher>();
                _AircraftDetailFetcher.Fetched += AircraftDetailFetcher_Fetched;
                _CallsignRouteFetcher = Factory.ResolveSingleton<ICallsignRouteFetcher>();
                _CallsignRouteFetcher.Fetched += CallsignRouteFetcher_Fetched;
                _AirPressureManager = Factory.ResolveSingleton<IAirPressureManager>();

                _Started = true;
            }

            if(!IsTracking) {
                HookListener();
                IsTracking = true;
                OnTrackingStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Stop()
        {
            if(IsTracking) {
                IsTracking = false;
                UnhookListener();
                ResetAircraftList();
                OnTrackingStateChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Hooks the listener.
        /// </summary>
        private void HookListener()
        {
            if(_Port30003Listener != null && !_Port30003ListenerHooked) {
                _Port30003ListenerHooked = true;
                _DataVersion = Math.Max(_Clock.UtcNow.Ticks, _DataVersion + 1);
                _Port30003Listener.Port30003MessageReceived += BaseStationListener_MessageReceived;
                _Port30003Listener.SourceChanged += BaseStationListener_SourceChanged;
                _Port30003Listener.PositionReset += BaseStationListener_PositionReset;
            }
        }

        /// <summary>
        /// Unhooks the listener.
        /// </summary>
        private void UnhookListener()
        {
            if(_Port30003Listener != null && _Port30003ListenerHooked) {
                _Port30003ListenerHooked = false;
                _Port30003Listener.Port30003MessageReceived -= BaseStationListener_MessageReceived;
                _Port30003Listener.SourceChanged -= BaseStationListener_SourceChanged;
                _Port30003Listener.PositionReset -= BaseStationListener_PositionReset;
            }
        }

        /// <summary>
        /// Reads all of the important values out of the configuration file.
        /// </summary>
        private void LoadConfiguration()
        {
            var configuration = Factory.ResolveSingleton<IConfigurationStorage>().Load();

            lock(_ConfigurationLock) {
                _ShortTrailLengthSeconds = configuration.GoogleMapSettings.ShortTrailLengthSeconds;
                _SnapshotTimeoutSecondsModeS = configuration.BaseStationSettings.DisplayTimeoutSeconds;
                _SnapshotTimeoutMinutesSatcom = configuration.BaseStationSettings.SatcomDisplayTimeoutMinutes;
                _TrackingTimeoutSecondsModeS = configuration.BaseStationSettings.TrackingTimeoutSeconds;
                _TrackingTimeoutMinutesSatcom = configuration.BaseStationSettings.SatcomTrackingTimeoutMinutes;
                _PreferIataAirportCodes = configuration.GoogleMapSettings.PreferIataAirportCodes;
            }
        }
        #endregion

        #region ProcessMessage, ApplyMessageToAircraft, CalculateTrack
        /// <summary>
        /// Adds information contained within the message to the object held for the aircraft (creating a new object if one
        /// does not already exist).
        /// </summary>
        /// <param name="message"></param>
        /// <param name="isOutOfBand"></param>
        /// <param name="isSatcomFeed"></param>
        private void ProcessMessage(BaseStationMessage message, bool isOutOfBand, bool isSatcomFeed)
        {
            try {
                if(message.MessageType == BaseStationMessageType.Transmission) {
                    var icaoNumber = CustomConvert.Icao24(message.Icao24);
                    if(icaoNumber != -1) {
                        var now = _Clock.UtcNow;

                        var isInsane = false;
                        var isOnGround = message.OnGround.GetValueOrDefault();
                        var altitudeCertainty = isOnGround ? Certainty.ProbablyRight : Certainty.Uncertain;
                        var positionCertainty = Certainty.Uncertain;
                        if(message.Altitude != null && !isOnGround) {
                            isInsane = (altitudeCertainty = _SanityChecker.CheckAltitude(icaoNumber, now, message.Altitude.Value)) == Certainty.CertainlyWrong;
                        }
                        if(!isInsane && message.Latitude != null && message.Longitude != null && (message.Latitude != 0.0 || message.Longitude != 0.0)) {
                            isInsane = (positionCertainty = _SanityChecker.CheckPosition(icaoNumber, now, message.Latitude.Value, message.Longitude.Value)) == Certainty.CertainlyWrong;
                        }

                        if(!isInsane) {
                            bool isNewAircraft = false;

                            var aircraftMap = _AircraftMap;
                            IAircraft aircraft;
                            isNewAircraft = !aircraftMap.TryGetValue(icaoNumber, out aircraft);
                            if(isNewAircraft) {
                                aircraft = Factory.Resolve<IAircraft>();
                                aircraft.UniqueId = icaoNumber;
                            } else if(isOutOfBand) {
                                if(aircraft.Latitude.GetValueOrDefault() != 0 || aircraft.Longitude.GetValueOrDefault() != 0) {
                                    if(!aircraft.PositionIsMlat.GetValueOrDefault() && aircraft.ReceiverId == aircraft.PositionReceiverId) {
                                        aircraft = null;
                                    }
                                }
                            }

                            if(aircraft != null) {
                                ApplyMessageToAircraft(icaoNumber, message, aircraft, isNewAircraft, isOutOfBand, isSatcomFeed);
                            }

                            if(!isOutOfBand && altitudeCertainty == Certainty.ProbablyRight && positionCertainty == Certainty.ProbablyRight) {
                                if(PolarPlotter != null) {
                                    PolarPlotter.AddCheckedCoordinate(icaoNumber, isOnGround ? 0 : message.Altitude.Value, message.Latitude.Value, message.Longitude.Value);
                                }
                            }

                            if(isNewAircraft) {
                                var added = false;
                                lock(_AircraftListLock) {
                                    if(!_AircraftMap.ContainsKey(icaoNumber)) {
                                        var newMap = CollectionHelper.ShallowCopy(_AircraftMap);
                                        newMap.Add(icaoNumber, aircraft);
                                        _AircraftMap = newMap;
                                        added = true;
                                    }
                                }
                                if(added) {
                                    OnCountChanged(EventArgs.Empty);
                                }
                            }
                        }
                    }
                }
            } catch(Exception ex) {
                Debug.WriteLine(String.Format("BaseStationAircraftList.ProcessMessage caught exception: {0}", ex.ToString()));
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        private void ApplyMessageToAircraft(int icaoNumber, BaseStationMessage message, IAircraft aircraft, bool isNewAircraft, bool isOutOfBand, bool isSatcomFeed)
        {
            var now = _Clock.UtcNow;

            // We want to retrieve all of the lookups without writing anything to the aircraft. Then all of the values
            // that need changing on the aircraft will be set in one lock with one DataVersion so they're all consistent.

            CodeBlock codeBlock = null;
            if(isNewAircraft) codeBlock = StandingDataManager.FindCodeBlock(message.Icao24);

            bool callsignChanged;
            string operatorIcao;
            lock(aircraft) {  // <-- nothing should be changing Callsign, we're the only thread that writes it, but just in case...
                callsignChanged = !String.IsNullOrEmpty(message.Callsign) && message.Callsign != aircraft.Callsign;
                operatorIcao = aircraft.OperatorIcao;
            }

            var track = CalculateTrack(message, aircraft);

            lock(aircraft) {
                GenerateDataVersion(aircraft);

                if(isNewAircraft) {
                    aircraft.FirstSeen = now;
                    aircraft.TransponderType = TransponderType.ModeS;
                }

                aircraft.LastUpdate = now;
                if(isSatcomFeed) {
                    aircraft.LastSatcomUpdate = now;
                } else {
                    aircraft.LastModeSUpdate = now;
                }

                if(track != null) aircraft.Track = track;
                if(message.Track != null && message.Track != 0.0) aircraft.IsTransmittingTrack = true;
                if(message.Latitude.GetValueOrDefault() != 0.0 || message.Longitude.GetValueOrDefault() != 0.0) {
                    aircraft.PositionReceiverId = message.ReceiverId;
                    aircraft.Latitude = message.Latitude.GetValueOrDefault();
                    aircraft.Longitude = message.Longitude.GetValueOrDefault();
                    aircraft.PositionIsMlat = message.IsMlat || isOutOfBand;

                    aircraft.UpdateCoordinates(now, _ShortTrailLengthSeconds);
                    RefreshAircraftAirPressure(aircraft, now);
                }

                if(!isOutOfBand || isNewAircraft) {                 // new aircraft should never be out-of-band, but if they are then we need to treat them normally
                    aircraft.ReceiverId = message.ReceiverId;
                    aircraft.SignalLevel = message.SignalLevel;
                    aircraft.IsTisb = message.IsTisb;
                    ++aircraft.CountMessagesReceived;
                    if(aircraft.Icao24 == null) {
                        aircraft.Icao24 = icaoNumber.ToString("X6");
                    }

                    if(!String.IsNullOrEmpty(message.Callsign)) aircraft.Callsign = message.Callsign;
                    if(message.GroundSpeed != null) aircraft.GroundSpeed = message.GroundSpeed;
                    if(message.VerticalRate != null) aircraft.VerticalRate = message.VerticalRate;
                    if(message.OnGround != null) aircraft.OnGround = message.OnGround;
                    if(message.Squawk != null) {
                        aircraft.Squawk = message.Squawk;
                        aircraft.Emergency = message.Squawk == 7500 || message.Squawk == 7600 || message.Squawk == 7700;
                    }
                    if(message.IdentActive != null) {
                      aircraft.IdentActive = message.IdentActive;
                    }

                    if(aircraft.TransponderType == TransponderType.ModeS) {
                        if((message.GroundSpeed != null && message.GroundSpeed != 0) ||
                           (message.VerticalRate != null && message.VerticalRate != 0) ||
                           (message.Latitude != null && message.Latitude != 0) ||
                           (message.Longitude != null && message.Longitude != 0) ||
                           (message.Track != null && message.Track != 0)) {
                            aircraft.TransponderType = TransponderType.Adsb;
                        }
                    }

                    var supplementaryMessage = message != null && message.Supplementary != null ? message.Supplementary : null;
                    if(supplementaryMessage != null) {
                        ApplySupplementaryMessage(aircraft, supplementaryMessage, now);
                    }

                    if(message.Altitude != null) {
                        aircraft.Altitude = message.Altitude;
                        switch(aircraft.AltitudeType) {
                            case AltitudeType.Barometric:
                                if(aircraft.AirPressureInHg == null) {
                                    aircraft.GeometricAltitude = aircraft.Altitude;
                                } else {
                                    aircraft.GeometricAltitude = AirPressure.PressureAltitudeToGeometricAltitude(aircraft.Altitude, aircraft.AirPressureInHg.Value);
                                }
                                break;
                            case AltitudeType.Geometric:
                                aircraft.GeometricAltitude = aircraft.Altitude;
                                if(aircraft.AirPressureInHg != null) {
                                    aircraft.Altitude = AirPressure.GeometricAltitudeToPressureAltitude(aircraft.GeometricAltitude, aircraft.AirPressureInHg.Value);
                                }
                                break;
                            default:
                                throw new NotImplementedException();
                        }
                    }

                    var callsignRouteDetail = _CallsignRouteFetcher.RegisterAircraft(aircraft);
                    if(isNewAircraft || callsignChanged) {
                        if(callsignRouteDetail == null) {
                            ApplyRoute(aircraft, null, false, false);
                        } else {
                            ApplyRoute(aircraft, callsignRouteDetail.Route, callsignRouteDetail.IsPositioningFlight, callsignRouteDetail.IsCharterFlight);
                        }
                    }

                    ApplyCodeBlock(aircraft , codeBlock);

                    var aircraftDetail = _AircraftDetailFetcher.RegisterAircraft(aircraft);
                    if(isNewAircraft) {
                        if(aircraftDetail != null) ApplyAircraftDetail(aircraft, aircraftDetail);
                    }
                }
            }
        }

        private static void ApplySupplementaryMessage(IAircraft aircraft, BaseStationSupplementaryMessage supplementaryMessage, DateTime now)
        {
            if(supplementaryMessage.AltitudeIsGeometric != null) aircraft.AltitudeType = supplementaryMessage.AltitudeIsGeometric.Value ? AltitudeType.Geometric : AltitudeType.Barometric;
            if(supplementaryMessage.VerticalRateIsGeometric != null) aircraft.VerticalRateType = supplementaryMessage.VerticalRateIsGeometric.Value ? AltitudeType.Geometric : AltitudeType.Barometric;
            if(supplementaryMessage.SpeedType != null) aircraft.SpeedType = supplementaryMessage.SpeedType.Value;
            if(supplementaryMessage.CallsignIsSuspect != null) aircraft.CallsignIsSuspect = supplementaryMessage.CallsignIsSuspect.Value;
            if(supplementaryMessage.TrackIsHeading != null) aircraft.TrackIsHeading = supplementaryMessage.TrackIsHeading.Value;
            if(supplementaryMessage.TargetAltitude != null) aircraft.TargetAltitude = supplementaryMessage.TargetAltitude.Value;
            if(supplementaryMessage.TargetHeading != null) aircraft.TargetTrack = supplementaryMessage.TargetHeading.Value;

            var pressureSetting = supplementaryMessage.PressureSettingInHg;
            if(pressureSetting != null) {
                aircraft.AirPressureInHg = pressureSetting;
                aircraft.AirPressureLookedUpUtc = now.AddHours(1);      // Don't use downloaded air pressure for this aircraft for at least an hour
            }

            if(supplementaryMessage.TransponderType != null) {
                switch(supplementaryMessage.TransponderType.Value) {
                    case TransponderType.Adsb2:
                        aircraft.TransponderType = TransponderType.Adsb2;
                        break;
                    case TransponderType.Adsb1:
                        if(aircraft.TransponderType != TransponderType.Adsb2) {
                            aircraft.TransponderType = TransponderType.Adsb1;
                        }
                        break;
                    case TransponderType.Adsb0:
                        if(aircraft.TransponderType != TransponderType.Adsb1 && aircraft.TransponderType != TransponderType.Adsb2) {
                            aircraft.TransponderType = TransponderType.Adsb0;
                        }
                        break;
                    case TransponderType.Adsb:
                        if(aircraft.TransponderType == TransponderType.ModeS) {
                            aircraft.TransponderType = TransponderType.Adsb;
                        }
                        break;
                }
            }
        }

        private void RefreshAircraftAirPressure(IAircraft aircraft, DateTime now)
        {
            if(aircraft.Latitude != null && aircraft.Longitude != null) {
                var thresholdSeconds = aircraft.AirPressureInHg == null ? SecondsBeforeRetryAirPressureLookup
                                                                        : SecondsBeforeRefreshAirPressureLookup;
                if(aircraft.AirPressureLookedUpUtc == null || aircraft.AirPressureLookedUpUtc.Value.AddSeconds(thresholdSeconds) <= now) {
                    aircraft.AirPressureLookedUpUtc = now;
                    var airPressure = _AirPressureManager.Lookup.FindClosest(aircraft.Latitude.Value, aircraft.Longitude.Value);
                    if(airPressure != null) {
                        aircraft.AirPressureInHg = airPressure.PressureInHg;
                    }
                }
            }
        }

        /// <summary>
        /// Applies the aircraft details to the aircraft passed across. It is assumed that the aircraft has been
        /// locked for update and that the DataVersion is already correct.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="aircraftDetail"></param>
        private void ApplyAircraftDetail(IAircraft aircraft, AircraftDetail aircraftDetail)
        {
            var baseStationAircraft = aircraftDetail.Aircraft ?? _EmptyBaseStationAircraft;
            var operatorIcao = aircraft.OperatorIcao;

            aircraft.Registration =         aircraftDetail.DatabaseRegistration;
            aircraft.Type =                 aircraftDetail.ModelIcao;
            aircraft.Manufacturer =         aircraftDetail.Manufacturer;
            aircraft.Model =                aircraftDetail.ModelName;
            aircraft.ConstructionNumber =   aircraftDetail.Serial;
            aircraft.YearBuilt =            aircraftDetail.YearBuilt;
            aircraft.Operator =             aircraftDetail.OperatorName;
            aircraft.OperatorIcao =         aircraftDetail.OperatorIcao;
            aircraft.IsInteresting =        baseStationAircraft.Interested;
            aircraft.UserNotes =            baseStationAircraft.UserNotes;
            aircraft.UserTag =              baseStationAircraft.UserTag;
            aircraft.FlightsCount =         aircraftDetail.FlightsCount;

            if(operatorIcao != aircraft.OperatorIcao) {
                var callsignRouteDetail = _CallsignRouteFetcher.RegisterAircraft(aircraft);
                if(callsignRouteDetail != null) {
                    ApplyRoute(aircraft, callsignRouteDetail.Route, callsignRouteDetail.IsPositioningFlight, callsignRouteDetail.IsCharterFlight);
                }
            }
            ApplyAircraftPicture(aircraft, aircraftDetail.Picture);
            ApplyAircraftType(aircraft, aircraftDetail.AircraftType);
        }

        /// <summary>
        /// Applies the aircraft's picture details to the aircraft. Assumes that the aircraft has been locked
        /// and that the data version is already correct.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="pictureDetail"></param>
        private void ApplyAircraftPicture(IAircraft aircraft, PictureDetail pictureDetail)
        {
            var fileName = pictureDetail == null ? null : pictureDetail.FileName;
            var width = pictureDetail == null ? 0 : pictureDetail.Width;
            var height = pictureDetail == null ? 0 : pictureDetail.Height;

            aircraft.PictureFileName = fileName;
            aircraft.PictureWidth = width;
            aircraft.PictureHeight = height;
        }

        /// <summary>
        /// Applies the code block to the aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="codeBlock"></param>
        private static void ApplyCodeBlock(IAircraft aircraft, CodeBlock codeBlock)
        {
            if(codeBlock != null) {
                aircraft.Icao24Country = codeBlock.Country;
                aircraft.IsMilitary = codeBlock.IsMilitary;
            }
        }

        /// <summary>
        /// Applies route data to the aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="route"></param>
        /// <param name="isPositioningFlight"></param>
        /// <param name="isCharterFlight"></param>
        private void ApplyRoute(IAircraft aircraft, Route route, bool isPositioningFlight, bool isCharterFlight)
        {
            var origin = route == null ? null : Describe.Airport(route.From, _PreferIataAirportCodes);
            var destination = route == null ? null : Describe.Airport(route.To, _PreferIataAirportCodes);
            var stopovers = new List<string>();
            if(route != null) {
                foreach(var stopover in route.Stopovers) {
                    stopovers.Add(Describe.Airport(stopover, _PreferIataAirportCodes));
                }
            }

            if(aircraft.Origin != origin)           aircraft.Origin = origin;
            if(aircraft.Destination != destination) aircraft.Destination = destination;
            if(!aircraft.Stopovers.SequenceEqual(stopovers)) {
                aircraft.Stopovers.Clear();
                foreach(var stopover in stopovers) {
                    aircraft.Stopovers.Add(stopover);
                }
            }

            if(aircraft.IsPositioningFlight != isPositioningFlight) {
                aircraft.IsPositioningFlight = isPositioningFlight;
            }
            if(aircraft.IsCharterFlight != isCharterFlight) {
                aircraft.IsCharterFlight = isCharterFlight;
            }
        }

        /// <summary>
        /// If the message contains a track then it is simply returned, otherwise if it's possible to calculate the track then
        /// it is calculated and returned.
        /// </summary>
        /// <param name="message"></param>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private float? CalculateTrack(BaseStationMessage message, IAircraft aircraft)
        {
            var result = message.Track;

            var onGround = message.OnGround.GetValueOrDefault();
            var positionPresent = message.Latitude != null && message.Longitude != null;
            var trackNeverTransmitted = !aircraft.IsTransmittingTrack;

            if(result == 0.0 && trackNeverTransmitted) result = null;

            if(positionPresent && (onGround || trackNeverTransmitted)) {
                var calculatedTrackCoordinates = _CalculatedTrackCoordinates;
                TrackCalculationParameters calcParameters;
                calculatedTrackCoordinates.TryGetValue(aircraft.UniqueId, out calcParameters);
                if(calcParameters != null && onGround && calcParameters.TrackFrozenAt.AddMinutes(30) <= _Clock.UtcNow) {
                    lock(_AircraftListLock) {
                        var newMap = CollectionHelper.ShallowCopy(_CalculatedTrackCoordinates);
                        newMap.Remove(aircraft.UniqueId);
                        _CalculatedTrackCoordinates = calculatedTrackCoordinates = newMap;
                        calcParameters = null;
                    }
                }

                var trackSuspect = message.Track == null || message.Track == 0.0;
                var trackFrozen = onGround && (calcParameters == null || calcParameters.TrackFrozen);
                if(trackSuspect || trackFrozen) {
                    var trackCalculated = false;
                    if(calcParameters == null) {
                        calcParameters = new TrackCalculationParameters() {
                            LastLatitude = message.Latitude.Value,
                            LastLongitude = message.Longitude.Value,
                            LastTransmittedTrack = message.Track,
                            TrackFrozen = true,
                            TrackFrozenAt = _Clock.UtcNow
                        };
                        lock(_AircraftListLock) {
                            var newMap = CollectionHelper.ShallowCopy(_CalculatedTrackCoordinates);
                            newMap.Add(aircraft.UniqueId, calcParameters);
                            _CalculatedTrackCoordinates = calculatedTrackCoordinates = newMap;
                        }
                        trackCalculated = true;
                    } else if(message.Latitude != calcParameters.LastLatitude || message.Longitude != calcParameters.LastLongitude) {
                        if(trackFrozen && onGround && calcParameters.LastTransmittedTrack != message.Track) {
                            trackFrozen = calcParameters.TrackFrozen = false;
                        }
                        if(trackSuspect || trackFrozen) {
                            var minimumDistanceKm = message.OnGround.GetValueOrDefault() ? 0.010 : 0.25;
                            if(GreatCircleMaths.Distance(message.Latitude, message.Longitude, calcParameters.LastLatitude, calcParameters.LastLongitude).GetValueOrDefault() >= minimumDistanceKm) {
                                result = (float?)GreatCircleMaths.Bearing(calcParameters.LastLatitude, calcParameters.LastLongitude, message.Latitude, message.Longitude, null, false, false);
                                result = Round.Track(result);
                                calcParameters.LastLatitude = message.Latitude.Value;
                                calcParameters.LastLongitude = message.Longitude.Value;
                                trackCalculated = true;
                            }
                            calcParameters.LastTransmittedTrack = message.Track;
                        }
                    }
                    if(!trackCalculated && (trackSuspect || trackFrozen)) {
                        result = aircraft.Track;
                    }
                }
            }

            return result;
        }
        #endregion

        #region GenerateDataVersion
        /// <summary>
        /// Sets a valid DataVersion for an aircraft. The DataVersion should increment across all aircraft changes
        /// in the aircraft list, not just for a single aircraft.
        /// </summary>
        /// <param name="aircraft"></param>
        private void GenerateDataVersion(IAircraft aircraft)
        {
            lock(aircraft) {
                aircraft.DataVersion = Interlocked.Increment(ref _DataVersion);
            }
        }
        #endregion

        #region FindAircraft, TakeSnapshot
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public IAircraft FindAircraft(int uniqueId)
        {
            IAircraft result = null;

            var aircraftMap = _AircraftMap;
            IAircraft aircraft;
            if(aircraftMap.TryGetValue(uniqueId, out aircraft)) {
                lock(aircraft) {
                    result = (IAircraft)aircraft.Clone();
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="snapshotTimeStamp"></param>
        /// <param name="snapshotDataVersion"></param>
        /// <returns></returns>
        public List<IAircraft> TakeSnapshot(out long snapshotTimeStamp, out long snapshotDataVersion)
        {
            snapshotTimeStamp = _Clock.UtcNow.Ticks;
            snapshotDataVersion = -1L;

            long modeSThreshold;
            long satcomThreshold;
            lock(_ConfigurationLock) {
                modeSThreshold = snapshotTimeStamp - (_SnapshotTimeoutSecondsModeS * TicksPerSecond);
                satcomThreshold = snapshotTimeStamp - (_SnapshotTimeoutMinutesSatcom * TicksPerSecond * 60L);
            }

            List<IAircraft> result = new List<IAircraft>();
            var aircraftMap = _AircraftMap;
            foreach(var aircraft in aircraftMap.Values) {
                if(aircraft.LastModeSUpdate.Ticks < modeSThreshold && aircraft.LastSatcomUpdate.Ticks < satcomThreshold) {
                    continue;
                }
                if(aircraft.DataVersion > snapshotDataVersion) {
                    snapshotDataVersion = aircraft.DataVersion;
                }
                result.Add((IAircraft)aircraft.Clone());
            }

            return result;
        }
        #endregion

        #region ApplyAircraftType, RefreshCodeBlocks
        /// <summary>
        /// Applies the aircraft type details to the aircraft. It is assumed that the aircraft has been
        /// locked and the data version is correct.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="typeDetails"></param>
        private static void ApplyAircraftType(IAircraft aircraft, AircraftType typeDetails)
        {
            aircraft.NumberOfEngines = typeDetails == null ? null : typeDetails.Engines;
            aircraft.EngineType = typeDetails == null ? EngineType.None : typeDetails.EngineType;
            aircraft.EnginePlacement = typeDetails == null ? EnginePlacement.Unknown : typeDetails.EnginePlacement;
            aircraft.Species = typeDetails == null ? Species.None : typeDetails.Species;
            aircraft.WakeTurbulenceCategory = typeDetails == null ? WakeTurbulenceCategory.None : typeDetails.WakeTurbulenceCategory;
        }

        /// <summary>
        /// Refreshes code block information.
        /// </summary>
        private void RefreshCodeBlocks()
        {
            var standingDataManager = Factory.ResolveSingleton<IStandingDataManager>();

            var aircraftMap = _AircraftMap;
            foreach(var aircraft in aircraftMap.Values) {
                var codeBlock = standingDataManager.FindCodeBlock(aircraft.Icao24);
                if(codeBlock != null && (aircraft.Icao24Country != codeBlock.Country || aircraft.IsMilitary != codeBlock.IsMilitary)) {
                    lock(aircraft) {
                        GenerateDataVersion(aircraft);
                        ApplyCodeBlock(aircraft, codeBlock);
                    }
                }
            }
        }
        #endregion

        #region RemoveOldAircraft, ResetAircraftList, 
        /// <summary>
        /// Removes aircraft that have not been seen for a while.
        /// </summary>
        private void RemoveOldAircraft()
        {
            var aircraftMap = _AircraftMap;
            var removeList = new List<int>();
            var now = _Clock.UtcNow;
            var modeSThreshold = now.Ticks - (_TrackingTimeoutSecondsModeS * TicksPerSecond);
            var satcomThreshold = now.Ticks - (_TrackingTimeoutMinutesSatcom * TicksPerSecond * 60L);

            foreach(var aircraft in aircraftMap.Values) {
                if(aircraft.LastModeSUpdate.Ticks < modeSThreshold && aircraft.LastSatcomUpdate.Ticks < satcomThreshold) {
                    removeList.Add(aircraft.UniqueId);
                }
            }

            if(removeList.Count > 0) {
                lock(_AircraftListLock) {
                    aircraftMap = CollectionHelper.ShallowCopy(_AircraftMap);
                    var calculatedTrackCoordinates = CollectionHelper.ShallowCopy(_CalculatedTrackCoordinates);

                    foreach(var uniqueId in removeList) {
                        if(aircraftMap.ContainsKey(uniqueId)) {
                            aircraftMap.Remove(uniqueId);
                        }

                        if(_CalculatedTrackCoordinates.ContainsKey(uniqueId)) {
                            calculatedTrackCoordinates.Remove(uniqueId);
                        }
                    }

                    _AircraftMap = aircraftMap;
                    _CalculatedTrackCoordinates = calculatedTrackCoordinates;
                }

                OnCountChanged(EventArgs.Empty);
            }
        }

        /// <summary>
        /// Removes all of the aircraft in the aircraft list.
        /// </summary>
        private void ResetAircraftList()
        {
            lock(_AircraftListLock) {
                _AircraftMap = new Dictionary<int,IAircraft>();
                _CalculatedTrackCoordinates = new Dictionary<int,TrackCalculationParameters>();
            }

            OnCountChanged(EventArgs.Empty);
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Raised when the aircraft detail fetcher fetches an aircraft's record from the database, or
        /// detects that the details have been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AircraftDetailFetcher_Fetched(object sender, EventArgs<AircraftDetail> args)
        {
            try {
                var aircraftDetail = args.Value;
                var uniqueId = CustomConvert.Icao24(aircraftDetail.Icao24);
                if(uniqueId != -1) {
                    var aircraftMap = _AircraftMap;
                    IAircraft aircraft;
                    if(aircraftMap.TryGetValue(uniqueId, out aircraft)) {
                        lock(aircraft) {
                            GenerateDataVersion(aircraft);
                            ApplyAircraftDetail(aircraft, aircraftDetail);
                        }
                    }
                }
            } catch(ThreadAbortException) {
                // Ignore these, they get rethrown automatically by .NET
            } catch(Exception ex) {
                // We shouldn't get an exception :) But if we do we want it to go through the normal
                // exception handling mechanism.
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Raised when the callsign route fetcher indicates that a route has been loaded or changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CallsignRouteFetcher_Fetched(object sender, EventArgs<CallsignRouteDetail> args)
        {
            try {
                var callsignRouteDetail = args.Value;
                var uniqueId = CustomConvert.Icao24(callsignRouteDetail.Icao24);
                if(uniqueId != -1) {
                    var aircraftMap = _AircraftMap;
                    IAircraft aircraft;
                    if(aircraftMap.TryGetValue(uniqueId, out aircraft) && aircraft.Callsign == callsignRouteDetail.Callsign) {
                        lock(aircraft) {
                            GenerateDataVersion(aircraft);
                            ApplyRoute(aircraft, callsignRouteDetail.Route, callsignRouteDetail.IsPositioningFlight, callsignRouteDetail.IsCharterFlight);
                        }
                    }
                }
            } catch(ThreadAbortException) {
                // Ignore these, they get rethrown automatically by .NET
            } catch(Exception ex) {
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }

        /// <summary>
        /// Raised by <see cref="IListener"/> when a message is received from BaseStation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationListener_MessageReceived(object sender, BaseStationMessageEventArgs args)
        {
            if(IsTracking) {
                ProcessMessage(args.Message, args.IsOutOfBand, args.IsSatcomFeed);
            }
        }

        /// <summary>
        /// Raised by <see cref="IListener"/> when the listener's source of feed data is changing.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationListener_SourceChanged(object sender, EventArgs args)
        {
            ResetAircraftList();
        }

        /// <summary>
        /// Raised by the <see cref="IListener"/> when the listener detects that the positions up to
        /// now are not correct and need to be thrown away.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationListener_PositionReset(object sender, EventArgs<string> args)
        {
            var key = CustomConvert.Icao24(args.Value);
            if(key != -1) {
                _SanityChecker.ResetAircraft(key);

                var aircraftMap = _AircraftMap;
                IAircraft aircraft;
                if(aircraftMap.TryGetValue(key, out aircraft)) {
                    lock(aircraft) {
                        aircraft.ResetCoordinates();
                    }
                }
            }
        }

        /// <summary>
        /// Raised when the user changes the configuration.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            LoadConfiguration();
        }

        /// <summary>
        /// Periodically raised on a background thread by the heartbeat service.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Heartbeat_SlowTick(object sender, EventArgs args)
        {
            var now = _Clock.UtcNow;

            // Remove old aircraft once every ten minutes. We don't have test coverage for this because we cannot
            // observe the effect - taking a snapshot of the aircraft list also removes old aircraft. This is just
            // a failsafe to prevent a buildup of objects when no-one is using the website.
            if(_LastRemoveOldAircraftTime.AddSeconds(_TrackingTimeoutSecondsModeS) <= now) {
                _LastRemoveOldAircraftTime = now;
                RemoveOldAircraft();
            }
        }

        /// <summary>
        /// Raised after the standing data has been loaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StandingDataManager_LoadCompleted(object sender, EventArgs args)
        {
            RefreshCodeBlocks();
        }
        #endregion
    }
}
