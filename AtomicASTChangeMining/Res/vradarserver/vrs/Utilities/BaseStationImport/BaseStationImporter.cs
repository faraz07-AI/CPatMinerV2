﻿// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;

namespace BaseStationImport
{
    /// <summary>
    /// Handles the importing of BaseStation databases.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When using the importer it is necessary to keep in mind the relationships between the four tables in
    /// Kinetic's BaseStation schema that the importer deals with.
    /// </para>
    /// <para>
    /// Aircraft has an ID that is referenced by Flights. Session has an ID that is referenced by Flights.
    /// Location has an ID that is referenced by Session.
    /// </para>
    /// <para>
    /// This means that if you want to copy flights from one BaseStation to another then you need to copy
    /// aircraft, locations and sessions as well. The importer will let you skip the copying of a table but if
    /// you import flights and skip sessions, locations and aircraft then the importer will try to find records
    /// in the target database that correspond with records from the source so that it can map aircraft and
    /// session IDs for the flights that it imports.
    /// </para>
    /// <para>
    /// Aircraft matches are made on the ModeS field (case sensitive), Location matches are made on the
    /// location name (case sensitive) and Session matches are made on the session start time.
    /// </para>
    /// <para>
    /// If a record is imported more than once then the second run will just update the existing record created
    /// by the first run. It will not create duplicate records.
    /// </para>
    /// <para>
    /// Record IDs are not preserved by the importer.
    /// </para>
    /// </remarks>
    class BaseStationImporter
    {
        /// <summary>
        /// A map of source location IDs to target location IDs.
        /// </summary>
        private Dictionary<int, int> _LocationMap = new Dictionary<int, int>();

        /// <summary>
        /// A map of source session IDs to target session IDs.
        /// </summary>
        private Dictionary<int, int> _SessionMap = new Dictionary<int, int>();

        /// <summary>
        /// A map of source aircraft IDs to target aircraft IDs.
        /// </summary>
        private Dictionary<int, int> _AircraftMap = new Dictionary<int, int>();

        /// <summary>
        /// Gets or sets the source BaseStation database.
        /// </summary>
        public IBaseStationDatabase Source { get; set; }

        /// <summary>
        /// Gets or sets the target BaseStation database.
        /// </summary>
        public IBaseStationDatabase Target { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the schema on the target should not be updated before import begins.
        /// </summary>
        public bool SuppressSchemaUpdate { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that aircraft should be imported.
        /// </summary>
        public bool ImportAircraft { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that sessions should be imported.
        /// </summary>
        public bool ImportSessions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that locations should be imported.
        /// </summary>
        public bool ImportLocations { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that flights should be imported.
        /// </summary>
        public bool ImportFlights { get; set; }

        /// <summary>
        /// Gets or sets the date of the earliest flight to import.
        /// </summary>
        public DateTime EarliestFlight { get; set; }

        /// <summary>
        /// Gets the <see cref="EarliestFlight"/> as a nullable date, where null indicates that the min value was supplied.
        /// </summary>
        private DateTime? EarliestFlightCriteria => EarliestFlight.Date == DateTime.MinValue.Date ? (DateTime?)null : EarliestFlight.Date;

        /// <summary>
        /// Gets or sets the date of the latest flight to import.
        /// </summary>
        public DateTime LatestFlight { get; set; }

        /// <summary>
        /// Gets the <see cref="LatestFlight"/> as a nullable date, where null indicates that the max value was supplied.
        /// </summary>
        private DateTime? LatestFlightCriteria => LatestFlight.Date == DateTime.MaxValue.Date ? (DateTime?)null : LatestFlight.Date.AddDays(1).AddTicks(-1);

        /// <summary>
        /// Gets or sets the number of aircraft records to upsert at a time.
        /// </summary>
        public int AircraftPageSize { get; set; } = 1000;

        /// <summary>
        /// Gets or sets the number of flight records to copy at a time.
        /// </summary>
        public int FlightPageSize { get; set; } = 1000;

        /// <summary>
        /// Gets the number of locations that were in the source but could not be loaded from the target.
        /// </summary>
        public int MissingLocations { get; private set; }

        /// <summary>
        /// Gets the number of sessions that were in the source but could not be loaded from the target.
        /// </summary>
        public int MissingSessions { get; private set; }

        /// <summary>
        /// Gets the number of aircraft that were in the source but could not be loaded from the target.
        /// </summary>
        public int MissingAircraft { get; private set; }

        /// <summary>
        /// Gets the number of sessions that had to be skipped because there was no location in the target to attach them to.
        /// </summary>
        public int SkippedSessions { get; private set; }

        /// <summary>
        /// Gets the number of flights that had to be skipped because there was either no session or no aircraft to attach them to.
        /// </summary>
        public int SkippedFlights { get; private set; }

        /// <summary>
        /// Raised whenever the importer starts importing or loading a new table.
        /// </summary>
        public EventHandler<EventArgs<string>> TableChanged;

        /// <summary>
        /// Raises <see cref="TableChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnTableChanged(EventArgs<string> args)
        {
            TableChanged?.Invoke(this, args);
        }

        /// <summary>
        /// Raised whenever there is some progress to report.
        /// </summary>
        public EventHandler<ProgressEventArgs> ProgressChanged;

        /// <summary>
        /// Raises <see cref="ProgressChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnProgressChanged(ProgressEventArgs args)
        {
            ProgressChanged?.Invoke(this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Import()
        {
            UpdateSchema();

            ProcessLocations();
            ProcessSessions();
            ProcessAircraft();
            ProcessFlights();
        }

        private void UpdateSchema()
        {
            if(!SuppressSchemaUpdate) {
                OnTableChanged(new EventArgs<string>("Schema"));
                var progress = new ProgressEventArgs() {
                    Caption = "Applying schema",
                    TotalItems = 1,
                };
                OnProgressChanged(progress);

                Target.CreateDatabaseIfMissing(Target.FileName);

                progress.CurrentItem = 1;
                OnProgressChanged(progress);
            }
        }

        /// <summary>
        /// Imports Location records.
        /// </summary>
        private void ProcessLocations()
        {
            if(ImportLocations || ImportSessions || ImportFlights) {
                LoadOrImportLocations();
            }
        }

        private void LoadOrImportLocations()
        {
            OnTableChanged(new EventArgs<string>("Locations"));
            var progress = new ProgressEventArgs() {
                Caption =       ImportLocations ? "Importing locations" : "Loading locations",
                TotalItems =    -1,
            };
            OnProgressChanged(progress);

            _LocationMap.Clear();
            var allSource = Source.GetLocations();
            var allDest =   Target.GetLocations();

            progress.TotalItems = allSource.Count;
            progress.CurrentItem = 0;
            OnProgressChanged(progress);

            foreach(var rec in allSource) {
                var sourceID = rec.LocationID;

                var existing = allDest.FirstOrDefault(r => r.LocationID > 0 && String.Equals(r.LocationName, rec.LocationName));
                if(existing == null) {
                    rec.LocationID = 0;
                    if(ImportLocations) {
                        Target.InsertLocation(rec);
                    }
                } else {
                    rec.LocationID = existing.LocationID;
                    existing.LocationID = -1;
                    if(ImportLocations) {
                        Target.UpdateLocation(rec);
                    }
                }

                if(rec.LocationID == 0) {
                    ++MissingLocations;
                } else {
                    _LocationMap.Add(sourceID, rec.LocationID);
                }

                ++progress.CurrentItem;
                OnProgressChanged(progress);
            }

            progress.CurrentItem = progress.TotalItems;
            OnProgressChanged(progress);
        }

        /// <summary>
        /// Imports Session records.
        /// </summary>
        private void ProcessSessions()
        {
            if(ImportSessions || ImportFlights) {
                LoadOrImportSessions();
            }
        }

        private void LoadOrImportSessions()
        {
            OnTableChanged(new EventArgs<string>("Sessions"));
            var progress = new ProgressEventArgs() {
                Caption =       ImportSessions ? "Importing sessions" : "Loading sessions",
                TotalItems =    -1,
            };
            OnProgressChanged(progress);

            _SessionMap.Clear();
            var allSource = Source.GetSessions();
            var allDest = Target.GetSessions();

            progress.TotalItems = allSource.Count;
            progress.CurrentItem = 0;
            OnProgressChanged(progress);

            foreach(var rec in allSource) {
                if(!_LocationMap.TryGetValue(rec.LocationID, out var destLocationID)) {
                    ++SkippedSessions;
                } else {
                    var sourceID = rec.SessionID;
                    rec.LocationID = destLocationID;

                    var existing = allDest.FirstOrDefault(r => r.SessionID > 0 && r.StartTime == rec.StartTime);
                    if(existing == null) {
                        rec.SessionID = 0;
                        if(ImportSessions) {
                            Target.InsertSession(rec);
                        }
                    } else {
                        rec.SessionID = existing.SessionID;
                        if(ImportSessions) {
                            Target.UpdateSession(rec);
                        }
                    }

                    if(rec.SessionID == 0) {
                        ++MissingSessions;
                    } else {
                        _SessionMap.Add(sourceID, rec.SessionID);
                    }
                }

                ++progress.CurrentItem;
                OnProgressChanged(progress);
            }

            progress.CurrentItem = progress.TotalItems;
            OnProgressChanged(progress);
        }

        /// <summary>
        /// Imports aircraft records.
        /// </summary>
        private void ProcessAircraft()
        {
            if(ImportAircraft || ImportFlights) {
                LoadOrImportAircraft();
            }
        }

        private void LoadOrImportAircraft()
        {
            OnTableChanged(new EventArgs<string>("Aircraft"));
            var progress = new ProgressEventArgs() {
                Caption =       ImportAircraft ? "Importing aircraft" : "Loading aircraft",
                TotalItems =    -1,
            };
            OnProgressChanged(progress);

            _AircraftMap.Clear();
            var allSource = Source.GetAllAircraft();

            progress.TotalItems = allSource.Count;
            progress.CurrentItem = 0;
            OnProgressChanged(progress);

            if(!ImportAircraft) {
                var allDest = Target.GetAllAircraft().ToDictionary(r => r.ModeS, r => r);
                foreach(var src in allSource) {
                    if(allDest.TryGetValue(src.ModeS, out var dest)) {
                        _AircraftMap.Add(src.AircraftID, dest.AircraftID);
                    } else {
                        ++MissingAircraft;
                    }
                    ++progress.CurrentItem;
                    OnProgressChanged(progress);
                }
            } else {
                var countUpserted = 0;

                while(countUpserted < allSource.Count) {
                    var upsertKeys = new HashSet<string>();
                    var upsertCandidates = new List<BaseStationAircraftUpsert>();
                    var subset = allSource.Skip(countUpserted).Take(AircraftPageSize).ToArray();

                    foreach(var src in subset) {
                        if(!upsertKeys.Contains(src.ModeS)) {
                            upsertCandidates.Add(new BaseStationAircraftUpsert(src));
                            upsertKeys.Add(src.ModeS);
                        }
                    }

                    var upserted = Target.UpsertManyAircraft(upsertCandidates).ToDictionary(r => r.ModeS, r => r);

                    foreach(var src in subset) {
                        if(upserted.TryGetValue(src.ModeS, out var rec)) {
                            _AircraftMap.Add(src.AircraftID, rec.AircraftID);
                        }
                    }

                    countUpserted += subset.Length;
                    progress.CurrentItem = countUpserted;
                    OnProgressChanged(progress);
                }
            }

            progress.CurrentItem = progress.TotalItems;
            OnProgressChanged(progress);
        }

        /// <summary>
        /// Imports flight records.
        /// </summary>
        private void ProcessFlights()
        {
            if(ImportFlights) {
                OnTableChanged(new EventArgs<string>("Flights"));
                var progress = new ProgressEventArgs() {
                    Caption =       "Importing flights",
                    TotalItems =    -1,
                };
                OnProgressChanged(progress);

                var criteria = new SearchBaseStationCriteria();
                if(EarliestFlightCriteria != null || LatestFlightCriteria != null) {
                    criteria.Date = new FilterRange<DateTime>() {
                        Condition =     FilterCondition.Between,
                        LowerValue =    EarliestFlightCriteria,
                        UpperValue =    LatestFlightCriteria,
                    };
                }

                var countFlights = Source.GetCountOfFlights(criteria);
                var countSource = 0;
                var countDest = 0;
                var startRow = 0;

                progress.TotalItems = countFlights;
                progress.CurrentItem = 0;
                OnProgressChanged(progress);

                while(startRow < countFlights) {
                    var allSource = Source.GetFlights(criteria, startRow, startRow + (FlightPageSize - 1), "DATE", true, null, false);
                    countSource += allSource.Count;

                    var upsertCandidates = new List<BaseStationFlightUpsert>();
                    var upsertKeys = new HashSet<string>();
                    foreach(var candidate in allSource) {
                        var key = $"{candidate.AircraftID}-{candidate.StartTime}";
                        if(!upsertKeys.Contains(key)) {
                            if(!_AircraftMap.TryGetValue(candidate.AircraftID, out var aircraftID) || !_SessionMap.TryGetValue(candidate.SessionID, out var sessionID)) {
                                ++SkippedFlights;
                            } else {
                                upsertCandidates.Add(new BaseStationFlightUpsert(candidate) {
                                    AircraftID = aircraftID,
                                    SessionID =  sessionID,
                                });
                                upsertKeys.Add(key);
                            }
                        }
                    }

                    var upserted = Target.UpsertManyFlights(upsertCandidates);
                    countDest += upserted.Length;
                    startRow += FlightPageSize;

                    progress.CurrentItem += allSource.Count;
                    OnProgressChanged(progress);
                }

                progress.CurrentItem = progress.TotalItems;
                OnProgressChanged(progress);
            }
        }
    }
}
