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

namespace VirtualRadar.Interface
{
    /// <summary>
    /// Holds the detail for an aircraft whose details were fetched by <see cref="IAircraftOnlineLookup"/>.
    /// </summary>
    public class AircraftOnlineLookupDetail
    {
        /// <summary>
        /// Gets or sets the cache's identifier for the record. This is not sent by the online service, it
        /// is for use by the cache. This is ignored by <see cref="ContentEquals"/>.
        /// </summary>
        public long AircraftDetailId { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's ICAO.
        /// </summary>
        public string Icao { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's registration.
        /// </summary>
        public string Registration { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's country of registration.
        /// </summary>
        public string Country { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's manufacturer.
        /// </summary>
        public string Manufacturer { get; set; }

        /// <summary>
        /// Gets or sets the aircraft model.
        /// </summary>
        public string Model { get; set; }

        /// <summary>
        /// Gets or sets the aircraft model's ICAO 8643 code.
        /// </summary>
        public string ModelIcao { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's operator.
        /// </summary>
        public string Operator { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's operator ICAO.
        /// </summary>
        public string OperatorIcao { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's serial number.
        /// </summary>
        public string Serial { get; set; }

        /// <summary>
        /// Gets or sets the aircraft's year of manufacture.
        /// </summary>
        public int? YearBuilt { get; set; }

        /// <summary>
        /// Gets or sets the date that the cache record was created. This is not sent by the online service, it
        /// is for use by the cache. It is not considered by <see cref="ContentEquals"/>.
        /// </summary>
        public DateTime? CreatedUtc { get; set; }

        /// <summary>
        /// Gets or sets the date that the cache record was last updated. This is not sent by the online service,
        /// it is for use by the cache. It is not considered by <see cref="ContentEquals"/>.
        /// </summary>
        public DateTime? UpdatedUtc { get; set; }

        /// <summary>
        /// Returns true if the content (everything excluding <see cref="AircraftDetailId"/>, <see cref="CreatedUtc"/> and <see cref="UpdatedUtc"/>)
        /// of the other object passed in is the same as this object's content.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool ContentEquals(AircraftOnlineLookupDetail other)
        {
            var result = Object.ReferenceEquals(this, other);
            if(!result) {
                result = other != null &&
                         Icao ==            other.Icao &&
                         Registration ==    other.Registration &&
                         Country ==         other.Country &&
                         Manufacturer ==    other.Manufacturer &&
                         Model ==           other.Model &&
                         ModelIcao ==       other.ModelIcao &&
                         Operator ==        other.Operator &&
                         OperatorIcao ==    other.OperatorIcao &&
                         Serial ==          other.Serial &&
                         YearBuilt ==       other.YearBuilt;
            }

            return result;
        }
    }
}
