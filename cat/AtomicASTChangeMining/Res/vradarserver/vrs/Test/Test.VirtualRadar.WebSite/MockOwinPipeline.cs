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
using System.Threading.Tasks;

namespace Test.VirtualRadar.WebSite
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    /// <summary>
    /// Mocks an OWIN pipeline.
    /// </summary>
    class MockOwinPipeline
    {
        /// <summary>
        /// Gets or sets a flag indicating that the next task in the pipeline was called.
        /// </summary>
        public bool NextMiddlewareCalled { get; set; }

        /// <summary>
        /// Gets or sets an action that is called when the next middleware is called.
        /// </summary>
        public Action<IDictionary<string, object>> NextMiddlewareCallback { get; set; }

        /// <summary>
        /// Calls the middleware passed across. Sets or clears <see cref="NextMiddlewareCalled"/> if the
        /// middleware calls the next function in the chain.
        /// </summary>
        /// <param name="appFunc"></param>
        /// <param name="environment"></param>
        public void BuildAndCallMiddleware(Func<AppFunc, AppFunc> middlewareEntryPoint, IDictionary<string, object> environment)
        {
            NextMiddlewareCalled = false;

            AppFunc nextMiddleware = (IDictionary<string, object> env) => {
                NextMiddlewareCalled = true;
                NextMiddlewareCallback?.Invoke(env);
                return Task.FromResult(0);
            };

            AppFunc testMiddleware = middlewareEntryPoint(nextMiddleware);
            testMiddleware.Invoke(environment);
        }

        /// <summary>
        /// Calls the middleware passed across. Sets or clears <see cref="NextMiddlewareCalled"/> if the
        /// middleware calls the next function in the chain.
        /// </summary>
        /// <param name="middlewareEntryPoint"></param>
        /// <param name="environment"></param>
        public void BuildAndCallMiddleware(Func<AppFunc, AppFunc> middlewareEntryPoint, MockOwinEnvironment environment)
        {
            BuildAndCallMiddleware(middlewareEntryPoint, environment.Environment);
        }
    }
}
