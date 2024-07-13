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

namespace VirtualRadar.Plugin.FeedFilter
{
    /// <summary>
    /// All of the configuration settings for the filter.
    /// </summary>
    class FilterConfiguration
    {
        /// <summary>
        /// Gets a list of aircraft ICAO codes that either must not be allowed through to the rest of the system
        /// or are the only ones that can be allowed through to the rest of the system.
        /// </summary>
        public List<string> Icaos { get; private set; }

        /// <summary>
        /// Gets a list of ranges of ICAOs that must either be blocked or be the only ones allowed through.
        /// </summary>
        public List<IcaoRange> IcaoRanges { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether aircraft in <see cref="Icaos"/> are to be blocked or whether
        /// they are the only ICAOs that can be used.
        /// </summary>
        public bool ProhibitIcaos { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that MLAT positions must not be allowed through the filter.
        /// </summary>
        public bool ProhibitMlat { get; set; }

        /// <summary>
        /// Gets or sets a value that is incremented every time the settings are saved.
        /// </summary>
        public long DataVersion { get; set; }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FilterConfiguration()
        {
            Icaos = new List<string>();
            IcaoRanges = new List<IcaoRange>();
            ProhibitIcaos = true;
        }
    }
}
