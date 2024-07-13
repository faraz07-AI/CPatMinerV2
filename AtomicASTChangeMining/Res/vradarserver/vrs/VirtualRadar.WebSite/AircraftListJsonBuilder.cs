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
using System.IO;
using System.Linq;
using System.Text;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// An object that can translate a list of <see cref="IAircraft"/> into an <see cref="AircraftListJson"/>.
    /// </summary>
    class AircraftListJsonBuilder : IAircraftListJsonBuilder
    {
        #region Private class - ConfiguredFolder
        /// <summary>
        /// Records the configuration setting for a folder and the result of a check on whether it exists or not.
        /// </summary>
        class ConfiguredFolder
        {
            public string Folder;

            public bool Tested;

            public bool Exists;

            public bool CheckConfiguration(string folder, IFileSystemProvider provider)
            {
                if(Folder != folder || !Tested) {
                    Folder = folder;
                    Tested = true;
                    try {
                        Exists = !String.IsNullOrEmpty(Folder) && provider.DirectoryExists(Folder);
                    } catch {
                        Exists =  false;
                    }
                }

                return Exists;
            }
        }
        #endregion

        #region Fields
        /// <summary>
        /// True if the class has been initialised.
        /// </summary>
        private bool _Initialised;

        /// <summary>
        /// The number of seconds added to the display timeout when deciding if a position
        /// is stale.
        /// </summary>
        private const int BoostStalePositionSeconds = 60;

        /// <summary>
        /// The singleton ISharedConfiguration object.
        /// </summary>
        private ISharedConfiguration _SharedConfiguration;

        /// <summary>
        /// The singleton IFeedManager object.
        /// </summary>
        private IFeedManager _FeedManager;

        /// <summary>
        /// An aircraft list that is permanently empty.
        /// </summary>
        private IAircraftList _EmptyAircraftList;

        /// <summary>
        /// The configured flags folder.
        /// </summary>
        private ConfiguredFolder _ConfiguredFlagsFolder = new ConfiguredFolder();

        /// <summary>
        /// The configured picutures folder.
        /// </summary>
        private ConfiguredFolder _ConfiguredPicturesFolder = new ConfiguredFolder();

        /// <summary>
        /// The configured silhouettes folder.
        /// </summary>
        private ConfiguredFolder _ConfiguredSilhouettesFolder = new ConfiguredFolder();

        /// <summary>
        /// The object that abstracts away file system operations away so they can be tested.
        /// </summary>
        private IFileSystemProvider _FileSystemProvider;

        /// <summary>
        /// The object that abstracts away time operations so they can be tested.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// True if the server allows aircraft pictures to be sent to Internet clients.
        /// </summary>
        private bool _ShowPicturesToInternetClients;

        /// <summary>
        /// True if the operator flags folder has been correctly configured.
        /// </summary>
        private bool _ShowFlags;

        /// <summary>
        /// True if the pictures folder has been correctly configured.
        /// </summary>
        private bool _ShowPictures;

        /// <summary>
        /// True if silhouettes have been correctly configured.
        /// </summary>
        private bool _ShowSilhouettes;

        /// <summary>
        /// The number of seconds of positions to show on short trails.
        /// </summary>
        private int _ShortTrailLength;

        /// <summary>
        /// The default aircraft list feed.
        /// </summary>
        private int _DefaultAircraftListFeedId;
        #endregion

        #region Initialise, LoadConfiguration
        /// <summary>
        /// See interface docs.
        /// </summary>
        private void Initialise()
        {
            if(!_Initialised) {
                _Initialised = true;

                _SharedConfiguration = Factory.ResolveSingleton<ISharedConfiguration>();
                _FeedManager = Factory.ResolveSingleton<IFeedManager>();
                _EmptyAircraftList = Factory.Resolve<ISimpleAircraftList>();
                _FileSystemProvider = Factory.Resolve<IFileSystemProvider>();
                _Clock = Factory.Resolve<IClock>();
            }
        }

        /// <summary>
        /// Refreshes the flags from the shared configuration.
        /// </summary>
        private void RefreshConfigurationSettings()
        {
            var config = _SharedConfiguration.Get();

            _DefaultAircraftListFeedId = config.GoogleMapSettings.WebSiteReceiverId;
            _ShowPicturesToInternetClients = config.InternetClientSettings.CanShowPictures;
            _ShowFlags = _ConfiguredFlagsFolder.CheckConfiguration(config.BaseStationSettings.OperatorFlagsFolder, _FileSystemProvider);
            _ShowPictures = _ConfiguredPicturesFolder.CheckConfiguration(config.BaseStationSettings.PicturesFolder, _FileSystemProvider);
            _ShowSilhouettes = _ConfiguredSilhouettesFolder.CheckConfiguration(config.BaseStationSettings.SilhouettesFolder, _FileSystemProvider);
            _ShortTrailLength = config.GoogleMapSettings.ShortTrailLengthSeconds;
        }
        #endregion

        #region Build
        /// <summary>
        /// Returns a fully-formed <see cref="AircraftListJson"/> from the aircraft list passed across.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="ignoreInvisibleFeeds"></param>
        /// <param name="fallbackToDefaultFeed">
        /// <returns></returns>
        public AircraftListJson Build(AircraftListJsonBuilderArgs args, bool ignoreInvisibleFeeds, bool fallbackToDefaultFeed)
        {
            if(args == null) throw new ArgumentNullException("args");

            Initialise();
            RefreshConfigurationSettings();

            var feedId = args.SourceFeedId == -1 ? _DefaultAircraftListFeedId : args.SourceFeedId;

            IAircraftList aircraftList = null;
            if(args.IsFlightSimulatorList) aircraftList = args.AircraftList;
            else {
                var selectedFeed = _FeedManager.GetByUniqueId(feedId, ignoreInvisibleFeeds);
                if(selectedFeed == null && fallbackToDefaultFeed) {
                    feedId = _DefaultAircraftListFeedId;
                    selectedFeed = _FeedManager.GetByUniqueId(feedId, ignoreInvisibleFeeds: true);
                }
                aircraftList = selectedFeed == null ? null : selectedFeed.AircraftList;
            }
            if(aircraftList == null) {
                aircraftList = _EmptyAircraftList;
                feedId = args.SourceFeedId;
            }

            var result = new AircraftListJson() {
                FlagHeight = 20,
                FlagWidth = 85,
                ShowFlags = _ShowFlags && !args.IsFlightSimulatorList,
                ShowPictures = _ShowPictures && (!args.IsInternetClient || _ShowPicturesToInternetClients) && !args.IsFlightSimulatorList,
                ShowSilhouettes = _ShowSilhouettes && !args.IsFlightSimulatorList,
                ShortTrailLengthSeconds = _ShortTrailLength,
                Source = (int)aircraftList.Source,
                SourceFeedId = feedId,
            };

            if(!args.FeedsNotRequired) {
                foreach(var feed in _FeedManager.VisibleFeeds) {
                    var plottingAircraftList = feed.AircraftList as IPolarPlottingAircraftList;
                    result.Feeds.Add(new FeedJson() {
                        UniqueId = feed.UniqueId,
                        Name = feed.Name,
                        HasPolarPlot = plottingAircraftList?.PolarPlotter != null
                    });
                }
            }

            long timestamp, dataVersion;
            var aircraft = aircraftList.TakeSnapshot(out timestamp, out dataVersion);
            result.AvailableAircraft = aircraft.Count;

            if(args.IgnoreUnchanged) {
                aircraft = aircraft.Where(r => r.DataVersion > args.PreviousDataVersion).ToList();
            }

            result.LastDataVersion = dataVersion.ToString();
            result.ServerTime = JavascriptHelper.ToJavascriptTicks(timestamp);

            Dictionary<int, double?> distances = new Dictionary<int,double?>();
            aircraft = FilterAircraft(aircraft, args, distances);
            SortAircraft(aircraft, args, distances);
            CopyAircraft(result, aircraft, args, distances);

            return result;
        }

        /// <summary>
        /// Returns a filtered list of aircraft and at the same time calculates the distances from the browser location to each aircraft.
        /// </summary>
        /// <param name="aircraftListSnapshot"></param>
        /// <param name="args"></param>
        /// <param name="distances"></param>
        /// <returns></returns>
        /// <remarks>Distance calculations can be expensive, hence the reason why we try to minimise the number of times that they are performed.</remarks>
        private List<IAircraft> FilterAircraft(List<IAircraft> aircraftListSnapshot, AircraftListJsonBuilderArgs args, Dictionary<int, double?> distances)
        {
            List<IAircraft> result = new List<IAircraft>();

            foreach(var aircraft in aircraftListSnapshot) {
                if(PassesFilter(aircraft, args, distances)) result.Add(aircraft);
            }

            return result;
        }

        /// <summary>
        /// Sorts the aircraft list using the parameters in the args object.
        /// </summary>
        /// <param name="aircraftListSnapshot"></param>
        /// <param name="args"></param>]
        /// <param name="distances"></param>
        private void SortAircraft(List<IAircraft> aircraftListSnapshot, AircraftListJsonBuilderArgs args, Dictionary<int, double?> distances)
        {
            if(args.SortBy.Count > 0) {
                IAircraftComparer comparer = Factory.Resolve<IAircraftComparer>();
                comparer.BrowserLocation = args.BrowserLatitude == null || args.BrowserLongitude == null ? null : new Coordinate((float)args.BrowserLatitude, (float)args.BrowserLongitude);
                foreach(var sortBy in args.SortBy) {
                    comparer.SortBy.Add(sortBy);
                }
                foreach(var distance in distances) {
                    comparer.PrecalculatedDistances.Add(distance.Key, distance.Value);
                }

                aircraftListSnapshot.Sort(comparer);
            }
        }

        /// <summary>
        /// Copies the aircraft from the snapshot to the JSON object.
        /// </summary>
        /// <param name="aircraftListJson"></param>
        /// <param name="aircraftListSnapshot"></param>
        /// <param name="args"></param>
        /// <param name="distances"></param>
        private void CopyAircraft(AircraftListJson aircraftListJson, List<IAircraft> aircraftListSnapshot, AircraftListJsonBuilderArgs args, Dictionary<int, double?> distances)
        {
            var now = _Clock.UtcNow;
            var configuration = _SharedConfiguration.Get();
            var positionTimeoutThreshold = now.AddSeconds(-(configuration.BaseStationSettings.DisplayTimeoutSeconds + BoostStalePositionSeconds));

            HashSet<int> previousAircraftSet = null;
            List<int> previousAircraft = args.PreviousAircraft;
            if(previousAircraft.Count > 15) {
                previousAircraftSet = new HashSet<int>(previousAircraft);
                previousAircraft = null;
            }

            double? distance = null;
            for(var i = 0;i < aircraftListSnapshot.Count;++i) {
                var aircraftSnapshot = aircraftListSnapshot[i];
                if(distances != null) {
                    distances.TryGetValue(aircraftSnapshot.UniqueId, out distance);
                }

                var aircraftJson = new AircraftJson() {
                    UniqueId = aircraftSnapshot.UniqueId,
                    IsSatcomFeed = aircraftSnapshot.LastSatcomUpdate != DateTime.MinValue,
                };
                if(!args.OnlyIncludeMessageFields) {
                    aircraftJson.BearingFromHere = GreatCircleMaths.Bearing(args.BrowserLatitude, args.BrowserLongitude, aircraftSnapshot.Latitude, aircraftSnapshot.Longitude, null, false, true);
                    aircraftJson.DistanceFromHere = distance == null ? (double?)null : Math.Round(distance.Value, 2);
                    if(aircraftJson.BearingFromHere != null) aircraftJson.BearingFromHere = Math.Round(aircraftJson.BearingFromHere.Value, 1);
                }

                var firstTimeSeen = previousAircraft != null ? !previousAircraft.Contains(aircraftSnapshot.UniqueId)
                                                             : !previousAircraftSet.Contains(aircraftSnapshot.UniqueId);

                if(firstTimeSeen || aircraftSnapshot.AirPressureInHgChanged > args.PreviousDataVersion)                 aircraftJson.AirPressureInHg = aircraftSnapshot.AirPressureInHg;
                if(firstTimeSeen || aircraftSnapshot.AltitudeChanged > args.PreviousDataVersion)                        aircraftJson.Altitude = aircraftSnapshot.Altitude;
                if(firstTimeSeen || aircraftSnapshot.AltitudeTypeChanged > args.PreviousDataVersion)                    aircraftJson.AltitudeType = (int)aircraftSnapshot.AltitudeType;
                if(firstTimeSeen || aircraftSnapshot.CallsignChanged > args.PreviousDataVersion)                        aircraftJson.Callsign = aircraftSnapshot.Callsign;
                if(firstTimeSeen || aircraftSnapshot.CallsignIsSuspectChanged > args.PreviousDataVersion)               aircraftJson.CallsignIsSuspect = aircraftSnapshot.CallsignIsSuspect;
                if(firstTimeSeen || aircraftSnapshot.GroundSpeedChanged > args.PreviousDataVersion)                     aircraftJson.GroundSpeed = Round.GroundSpeed(aircraftSnapshot.GroundSpeed);
                if(firstTimeSeen || aircraftSnapshot.EmergencyChanged > args.PreviousDataVersion)                       aircraftJson.Emergency = aircraftSnapshot.Emergency;
                if(firstTimeSeen || aircraftSnapshot.GeometricAltitudeChanged > args.PreviousDataVersion)               aircraftJson.GeometricAltitude = aircraftSnapshot.GeometricAltitude;
                if(firstTimeSeen || args.AlwaysShowIcao || aircraftSnapshot.Icao24Changed > args.PreviousDataVersion)   aircraftJson.Icao24 = aircraftSnapshot.Icao24;
                if(firstTimeSeen || aircraftSnapshot.IdentActiveChanged > args.PreviousDataVersion)                     aircraftJson.IdentActive = aircraftSnapshot.IdentActive;
                if(firstTimeSeen || aircraftSnapshot.IsTisbChanged > args.PreviousDataVersion)                          aircraftJson.IsTisb = aircraftSnapshot.IsTisb;
                if(firstTimeSeen || aircraftSnapshot.LatitudeChanged > args.PreviousDataVersion)                        aircraftJson.Latitude = Round.Coordinate(aircraftSnapshot.Latitude);
                if(firstTimeSeen || aircraftSnapshot.LongitudeChanged > args.PreviousDataVersion)                       aircraftJson.Longitude = Round.Coordinate(aircraftSnapshot.Longitude);
                if(firstTimeSeen || aircraftSnapshot.OnGroundChanged > args.PreviousDataVersion)                        aircraftJson.OnGround = aircraftSnapshot.OnGround;
                if(firstTimeSeen || aircraftSnapshot.PositionIsMlatChanged > args.PreviousDataVersion)                  aircraftJson.PositionIsMlat = aircraftSnapshot.PositionIsMlat;
                if(firstTimeSeen || aircraftSnapshot.SignalLevelChanged > args.PreviousDataVersion)                     { aircraftJson.HasSignalLevel = aircraftSnapshot.SignalLevel != null; aircraftJson.SignalLevel = aircraftSnapshot.SignalLevel; }
                if(firstTimeSeen || aircraftSnapshot.SpeedTypeChanged > args.PreviousDataVersion)                       aircraftJson.SpeedType = (int)aircraftSnapshot.SpeedType;
                if(firstTimeSeen || aircraftSnapshot.SquawkChanged > args.PreviousDataVersion)                          aircraftJson.Squawk = String.Format("{0:0000}", aircraftSnapshot.Squawk);
                if(firstTimeSeen || aircraftSnapshot.TargetAltitudeChanged > args.PreviousDataVersion)                  aircraftJson.TargetAltitude = aircraftSnapshot.TargetAltitude;
                if(firstTimeSeen || aircraftSnapshot.TargetTrackChanged > args.PreviousDataVersion)                     aircraftJson.TargetTrack = aircraftSnapshot.TargetTrack;
                if(firstTimeSeen || aircraftSnapshot.TrackChanged > args.PreviousDataVersion)                           aircraftJson.Track = Round.Track(aircraftSnapshot.Track);
                if(firstTimeSeen || aircraftSnapshot.TrackIsHeadingChanged > args.PreviousDataVersion)                  aircraftJson.TrackIsHeading = aircraftSnapshot.TrackIsHeading;
                if(firstTimeSeen || aircraftSnapshot.TransponderTypeChanged > args.PreviousDataVersion)                 aircraftJson.TransponderType = (int)aircraftSnapshot.TransponderType;
                if(firstTimeSeen || aircraftSnapshot.VerticalRateChanged > args.PreviousDataVersion)                    aircraftJson.VerticalRate = aircraftSnapshot.VerticalRate;
                if(firstTimeSeen || aircraftSnapshot.VerticalRateTypeChanged > args.PreviousDataVersion)                aircraftJson.VerticalRateType = (int)aircraftSnapshot.VerticalRateType;

                if(args.OnlyIncludeMessageFields) {
                    if(aircraftJson.Latitude != null || aircraftJson.Longitude != null || aircraftJson.PositionIsMlat != null) {
                        if(aircraftJson.Latitude == null)       aircraftJson.Latitude       = Round.Coordinate(aircraftSnapshot.Latitude);
                        if(aircraftJson.Longitude == null)      aircraftJson.Longitude      = Round.Coordinate(aircraftSnapshot.Longitude);
                        if(aircraftJson.PositionIsMlat == null) aircraftJson.PositionIsMlat = aircraftSnapshot.PositionIsMlat;
                    }
                    if(aircraftJson.Altitude != null || aircraftJson.GeometricAltitude != null || aircraftJson.AltitudeType != null) {
                        if(aircraftJson.Altitude == null)               aircraftJson.Altitude           = aircraftSnapshot.Altitude;
                        if(aircraftJson.AltitudeType == null)           aircraftJson.AltitudeType       = (int)aircraftSnapshot.AltitudeType;
                        if(aircraftJson.GeometricAltitude == null)      aircraftJson.GeometricAltitude  = aircraftSnapshot.GeometricAltitude;
                    }
                } else if(!args.OnlyIncludeMessageFields) {
                    if(firstTimeSeen || aircraftSnapshot.ConstructionNumberChanged > args.PreviousDataVersion)          aircraftJson.ConstructionNumber = aircraftSnapshot.ConstructionNumber;
                    if(firstTimeSeen || aircraftSnapshot.CountMessagesReceivedChanged > args.PreviousDataVersion)       aircraftJson.CountMessagesReceived = aircraftSnapshot.CountMessagesReceived;
                    if(firstTimeSeen || aircraftSnapshot.DestinationChanged > args.PreviousDataVersion)                 aircraftJson.Destination = aircraftSnapshot.Destination;
                    if(firstTimeSeen || aircraftSnapshot.EnginePlacementChanged > args.PreviousDataVersion)             aircraftJson.EnginePlacement = (int)aircraftSnapshot.EnginePlacement;
                    if(firstTimeSeen || aircraftSnapshot.EngineTypeChanged > args.PreviousDataVersion)                  aircraftJson.EngineType = (int)aircraftSnapshot.EngineType;
                    if(firstTimeSeen || aircraftSnapshot.FirstSeenChanged > args.PreviousDataVersion)                   aircraftJson.FirstSeen = aircraftSnapshot.FirstSeen;
                    if(firstTimeSeen || aircraftSnapshot.FlightsCountChanged > args.PreviousDataVersion)                aircraftJson.FlightsCount = aircraftSnapshot.FlightsCount;
                    if(firstTimeSeen || aircraftSnapshot.Icao24CountryChanged > args.PreviousDataVersion)               aircraftJson.Icao24Country = aircraftSnapshot.Icao24Country;
                    if(firstTimeSeen || aircraftSnapshot.Icao24InvalidChanged > args.PreviousDataVersion)               aircraftJson.Icao24Invalid = aircraftSnapshot.Icao24Invalid;
                    if(firstTimeSeen || aircraftSnapshot.IsCharterFlightChanged > args.PreviousDataVersion)             aircraftJson.IsCharterFlight = aircraftSnapshot.IsCharterFlight;
                    if(firstTimeSeen || aircraftSnapshot.IsInterestingChanged > args.PreviousDataVersion)               aircraftJson.IsInteresting = aircraftSnapshot.IsInteresting;
                    if(firstTimeSeen || aircraftSnapshot.IsMilitaryChanged > args.PreviousDataVersion)                  aircraftJson.IsMilitary = aircraftSnapshot.IsMilitary;
                    if(firstTimeSeen || aircraftSnapshot.IsPositioningFlightChanged > args.PreviousDataVersion)         aircraftJson.IsPositioningFlight = aircraftSnapshot.IsPositioningFlight;
                    if(firstTimeSeen || aircraftSnapshot.ManufacturerChanged > args.PreviousDataVersion)                aircraftJson.Manufacturer = aircraftSnapshot.Manufacturer;
                    if(firstTimeSeen || aircraftSnapshot.ModelChanged > args.PreviousDataVersion)                       aircraftJson.Model = aircraftSnapshot.Model;
                    if(firstTimeSeen || aircraftSnapshot.NumberOfEnginesChanged > args.PreviousDataVersion)             aircraftJson.NumberOfEngines = aircraftSnapshot.NumberOfEngines;
                    if(firstTimeSeen || aircraftSnapshot.OperatorChanged > args.PreviousDataVersion)                    aircraftJson.Operator = aircraftSnapshot.Operator;
                    if(firstTimeSeen || aircraftSnapshot.OperatorIcaoChanged > args.PreviousDataVersion)                aircraftJson.OperatorIcao = aircraftSnapshot.OperatorIcao;
                    if(firstTimeSeen || aircraftSnapshot.OriginChanged > args.PreviousDataVersion)                      aircraftJson.Origin = aircraftSnapshot.Origin;
                    if(firstTimeSeen || aircraftSnapshot.PictureFileNameChanged > args.PreviousDataVersion)             aircraftJson.HasPicture = !String.IsNullOrEmpty(aircraftSnapshot.PictureFileName);
                    if(firstTimeSeen || aircraftSnapshot.PictureHeightChanged > args.PreviousDataVersion)               aircraftJson.PictureHeight = aircraftSnapshot.PictureHeight == 0 ? (int?)null : aircraftSnapshot.PictureHeight;
                    if(firstTimeSeen || aircraftSnapshot.PictureWidthChanged > args.PreviousDataVersion)                aircraftJson.PictureWidth = aircraftSnapshot.PictureWidth == 0 ? (int?)null : aircraftSnapshot.PictureWidth;
                    if(firstTimeSeen || aircraftSnapshot.PositionTimeChanged > args.PreviousDataVersion)                aircraftJson.PositionTime = aircraftSnapshot.PositionTime == null ? (long?)null : JavascriptHelper.ToJavascriptTicks(aircraftSnapshot.PositionTime.Value);
                    if(firstTimeSeen || aircraftSnapshot.ReceiverIdChanged > args.PreviousDataVersion)                  aircraftJson.ReceiverId = aircraftSnapshot.ReceiverId;
                    if(firstTimeSeen || aircraftSnapshot.RegistrationChanged > args.PreviousDataVersion)                aircraftJson.Registration = aircraftSnapshot.Registration;
                    if(firstTimeSeen || aircraftSnapshot.SpeciesChanged > args.PreviousDataVersion)                     aircraftJson.Species = (int)aircraftSnapshot.Species;
                    if(firstTimeSeen || aircraftSnapshot.TypeChanged > args.PreviousDataVersion)                        aircraftJson.Type = aircraftSnapshot.Type;
                    if(firstTimeSeen || aircraftSnapshot.UserNotesChanged > args.PreviousDataVersion)                   aircraftJson.UserNotes = aircraftSnapshot.UserNotes;
                    if(firstTimeSeen || aircraftSnapshot.UserTagChanged > args.PreviousDataVersion)                     aircraftJson.UserTag = aircraftSnapshot.UserTag;
                    if(firstTimeSeen || aircraftSnapshot.WakeTurbulenceCategoryChanged > args.PreviousDataVersion)      aircraftJson.WakeTurbulenceCategory = (int)aircraftSnapshot.WakeTurbulenceCategory;
                    if(firstTimeSeen || aircraftSnapshot.YearBuiltChanged > args.PreviousDataVersion)                   aircraftJson.YearBuilt = aircraftSnapshot.YearBuilt;

                    if(aircraftSnapshot.Stopovers.Count > 0 && (firstTimeSeen || aircraftSnapshot.StopoversChanged > args.PreviousDataVersion)) {
                        aircraftJson.Stopovers = new List<string>();
                        aircraftJson.Stopovers.AddRange(aircraftSnapshot.Stopovers);
                    }

                    aircraftJson.SecondsTracked = (long)((now - aircraftSnapshot.FirstSeen).TotalSeconds);
                    aircraftJson.PositionIsStale =    !args.IsFlightSimulatorList                                   // Never flag FSX aircraft as stale
                                                   && aircraftSnapshot.LastSatcomUpdate == DateTime.MinValue        // Never flag satcom aircraft as having a stale position
                                                   && aircraftSnapshot.PositionTime != null
                                                   && aircraftSnapshot.PositionTime < positionTimeoutThreshold ? true : (bool?)null;
                }

                if(args.TrailType != TrailType.None) {
                    var hasTrail = false;
                    var isShort = false;
                    var showAltitude = false;
                    var showSpeed = false;
                    switch(args.TrailType) {
                        case TrailType.Short:           isShort = true; hasTrail = aircraftSnapshot.ShortCoordinates.Count > 0; break;
                        case TrailType.ShortAltitude:   showAltitude = true; aircraftJson.TrailType = "a"; goto case TrailType.Short;
                        case TrailType.ShortSpeed:      showSpeed = true; aircraftJson.TrailType = "s"; goto case TrailType.Short;
                        case TrailType.Full:            hasTrail = aircraftSnapshot.FullCoordinates.Count > 0; break;
                        case TrailType.FullAltitude:    showAltitude = true; aircraftJson.TrailType = "a"; goto case TrailType.Full;
                        case TrailType.FullSpeed:       showSpeed = true; aircraftJson.TrailType = "s"; goto case TrailType.Full;
                    }
                    if(hasTrail) BuildCoordinatesList(isShort, firstTimeSeen, aircraftJson, aircraftSnapshot, args, showAltitude, showSpeed);
                }

                aircraftListJson.Aircraft.Add(aircraftJson);
            }
        }

        /// <summary>
        /// Builds the full or short coordinates list and attaches it to the aircraft JSON object.
        /// </summary>
        /// <param name="shortCoordinates"></param>
        /// <param name="firstTimeSeen"></param>
        /// <param name="aircraftJson"></param>
        /// <param name="aircraftSnapshot"></param>
        /// <param name="args"></param>
        /// <param name="sendAltitude"></param>
        /// <param name="sendSpeed"></param>
        private void BuildCoordinatesList(bool shortCoordinates, bool firstTimeSeen, AircraftJson aircraftJson, IAircraft aircraftSnapshot, AircraftListJsonBuilderArgs args, bool sendAltitude, bool sendSpeed)
        {
            aircraftJson.ResetTrail = firstTimeSeen || args.ResendTrails || aircraftSnapshot.FirstCoordinateChanged > args.PreviousDataVersion;
            List<double?> list = new List<double?>();

            List<Coordinate> coordinates = shortCoordinates ? aircraftSnapshot.ShortCoordinates : aircraftSnapshot.FullCoordinates;
            Coordinate lastCoordinate = null;
            Coordinate nextCoordinate = null;
            for(var i = 0;i < coordinates.Count;++i) {
                var coordinate = nextCoordinate ?? coordinates[i];
                nextCoordinate = i + 1 == coordinates.Count ? null : coordinates[i + 1];

                if(aircraftJson.ResetTrail || coordinate.DataVersion > args.PreviousDataVersion) {
                    // If this is a full coordinate list then entries can be in here that only record a change in altitude or speed, and not a change in track. Those
                    // entries are to be ignored. They can be identified by having a track that is the same as their immediate neighbours in the list.
                    if(!shortCoordinates && lastCoordinate != null && nextCoordinate != null) {
                        var dupeHeading = lastCoordinate.Heading == coordinate.Heading && coordinate.Heading == nextCoordinate.Heading;
                        if(dupeHeading && !sendAltitude && !sendSpeed) continue;
                        var dupeAltitude = lastCoordinate.Altitude == coordinate.Altitude && coordinate.Altitude == nextCoordinate.Altitude;
                        if(sendAltitude && dupeHeading && dupeAltitude) continue;
                        var dupeSpeed = lastCoordinate.GroundSpeed == coordinate.GroundSpeed && coordinate.GroundSpeed == nextCoordinate.GroundSpeed;
                        if(sendSpeed && dupeHeading && dupeSpeed) continue;
                    }

                    list.Add(Round.Coordinate(coordinate.Latitude));
                    list.Add(Round.Coordinate(coordinate.Longitude));
                    if(shortCoordinates) list.Add(JavascriptHelper.ToJavascriptTicks(coordinate.Tick));
                    else                 list.Add((int?)coordinate.Heading);
                    if(sendAltitude)     list.Add(coordinate.Altitude);
                    else if(sendSpeed)   list.Add(coordinate.GroundSpeed);
                }

                lastCoordinate = coordinate;
            }

            if(aircraftSnapshot.Latitude != null && aircraftSnapshot.Longitude != null &&
               (lastCoordinate.Latitude != aircraftSnapshot.Latitude || lastCoordinate.Longitude != aircraftSnapshot.Longitude) &&
               aircraftSnapshot.PositionTimeChanged > args.PreviousDataVersion) {
                list.Add(Round.Coordinate(aircraftSnapshot.Latitude));
                list.Add(Round.Coordinate(aircraftSnapshot.Longitude));
                if(shortCoordinates) list.Add(JavascriptHelper.ToJavascriptTicks(aircraftSnapshot.PositionTime.Value));
                else                 list.Add((int?)Round.TrackHeading(aircraftSnapshot.Track));
                if(sendAltitude)     list.Add(Round.TrackAltitude(aircraftSnapshot.Altitude));
                else if(sendSpeed)   list.Add(Round.TrackGroundSpeed(aircraftSnapshot.GroundSpeed));
            }

            if(list.Count != 0) {
                if(shortCoordinates) aircraftJson.ShortCoordinates = list;
                else                 aircraftJson.FullCoordinates = list;
            }
        }

        /// <summary>
        /// Returns true if the aircraft passes the filter criteria passed across.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <param name="args"></param>
        /// <param name="distances"></param>
        /// <returns></returns>
        private bool PassesFilter(IAircraft aircraft, AircraftListJsonBuilderArgs args, Dictionary<int, double?> distances)
        {
            var filter = args.Filter;
            bool result = filter == null;

            var distance = args.IsFlightSimulatorList ? null : GreatCircleMaths.Distance(args.BrowserLatitude, args.BrowserLongitude, aircraft.Latitude, aircraft.Longitude);
            if(!result) {
                result = true;
                if(result && filter.Altitude != null)               result = filter.Altitude.Passes(aircraft.Altitude);
                if(result && filter.Callsign != null)               result = filter.Callsign.Passes(aircraft.Callsign);
                if(result && filter.EngineType != null)             result = filter.EngineType.Passes(aircraft.EngineType);
                if(result && filter.Icao24 != null)                 result = filter.Icao24.Passes(aircraft.Icao24);
                if(result && filter.Icao24Country != null)          result = filter.Icao24Country.Passes(aircraft.Icao24Country);
                if(result && filter.IsInteresting != null)          result = filter.IsInteresting.Passes(aircraft.IsInteresting);
                if(result && filter.IsMilitary != null)             result = filter.IsMilitary.Passes(aircraft.IsMilitary);
                if(result && filter.MustTransmitPosition != null)   result = filter.MustTransmitPosition.Passes(aircraft.Latitude != null && aircraft.Longitude != null);
                if(result && filter.Operator != null)               result = filter.Operator.Passes(aircraft.Operator);
                if(result && filter.OperatorIcao != null)           result = filter.OperatorIcao.Passes(aircraft.OperatorIcao);
                if(result && filter.PositionWithin != null)         result = args.SelectedAircraftId == aircraft.UniqueId || GeofenceChecker.IsWithinBounds(aircraft.Latitude, aircraft.Longitude, filter.PositionWithin);
                if(result && filter.Registration != null)           result = filter.Registration.Passes(aircraft.Registration);
                if(result && filter.Species != null)                result = filter.Species.Passes(aircraft.Species);
                if(result && filter.Squawk != null)                 result = filter.Squawk.Passes(aircraft.Squawk);
                if(result && filter.Type != null)                   result = filter.Type.Passes(aircraft.Type);
                if(result && filter.UserTag != null)                result = filter.UserTag.Passes(aircraft.UserTag);
                if(result && filter.WakeTurbulenceCategory != null) result = filter.WakeTurbulenceCategory.Passes(aircraft.WakeTurbulenceCategory);

                if(result && filter.Airport != null) result = PassesAirportFilter(filter.Airport, aircraft);

                if(result && filter.Distance != null) {
                    if(distance == null && filter.Distance.IsValid) result = false;
                    else result = filter.Distance.Passes(distance);
                }
            }

            if(result) distances.Add(aircraft.UniqueId, distance);

            return result;
        }

        /// <summary>
        /// Returns true if the aircraft passes the airport filter.
        /// </summary>
        /// <param name="filterString"></param>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        private bool PassesAirportFilter(FilterString filterString, IAircraft aircraft)
        {
            Func<string, string> extractAirportCode = (string airportDescription) => {
                var code = airportDescription;
                if(!String.IsNullOrEmpty(code)) {
                    var separatorIndex = code.IndexOf(' ');
                    if(separatorIndex != -1) code = code.Substring(0, separatorIndex);
                }
                return code;
            };

            var airportCodes = new List<string>();
            airportCodes.Add(extractAirportCode(aircraft.Origin));
            airportCodes.Add(extractAirportCode(aircraft.Destination));
            foreach(var stopover in aircraft.Stopovers) {
                airportCodes.Add(extractAirportCode(stopover));
            }

            return filterString.Passes(airportCodes);
        }
        #endregion
    }
}
