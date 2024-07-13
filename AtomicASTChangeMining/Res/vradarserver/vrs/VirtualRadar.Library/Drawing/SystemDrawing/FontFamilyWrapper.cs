﻿// Copyright © 2019 onwards, Andrew Whewell
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
using System.Drawing;
using VrsDrawing = VirtualRadar.Interface.Drawing;

namespace VirtualRadar.Library.Drawing.SystemDrawing
{
    /// <summary>
    /// Wrapper around an System.Drawing font family.
    /// </summary>
    class FontFamilyWrapper : CommonFontFamilyWrapper<FontFamily>
    {
        /// <summary>
        /// See interface docs.
        /// </summary>
        public override string Name => NativeFontFamily.Name;

        private VrsDrawing.FontStyle[] _AvailableStyles;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public override IEnumerable<VrsDrawing.FontStyle> AvailableStyles
        {
            get {
                var result = _AvailableStyles;
                if(result == null) {
                    var availableStyles = new List<VrsDrawing.FontStyle>();
                    foreach(VrsDrawing.FontStyle fontStyle in Enum.GetValues(typeof(VrsDrawing.FontStyle))) {
                        if(NativeFontFamily.IsStyleAvailable(Convert.ToSystemDrawingFontStyle(fontStyle))) {
                            availableStyles.Add(fontStyle);
                        }
                    }

                    result = availableStyles.ToArray();
                    _AvailableStyles = result;
                }
                return result;
            }
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        /// <param name="nativeFontFamily"></param>
        /// <param name="isCached"></param>
        public FontFamilyWrapper(FontFamily nativeFontFamily, bool isCached) : base(nativeFontFamily, isCached)
        {
            ;
        }
    }
}
