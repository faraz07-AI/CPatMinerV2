﻿// Copyright © 2016 onwards, Andrew Whewell
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
using VirtualRadar.Interface.Network;
using VirtualRadar.Localisation;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of <see cref="IRebroadcastFormatManager"/>.
    /// </summary>
    public class RebroadcastFormatManager : IRebroadcastFormatManager
    {
        /// <summary>
        /// A map of rebroadcast format identifiers to rebroadcast format providers. Take a
        /// reference to the map before using it.
        /// </summary>
        private Dictionary<string, IRebroadcastFormatProvider> _Providers;

        /// <summary>
        /// The lock that ensures that all writes to _Providers are single-threaded.
        /// </summary>
        private object _SyncLock;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(_SyncLock == null) {
                _SyncLock = new object();
                _Providers = new Dictionary<string, IRebroadcastFormatProvider>();

                RegisterProvider(new AircraftListJsonRebroadcastServer());
                RegisterProvider(new AvrRebroadcastServer());
                RegisterProvider(new CompressedVrsRebroadcastServer());
                RegisterProvider(new PassthroughRebroadcastServer());
                RegisterProvider(new Port30003RebroadcastServer(extendedBaseStationFormat:false));
                RegisterProvider(new Port30003RebroadcastServer(extendedBaseStationFormat:true));
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="provider"></param>
        public void RegisterProvider(IRebroadcastFormatProvider provider)
        {
            Initialise();

            if(provider == null || String.IsNullOrEmpty(provider.UniqueId)) {
                throw new InvalidOperationException("You must supply a provider and the UniqueId property must return a non-null non-empty string");
            }

            lock(_SyncLock) {
                var newMap = CollectionHelper.ShallowCopy(_Providers);
                if(newMap.ContainsKey(provider.UniqueId)) {
                    newMap[provider.UniqueId] = provider;
                } else {
                    newMap.Add(provider.UniqueId, provider);
                }
                _Providers = newMap;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public RebroadcastFormatName[] GetRegisteredFormats()
        {
            Initialise();
            var map = _Providers;

            return map.Values.OrderBy(r => r.ShortName).Select(r => RebroadcastFormatName.Create(r)).ToArray();
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        public IRebroadcastFormatProvider GetProvider(string providerId)
        {
            Initialise();

            IRebroadcastFormatProvider result = null;
            if(!String.IsNullOrEmpty(providerId)) {
                var map = _Providers;
                map.TryGetValue(providerId, out result);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        public IRebroadcastFormatProvider CreateProvider(string providerId)
        {
            var result = GetProvider(providerId);
            if(result != null) {
                result = result.CreateNewInstance();
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="providerId"></param>
        /// <returns></returns>
        public string ShortName(string providerId)
        {
            var provider = GetProvider(providerId);
            return provider == null ? Strings.Unknown : provider.ShortName;
        }
    }
}
