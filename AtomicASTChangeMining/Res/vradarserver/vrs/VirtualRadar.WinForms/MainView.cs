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
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Presenter;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.View;
using VirtualRadar.Interface.WebServer;
using VirtualRadar.Localisation;

namespace VirtualRadar.WinForms
{
    /// <summary>
    /// The WinForms implementation of <see cref="IMainView"/>.
    /// </summary>
    public partial class MainView : BaseForm, IMainView
    {
        #region Fields
        /// <summary>
        /// The presenter that is managing this view.
        /// </summary>
        private IMainPresenter _Presenter;

        /// <summary>
        /// The object that's handling online help for us.
        /// </summary>
        private OnlineHelpHelper _OnlineHelp;

        /// <summary>
        /// The current instance of the modeless dialog that displays the FSX connection, if any.
        /// </summary>
        private FlightSimulatorView _FlightSimulatorXView;

        /// <summary>
        /// The current instance of the modeless dialog that displays the XPlane connection.
        /// </summary>
        private XPlaneView _XPlaneView;

        /// <summary>
        /// The current instance of the modeless dialog that displays the statistics, if any, against the unique ID of the feed being displayed.
        /// </summary>
        private Dictionary<int, StatisticsView> _StatisticsViews = new Dictionary<int,StatisticsView>();

        /// <summary>
        /// True if the form has been closed.
        /// </summary>
        private bool _Closed;

        // Objects that are being held for _Presenter while it doesn't exist.
        private ISimpleAircraftList _FlightSimulatorXAircraftList;
        private IUniversalPlugAndPlayManager _UPnpManager;
        #endregion

        #region Properties
        private int _InvalidPluginCount;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public int InvalidPluginCount
        {
            get { return _InvalidPluginCount; }
            set
            {
                if(!SafelyInvoke(() => InvalidPluginCount = value)) {
                    _InvalidPluginCount = value;
                    toolStripDropDownButtonInvalidPluginCount.Text = String.Format(Strings.CountPluginsCouldNotBeLoaded, value);
                    toolStripDropDownButtonInvalidPluginCount.Visible = value != 0;
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
            set
            {
                if(!SafelyInvoke(() => NewVersionAvailable = value)) {
                    _NewVersionAvailable = value;
                    toolStripDropDownButtonLaterVersionAvailable.Visible = value;
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
        public string RebroadcastServersConfiguration
        {
            get { return rebroadcastStatusControl.Configuration; }
            set {
                if(!SafelyInvoke(() => RebroadcastServersConfiguration = value)) {
                    rebroadcastStatusControl.Configuration = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UPnpEnabled
        {
            get { return webServerStatusControl.UPnpEnabled; }
            set {
                if(!SafelyInvoke(() => UPnpEnabled = value)) {
                    webServerStatusControl.UPnpEnabled = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UPnpRouterPresent
        {
            get { return webServerStatusControl.UPnpRouterPresent; }
            set {
                if(!SafelyInvoke(() => UPnpRouterPresent = value)) {
                    webServerStatusControl.UPnpRouterPresent = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UPnpPortForwardingActive
        {
            get { return webServerStatusControl.UPnpPortForwardingActive; }
            set {
                if(!SafelyInvoke(() => UPnpPortForwardingActive = value)) {
                    webServerStatusControl.UPnpPortForwardingActive = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool WebServerIsOnline
        {
            get { return webServerStatusControl.ServerIsListening; }
            set {
                if(!SafelyInvoke(() => WebServerIsOnline = value)) {
                    webServerStatusControl.ServerIsListening = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string WebServerLocalAddress
        {
            get { return webServerStatusControl.LocalAddress; }
            set {
                if(!SafelyInvoke(() => WebServerLocalAddress = value)) {
                    webServerStatusControl.LocalAddress = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string WebServerNetworkAddress
        {
            get { return webServerStatusControl.NetworkAddress; }
            set {
                if(!SafelyInvoke(() => WebServerNetworkAddress = value)) {
                    webServerStatusControl.NetworkAddress = value;
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public string WebServerExternalAddress
        {
            get { return webServerStatusControl.InternetAddress; }
            set {
                if(!SafelyInvoke(() => WebServerExternalAddress = value)) {
                    webServerStatusControl.InternetAddress = value;
                }
            }
        }
        #endregion

        #region Events exposed
        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler CheckForNewVersion;

        /// <summary>
        /// Raises <see cref="CheckForNewVersion"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnCheckForNewVersion(EventArgs args)
        {
            EventHelper.Raise(CheckForNewVersion, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<IFeed>> ReconnectFeed;

        /// <summary>
        /// Raises <see cref="ReconnectFeed"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnReconnectFeed(EventArgs<IFeed> args)
        {
            EventHelper.Raise(ReconnectFeed, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<IFeed>> ResetPolarPlot;

        /// <summary>
        /// Raises <see cref="ResetPolarPlot"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnResetPolarPlot(EventArgs<IFeed> args)
        {
            EventHelper.Raise(ResetPolarPlot, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ToggleServerStatus;

        /// <summary>
        /// Raises <see cref="ToggleServerStatus"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnToggleServerStatus(EventArgs args)
        {
            EventHelper.Raise(ToggleServerStatus, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler ToggleUPnpStatus;

        /// <summary>
        /// Raises <see cref="ToggleUPnpStatus"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnToggleUPnpStatus(EventArgs args)
        {
            EventHelper.Raise(ToggleUPnpStatus, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler RefreshTimerTicked;

        /// <summary>
        /// Raises <see cref="RefreshTimerTicked"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnRefreshTimerTicked(EventArgs args)
        {
            EventHelper.Raise(RefreshTimerTicked, this, args);
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public MainView() : base()
        {
            InitializeComponent();
        }
        #endregion

        #region Initialise
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="uPnpManager"></param>
        /// <param name="flightSimulatorXAircraftList"></param>
        public void Initialise(IUniversalPlugAndPlayManager uPnpManager, ISimpleAircraftList flightSimulatorXAircraftList)
        {
            _FlightSimulatorXAircraftList = flightSimulatorXAircraftList;
            _UPnpManager = uPnpManager;

            webServerStatusControl.UPnpIsSupported = true; // IsSupported was in the original UPnP code but was dropped in the re-write. Keeping UI elements for it in case I decide to put it back.
        }
        #endregion

        #region ShowView, CloseView
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <returns></returns>
        public override DialogResult ShowView()
        {
            Application.Run(this);

            return DialogResult.OK;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void CloseView()
        {
            if(InvokeRequired) {
                BeginInvoke(new MethodInvoker(() => CloseView()));
            } else {
                if(!_Closed) {
                    Close();
                }
            }
        }
        #endregion

        #region BubbleExceptionToGui
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="ex"></param>
        public void BubbleExceptionToGui(Exception ex)
        {
            if(!(ex is ThreadAbortException)) {
                if(!SafelyInvoke(() => BubbleExceptionToGui(ex))) {
                    throw new ApplicationException("Exception thrown on background thread", ex);
                }
            }
        }
        #endregion

        #region ShowServerRequests
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="serverRequests"></param>
        public void ShowServerRequests(ServerRequest[] serverRequests)
        {
            if(!SafelyInvoke(() => ShowServerRequests(serverRequests))) {
                webServerStatusControl.ShowServerRequests(serverRequests);
            }
        }
        #endregion

        #region ShowFeeds, UpdateFeedCounters, ShowFeedConnectionStatus
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="receivers"></param>
        public void ShowFeeds(FeedStatus[] receivers)
        {
            feedStatusControl.ShowFeeds(receivers);

            var openStatisticFeedIds = _StatisticsViews.Keys.ToArray();
            var closeStatisticFeedIds = openStatisticFeedIds.Except(receivers.Select(r => r.FeedId)).ToArray();
            foreach(var feedId in closeStatisticFeedIds) {
                CloseStatisticsView(feedId, _StatisticsViews[feedId]);
            }

            foreach(var kvp in _StatisticsViews) {
                kvp.Value.FeedName = receivers.Single(r => r.FeedId == kvp.Key).Name;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feeds"></param>
        public void UpdateFeedCounters(FeedStatus[] feeds)
        {
            feedStatusControl.ShowFeeds(feeds);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="feed"></param>
        public void ShowFeedConnectionStatus(FeedStatus feed)
        {
            feedStatusControl.ShowFeed(feed);
        }
        #endregion

        #region ShowStatisticsView, CloseStatisticsView
        private void ShowStatisticsView(IFeed feed)
        {
            if(feed is INetworkFeed networkFeed && networkFeed.Listener.Statistics != null) {
                if(_StatisticsViews.TryGetValue(networkFeed.UniqueId, out StatisticsView view)) {
                    view.WindowState = FormWindowState.Normal;
                    view.Activate();
                } else {
                    view = new StatisticsView {
                        Statistics = networkFeed.Listener.Statistics,
                        FeedName = networkFeed.Name
                    };
                    view.CloseClicked += StatisticsView_CloseClicked;
                    view.Show();

                    _StatisticsViews.Add(networkFeed.UniqueId, view);
                }
            }
        }

        private void CloseStatisticsView(int feedId, StatisticsView statisticsView)
        {
            statisticsView.CloseClicked -= StatisticsView_CloseClicked;
            statisticsView.Close();
            statisticsView.Dispose();
            _StatisticsViews.Remove(feedId);
        }
        #endregion

        #region ShowManualVersionCheckResult, ShowWebRequestHasBeenServiced
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="newVersionAvailable"></param>
        public void ShowManualVersionCheckResult(bool newVersionAvailable)
        {
            MessageBox.Show(newVersionAvailable ? Strings.LaterVersionAvailable : Strings.LatestVersion, Strings.VersionCheckResult);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="connections"></param>
        public void ShowRebroadcastServerStatus(IList<RebroadcastServerConnection> connections)
        {
            if(!SafelyInvoke(() => ShowRebroadcastServerStatus(connections))) {
                rebroadcastStatusControl.DisplayRebroadcastServerConnections(connections);
            }
        }
        #endregion

        #region ShowSettingsConfigurationUI
        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="openOnPageTitle"></param>
        /// <param name="openOnConfigurationObject"></param>
        public void ShowSettingsConfigurationUI(string openOnPageTitle, object openOnConfigurationObject)
        {
            using(var dialog = new SettingsView()) {
                dialog.OpenOnPageTitle = openOnPageTitle;
                dialog.OpenOnRecord = openOnConfigurationObject;

                dialog.ShowDialog(this);
            }
        }
        #endregion

        #region MinimiseToNotificationTray, RestoreFromNotificationTray
        private void MinimiseToNotificationTray()
        {
            notifyIcon.Visible = true;
            Hide();
        }

        private void RestoreFromNotificationTray()
        {
            Show();
            WindowState = FormWindowState.Normal;
            Activate();
        }
        #endregion

        #region Events consumed
        /// <summary>
        /// Called when the form is closed.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _Closed = true;
        }

        /// <summary>
        /// Called when the form is ready for use but not yet on screen.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if(!DesignMode) {
                toolStripDropDownButtonInvalidPluginCount.Visible = false;
                toolStripDropDownButtonLaterVersionAvailable.Visible = false;

                Localise.Form(this);
                Localise.ToolStrip(contextMenuStripNotifyIcon);
                notifyIcon.Text = Strings.VirtualRadarServer;

                _OnlineHelp = new OnlineHelpHelper(this, OnlineHelpAddress.WinFormsMainDialog);

                _Presenter = Factory.Resolve<IMainPresenter>();
                _Presenter.Initialise(this);
                _Presenter.UPnpManager = _UPnpManager;

                var runtimeEnvironment = Factory.ResolveSingleton<IRuntimeEnvironment>();
                if(runtimeEnvironment.Is64BitProcess) {
                    Text = $"{Text} ({Strings.Title64Bit})";
                }

                var applicationInfo = Factory.Resolve<IApplicationInformation>();
                if(applicationInfo.IsBeta) {
                    Text = $"{Text} [{Strings.Beta}]";
                }
            }
        }

        /// <summary>
        /// Called when the form is resized.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnResize(EventArgs e)
        {
            if(WindowState == FormWindowState.Minimized) {
                var configuration = Factory.ResolveSingleton<IConfigurationStorage>().Load();
                if(configuration.BaseStationSettings.MinimiseToSystemTray) MinimiseToNotificationTray();
            } else if(WindowState == FormWindowState.Normal) {
                if(notifyIcon.Visible) notifyIcon.Visible = false;
            }

            base.OnResize(e);
        }

        private void menuAboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var dialog = new AboutView()) {
                dialog.ShowDialog();
            }
        }

        private void menuCheckForUpdatesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OnCheckForNewVersion(EventArgs.Empty);
        }

        private void menuConnectionClientLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var dialog = new ConnectionClientLogView()) {
                dialog.ShowDialog();
            }
        }

        private void menuConnectionSessionLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var dialog = new ConnectionSessionLogView()) {
                dialog.ShowDialog();
            }
        }

        private void menuDownloadDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(DownloadDataView dialog = new DownloadDataView()) {
                dialog.ShowDialog();
            }
        }

        private void menuExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void menuXPlaneModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_XPlaneView != null) {
                _XPlaneView.Activate();
            } else {
                _XPlaneView = new XPlaneView();
                _XPlaneView.CloseClicked += XPlaneView_CloseClicked;
                _XPlaneView.Show();
            }
        }

        private void XPlaneView_CloseClicked(object sender, EventArgs e)
        {
            if(_XPlaneView != null) {
                _XPlaneView.CloseClicked -= XPlaneView_CloseClicked;
                _XPlaneView.Close();
                _XPlaneView.Dispose();
                _XPlaneView = null;
            }
        }

        private void menuFlightSimulatorXModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(_FlightSimulatorXView != null) _FlightSimulatorXView.Activate();
            else {
                var webServer = Factory.ResolveSingleton<IAutoConfigWebServer>().WebServer;
                _FlightSimulatorXView = new FlightSimulatorView();
                _FlightSimulatorXView.CloseClicked += FlightSimulatorXView_CloseClicked;
                _FlightSimulatorXView.Initialise(null, _FlightSimulatorXAircraftList, webServer);
                _FlightSimulatorXView.Show();
            }
        }

        private void FlightSimulatorXView_CloseClicked(object sender, EventArgs e)
        {
            if(_FlightSimulatorXView != null) {
                _FlightSimulatorXView.CloseClicked -= FlightSimulatorXView_CloseClicked;
                _FlightSimulatorXView.Close();
                _FlightSimulatorXView.Dispose();
                _FlightSimulatorXView = null;
            }
        }

        private void menuOpenVirtualRadarLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start(LogFileName);
        }

        private void menuPluginsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var dialog = new PluginsView()) {
                dialog.ShowDialog();
            }
        }

        private void menuOptionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShowSettingsConfigurationUI(null, null);
        }

        private void webServerStatusControl_ToggleServerStatus(object sender, EventArgs e)
        {
            OnToggleServerStatus(e);
        }

        private void webServerStatusControl_ToggleUPnpStatus(object sender, EventArgs e)
        {
            OnToggleUPnpStatus(e);
        }

        private void toolStripDropDownButtonInvalidPluginCount_Click(object sender, EventArgs e)
        {
            using(var dialog = new InvalidPluginsView()) {
                dialog.ShowDialog();
            }
        }

        private void toolStripDropDownButtonLaterVersionAvailable_Click(object sender, EventArgs e)
        {
            Process.Start(NewVersionDownloadUrl);
        }

        private void AttachReceiverFeedMenuItems(ToolStripMenuItem menuItem, EventHandler clickHandler, Func<IFeed, bool> enabledDelegate = null)
        {
            foreach(ToolStripItem feedItem in menuItem.DropDownItems) {
                feedItem.Click -= clickHandler;
            }
            menuItem.DropDownItems.Clear();

            var feeds = _Presenter.GetReceiverFeeds();
            foreach(var feed in feeds.OrderBy(r => r.Name)) {
                var feedItem = menuItem.DropDownItems.Add(feed.Name);
                feedItem.Tag = feed.UniqueId;
                feedItem.Click += clickHandler;

                if(enabledDelegate != null) feedItem.Enabled = enabledDelegate(feed);
            }
        }

        private IFeed GetFeedItemFeed(int feedId)
        {
            return _Presenter.GetReceiverFeeds().FirstOrDefault(r => r.UniqueId == feedId);
        }

        private IFeed GetFeedItemFeed(ToolStripItem feedMenuItem)
        {
            var feedId = (int)feedMenuItem.Tag;
            return GetFeedItemFeed(feedId);
        }

        private void menuFileToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            AttachReceiverFeedMenuItems(menuStatisticsToolStripMenuItem, feedItem_ShowStatistics);
        }

        private void feedItem_ShowStatistics(object sender, EventArgs args)
        {
            var menuItem = (ToolStripItem)sender;
            var feed = GetFeedItemFeed(menuItem);
            ShowStatisticsView(feed);
        }

        private void StatisticsView_CloseClicked(object sender, EventArgs e)
        {
            var view = sender as StatisticsView;
            var uniqueId = _StatisticsViews.Where(r => r.Value == view).Select(r => r.Key).FirstOrDefault();
            if(uniqueId != 0) CloseStatisticsView(uniqueId, view);
        }

        private void menuToolsToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            AttachReceiverFeedMenuItems(menuReconnectToDataFeedToolStripMenuItem, feedItem_ReconnectToDataFeedClicked);
            AttachReceiverFeedMenuItems(
                menuResetReceiverRangeToolStripMenuItem,
                feedItem_ResetReceiverRangeFeedClicked,
                feed => (feed?.AircraftList as IPolarPlottingAircraftList)?.PolarPlotter != null
            );
        }

        private void feedItem_ReconnectToDataFeedClicked(object sender, EventArgs args)
        {
            var menuItem = (ToolStripItem)sender;
            var feed = GetFeedItemFeed(menuItem);
            if(feed != null) OnReconnectFeed(new EventArgs<IFeed>(feed));
        }

        private void feedItem_ResetReceiverRangeFeedClicked(object sender, EventArgs args)
        {
            var menuItem = (ToolStripItem)sender;
            var feed = GetFeedItemFeed(menuItem);
            if(feed != null) OnResetPolarPlot(new EventArgs<IFeed>(feed));
        }

        private void feedStatusControl_ReconnectFeedId(object sender, Controls.FeedIdEventArgs e)
        {
            var feed = _Presenter.GetReceiverFeeds().FirstOrDefault(r => r.UniqueId == e.FeedId);
            if(feed != null) OnReconnectFeed(new EventArgs<IFeed>(feed));
        }

        private void feedStatusControl_ShowFeedIdStatistics(object sender, Controls.FeedIdEventArgs e)
        {
            var feed = _Presenter.GetReceiverFeeds().FirstOrDefault(r => r.UniqueId == e.FeedId);
            if(feed != null) ShowStatisticsView(feed);
        }

        private void feedStatusControl_ResetPolarPlotter(object sender, Controls.FeedIdEventArgs e)
        {
            var feed = _Presenter.GetReceiverFeeds().FirstOrDefault(r => r.UniqueId == e.FeedId);
            if(feed != null) OnResetPolarPlot(new EventArgs<IFeed>(feed));
        }

        private void feedStatusControl_ConfigureFeed(object sender, Controls.FeedIdEventArgs e)
        {
            var feedConfiguration = _Presenter.GetFeedConfigurationObject(e.FeedId);
            if(feedConfiguration != null) ShowSettingsConfigurationUI(null, feedConfiguration);
        }

        private void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            RestoreFromNotificationTray();
        }

        private void showWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            RestoreFromNotificationTray();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void rebroadcastStatusControl_ShowRebroadcastServersConfigurationClicked(object sender, EventArgs e)
        {
            ShowSettingsConfigurationUI(Strings.RebroadcastServersTitle, null);
        }

        private void menuOpenConnectionLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var dialog = new ConnectorActivityLogView()) {
                dialog.ShowDialog(this);
            }
        }

        private void menuShowQueuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var dialog = new BackgroundThreadQueuesView()) {
                dialog.ShowDialog(this);
            }
        }

        private void menuAircraftOnlineDetailLookupLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using(var dialog = new AircraftOnlineLookupLogView()) {
                dialog.ShowDialog(this);
            }
        }
        #endregion
    }
}
