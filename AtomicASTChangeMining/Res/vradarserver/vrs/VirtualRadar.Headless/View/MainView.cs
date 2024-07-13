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
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;

namespace VirtualRadar.Headless.View
{
    /// <summary>
    /// The headless implementation of the main view.
    /// </summary>
    class MainView : BaseView, IMainView
    {
        // Disable the warning about events not being used
        #pragma warning disable 0067

        // Objects that are being held for _Presenter while it doesn't exist.
        private IUniversalPlugAndPlayManager _UPnpManager;

        /// <summary>
        /// True if <see cref="CloseView"/> is called.
        /// </summary>
        private bool _ForceQuit;

        private int _InvalidPluginCount;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int InvalidPluginCount
        {
            get { return _InvalidPluginCount; }
            set {
                if(_InvalidPluginCount != value) {
                    _InvalidPluginCount = value;
                    if(_InvalidPluginCount != 0) {
                        _Console.WriteLine(Strings.InvalidPluginsCount, _InvalidPluginCount);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string LogFileName { get; set; }

        private bool _NewVersionAvailable;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool NewVersionAvailable
        {
            get { return _NewVersionAvailable; }
            set {
                if(_NewVersionAvailable != value) {
                    _NewVersionAvailable = value;
                    if(_NewVersionAvailable) {
                        _Console.WriteLine(Strings.LaterVersionAvailable);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string NewVersionDownloadUrl { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string RebroadcastServersConfiguration { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UPnpEnabled { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UPnpRouterPresent { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UPnpPortForwardingActive { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WebServerIsOnline { get; set; }

        private string _WebServerLocalAddress;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string WebServerLocalAddress
        {
            get { return _WebServerLocalAddress; }
            set {
                if(_WebServerLocalAddress != value) {
                    _WebServerLocalAddress = value;
                    if(!String.IsNullOrEmpty(_WebServerLocalAddress)) {
                        _Console.WriteLine(Strings.LocalAddress, _WebServerLocalAddress);
                    }
                }
            }
        }

        private string _WebServerNetworkAddress;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string WebServerNetworkAddress
        {
            get { return _WebServerNetworkAddress; }
            set {
                if(_WebServerNetworkAddress != value) {
                    _WebServerNetworkAddress = value;
                    if(!String.IsNullOrEmpty(_WebServerNetworkAddress)) {
                        _Console.WriteLine(Strings.NetworkAddress, _WebServerNetworkAddress);
                    }
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string WebServerExternalAddress { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CheckForNewVersion;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<IFeed>> ReconnectFeed;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<IFeed>> ResetPolarPlot;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ToggleServerStatus;

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ToggleUPnpStatus;

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public DialogResult ShowView()
        {
            var presenter = Factory.Resolve<IMainPresenter>();
            presenter.UPnpManager = _UPnpManager;
            presenter.Initialise(this);

            _Console.WriteLine(Strings.PressQToQuit);
            for(var quit = false;!quit && !_ForceQuit;) {
                Thread.Sleep(1);

                if(_Console.KeyAvailable) {
                    var key = _Console.ReadKey(intercept: true);
                    if(key != null && key.Key == ConsoleKey.Q) {
                        quit = true;
                    }
                }
            }

            return DialogResult.OK;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void CloseView()
        {
            _ForceQuit = true;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uPnpManager"></param>
        /// <param name="flightSimulatorXAircraftList"></param>
        public void Initialise(IUniversalPlugAndPlayManager uPnpManager, ISimpleAircraftList flightSimulatorXAircraftList)
        {
            _UPnpManager = uPnpManager;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ex"></param>
        public void BubbleExceptionToGui(Exception ex)
        {
            var exceptionReporter = Factory.Resolve<IExceptionReporter>();
            exceptionReporter.ShowUnhandledException(ex);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="newVersionAvailable"></param>
        public void ShowManualVersionCheckResult(bool newVersionAvailable)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feeds"></param>
        public void ShowFeeds(FeedStatus[] feeds)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feeds"></param>
        public void UpdateFeedCounters(FeedStatus[] feeds)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="serverRequests"></param>
        public void ShowServerRequests(ServerRequest[] serverRequests)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feed"></param>
        public void ShowFeedConnectionStatus(FeedStatus feed)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="connections"></param>
        public void ShowRebroadcastServerStatus(IList<RebroadcastServerConnection> connections)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="openOnPageTitle"></param>
        /// <param name="openOnConfigurationObject"></param>
        public void ShowSettingsConfigurationUI(string openOnPageTitle, object openOnConfigurationObject)
        {
            ;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="isBusy"></param>
        /// <param name="previousState"></param>
        /// <returns></returns>
        public object ShowBusy(bool isBusy, object previousState)
        {
            return new object();
        }

    }
}
