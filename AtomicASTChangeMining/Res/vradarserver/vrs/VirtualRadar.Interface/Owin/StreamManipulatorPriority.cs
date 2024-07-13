﻿// Copyright © 2017 onwards, Andrew Whewell
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
using System.Threading.Tasks;

namespace VirtualRadar.Interface.Owin
{
    /// <summary>
    /// A static class enumeration that lays out the order in which stream manipulators are run.
    /// </summary>
    public static class StreamManipulatorPriority
    {
        /// <summary>
        /// The lowest priority that any VRS manipulator uses.
        /// </summary>
        public static readonly int LowestVrsManipulatorPriority = 0;

        /// <summary>
        /// The normal priority for Javascript manipulation.
        /// </summary>
        public static int JavascriptManipulator = LowestVrsManipulatorPriority;

        /// <summary>
        /// The normal priority for HTML manipulation.
        /// </summary>
        public static int HtmlManipulator = JavascriptManipulator + 1000;

        /// <summary>
        /// The normal priority for compressing the response stream if required.
        /// </summary>
        public static int CompressionManipulator = HtmlManipulator + 2000;

        /// <summary>
        /// The highest priority used by VRS content middleware.
        /// </summary>
        public static readonly int HighestVrsManipulatorPriority = CompressionManipulator;
    }
}
