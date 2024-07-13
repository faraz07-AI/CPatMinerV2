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
using System.Text;
using System.ComponentModel;
using System.Linq.Expressions;

namespace VirtualRadar.Interface.Settings
{
    /// <summary>
    /// The web site configuration options (originally these were just Google Map settings but they
    /// expanded over time - unfortunately I can't change the class name without breaking backwards
    /// compatibility).
    /// </summary>
    public class GoogleMapSettings : INotifyPropertyChanged
    {
        private MapProvider _MapProvider;
        /// <summary>
        /// Gets or sets the map provider to use.
        /// </summary>
        public MapProvider MapProvider
        {
            get { return _MapProvider; }
            set { SetField(ref _MapProvider, value, nameof(MapProvider)); }
        }

        private string _InitialSettings;
        /// <summary>
        /// Gets or sets the initial settings to use for new visitors.
        /// </summary>
        public string InitialSettings
        {
            get { return _InitialSettings; }
            set { SetField(ref _InitialSettings, value, nameof(InitialSettings)); }
        }

        private double _InitialMapLatitude;
        /// <summary>
        /// Gets or sets the initial latitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLatitude
        {
            get { return _InitialMapLatitude; }
            set { SetField(ref _InitialMapLatitude, value, nameof(InitialMapLatitude)); }
        }

        private double _InitialMapLongitude;
        /// <summary>
        /// Gets or sets the initial longitude to show. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public double InitialMapLongitude
        {
            get { return _InitialMapLongitude; }
            set { SetField(ref _InitialMapLongitude, value, nameof(InitialMapLongitude)); }
        }

        private string _InitialMapType;
        /// <summary>
        /// Gets or sets the initial map type to use (terrain, satellite etc.). This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public string InitialMapType
        {
            get { return _InitialMapType; }
            set { SetField(ref _InitialMapType, value, nameof(InitialMapType)); }
        }

        private int _InitialMapZoom;
        /// <summary>
        /// Gets or sets the initial level of zoom to use. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public int InitialMapZoom
        {
            get { return _InitialMapZoom; }
            set { SetField(ref _InitialMapZoom, value, nameof(InitialMapZoom)); }
        }

        private int _InitialRefreshSeconds;
        /// <summary>
        /// Gets or sets the initial refresh period. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        /// <remarks>
        /// For historical reasons the browser always adds one second to whatever value it has been configured to use. Setting 0 here indicates a 1 second refresh period,
        /// a 1 is 2 seconds and so on.
        /// </remarks>
        public int InitialRefreshSeconds
        {
            get { return _InitialRefreshSeconds; }
            set { SetField(ref _InitialRefreshSeconds, value, nameof(InitialRefreshSeconds)); }
        }

        private int _MinimumRefreshSeconds;
        /// <summary>
        /// Gets or sets the smallest refresh period that the browser will allow the user to set.
        /// </summary>
        /// <remarks>
        /// This setting is difficult to police in the server so it should just be taken as a hint to well-behaved code rather than a guarantee that the server will reject
        /// the second and subsequent request under this threshold.
        /// </remarks>
        public int MinimumRefreshSeconds
        {
            get { return _MinimumRefreshSeconds; }
            set { SetField(ref _MinimumRefreshSeconds, value, nameof(MinimumRefreshSeconds)); }
        }

        private int _ShortTrailLengthSeconds;
        /// <summary>
        /// Gets or sets the number of seconds that short trails are to be stored for.
        /// </summary>
        /// <remarks>
        /// Short trails are lines connecting the current position of the aircraft to each coordinate it was at over the past NN seconds. This property holds the NN value.
        /// </remarks>
        public int ShortTrailLengthSeconds
        {
            get { return _ShortTrailLengthSeconds; }
            set { SetField(ref _ShortTrailLengthSeconds, value, nameof(ShortTrailLengthSeconds)); }
        }

        private DistanceUnit _InitialDistanceUnit;
        /// <summary>
        /// Gets or sets the initial unit used to display distances. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public DistanceUnit InitialDistanceUnit
        {
            get { return _InitialDistanceUnit; }
            set { SetField(ref _InitialDistanceUnit, value, nameof(InitialDistanceUnit)); }
        }

        private HeightUnit _InitialHeightUnit;
        /// <summary>
        /// Gets or sets the initial unit used to display heights. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public HeightUnit InitialHeightUnit
        {
            get { return _InitialHeightUnit; }
            set { SetField(ref _InitialHeightUnit, value, nameof(InitialHeightUnit)); }
        }

        private SpeedUnit _InitialSpeedUnit;
        /// <summary>
        /// Gets or sets the initial unit used to display speeds. This is overridden by the user's own settings after they have viewed the page the first time.
        /// </summary>
        public SpeedUnit InitialSpeedUnit
        {
            get { return _InitialSpeedUnit; }
            set { SetField(ref _InitialSpeedUnit, value, nameof(InitialSpeedUnit)); }
        }

        private bool _PreferIataAirportCodes;
        /// <summary>
        /// Gets or sets a value indicating that IATA codes should be used to describe airports whenever possible.
        /// </summary>
        public bool PreferIataAirportCodes
        {
            get { return _PreferIataAirportCodes; }
            set { SetField(ref _PreferIataAirportCodes, value, nameof(PreferIataAirportCodes)); }
        }

        private bool _EnableBundling;
        /// <summary>
        /// Gets or sets a value indicating that the server should bundle multiple CSS and JavaScript files into a single download before serving them.
        /// </summary>
        public bool EnableBundling
        {
            get { return _EnableBundling; }
            set { SetField(ref _EnableBundling, value, nameof(EnableBundling)); }
        }

        private bool _EnableMinifying;
        /// <summary>
        /// Gets or sets a value indicating that the server should minify CSS and JavaScript files before serving them.
        /// </summary>
        public bool EnableMinifying
        {
            get { return _EnableMinifying; }
            set { SetField(ref _EnableMinifying, value, nameof(EnableMinifying)); }
        }

        private bool _EnableCompression;
        /// <summary>
        /// Gets or sets a value indicating that the server should compress responses.
        /// </summary>
        public bool EnableCompression
        {
            get { return _EnableCompression; }
            set { SetField(ref _EnableCompression, value, nameof(EnableCompression)); }
        }

        private int _WebSiteReceiverId;
        /// <summary>
        /// Gets or sets the receiver to show to the user when they visit the web site.
        /// </summary>
        public int WebSiteReceiverId
        {
            get { return _WebSiteReceiverId; }
            set { SetField(ref _WebSiteReceiverId, value, nameof(WebSiteReceiverId)); }
        }

        private string _DirectoryEntryKey;
        /// <summary>
        /// Gets or sets the key that directory entry requests must contain before the site will respond with directory entry information.
        /// </summary>
        public string DirectoryEntryKey
        {
            get { return _DirectoryEntryKey; }
            set { SetField(ref _DirectoryEntryKey, value, nameof(DirectoryEntryKey)); }
        }

        private int _ClosestAircraftReceiverId;
        /// <summary>
        /// Gets or sets the receiver to use when the closest aircraft desktop widget asks for details of the closest aircraft.
        /// </summary>
        public int ClosestAircraftReceiverId
        {
            get { return _ClosestAircraftReceiverId; }
            set { SetField(ref _ClosestAircraftReceiverId, value, nameof(ClosestAircraftReceiverId)); }
        }

        private int _FlightSimulatorXReceiverId;
        /// <summary>
        /// Gets or sets the receiver to use with the Flight Simulator X ride-along feature.
        /// </summary>
        public int FlightSimulatorXReceiverId
        {
            get { return _FlightSimulatorXReceiverId; }
            set { SetField(ref _FlightSimulatorXReceiverId, value, nameof(FlightSimulatorXReceiverId)); }
        }

        private ProxyType _ProxyType;
        /// <summary>
        /// Gets or sets the type of proxy that the server is sitting behind.
        /// </summary>
        public ProxyType ProxyType
        {
            get { return _ProxyType; }
            set { SetField(ref _ProxyType, value, nameof(ProxyType)); }
        }

        private bool _EnableCorsSupport;
        /// <summary>
        /// Gets or sets a value indicating that the web site should respond to the CORS OPTIONS request.
        /// </summary>
        public bool EnableCorsSupport
        {
            get { return _EnableCorsSupport; }
            set { SetField(ref _EnableCorsSupport, value, nameof(EnableCorsSupport)); }
        }

        private string _AllowCorsDomains;
        /// <summary>
        /// Gets or sets a semi-colon separated list of domains that can access the server's content via CORS.
        /// </summary>
        public string AllowCorsDomains
        {
            get { return _AllowCorsDomains; }
            set { SetField(ref _AllowCorsDomains, value, nameof(AllowCorsDomains)); }
        }

        private string _GoogleMapsApiKey;
        /// <summary>
        /// Gets or sets the Google Maps API key to use with the site.
        /// </summary>
        public string GoogleMapsApiKey
        {
            get { return _GoogleMapsApiKey; }
            set { SetField(ref _GoogleMapsApiKey, value, nameof(GoogleMapsApiKey)); }
        }

        private bool _UseGoogleMapsAPIKeyWithLocalRequests;
        /// <summary>
        /// True if the Google Maps API key should be used for requests from local addresses. This should
        /// only be switched on if the installation is behind a proxy.
        /// </summary>
        public bool UseGoogleMapsAPIKeyWithLocalRequests
        {
            get { return _UseGoogleMapsAPIKeyWithLocalRequests; }
            set { SetField(ref _UseGoogleMapsAPIKeyWithLocalRequests, value, nameof(UseGoogleMapsAPIKeyWithLocalRequests)); }
        }

        private string _TileServerSettingName;
        /// <summary>
        /// Gets or sets the name of the tile server setting to use with map providers that use tile servers.
        /// </summary>
        public string TileServerSettingName
        {
            get { return _TileServerSettingName; }
            set { SetField(ref _TileServerSettingName, value, nameof(TileServerSettingName)); }
        }

        private bool _UseSvgGraphicsOnDesktop;
        /// <summary>
        /// Gets or sets a value indicating that we want to use SVG graphics on desktop pages.
        /// </summary>
        public bool UseSvgGraphicsOnDesktop
        {
            get { return _UseSvgGraphicsOnDesktop; }
            set { SetField(ref _UseSvgGraphicsOnDesktop, value, nameof(UseSvgGraphicsOnDesktop)); }
        }

        private bool _UseSvgGraphicsOnMobile;
        /// <summary>
        /// Gets or sets a value indicating that we want to use SVG graphics on mobile pages.
        /// </summary>
        public bool UseSvgGraphicsOnMobile
        {
            get { return _UseSvgGraphicsOnMobile; }
            set { SetField(ref _UseSvgGraphicsOnMobile, value, nameof(UseSvgGraphicsOnMobile)); }
        }

        private bool _UseSvgGraphicsOnReports;
        /// <summary>
        /// Gets or sets a value indicating that we want to use SVG graphics on report pages.
        /// </summary>
        public bool UseSvgGraphicsOnReports
        {
            get { return _UseSvgGraphicsOnReports; }
            set { SetField(ref _UseSvgGraphicsOnReports, value, nameof(UseSvgGraphicsOnReports)); }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises <see cref="PropertyChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            var handler = PropertyChanged;
            if(handler != null) {
                handler(this, args);
            }
        }

        /// <summary>
        /// Sets the field's value and raises <see cref="PropertyChanged"/>, but only when the value has changed.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="field"></param>
        /// <param name="value"></param>
        /// <param name="fieldName"></param>
        /// <returns>True if the value was set because it had changed, false if the value did not change and the event was not raised.</returns>
        protected bool SetField<T>(ref T field, T value, string fieldName)
        {
            var result = !EqualityComparer<T>.Default.Equals(field, value);
            if(result) {
                field = value;
                OnPropertyChanged(new PropertyChangedEventArgs(fieldName));
            }

            return result;
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public GoogleMapSettings()
        {
            MapProvider = MapProvider.Leaflet;
            InitialMapLatitude = 51.47;
            InitialMapLongitude = -0.6;
            InitialMapType = "ROADMAP";
            InitialMapZoom = 11;
            InitialRefreshSeconds = MinimumRefreshSeconds = 1;
            ShortTrailLengthSeconds = 30;
            InitialDistanceUnit = DistanceUnit.NauticalMiles;
            InitialHeightUnit = HeightUnit.Feet;
            InitialSpeedUnit = SpeedUnit.Knots;
            EnableBundling = true;
            EnableMinifying = true;
            EnableCompression = true;
            PreferIataAirportCodes = true;
            UseSvgGraphicsOnDesktop = true;
            UseSvgGraphicsOnMobile = true;
            UseSvgGraphicsOnReports = true;
        }
    }
}
