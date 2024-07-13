﻿// Copyright © 2018 onwards, Andrew Whewell
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
using System.Net;
using System.Text;
using InterfaceFactory;
using Newtonsoft.Json;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Settings
{
    /// <summary>
    /// Default implementation of <see cref="ITileServerSettingsDownloader"/>
    /// </summary>
    class TileServerSettingsDownloader : ITileServerSettingsDownloader
    {
        internal static string TileServerSettingsUrl { get; }

        /// <summary>
        /// Static ctor.
        /// </summary>
        static TileServerSettingsDownloader()
        {
            var webAddressManager = Factory.ResolveSingleton<IWebAddressManager>();
            TileServerSettingsUrl = webAddressManager.RegisterAddress("vrs-tile-server-settings", "http://sdm.virtualradarserver.co.uk/api/1.01/tile-servers");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="timeoutSeconds"></param>
        /// <returns></returns>
        public TileServerSettings[] Download(int timeoutSeconds)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(TileServerSettingsUrl);
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.Timeout = timeoutSeconds * 1000;

            string jsonText = null;
            using(var response = request.GetResponse()) {
                using(var streamReader = new StreamReader(response.GetResponseStream())) {
                    jsonText = streamReader.ReadToEnd();
                }
            }

            TileServerSettings[] result = null;
            if(!String.IsNullOrEmpty(jsonText)) {
                result = JsonConvert.DeserializeObject<TileServerSettings[]>(jsonText);
            }

            return result;
        }
    }
}
