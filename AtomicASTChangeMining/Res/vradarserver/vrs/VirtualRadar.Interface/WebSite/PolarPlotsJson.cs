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
using System.Runtime.Serialization;
using VirtualRadar.Interface.Listener;

namespace VirtualRadar.Interface.WebSite
{
    /// <summary>
    /// The JSON that describes all of the available polar plots for a single feed.
    /// </summary>
    [DataContract]
    public class PolarPlotsJson
    {
        /// <summary>
        /// Gets or sets the unique identifier of the feed.
        /// </summary>
        [DataMember(Name="feedId")]
        public int FeedId { get; set; }

        /// <summary>
        /// Gets the list of slices.
        /// </summary>
        [DataMember(Name="slices")]
        public List<PolarPlotsSliceJson> Slices { get; private set; } = new List<PolarPlotsSliceJson>();

        /// <summary>
        /// Creates and returns a model from an <see cref="IPolarPlotter"/>.
        /// </summary>
        /// <param name="feedId"></param>
        /// <param name="polarPlotter"></param>
        /// <returns></returns>
        public static PolarPlotsJson ToModel(int feedId, IPolarPlotter polarPlotter)
        {
            PolarPlotsJson result = null;

            if(polarPlotter != null) {
                result = new PolarPlotsJson() {
                    FeedId = feedId,
                };
                result.Slices.AddRange(polarPlotter.TakeSnapshot().Select(r => PolarPlotsSliceJson.ToModel(r)));
            }

            return result;
        }
    }
}
