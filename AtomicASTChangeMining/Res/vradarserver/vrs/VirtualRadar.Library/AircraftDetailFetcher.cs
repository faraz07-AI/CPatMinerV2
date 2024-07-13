﻿// Copyright © 2014 onwards, Andrew Whewell
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
using System.Linq;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Library
{
    /// <summary>
    /// The default implementation of <see cref="IAircraftDetailFetcher"/>.
    /// </summary>
    sealed class AircraftDetailFetcher : AircraftFetcher<string, AircraftDetail>, IAircraftDetailFetcher
    {
        #region Private class - PictureDetail
        /// <summary>
        /// Holds the information that we need to look up an aircraft's local picture.
        /// </summary>
        class LookupPictureDetail
        {
            public string Icao { get; set; }

            public string Registration { get; set; }

            public PictureDetail PictureDetail { get; set; }

            public PictureDetail Result { get; set; }
        }
        #endregion

        #region Fields
        /// <summary>
        /// The auto-config database that is being used.
        /// </summary>
        private IAutoConfigBaseStationDatabase _AutoConfigDatabase;

        /// <summary>
        /// The object that fetches aircraft details from the online lookup service.
        /// </summary>
        private IAircraftOnlineLookupManager _AircraftOnlineLookupManager;

        /// <summary>
        /// The picture folder directory cache that is being used.
        /// </summary>
        private IDirectoryCache _PictureFolderCache;

        /// <summary>
        /// The object that finds pictures for aircraft.
        /// </summary>
        private IAircraftPictureManager _PictureManager;

        /// <summary>
        /// The object that fetches standing data records for us.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        /// <summary>
        /// True if the standing data is to be reloaded even if values it depends upon have not changed.
        /// </summary>
        private bool _ForceRefreshOfStandingData;

        /// <summary>
        /// A lock that prevents multiple threads from changing <see cref="_ForceRefreshOfStandingData"/>.
        /// </summary>
        private object _ForceRefreshOfStandingDataSyncLock = new object();

        /// <summary>
        /// The thread that picture lookups are made on. We need a separate thread because if picture lookups are slow
        /// (e.g. they're over an Internet VPN) then they will prevent the database lookup from appearing on the site
        /// until they're complete.
        /// </summary>
        private BackgroundThreadQueue<LookupPictureDetail> _PictureLookupThread;

        /// <summary>
        /// The results of the picture lookup thread's endeavours.
        /// </summary>
        private List<LookupPictureDetail> _PictureLookupResults = new List<LookupPictureDetail>();

        /// <summary>
        /// The lock on the <see cref="_PictureLookupResults"/> list.
        /// </summary>
        private object _PictureLookupResultsSyncLock = new object();

        /// <summary>
        /// True if a picture lookup exception has been logged. We only log the first ever exception to avoid saturating the log.
        /// </summary>
        private bool _LoggedPictureLookupException;
        #endregion

        #region Properties
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override int AutomaticRecheckIntervalMilliseconds
        {
            get { return 60000; }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override int AutomaticDeregisterIntervalMilliseconds
        {
            get { return 90000; }
        }
        #endregion

        #region Events
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<AircraftDetail>> Fetched;

        /// <summary>
        /// Raises <see cref="Fetched"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnFetched(EventArgs<AircraftDetail> args)
        {
            EventHelper.Raise(Fetched, this, args);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if(disposing && !Disposed) {
                if(_AutoConfigDatabase != null) {
                    _AutoConfigDatabase.Database.AircraftUpdated -= BaseStationDatabase_AircraftUpdated;
                    _AutoConfigDatabase.Database.FileNameChanged -= BaseStationDatabase_FileNameChanged;
                }
                if(_PictureFolderCache != null) {
                    Factory.ResolveSingleton<IAutoConfigPictureFolderCache>().CacheConfigurationChanged -= AutoConfigPictureFolderCache_CacheConfigurationChanged;
                }
                if(_StandingDataManager != null) {
                    _StandingDataManager.LoadCompleted -= StandingDataManager_LoadCompleted;
                }
            }
            base.Dispose(disposing);
        }
        #endregion

        #region DoInitialise
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoInitialise()
        {
            _AutoConfigDatabase = Factory.ResolveSingleton<IAutoConfigBaseStationDatabase>();
            _AutoConfigDatabase.Database.AircraftUpdated += BaseStationDatabase_AircraftUpdated;
            _AutoConfigDatabase.Database.FileNameChanged += BaseStationDatabase_FileNameChanged;

            _AircraftOnlineLookupManager = Factory.ResolveSingleton<IAircraftOnlineLookupManager>();
            _AircraftOnlineLookupManager.AircraftFetched += AircraftOnlineLookupManager_AircraftFetched;

            _PictureManager = Factory.ResolveSingleton<IAircraftPictureManager>();
            var autoConfigurationPictureFolderCache = Factory.ResolveSingleton<IAutoConfigPictureFolderCache>();
            _PictureFolderCache = autoConfigurationPictureFolderCache.DirectoryCache;
            autoConfigurationPictureFolderCache.CacheConfigurationChanged += AutoConfigPictureFolderCache_CacheConfigurationChanged;

            _StandingDataManager = Factory.ResolveSingleton<IStandingDataManager>();
            _StandingDataManager.LoadCompleted += StandingDataManager_LoadCompleted;

            if(_PictureLookupThread == null) {
                _PictureLookupThread = new BackgroundThreadQueue<LookupPictureDetail>("PictureLookup", BackgroundThreadQueueMechanism.ThreadPool) {
                    MaximumParallelThreads = 10,
                };
                _PictureLookupThread.StartBackgroundThread(PictureLookupThread_ProcessLookup, PictureLookupThread_ProcessException);
            }

            base.DoInitialise();
        }
        #endregion

        #region RegisterAircraft
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="aircraft"></param>
        /// <returns></returns>
        public AircraftDetail RegisterAircraft(IAircraft aircraft)
        {
            if(aircraft == null) {
                throw new ArgumentNullException(nameof(aircraft));
            }

            AircraftDetail result = null;
            if(!String.IsNullOrEmpty(aircraft.Icao24)) {
                result = DoRegisterAircraft(aircraft.Icao24, aircraft);
            }

            return result;
        }
        #endregion

        #region DoFetchAircraft, DoFetchManyAircraft
        /// <summary>
        /// Fetches the details for a single aircraft.
        /// </summary>
        /// <param name="fetchedDetail"></param>
        /// <returns></returns>
        protected override AircraftDetail DoFetchAircraft(AircraftFetcher<string, AircraftDetail>.FetchedDetail fetchedDetail)
        {
            var databaseAircraft = _AutoConfigDatabase.Database.GetAircraftByCode(fetchedDetail.Aircraft.Icao24);
            var onlineAircraft = _AircraftOnlineLookupManager.Lookup(fetchedDetail.Aircraft.Icao24, databaseAircraft, searchedForBaseStationAircraft: true);

            return ApplyDatabaseRecord(fetchedDetail.Detail, databaseAircraft, onlineAircraft, fetchedDetail.Aircraft, fetchedDetail.IsFirstFetch);
        }

        /// <summary>
        /// Fetches many aircraft simultaneously.
        /// </summary>
        /// <param name="fetchedDetails"></param>
        /// <returns></returns>
        protected override bool DoFetchManyAircraft(IEnumerable<AircraftFetcher<string, AircraftDetail>.FetchedDetail> fetchedDetails)
        {
            var allIcaos = fetchedDetails.Select(r => r.Key).ToArray();
            var newIcaos = fetchedDetails.Where(r => r.Detail?.Aircraft == null).Select(r => r.Key).ToArray();
            var existingIcaos = fetchedDetails.Where(r => r.Detail?.Aircraft != null).Select(r => r.Key).ToArray();

            var aircraftAndFlightCounts = _AutoConfigDatabase.Database.GetManyAircraftAndFlightsCountByCode(newIcaos);
            var aircraft = _AutoConfigDatabase.Database.GetManyAircraftByCode(existingIcaos);

            var combinedAircraft = new Dictionary<string, BaseStationAircraft>();
            foreach(var kvp in aircraftAndFlightCounts) {
                combinedAircraft.Add(kvp.Key, kvp.Value);
            }
            foreach(var kvp in aircraft) {
                combinedAircraft.Add(kvp.Key, kvp.Value);
            }
            var allOnlineAircraft = _AircraftOnlineLookupManager.LookupMany(allIcaos, combinedAircraft);

            foreach(var kvp in fetchedDetails) {
                var icao24 = kvp.Key;
                var fetchedDetail = kvp;

                BaseStationAircraftAndFlightsCount databaseAircraftAndFlights = null;
                if(!aircraft.TryGetValue(icao24, out var databaseAircraft)) {
                    aircraftAndFlightCounts.TryGetValue(icao24, out databaseAircraftAndFlights);
                }
                allOnlineAircraft.TryGetValue(icao24, out var onlineAircraft);

                kvp.Detail = ApplyDatabaseRecord(
                    fetchedDetail.Detail,
                    databaseAircraftAndFlights ?? databaseAircraft,
                    onlineAircraft,
                    fetchedDetail.Aircraft,
                    fetchedDetail.IsFirstFetch,
                    databaseAircraftAndFlights != null ? databaseAircraftAndFlights.FlightsCount
                                                       : fetchedDetail.Detail?.FlightsCount ?? 0
                );
            }

            return true;
        }

        /// <summary>
        /// Applies the fetched aircraft record to the aircraft detail, raising any events appropriate.
        /// </summary>
        /// <param name="detail"></param>
        /// <param name="databaseAircraft"></param>
        /// <param name="aircraft"></param>
        /// <param name="onlineAircraft"></param>
        /// <param name="isFirstFetch"></param>
        /// <param name="flightsCount"></param>
        private AircraftDetail ApplyDatabaseRecord(AircraftDetail detail, BaseStationAircraft databaseAircraft, AircraftOnlineLookupDetail onlineAircraft, IAircraft aircraft, bool isFirstFetch, int flightsCount = -1)
        {
            var databaseAircraftChanged = detail == null;
            if(!databaseAircraftChanged) {
                if(detail.Aircraft == null) {
                    databaseAircraftChanged = databaseAircraft != null;
                } else {
                    databaseAircraftChanged = !detail.Aircraft.Equals(databaseAircraft);
                    if(!databaseAircraftChanged && flightsCount > -1 && flightsCount != detail.FlightsCount) {
                        databaseAircraftChanged = true;
                    }
                }
            }

            var onlineAircraftChanged = detail == null;
            if(!onlineAircraftChanged) {
                if(detail.OnlineAircraft == null) {
                    onlineAircraftChanged = onlineAircraft != null;
                } else {
                    onlineAircraftChanged = !detail.OnlineAircraft.ContentEquals(onlineAircraft);
                }
            }

            if(_ForceRefreshOfStandingData && detail != null) {
                detail.AircraftType = null;
            }

            var icaoTypeCode = databaseAircraft?.ICAOTypeCode;
            if(String.IsNullOrEmpty(icaoTypeCode) && onlineAircraft != null) {
                icaoTypeCode = onlineAircraft.ModelIcao;
            }

            var aircraftType = detail?.AircraftType;
            var aircraftTypeChanged = detail == null;
            if(icaoTypeCode != null && (_ForceRefreshOfStandingData || detail == null || detail.Aircraft == null || detail.ModelIcao != icaoTypeCode)) {
                aircraftType = _StandingDataManager.FindAircraftType(icaoTypeCode);
                aircraftTypeChanged = true;
            }

            if(databaseAircraftChanged || onlineAircraftChanged || aircraftTypeChanged) {
                if(flightsCount == -1) {
                    flightsCount = detail != null ? detail.FlightsCount
                                          : databaseAircraft == null ? 0
                                          : _AutoConfigDatabase.Database.GetCountOfFlightsForAircraft(databaseAircraft, new SearchBaseStationCriteria() {
                                                Date = new FilterRange<DateTime>(DateTime.MinValue, DateTime.MaxValue),
                                            });
                }

                detail = new AircraftDetail() {
                    Aircraft =          databaseAircraft,
                    OnlineAircraft =    onlineAircraft,
                    AircraftType =      aircraftType,
                    FlightsCount =      flightsCount,
                    Icao24 =            aircraft.Icao24,
                    Picture =           detail?.Picture,
                };
                OnFetched(new EventArgs<AircraftDetail>(detail));
            }

            var registration = databaseAircraft != null ? databaseAircraft.Registration : aircraft.Registration;
            if(registration == null && onlineAircraft != null) {
                registration = onlineAircraft.Registration;
            }

            _PictureLookupThread?.Enqueue(new LookupPictureDetail() {
                Icao =          aircraft.Icao24,
                Registration =  registration,
                PictureDetail = detail?.Picture,
            });

            return detail;
        }

        /// <summary>
        /// Processes the successful completion of a picture lookup operation on the background thread.
        /// </summary>
        /// <param name="lookupPictureDetail"></param>
        private void ApplyPictureLookup(LookupPictureDetail lookupPictureDetail)
        {
            var fetchedDetail = GetFetchedDetailUnderLock(lookupPictureDetail.Icao);
            if(fetchedDetail != null) {
                var detail = fetchedDetail.Detail;
                if(detail != null) {
                    var pictureDetail = lookupPictureDetail.Result;
                    var pictureDetailChanged = false;
                    if(detail.Picture == null) {
                        pictureDetailChanged = pictureDetail != null;
                    } else {
                        pictureDetailChanged = !detail.Picture.Equals(pictureDetail);
                    }

                    if(pictureDetailChanged) {
                        fetchedDetail.Detail.Picture = pictureDetail;
                        OnFetched(new EventArgs<AircraftDetail>(detail));
                    }
                }
            }
        }
        #endregion

        #region DoExtraFastTimerTickWork
        /// <summary>
        /// Called when the fast timer has ticked.
        /// </summary>
        protected override void DoExtraFastTimerTickWork()
        {
            List<LookupPictureDetail> pictureDetails = null;
            lock(_PictureLookupResultsSyncLock) {
                if(_PictureLookupResults.Count > 0) {
                    pictureDetails = new List<LookupPictureDetail>(_PictureLookupResults);
                    _PictureLookupResults.Clear();
                }
            }

            if(pictureDetails != null) {
                foreach(var lookupPictureResult in pictureDetails) {
                    ApplyPictureLookup(lookupPictureResult);
                }
            }
        }
        #endregion

        #region RefetchWithoutDatabaseSearch
        /// <summary>
        /// Refetches all of the registered aircraft without also running database searches.
        /// </summary>
        private void RefetchWithoutDatabaseSearch()
        {
            var allRegistered = GetRegisteredAircraft();
            foreach(var registered in allRegistered) {
                var currentIcao24 = registered.Key;
                var currentDetail = registered.Value;
                CallWithinFetchLock(currentIcao24, (string icao24, AircraftDetail detail, bool isFirstFetch, IAircraft aircraft) => {
                    if(detail != null) {
                        detail = ApplyDatabaseRecord(detail, detail.Aircraft, detail.OnlineAircraft, aircraft, isFirstFetch);
                    }

                    return detail;
                });
            }
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called when the online lookup manager has finished fetching aircraft details for some aircraft.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <remarks>
        /// Note that in principle we might not have asked for any of the aircraft that were fetched, it's all done
        /// via singleton objects so it could be something else that asked for these.
        /// </remarks>
        private void AircraftOnlineLookupManager_AircraftFetched(object sender, AircraftOnlineLookupEventArgs args)
        {
            try {
                foreach(var onlineAircraft in args.AircraftDetails) {
                    CallWithinFetchLock(onlineAircraft.Icao.ToUpperInvariant(), (string icao24, AircraftDetail detail, bool isFirstFetch, IAircraft aircraft) => {
                        return ApplyDatabaseRecord(detail, detail.Aircraft, onlineAircraft, aircraft, isFirstFetch);
                    });
                }
            } catch(ThreadAbortException) {
                // Gets rethrown
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception during application of online lookup aircraft: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the database reports that an aircraft record has been updated.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationDatabase_AircraftUpdated(object sender, EventArgs<BaseStationAircraft> args)
        {
            try {
                var databaseAircraft = args.Value;

                if(databaseAircraft != null && !String.IsNullOrEmpty(databaseAircraft.ModeS)) {
                    CallWithinFetchLock(databaseAircraft.ModeS.ToUpperInvariant(), (string icao24, AircraftDetail detail, bool isFirstFetch, IAircraft aircraft) => {
                        return ApplyDatabaseRecord(detail, databaseAircraft, detail.OnlineAircraft, aircraft, isFirstFetch);
                    });
                }
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception during refresh of aircraft database detail: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the database reports a configuration change.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void BaseStationDatabase_FileNameChanged(object sender, EventArgs args)
        {
            try {
                ForceRefetchOnFastTick = true;
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception during refresh of aircraft database detail: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the picture folder cache's configuration has been changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void AutoConfigPictureFolderCache_CacheConfigurationChanged(object sender, EventArgs args)
        {
            try {
                _LoggedPictureLookupException = false;
                RefetchWithoutDatabaseSearch();
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception during refresh of aircraft pictures: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when the standing data has been reloaded.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void StandingDataManager_LoadCompleted(object sender, EventArgs args)
        {
            try {
                lock(_ForceRefreshOfStandingDataSyncLock) {
                    var existingForceFlag = _ForceRefreshOfStandingData;
                    try {
                        _ForceRefreshOfStandingData = true;
                        RefetchWithoutDatabaseSearch();
                    } finally {
                        _ForceRefreshOfStandingData = existingForceFlag;
                    }
                }
            } catch(ThreadAbortException) {
                // Will automatically get re-thrown - we don't want these logged
            } catch(Exception ex) {
                var log = Factory.ResolveSingleton<ILog>();
                log.WriteLine("Caught exception during refetch of aircraft type codes: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when a picture needs to be fetched.
        /// </summary>
        /// <param name="lookupPictureDetail"></param>
        private void PictureLookupThread_ProcessLookup(LookupPictureDetail lookupPictureDetail)
        {
            var lookupResult = _PictureManager.FindPicture(_PictureFolderCache, lookupPictureDetail.Icao, lookupPictureDetail.Registration, lookupPictureDetail.PictureDetail);
            lock(_PictureLookupResultsSyncLock) {
                lookupPictureDetail.Result = lookupResult;
                _PictureLookupResults.Add(lookupPictureDetail);
            }
        }

        /// <summary>
        /// Called when the picture lookup throws an exception.
        /// </summary>
        /// <param name="ex"></param>
        private void PictureLookupThread_ProcessException(Exception ex)
        {
            if(!_LoggedPictureLookupException) {
                lock(_PictureLookupResultsSyncLock) {
                    if(!_LoggedPictureLookupException) {
                        var log = Factory.ResolveSingleton<ILog>();
                        if(log != null) {
                            _LoggedPictureLookupException = true;
                            log.WriteLine("Caught exception during fetch of aircraft picture: {0}", ex.ToString());
                        }
                    }
                }
            }
        }
        #endregion
    }
}
