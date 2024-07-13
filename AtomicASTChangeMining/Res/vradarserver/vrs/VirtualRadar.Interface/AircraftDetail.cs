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
using System.Globalization;
using System.Linq;
using System.Text;
using VirtualRadar.Interface.Database;
using VirtualRadar.Interface.StandingData;

namespace VirtualRadar.Interface
{
    /// <summary>
    /// The DTO returned by <see cref="IAircraftDetailFetcher"/>.
    /// </summary>
    public class AircraftDetail
    {
        /// <summary>
        /// Gets or sets the ICAO24 for the aircraft. This is always supplied.
        /// </summary>
        public string Icao24 { get; set; }

        /// <summary>
        /// Gets or sets the database aircraft record. This can be null if no database record has been
        /// fetched for the aircraft (yet).
        /// </summary>
        public BaseStationAircraft Aircraft { get; set; }

        /// <summary>
        /// Gets or sets the results of the aircraft online lookup. This can be null if no lookup has yet
        /// been performed.
        /// </summary>
        public AircraftOnlineLookupDetail OnlineAircraft { get; set; }

        /// <summary>
        /// Gets or sets information about the picture associated with the aircraft. This can be null if
        /// no picture has been found for the aircraft (yet).
        /// </summary>
        public PictureDetail Picture { get; set; }

        /// <summary>
        /// Gets or sets the information gleaned from the aircraft's type. This can be null if the SDM
        /// files aren't in place, or they don't contain information about the type, or if the database
        /// record doesn't exist.
        /// </summary>
        public AircraftType AircraftType { get; set; }

        /// <summary>
        /// Gets or sets the number of flights seen for the aircraft. This is only set once, when the
        /// aircraft record is first read. It is never refreshed.
        /// </summary>
        public int FlightsCount { get; set; }

        /// <summary>
        /// Gets the registration. If the BaseStation database record is present it takes precedence over the OnlineAircraft record.
        /// </summary>
        public string DatabaseRegistration
        {
            get {
                var result = Aircraft == null ? null : Aircraft.Registration;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.Registration;
                return result;
            }
        }

        /// <summary>
        /// Gets the manufacturer.
        /// </summary>
        public string Manufacturer
        {
            get {
                var result = Aircraft == null ? null : Aircraft.Manufacturer;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.Manufacturer;
                return result;
            }
        }

        /// <summary>
        /// Gets the model ICAO. If the BaseStation database record is present it takes precedence over the OnlineAircraft record.
        /// </summary>
        public string ModelIcao
        {
            get {
                var result = Aircraft == null ? null : Aircraft.ICAOTypeCode;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.ModelIcao;
                return result;
            }
        }

        /// <summary>
        /// Gets the model name. If the BaseStation database record is present it takes precedence over the OnlineAircraft record.
        /// </summary>
        public string ModelName
        {
            get {
                var result = Aircraft == null ? null : Aircraft.Type;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.Model;
                return result;
            }
        }

        /// <summary>
        /// Gets the operator code. If the BaseStation database record is present it takes precedence over the OnlineAircraft record.
        /// </summary>
        public string OperatorIcao
        {
            get {
                var result = Aircraft == null ? null : Aircraft.OperatorFlagCode;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.OperatorIcao;
                return result;
            }
        }

        /// <summary>
        /// Gets the operator name. If the BaseStation database record is present it takes precedence over the OnlineAircraft record.
        /// </summary>
        public string OperatorName
        {
            get {
                var result = Aircraft == null ? null : Aircraft.RegisteredOwners;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.Operator;
                return result;
            }
        }

        /// <summary>
        /// Gets the serial number.
        /// </summary>
        public string Serial
        {
            get {
                var result = Aircraft == null ? null : Aircraft.SerialNo;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.Serial;
                return result;
            }
        }

        /// <summary>
        /// Gets the year built.
        /// </summary>
        public string YearBuilt
        {
            get {
                var result = Aircraft == null ? null : Aircraft.YearBuilt;
                if(String.IsNullOrEmpty(result) && OnlineAircraft != null) result = OnlineAircraft.YearBuilt == null ? null : OnlineAircraft.YearBuilt.Value.ToString(CultureInfo.InvariantCulture);
                return result;
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Icao24 ?? "";
        }
    }
}
