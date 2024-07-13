﻿// Copyright © 2015 onwards, Andrew Whewell
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
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Plugin.BaseStationDatabaseWriter
{
    /// <summary>
    /// See interface documents.
    /// </summary>
    class OnlineLookupCache : IOnlineLookupCache
    {
        /// <summary>
        /// The object that can look up Mode-S countries for us.
        /// </summary>
        private IStandingDataManager _StandingDataManager;

        private bool _Enabled;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool Enabled
        {
            get { return _Enabled; }
            set {
                if(value != _Enabled) {
                    _Enabled = value;
                    OnEnabledChanged(EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool RefreshOutOfDateAircraft { get; set; }

        /// <summary>
        /// Another interpretation of <see cref="RefreshOutOfDateAircraft"/> that more closely matches <see cref="IBaseStationDatabase"/> call parameters.
        /// </summary>
        private bool OnlyUpdateIfMarkedAsMissing { get => !RefreshOutOfDateAircraft; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public IBaseStationDatabase Database { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler EnabledChanged;

        /// <summary>
        /// Raises <see cref="EnabledChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnEnabledChanged(EventArgs args)
        {
            EventHelper.Raise(EnabledChanged, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public OnlineLookupCache()
        {
            _StandingDataManager = Factory.ResolveSingleton<IStandingDataManager>();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        /// <param name="baseStationAircraft"></param>
        /// <param name="searchedForBaseStationAircraft"></param>
        /// <returns></returns>
        public AircraftOnlineLookupDetail Load(string icao, BaseStationAircraft baseStationAircraft, bool searchedForBaseStationAircraft)
        {
            return Convert(baseStationAircraft);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        /// <param name="baseStationAircraft"></param>
        /// <returns></returns>
        public Dictionary<string, AircraftOnlineLookupDetail> LoadMany(IEnumerable<string> icaos, IDictionary<string, BaseStationAircraft> baseStationAircraft)
        {
            var result = new Dictionary<string, AircraftOnlineLookupDetail>();

            if(baseStationAircraft != null) {
                foreach(var icao in icaos) {
                    if(baseStationAircraft.TryGetValue(icao, out var databaseRecord)) {
                        result.Add(icao, Convert(databaseRecord));
                    } else {
                        result.Add(icao, null);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Converts a <see cref="BaseStationAircraft"/> record into an <see cref="AircraftOnlineLookupDetail"/> object.
        /// </summary>
        /// <param name="baseStationAircraft"></param>
        /// <returns></returns>
        private AircraftOnlineLookupDetail Convert(BaseStationAircraft baseStationAircraft)
        {
            return IsEmptyButNotMissing(baseStationAircraft) ? null : new AircraftOnlineLookupDetail() {
                AircraftDetailId =  baseStationAircraft.AircraftID,
                Icao =              baseStationAircraft.ModeS,
                Registration =      baseStationAircraft.Registration,
                Country =           !String.IsNullOrEmpty(baseStationAircraft.Country) ? baseStationAircraft.Country : baseStationAircraft.ModeSCountry,
                Manufacturer =      baseStationAircraft.Manufacturer,
                Model =             baseStationAircraft.Type,
                ModelIcao =         baseStationAircraft.ICAOTypeCode,
                Operator =          baseStationAircraft.RegisteredOwners,
                OperatorIcao =      baseStationAircraft.OperatorFlagCode,
                Serial =            baseStationAircraft.SerialNo,
                YearBuilt =         ConvertYearBuilt(baseStationAircraft.YearBuilt),
                CreatedUtc =        ConvertToUtc(baseStationAircraft.FirstCreated),
                UpdatedUtc =        ConvertToUtc(baseStationAircraft.LastModified),
            };
        }

        private bool IsEmptyButNotMissing(BaseStationAircraft baseStationAircraft)
        {
            var result = baseStationAircraft == null;
            if(!result) {
                result = String.IsNullOrEmpty(baseStationAircraft.Registration) &&
                         String.IsNullOrEmpty(baseStationAircraft.Manufacturer) &&
                         String.IsNullOrEmpty(baseStationAircraft.Type) &&
                         String.IsNullOrEmpty(baseStationAircraft.OperatorFlagCode) &&
                         baseStationAircraft.UserString1 != "Missing";
            }

            return result;
        }

        private int? ConvertYearBuilt(string yearBuiltText)
        {
            int? result = null;

            if(!String.IsNullOrEmpty(yearBuiltText)) {
                int yearBuilt;
                if(int.TryParse(yearBuiltText, out yearBuilt)) {
                    result = yearBuilt;
                }
            }

            return result;
        }

        private DateTime ConvertToUtc(DateTime localTime)
        {
            return localTime.Year < 2 ? localTime : localTime.ToUniversalTime();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icao"></param>
        public void RecordMissing(string icao)
        {
            var database = Database;
            if(database != null && Enabled) {
                database.RecordMissingAircraft(icao);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="icaos"></param>
        public void RecordManyMissing(IEnumerable<string> icaos)
        {
            var database = Database;
            if(database != null && Enabled) {
                database.RecordManyMissingAircraft(icaos);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="lookupDetail"></param>
        public void Save(AircraftOnlineLookupDetail lookupDetail)
        {
            var database = Database;
            if(database != null && Enabled) {
                var upsertDetails = BuildUpsertDetails(lookupDetail, DateTime.Now);
                database.UpsertAircraftLookup(upsertDetails, OnlyUpdateIfMarkedAsMissing);
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="lookupDetails"></param>
        public void SaveMany(IEnumerable<AircraftOnlineLookupDetail> lookupDetails)
        {
            var database = Database;
            if(database != null && Enabled) {
                var map = new Dictionary<string, AircraftOnlineLookupDetail>();
                string normaliseIcao(string icao) => icao.ToUpperInvariant();

                foreach(var lookupDetail in lookupDetails) {
                    var icao = normaliseIcao(lookupDetail.Icao);
                    if(!map.ContainsKey(icao)) {
                        map.Add(icao, lookupDetail);
                    }
                }

                var now = DateTime.Now;
                var upsertDetails = map.Select(r => BuildUpsertDetails(r.Value, now));
                database.UpsertManyAircraftLookup(upsertDetails, OnlyUpdateIfMarkedAsMissing);
            }
        }

        /// <summary>
        /// Populates a BaseStationAircraft record with values from the lookup detail.
        /// </summary>
        /// <param name="lookupDetail"></param>
        /// <param name="localNow"></param>
        /// <returns></returns>
        private BaseStationAircraftUpsertLookup BuildUpsertDetails(AircraftOnlineLookupDetail lookupDetail, DateTime localNow)
        {
            var codeBlock = _StandingDataManager.FindCodeBlock(lookupDetail.Icao);
            if(codeBlock?.Country?.StartsWith("Unknown ") ?? false) {
                codeBlock = null;
            }

            return new BaseStationAircraftUpsertLookup() {
                ModeS =             lookupDetail.Icao,
                Registration =      lookupDetail.Registration,
                Country =           lookupDetail.Country,               // If it's coming from SDM then this is also the Mode-S country, but we might not be saving SDM aircraft records...
                ModeSCountry =      codeBlock?.Country,
                Manufacturer =      lookupDetail.Manufacturer,
                Type =              lookupDetail.Model,
                ICAOTypeCode =      lookupDetail.ModelIcao,
                RegisteredOwners =  lookupDetail.Operator,
                OperatorFlagCode =  lookupDetail.OperatorIcao,
                SerialNo =          lookupDetail.Serial,
                YearBuilt =         lookupDetail.YearBuilt?.ToString(),
                LastModified =      localNow,                           // BaseStation needs local dates, not UTC
            };
        }
    }
}
