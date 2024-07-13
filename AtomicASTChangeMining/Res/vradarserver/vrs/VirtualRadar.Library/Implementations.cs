// Copyright � 2010 onwards, Andrew Whewell
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
using System.Text;
using VirtualRadar.Interface;
using InterfaceFactory;

namespace VirtualRadar.Library
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
            factory.Register<VirtualRadar.Interface.Adsb.IAdsbTranslator, Adsb.AdsbTranslator>();
            factory.Register<VirtualRadar.Interface.Adsb.ICompactPositionReporting, Adsb.CompactPositionReporting>();
            factory.Register<VirtualRadar.Interface.BaseStation.IBaseStationAircraftList, BaseStation.BaseStationAircraftList>();
            factory.Register<VirtualRadar.Interface.BaseStation.IBaseStationMessageCompressor, BaseStation.BaseStationMessageCompressor>();
            factory.Register<VirtualRadar.Interface.BaseStation.IBaseStationMessageTranslator, BaseStation.BaseStationMessageTranslator>();
            factory.Register<VirtualRadar.Interface.BaseStation.IRawMessageTranslator, BaseStation.RawMessageTranslator>();
            factory.Register<VirtualRadar.Interface.FlightSimulator.IFlightSimulator, FlightSimulator.FlightSimulator>();
            factory.Register<VirtualRadar.Interface.Listener.IAircraftListJsonMessageBytesExtractor, Listener.AircraftListJsonMessageBytesExtractor>();
            factory.Register<VirtualRadar.Interface.Listener.IAircraftListJsonMessageConverter, Listener.AircraftListJsonMessageConverter>();
            factory.Register<VirtualRadar.Interface.Listener.IAirnavXRangeMessageBytesExtractor, Listener.AirnavXRangeMessageBytesExtractor>();
            factory.Register<VirtualRadar.Interface.Listener.IAirnavXRangeMessageConverter, Listener.AirnavXRangeMessageConverter>();
            factory.Register<VirtualRadar.Interface.Listener.IBeastMessageBytesExtractor, Listener.BeastMessageBytesExtractor>();
            factory.Register<VirtualRadar.Interface.Listener.ICompressedMessageBytesExtractor, Listener.CompressedMessageBytesExtractor>();
            factory.Register<VirtualRadar.Interface.Listener.IFeedManager, Listener.FeedManager>();
            factory.Register<VirtualRadar.Interface.Listener.IListener, Listener.Listener>();
            factory.Register<VirtualRadar.Interface.Listener.IMergedFeedComponentListener, Listener.MergedFeedComponentListener>();
            factory.Register<VirtualRadar.Interface.Listener.IMergedFeedFeed, Listener.MergedFeedFeed>();
            factory.Register<VirtualRadar.Interface.Listener.IMergedFeedListener, Listener.MergedFeedListener>();
            factory.Register<VirtualRadar.Interface.Listener.IPlaneFinderMessageBytesExtractor, Listener.PlaneFinderMessageBytesExtractor>();
            factory.Register<VirtualRadar.Interface.Listener.IPolarPlotter, Listener.PolarPlotter>();
            factory.Register<VirtualRadar.Interface.Listener.IPort30003MessageBytesExtractor, Listener.Port30003MessageBytesExtractor>();
            factory.Register<VirtualRadar.Interface.Listener.IReceiverFeed, Listener.ReceiverFeed>();
            factory.Register<VirtualRadar.Interface.Listener.IReceiverFormatManager, Listener.ReceiverFormatManager>();
            factory.Register<VirtualRadar.Interface.Listener.ISbs3MessageBytesExtractor, Listener.Sbs3MessageBytesExtractor>();
            factory.Register<VirtualRadar.Interface.ModeS.IModeSParity, ModeS.ModeSParity>();
            factory.Register<VirtualRadar.Interface.ModeS.IModeSTranslator, ModeS.ModeSTranslator>();
            factory.Register<VirtualRadar.Interface.Network.IConnectorActivityLog, Network.ConnectorActivityLog>();
            factory.Register<VirtualRadar.Interface.Network.IConnectorSnapshotLogger, Network.ConnectorSnapshotLogger>();
            factory.Register<VirtualRadar.Interface.Network.IHttpConnector, Network.HttpConnector>();
            factory.Register<VirtualRadar.Interface.Network.INetworkConnector, Network.NetworkConnector>();
            factory.Register<VirtualRadar.Interface.Network.IPassphraseAuthentication, Network.PassphraseAuthentication>();
            factory.Register<VirtualRadar.Interface.Network.IRebroadcastFormatManager, Network.RebroadcastFormatManager>();
            factory.Register<VirtualRadar.Interface.Network.IRebroadcastServer, Network.RebroadcastServer>();
            factory.Register<VirtualRadar.Interface.Network.IRebroadcastServerManager, Network.RebroadcastServerManager>();
            factory.Register<VirtualRadar.Interface.Network.ISerialConnector, Network.SerialConnector>();
            factory.Register<VirtualRadar.Interface.Network.ITcpConnectionStateService, Network.TcpConnectionStateService>();
            factory.Register<VirtualRadar.Interface.Presenter.IAboutPresenter, Presenter.AboutPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IAircraftOnlineLookupLogPresenter, Presenter.AircraftOnlineLookupLogPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IBackgroundThreadQueuesPresenter, Presenter.BackgroundThreadQueuesPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.ICidrEditPresenter, Presenter.CidrEditPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IConnectionClientLogPresenter, Presenter.ConnectionClientLogPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IConnectionSessionLogPresenter, Presenter.ConnectionSessionLogPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IConnectorActivityLogPresenter, Presenter.ConnectorActivityLogPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IDownloadDataPresenter, Presenter.DownloadDataPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IFlightSimulatorPresenter, Presenter.FlightSimulatorPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IInvalidPluginsPresenter, Presenter.InvalidPluginsPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IMainPresenter, Presenter.MainPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IPluginsPresenter, Presenter.PluginsPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.ISettingsPresenter, Presenter.SettingsPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IShutdownPresenter, Presenter.ShutdownPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.ISplashPresenter, Presenter.SplashPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IStatisticsPresenter, Presenter.StatisticsPresenter>();
            factory.Register<VirtualRadar.Interface.Presenter.IXPlanePresenter, Presenter.XPlanePresenter>();
            factory.Register<VirtualRadar.Interface.Settings.IConfigurationListener, Settings.ConfigurationListener>();
            factory.Register<VirtualRadar.Interface.Settings.IConfigurationStorage, Settings.ConfigurationStorage>();
            factory.Register<VirtualRadar.Interface.Settings.IInstallerSettingsStorage, Settings.InstallerSettingsStorage>();
            factory.Register<VirtualRadar.Interface.Settings.IPluginManifestStorage, Settings.PluginManifestStorage>();
            factory.Register<VirtualRadar.Interface.Settings.IPluginSettingsStorage, Settings.PluginSettingsStorage>();
            factory.Register<VirtualRadar.Interface.Settings.ISavedPolarPlotStorage, Settings.SavedPolarPlotStorage>();
            factory.Register<VirtualRadar.Interface.Settings.ISharedConfiguration, Settings.SharedConfiguration>();
            factory.Register<VirtualRadar.Interface.Settings.ISiteSettingsParser, Settings.SiteSettingsParser>();
            factory.Register<VirtualRadar.Interface.Settings.ITileServerSettingsDownloader, Settings.TileServerSettingsDownloader>();
            factory.Register<VirtualRadar.Interface.Settings.ITileServerSettingsManager, Settings.TileServerSettingsManager>();
            factory.Register<VirtualRadar.Interface.Settings.ITileServerSettingsStorage, Settings.TileServerSettingsStorage>();
            factory.Register<VirtualRadar.Interface.XPlane.IXPlaneConnection, XPlane.XPlaneConnection>();
            factory.Register<VirtualRadar.Interface.XPlane.IXPlaneSettingsStorage, XPlane.XPlaneSettingsStorage>();
            factory.Register<VirtualRadar.Interface.XPlane.IXPlaneUdp, XPlane.XPlaneUdp>();
            factory.Register<VirtualRadar.Interface.Settings.IUserCache, Settings.UserCache>();
            factory.Register<IAccessFilter, AccessFilter>();
            factory.Register<IAircraft, Aircraft>();
            factory.Register<IAircraftComparer, AircraftComparer>();
            factory.Register<IAircraftDetailFetcher, AircraftDetailFetcher>();
            factory.Register<IAircraftOnlineLookup, AircraftOnlineLookup>();
            factory.Register<IAircraftOnlineLookupLog, AircraftOnlineLookupLog>();
            factory.Register<IAircraftOnlineLookupManager, AircraftOnlineLookupManager>();
            factory.Register<IAircraftOnlineLookupProvider, AircraftOnlineLookupProvider>();
            factory.Register<IAircraftPictureManager, AircraftPictureManager>();
            factory.Register<IAircraftSanityChecker, AircraftSanityChecker>();
            factory.Register<IAirportDataDotCom, AirportDataDotCom>();
            factory.Register<IAirPressureDownloader, AirPressureDownloader>();
            factory.Register<IAirPressureLookup, AirPressureLookup>();
            factory.Register<IAirPressureManager, AirPressureManager>();
            factory.Register<IAudio, Audio>();
            factory.Register<IAutoConfigPictureFolderCache, AutoConfigPictureFolderCache>();
            factory.Register<IBackgroundWorker, BackgroundWorker>();
            factory.Register<IBitStream, BitStream>();
            factory.Register<ICallsignParser, CallsignParser>();
            factory.Register<IClock, Clock>();
            factory.Register<IConnectionLogger, ConnectionLogger>();
            factory.Register<IConsole, ConsoleWrapper>();
            factory.Register<IDirectoryCache, DirectoryCache>();
            factory.Register<IExceptionReporter, ExceptionReporter>();
            factory.Register<IExternalIPAddressService, ExternalIPAddressService>();
            factory.Register<IFileSystemProvider, FileSystemProvider>();
            factory.Register<IFileSystemWatcher, FileSystemWatcherWrapper>();
            factory.Register<IHeartbeatService, HeartbeatService>();
            factory.Register<IImageDimensionsFetcher, ImageDimensionsFetcher>();
            factory.Register<IImageFileManager, ParallelAccessImageFileManager>();
            factory.Register<ILog, Log>();
            factory.Register<INewVersionChecker, NewVersionChecker>();
            factory.Register<IPluginManager, PluginManager>();
            factory.Register<IRegistrationPrefixLookup, RegistrationPrefixLookup>();
            factory.Register<IRuntimeEnvironment, RuntimeEnvironment>();
            factory.Register<ISimpleAircraftList, SimpleAircraftList>();
            factory.Register<IStatistics, Statistics>();
            factory.Register<ITimer, Timer>();
            factory.Register<IWebAddressManager, WebAddressManager>();
            factory.Register<IXmlSerialiser, XmlSerialiser>();

            // System.Drawing graphics
            factory.Register<VirtualRadar.Interface.Drawing.IBrushFactory,  Drawing.SystemDrawing.BrushFactory>();
            factory.Register<VirtualRadar.Interface.Drawing.IFontFactory,   Drawing.SystemDrawing.FontFactory>();
            factory.Register<VirtualRadar.Interface.Drawing.IImageFile,     Drawing.SystemDrawing.ImageFile>();
            factory.Register<VirtualRadar.Interface.Drawing.IPenFactory,    Drawing.SystemDrawing.PenFactory>();

            factory.Register<IFlightSimulatorAircraftList, FlightSimulator.FlightSimulatorAircraftList>();
            if(Type.GetType("Mono.Runtime") == null) {
                factory.Register<IShutdownSignalHandler, DotNetShutdownSignalHandler>();
                factory.Register<ISpeechSynthesizerWrapper, DotNetSpeechSynthesizerWrapper>();
                factory.Register<VirtualRadar.Interface.FlightSimulator.ISimConnectWrapper, FlightSimulator.DotNetSimConnectWrapper>();
            } else {
                factory.Register<IShutdownSignalHandler, MonoShutdownSignalHandler>();
                factory.Register<ISpeechSynthesizerWrapper, MonoSpeechSynthesizerWrapper>();
                factory.Register<VirtualRadar.Interface.FlightSimulator.ISimConnectWrapper, FlightSimulator.MonoSimConnectWrapper>();
            }
        }
    }
}
