﻿// Copyright © 2010 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using InterfaceFactory;
using VirtualRadar.Interface.WebSite;

namespace VirtualRadar.WebSite
{
    /// <summary>
    /// Initialises the class factory with all the standard implementations in this library.
    /// </summary>
    public static class Implementations
    {
        /// <summary>
        /// Initialises the class factory with all the standard implementations in this library.
        /// </summary>
        /// <param name="factory"></param>
        public static void Register(IClassFactory factory)
        {
            factory.Register<Interface.Owin.IAccessConfiguration, MiddlewareConfiguration.AccessConfiguration>();
            factory.Register<Interface.Owin.IAccessFilter, Middleware.AccessFilter>();
            factory.Register<Interface.Owin.IAudioServer, Middleware.AudioServer>();
            factory.Register<Interface.Owin.IAuthenticationConfiguration, MiddlewareConfiguration.AuthenticationConfiguration>();
            factory.Register<Interface.Owin.IAutoConfigCompressionManipulator, StreamManipulator.AutoConfigCompressionManipulator>();
            factory.Register<Interface.Owin.IBasicAuthenticationFilter, Middleware.BasicAuthenticationFilter>();
            factory.Register<Interface.Owin.IBundlerConfiguration, MiddlewareConfiguration.BundlerConfiguration>();
            factory.Register<Interface.Owin.IBundlerHtmlManipulator, StreamManipulator.BundlerHtmlManipulator>();
            factory.Register<Interface.Owin.IBundlerServer, Middleware.BundlerServer>();
            factory.Register<Interface.Owin.ICorsHandler, Middleware.CorsHandler>();
            factory.Register<Interface.Owin.IFileSystemServer, Middleware.FileSystemServer>();
            factory.Register<Interface.Owin.IFileSystemServerConfiguration, MiddlewareConfiguration.FileSystemServerConfiguration>();
            factory.Register<Interface.Owin.IHtmlManipulator, StreamManipulator.HtmlManipulator>();
            factory.Register<Interface.Owin.IHtmlManipulatorConfiguration, MiddlewareConfiguration.HtmlManipulatorConfiguration>();
            factory.Register<Interface.Owin.IImageServer, Middleware.ImageServer>();
            factory.Register<Interface.Owin.IImageServerConfiguration, MiddlewareConfiguration.ImageServerConfiguration>();
            factory.Register<Interface.Owin.ILoopbackHost, LoopbackHost>();
            factory.Register<Interface.Owin.IJavascriptManipulator, StreamManipulator.JavascriptManipulator>();
            factory.Register<Interface.Owin.IJavascriptManipulatorConfiguration, MiddlewareConfiguration.JavascriptManipulatorConfiguration>();
            factory.Register<Interface.Owin.IMapPluginHtmlManipulator, StreamManipulator.MapPluginHtmlManipulator>();
            factory.Register<Interface.Owin.IRedirectionConfiguration, MiddlewareConfiguration.RedirectionConfiguration>();
            factory.Register<Interface.Owin.IRedirectionFilter, Middleware.RedirectionFilter>();

            factory.Register<IAircraftListJsonBuilder, AircraftListJsonBuilder>();
            factory.Register<IHtmlLocaliser, HtmlLocaliser>();
            factory.Register<IMinifier, Minifier>();
            factory.Register<IWebAdminViewManager, WebAdminViewManagerStub>();
            factory.Register<IWebSite, WebSite>();
            factory.Register<IWebSiteExtender, WebSiteExtender>();
            factory.Register<IWebSiteGraphics, WebSiteGraphics>();
            factory.Register<IWebSitePipelineBuilder, WebSitePipelineBuilder>();

            AWhewell.Owin.Implementations.Register(factory);
            AWhewell.Owin.WebApi.Implementations.Register(factory);
        }
    }
}
