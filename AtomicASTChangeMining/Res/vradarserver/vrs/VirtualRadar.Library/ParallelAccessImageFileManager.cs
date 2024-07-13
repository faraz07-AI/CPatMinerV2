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
using VirtualRadar.Interface.WebSite;
using InterfaceFactory;
using System.IO;
using VirtualRadar.Interface.Owin;
using VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library
{
    /// <summary>
    /// A simple implementation of <see cref="IImageFileManager"/> that doesn't cache and doesn't
    /// force serial access to different drives or network shares.
    /// </summary>
    /// <remarks>
    /// Since this was originally written a cache has been introduced, but it's just a simple
    /// cache on the web site images. Saves having to fetch every image through the web server.
    /// </remarks>
    class ParallelAccessImageFileManager : IImageFileManager
    {
        /// <summary>
        /// Describes an entry in the cache of web site images.
        /// </summary>
        class CacheEntry
        {
            public string NormalisedFileName;

            public IImage Image;

            public DateTime LastFetchedUtc;
        }

        /// <summary>
        /// The number of seconds that entries can stay in the cache for.
        /// </summary>
        private const int WebSiteCacheExpirySeconds = 60;

        /// <summary>
        /// Map of normalised filenames to images fetched from the web site.
        /// </summary>
        private Dictionary<string, CacheEntry> _WebSiteImageCache = new Dictionary<string,CacheEntry>();

        /// <summary>
        /// Lock object on the cache.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// The clock object.
        /// </summary>
        private IClock _Clock;

        /// <summary>
        /// The loopback host that OWIN requests can be sent through.
        /// </summary>
        private ILoopbackHost _LoopbackHost;

        /// <summary>
        /// True if the class has been initialised.
        /// </summary>
        private bool _Initialised;

        private void Initialise()
        {
            if(!_Initialised) {
                lock(_SyncLock) {
                    if(!_Initialised) {
                        _Initialised = true;
                        _Clock = Factory.Resolve<IClock>();
                        _LoopbackHost = Factory.Resolve<ILoopbackHost>();
                        _LoopbackHost.ConfigureStandardPipeline();
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public IImage LoadFromFile(string fileName)
        {
            Initialise();

            IImage result = null;
            if(!String.IsNullOrEmpty(fileName)) {
                var imageFile = Factory.ResolveSingleton<IImageFile>();
                result = imageFile.LoadFromFile(fileName);
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public Size ImageFileDimensions(string fileName)
        {
            Size result = Size.Empty;
            Initialise();

            var image = LoadFromFile(fileName);
            if(image != null) {
                try {
                    result = image.Size;
                } finally {
                    image.Dispose();
                }
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="webPathAndFileName"></param>
        /// <param name="useImageCache"></param>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        public IImage LoadFromStandardPipeline(string webPathAndFileName, bool useImageCache, IDictionary<string, object> owinEnvironment)
        {
            return LoadFromSiteOrCache(webPathAndFileName, useImageCache, () => FetchFromOwinPipeline(webPathAndFileName, owinEnvironment));
        }

        private IImage LoadFromSiteOrCache(string webPathAndFileName, bool useImageCache, Func<IImage> buildImage)
        {
            IImage result = null;

            Initialise();
            if(!useImageCache) {
                result = buildImage();
            } else {
                var normalisedName = NormaliseWebPath(webPathAndFileName);
                CacheEntry cacheEntry;
                lock(_SyncLock) {
                    _WebSiteImageCache.TryGetValue(normalisedName, out cacheEntry);
                    if(cacheEntry != null && cacheEntry.LastFetchedUtc >= _Clock.UtcNow.AddSeconds(-WebSiteCacheExpirySeconds)) {
                        if(cacheEntry.Image != null) {
                            result = cacheEntry.Image.Clone();
                        }
                    }
                }

                if(cacheEntry == null || (cacheEntry.Image != null && result == null)) {
                    // This can lead to double-fetches, however I would rather have those than have multiple threads block while images are
                    // being fetched serially.
                    result = buildImage();

                    cacheEntry = new CacheEntry() {
                        Image = result == null ? null : result.Clone(),
                        LastFetchedUtc = _Clock.UtcNow,
                        NormalisedFileName = normalisedName,
                    };

                    lock(_SyncLock) {
                        if(!_WebSiteImageCache.TryGetValue(normalisedName, out CacheEntry oldEntry)) {
                            _WebSiteImageCache.Add(normalisedName, cacheEntry);
                        } else {
                            if(oldEntry.Image != null) {
                                oldEntry.Image.Dispose();
                                oldEntry.Image = null;
                            }
                            _WebSiteImageCache[normalisedName] = cacheEntry;
                        }
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Fetches an image from the standard OWIN pipeline.
        /// </summary>
        /// <param name="webPathAndFileName"></param>
        /// <param name="owinEnvironment"></param>
        /// <returns></returns>
        private IImage FetchFromOwinPipeline(string webPathAndFileName, IDictionary<string, object> owinEnvironment)
        {
            return ExtractFromSimpleContent(_LoopbackHost.SendSimpleRequest(webPathAndFileName, owinEnvironment));
        }

        private IImage ExtractFromSimpleContent(SimpleContent simpleContent)
        {
            IImage result = null;

            if(simpleContent != null && simpleContent.HttpStatusCode == System.Net.HttpStatusCode.OK) {
                var imageFile = Factory.ResolveSingleton<IImageFile>();
                result = imageFile.LoadFromByteArray(simpleContent.Content);
            }

            return result;
        }

        /// <summary>
        /// Normalises a web path and filename.
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        private string NormaliseWebPath(string path)
        {
            return (path ?? "").ToUpperInvariant();
        }
    }
}
