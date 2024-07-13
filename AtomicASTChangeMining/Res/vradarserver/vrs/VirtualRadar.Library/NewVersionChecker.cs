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
using System.Linq;
using System.Text;
using VirtualRadar.Interface;
using System.Net;
using System.IO;
using System.Reflection;
using InterfaceFactory;
using System.Windows.Forms;

namespace VirtualRadar.Library
{
    /// <summary>
    /// Default implementation of <see cref="INewVersionChecker"/>.
    /// </summary>
    sealed class NewVersionChecker : INewVersionChecker
    {
        /// <summary>
        /// The default implementation of <see cref="INewVersionCheckerProvider"/>.
        /// </summary>
        class DefaultProvider : INewVersionCheckerProvider
        {
            /// <summary>
            /// See interface docs.
            /// </summary>
            /// <param name="url"></param>
            /// <returns></returns>
            public string DownloadFileContent(string url)
            {
                string result = null;

                var webRequest = WebRequest.Create(url);
                using(var webResponse = WebRequestHelper.GetResponse(webRequest)) {
                    using(var streamReader = new StreamReader(WebRequestHelper.GetResponseStream(webResponse))) {
                        result = streamReader.ReadToEnd();
                    }
                }

                return result;
            }
        }

        private static string _DownloadUrl { get; }
        private static string _LatestVersionUrl { get; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public INewVersionCheckerProvider Provider { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool IsNewVersionAvailable { get; private set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string DownloadUrl => _DownloadUrl;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler NewVersionAvailable;

        /// <summary>
        /// Raises <see cref="NewVersionAvailable"/>.
        /// </summary>
        /// <param name="args"></param>
        private void OnNewVersionAvailable(EventArgs args)
        {
            EventHelper.Raise(NewVersionAvailable, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public NewVersionChecker()
        {
            Provider = new DefaultProvider();
        }

        /// <summary>
        /// Static ctor.
        /// </summary>
        static NewVersionChecker()
        {
            var webAddressManager = Factory.ResolveSingleton<IWebAddressManager>();
            _DownloadUrl =      webAddressManager.RegisterAddress("vrs-download",       "https://www.virtualradarserver.co.uk");
            _LatestVersionUrl = webAddressManager.RegisterAddress("vrs-latest-version", "http://www.virtualradarserver.co.uk/LatestVersion.txt");
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public bool CheckForNewVersion()
        {
            var result = false;

            var content = Provider.DownloadFileContent(_LatestVersionUrl);
            if(!String.IsNullOrEmpty(content)) {
                var applicationInfo = Factory.Resolve<IApplicationInformation>();
                var thisVersion = applicationInfo.Version;
                if(applicationInfo.IsBeta) {
                    thisVersion = new Version(applicationInfo.BetaBasedOnFullVersion);
                }

                result = VersionComparer.Compare(content, thisVersion) > 0;

                IsNewVersionAvailable = result;
                if(result) {
                    OnNewVersionAvailable(EventArgs.Empty);
                }
            }

            return result;
        }
    }
}
