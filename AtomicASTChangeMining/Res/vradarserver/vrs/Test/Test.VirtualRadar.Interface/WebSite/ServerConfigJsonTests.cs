﻿// Copyright © 2013 onwards, Andrew Whewell
// All rights reserved.
//
// Redistribution and use of this software in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//    * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//    * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
//    * Neither the name of the author nor the names of the program's contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE AUTHORS OF THE SOFTWARE BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualRadar.Interface.WebSite;
using Test.Framework;
using VirtualRadar.Interface;
using InterfaceFactory;
using Moq;
using VirtualRadar.Interface.Settings;

namespace Test.VirtualRadar.Interface.WebSite
{
    [TestClass]
    public class ServerConfigJsonTests
    {
        public TestContext TestContext { get; set; }

        private IClassFactory _Snapshot;
        private Mock<IRuntimeEnvironment> _RuntimeEnvironment;
        private Mock<IApplicationInformation> _ApplicationInformation;
        private Mock<ISharedConfiguration> _SharedConfiguration;
        private Configuration _Configuration;
        private Mock<ITileServerSettingsManager> _TileServerSettingsManager;

        [TestInitialize]
        public void TestInitialise()
        {
            _Snapshot = Factory.TakeSnapshot();

            _RuntimeEnvironment = TestUtilities.CreateMockSingleton<IRuntimeEnvironment>();
            _ApplicationInformation = TestUtilities.CreateMockImplementation<IApplicationInformation>();
            _SharedConfiguration = TestUtilities.CreateMockSingleton<ISharedConfiguration>();
            _TileServerSettingsManager = TestUtilities.CreateMockSingleton<ITileServerSettingsManager>();
            _Configuration = new Configuration();
            _SharedConfiguration.Setup(r => r.Get()).Returns(_Configuration);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_Snapshot);
        }

        [TestMethod]
        public void ServerConfigJson_Constructor_Initialises_To_Known_State_And_Properties_Work()
        {
            var json = new ServerConfigJson();

            Assert.AreEqual(0, json.Receivers.Count);
            Assert.AreEqual(0, json.TileServerLayers.Count);

            TestUtilities.TestProperty(json, r => r.GoogleMapsApiKey, null, "API Key");
            TestUtilities.TestProperty(json, r => r.InitialDistanceUnit, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialHeightUnit, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialLatitude, 0.00, 1.234);
            TestUtilities.TestProperty(json, r => r.InitialLongitude, 0.00, 1.234);
            TestUtilities.TestProperty(json, r => r.InitialMapType, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialSettings, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialSpeedUnit, null, "Abc");
            TestUtilities.TestProperty(json, r => r.InitialZoom, 0, 123);
            TestUtilities.TestProperty(json, r => r.InternetClientCanRunReports, false);
            TestUtilities.TestProperty(json, r => r.InternetClientCanShowPinText, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanPlayAudio, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanSeeAircraftPictures, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanSeePolarPlots, false);
            TestUtilities.TestProperty(json, r => r.InternetClientsCanSubmitRoutes, false);
            TestUtilities.TestProperty(json, r => r.InternetClientTimeoutMinutes, 0, 123);
            TestUtilities.TestProperty(json, r => r.IsAudioEnabled, false);
            TestUtilities.TestProperty(json, r => r.IsLocalAddress, false);
            TestUtilities.TestProperty(json, r => r.IsMono, false);
            TestUtilities.TestProperty(json, r => r.MinimumRefreshSeconds, 0, 123);
            TestUtilities.TestProperty(json, r => r.RefreshSeconds, 0, 123);
            TestUtilities.TestProperty(json, r => r.TileServerSettings, null, new TileServerSettings());
            TestUtilities.TestProperty(json, r => r.UseMarkerLabels, false);
            TestUtilities.TestProperty(json, r => r.UseSvgGraphicsOnDesktop, false);
            TestUtilities.TestProperty(json, r => r.UseSvgGraphicsOnMobile, false);
            TestUtilities.TestProperty(json, r => r.UseSvgGraphicsOnReports, false);
            TestUtilities.TestProperty(json, r => r.VrsVersion, null, "Abc");
        }

        [TestMethod]
        public void ServerConfigJson_Clone_Creates_Copy()
        {
            foreach(var property in typeof(ServerConfigJson).GetProperties()) {
                for(var pass = 0;pass < 2;++pass) {
                    var json = new ServerConfigJson();

                    object expected = null;
                    switch(property.Name) {
                        case nameof(ServerConfigJson.GoogleMapsApiKey):                         expected = json.GoogleMapsApiKey = pass == 0 ? "A" : "B"; break;
                        case nameof(ServerConfigJson.InitialDistanceUnit):                      expected = json.InitialDistanceUnit = pass == 0 ? "A": "B"; break;
                        case nameof(ServerConfigJson.InitialHeightUnit):                        expected = json.InitialHeightUnit = pass == 0 ? "A" : "B"; break;
                        case nameof(ServerConfigJson.InitialLatitude):                          expected = json.InitialLatitude = pass == 0 ? 1.234 : 5.678; break;
                        case nameof(ServerConfigJson.InitialLongitude):                         expected = json.InitialLongitude = pass == 0 ? 1.234 : 5.678; break;
                        case nameof(ServerConfigJson.InitialMapType):                           expected = json.InitialMapType = pass == 0 ? "A" : "B"; break;
                        case nameof(ServerConfigJson.InitialSettings):                          expected = json.InitialSettings = pass == 0 ? "A" : "B"; break;
                        case nameof(ServerConfigJson.InitialSpeedUnit):                         expected = json.InitialSpeedUnit = pass == 0 ? "A" : "B"; break;
                        case nameof(ServerConfigJson.InitialZoom):                              expected = json.InitialZoom = pass == 0 ? 1 : 2; break;
                        case nameof(ServerConfigJson.InternetClientCanRunReports):              expected = json.InternetClientCanRunReports = pass == 0; break;
                        case nameof(ServerConfigJson.InternetClientCanShowPinText):             expected = json.InternetClientCanShowPinText = pass == 0; break;
                        case nameof(ServerConfigJson.InternetClientsCanPlayAudio):              expected = json.InternetClientsCanPlayAudio = pass == 0; break;
                        case nameof(ServerConfigJson.InternetClientsCanSeeAircraftPictures):    expected = json.InternetClientsCanSeeAircraftPictures = pass == 0; break;
                        case nameof(ServerConfigJson.InternetClientsCanSeePolarPlots):          expected = json.InternetClientsCanSeePolarPlots = pass == 0; break;
                        case nameof(ServerConfigJson.InternetClientsCanSubmitRoutes):           expected = json.InternetClientsCanSubmitRoutes = pass == 0; break;
                        case nameof(ServerConfigJson.InternetClientTimeoutMinutes):             expected = json.InternetClientTimeoutMinutes = pass == 0 ? 1 : 2; break;
                        case nameof(ServerConfigJson.IsAudioEnabled):                           expected = json.IsAudioEnabled = pass == 0; break;
                        case nameof(ServerConfigJson.IsLocalAddress):                           expected = json.IsLocalAddress = pass == 0; break;
                        case nameof(ServerConfigJson.IsMono):                                   expected = json.IsMono = pass == 0; break;
                        case nameof(ServerConfigJson.MinimumRefreshSeconds):                    expected = json.MinimumRefreshSeconds = pass == 0 ? 1 : 2; break;
                        case nameof(ServerConfigJson.RefreshSeconds):                           expected = json.RefreshSeconds = pass == 0 ? 1 : 2; break;
                        case nameof(ServerConfigJson.UseMarkerLabels):                          expected = json.UseMarkerLabels = pass == 0; break;
                        case nameof(ServerConfigJson.UseSvgGraphicsOnDesktop):                  expected = json.UseSvgGraphicsOnDesktop = pass == 0; break;
                        case nameof(ServerConfigJson.UseSvgGraphicsOnMobile):                   expected = json.UseSvgGraphicsOnMobile = pass == 0; break;
                        case nameof(ServerConfigJson.UseSvgGraphicsOnReports):                  expected = json.UseSvgGraphicsOnReports = pass == 0; break;
                        case nameof(ServerConfigJson.VrsVersion):                               expected = json.VrsVersion = pass == 0 ? "A" : "B"; break;
                        case nameof(ServerConfigJson.Receivers):
                            json.Receivers.Add(new ServerReceiverJson() {
                                UniqueId = pass == 0 ? 1 : 2,
                                Name = pass == 0 ? "First" : "Second",
                            });
                            break;
                        case nameof(ServerConfigJson.TileServerLayers):
                            json.TileServerLayers.Add(new TileServerSettings() {
                                Name = pass == 0 ? "First" : "Second",
                            });
                            break;
                        case nameof(ServerConfigJson.TileServerSettings):
                            json.TileServerSettings = new TileServerSettings() {
                                Name = pass == 0 ? "First" : "Second",
                            };
                            break;
                        default:
                            throw new NotImplementedException(property.Name);
                    }

                    var actual = (ServerConfigJson)json.Clone();

                    switch(property.Name) {
                        case nameof(ServerConfigJson.Receivers):
                            Assert.AreEqual(json.Receivers.Count,       actual.Receivers.Count);
                            Assert.AreNotSame(json.Receivers[0],        actual.Receivers[0]);
                            Assert.AreEqual(json.Receivers[0].UniqueId, actual.Receivers[0].UniqueId);
                            Assert.AreEqual(json.Receivers[0].Name,     actual.Receivers[0].Name);
                            break;
                        case nameof(ServerConfigJson.TileServerLayers):
                            Assert.AreEqual(json.TileServerLayers.Count,    actual.TileServerLayers.Count);
                            Assert.AreNotSame(json.TileServerLayers[0],     actual.TileServerLayers[0]);
                            Assert.AreEqual(json.TileServerLayers[0].Name,  actual.TileServerLayers[0].Name);
                            break;
                        case nameof(ServerConfigJson.TileServerSettings):
                            Assert.AreNotSame(json.TileServerSettings,      actual.TileServerSettings);
                            Assert.AreEqual(json.TileServerSettings.Name,   actual.TileServerSettings.Name);
                            break;
                        default:
                            var actualValue = property.GetValue(actual, null);
                            Assert.AreEqual(expected, actualValue, "for property {0}", property.Name);
                            break;
                    }
                }
            }
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Sets_IsMono_When_Running_Under_Mono()
        {
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(true);

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.IsTrue(model.IsMono);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Clears_IsMono_When_Running_Under_DotNet()
        {
            _RuntimeEnvironment.Setup(r => r.IsMono).Returns(false);

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.IsFalse(model.IsMono);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Sets_VrsVersion_Correctly()
        {
            _ApplicationInformation.Setup(r => r.ShortVersion).Returns("1.2.3");

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.AreEqual("1.2.3", model.VrsVersion);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Sets_VrsVersion_Correctly_For_Beta_Versions()
        {
            _ApplicationInformation.Setup(r => r.ShortVersion).Returns("1.2.3");
            _ApplicationInformation.Setup(r => r.IsBeta).Returns(true);

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.AreEqual("1.2.3 Beta", model.VrsVersion);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Copies_Visible_Receiver_Details()
        {
            _Configuration.Receivers.Add(new Receiver() { UniqueId = 1, Name = "R1", ReceiverUsage = ReceiverUsage.MergeOnly });
            _Configuration.Receivers.Add(new Receiver() { UniqueId = 2, Name = "R2", ReceiverUsage = ReceiverUsage.HideFromWebSite, });
            _Configuration.Receivers.Add(new Receiver() { UniqueId = 3, Name = "R3", ReceiverUsage = ReceiverUsage.Normal, });
            _Configuration.MergedFeeds.Add(new MergedFeed() { UniqueId = 10, Name = "M1", ReceiverUsage = ReceiverUsage.MergeOnly });
            _Configuration.MergedFeeds.Add(new MergedFeed() { UniqueId = 11, Name = "M2", ReceiverUsage = ReceiverUsage.HideFromWebSite });
            _Configuration.MergedFeeds.Add(new MergedFeed() { UniqueId = 12, Name = "M3", ReceiverUsage = ReceiverUsage.Normal });

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.AreEqual(2, model.Receivers.Count);
            Assert.IsTrue(model.Receivers.Any(r => r.UniqueId == 3 && r.Name == "R3"));
            Assert.IsTrue(model.Receivers.Any(r => r.UniqueId == 12 && r.Name == "M3"));
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Sets_GoogleMapsApiKey_For_Internet_Clients_When_Key_Present()
        {
            _Configuration.GoogleMapSettings.GoogleMapsApiKey = "API Key";
            _Configuration.GoogleMapSettings.UseGoogleMapsAPIKeyWithLocalRequests = false;

            var model = ServerConfigJson.ToModel(isLocalAddress: false);

            Assert.AreEqual("API Key", model.GoogleMapsApiKey);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Sets_Null_GoogleMapsApiKey_For_Internet_Clients_When_Key_Missing()
        {
            _Configuration.GoogleMapSettings.GoogleMapsApiKey = "";
            _Configuration.GoogleMapSettings.UseGoogleMapsAPIKeyWithLocalRequests = false;

            var model = ServerConfigJson.ToModel(isLocalAddress: false);

            Assert.IsNull(model.GoogleMapsApiKey);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Sets_Null_GoogleMapsApiKey_For_Local_Clients_When_Key_Present()
        {
            _Configuration.GoogleMapSettings.GoogleMapsApiKey = "API Key";
            _Configuration.GoogleMapSettings.UseGoogleMapsAPIKeyWithLocalRequests = false;

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.IsNull(model.GoogleMapsApiKey);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Sets_GoogleMapsApiKey_To_Local_Clients_When_Key_Present_And_Switch_Set()
        {
            _Configuration.GoogleMapSettings.GoogleMapsApiKey = "API Key";
            _Configuration.GoogleMapSettings.UseGoogleMapsAPIKeyWithLocalRequests = true;

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.AreEqual("API Key", model.GoogleMapsApiKey);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Copies_Current_TileServerSettings_To_Model()
        {
            var tileServerSettings = new TileServerSettings();

            _Configuration.GoogleMapSettings.MapProvider = MapProvider.Leaflet;
            _Configuration.GoogleMapSettings.TileServerSettingName = "My Tile Server";
            _TileServerSettingsManager.Setup(r => r.GetTileServerSettings(MapProvider.Leaflet, "My Tile Server", true)).Returns(tileServerSettings);

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.AreSame(tileServerSettings, model.TileServerSettings);
        }

        [TestMethod]
        public void ServerConfigJson_ToModel_Copies_Layer_TileServerSettings_To_Model()
        {
            var tileServerSettings = new TileServerSettings[] {
                new TileServerSettings() { Name = "Second Layer", IsLayer = true, DisplayOrder = 2, },
                new TileServerSettings() { Name = "Third Layer",  IsLayer = true, DisplayOrder = 3, },
                new TileServerSettings() { Name = "First Layer",  IsLayer = true, DisplayOrder = 1, },
            };

            _Configuration.GoogleMapSettings.MapProvider = MapProvider.Leaflet;
            _Configuration.GoogleMapSettings.TileServerSettingName = "My Layer";
            _TileServerSettingsManager.Setup(r => r.GetAllTileLayerSettings(MapProvider.Leaflet)).Returns(tileServerSettings);

            var model = ServerConfigJson.ToModel(isLocalAddress: true);

            Assert.AreEqual(3, model.TileServerLayers.Count);
            Assert.AreSame(tileServerSettings[2], model.TileServerLayers[0]);
            Assert.AreSame(tileServerSettings[0], model.TileServerLayers[1]);
            Assert.AreSame(tileServerSettings[1], model.TileServerLayers[2]);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "SubstituteConfiguration$")]
        public void ServerConfigJson_ToModel_Fills_Model_Correctly()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var isLocalAddress = true;
            using(var cultureSwitcher = new CultureSwitcher(worksheet.String("Culture"))) {
                var configProperty = worksheet.String("ConfigProperty");
                var isMono = worksheet.Bool("IsMono");
                _RuntimeEnvironment.Setup(r => r.IsMono).Returns(isMono);

                switch(configProperty) {
                    case "VrsVersion":                  _ApplicationInformation.Setup(r => r.ShortVersion).Returns(worksheet.String("Value")); break;
                    case "IsLocalAddress":              isLocalAddress = worksheet.Bool("Value"); break;
                    case "IsMono":                      break;
                    case "InitialMapLatitude":          _Configuration.GoogleMapSettings.InitialMapLatitude = worksheet.Double("Value"); break;
                    case "InitialMapLongitude":         _Configuration.GoogleMapSettings.InitialMapLongitude = worksheet.Double("Value"); break;
                    case "InitialMapType":              _Configuration.GoogleMapSettings.InitialMapType = worksheet.EString("Value"); break;
                    case "InitialMapZoom":              _Configuration.GoogleMapSettings.InitialMapZoom = worksheet.Int("Value"); break;
                    case "InitialRefreshSeconds":       _Configuration.GoogleMapSettings.InitialRefreshSeconds = worksheet.Int("Value"); break;
                    case "InitialSettings":             _Configuration.GoogleMapSettings.InitialSettings = worksheet.EString("Value"); break;
                    case "MinimumRefreshSeconds":       _Configuration.GoogleMapSettings.MinimumRefreshSeconds = worksheet.Int("Value"); break;
                    case "InitialDistanceUnit":         _Configuration.GoogleMapSettings.InitialDistanceUnit = worksheet.ParseEnum<DistanceUnit>("Value"); break;
                    case "InitialHeightUnit":           _Configuration.GoogleMapSettings.InitialHeightUnit = worksheet.ParseEnum<HeightUnit>("Value"); break;
                    case "InitialSpeedUnit":            _Configuration.GoogleMapSettings.InitialSpeedUnit = worksheet.ParseEnum<SpeedUnit>("Value"); break;
                    case "CanRunReports":               _Configuration.InternetClientSettings.CanRunReports = worksheet.Bool("Value"); break;
                    case "CanShowPinText":              _Configuration.InternetClientSettings.CanShowPinText = worksheet.Bool("Value"); break;
                    case "TimeoutMinutes":              _Configuration.InternetClientSettings.TimeoutMinutes = worksheet.Int("Value"); break;
                    case "CanPlayAudio":                _Configuration.InternetClientSettings.CanPlayAudio = worksheet.Bool("Value"); break;
                    case "CanSubmitRoutes":             _Configuration.InternetClientSettings.CanSubmitRoutes = worksheet.Bool("Value"); break;
                    case "CanShowPictures":             _Configuration.InternetClientSettings.CanShowPictures = worksheet.Bool("Value"); break;
                    case "AudioEnabled":                _Configuration.AudioSettings.Enabled = worksheet.Bool("Value"); break;
                    case "CanShowPolarPlots":           _Configuration.InternetClientSettings.CanShowPolarPlots = worksheet.Bool("Value"); break;
                    case "UseMarkerLabels":             _Configuration.MonoSettings.UseMarkerLabels = worksheet.Bool("Value"); break;
                    case "UseSvgGraphicsOnDesktop":     _Configuration.GoogleMapSettings.UseSvgGraphicsOnDesktop = worksheet.Bool("Value"); break;
                    case "UseSvgGraphicsOnMobile":      _Configuration.GoogleMapSettings.UseSvgGraphicsOnMobile = worksheet.Bool("Value"); break;
                    case "UseSvgGraphicsOnReports":     _Configuration.GoogleMapSettings.UseSvgGraphicsOnReports = worksheet.Bool("Value"); break;
                    default:                            throw new NotImplementedException();
                }
            }

            var model = ServerConfigJson.ToModel(isLocalAddress);

            var propertyName = worksheet.String("ConfigProperty");
            switch(propertyName) {
                case "VrsVersion":                      Assert.AreEqual(worksheet.EString("JsonValue"), model.VrsVersion); break;
                case "IsLocalAddress":                  Assert.AreEqual(worksheet.Bool("JsonValue"), model.IsLocalAddress); break;
                case "IsMono":                          Assert.AreEqual(worksheet.Bool("JsonValue"), model.IsMono); break;
                case "InitialMapLatitude":              Assert.AreEqual(worksheet.Double("JsonValue"), model.InitialLatitude); break;
                case "InitialMapLongitude":             Assert.AreEqual(worksheet.Double("JsonValue"), model.InitialLongitude); break;
                case "InitialMapType":                  Assert.AreEqual(worksheet.EString("JsonValue"), model.InitialMapType); break;
                case "InitialMapZoom":                  Assert.AreEqual(worksheet.Int("JsonValue"), model.InitialZoom); break;
                case "InitialRefreshSeconds":           Assert.AreEqual(worksheet.Int("JsonValue"), model.RefreshSeconds); break;
                case "InitialSettings":                 Assert.AreEqual(worksheet.EString("JsonValue"), model.InitialSettings); break;
                case "MinimumRefreshSeconds":           Assert.AreEqual(worksheet.Int("JsonValue"), model.MinimumRefreshSeconds); break;
                case "InitialDistanceUnit":             Assert.AreEqual(worksheet.String("JsonValue"), model.InitialDistanceUnit); break;
                case "InitialHeightUnit":               Assert.AreEqual(worksheet.String("JsonValue"), model.InitialHeightUnit); break;
                case "InitialSpeedUnit":                Assert.AreEqual(worksheet.String("JsonValue"), model.InitialSpeedUnit); break;
                case "CanRunReports":                   Assert.AreEqual(worksheet.Bool("JsonValue"), model.InternetClientCanRunReports); break;
                case "CanShowPinText":                  Assert.AreEqual(worksheet.Bool("JsonValue"), model.InternetClientCanShowPinText); break;
                case "TimeoutMinutes":                  Assert.AreEqual(worksheet.Int("JsonValue"), model.InternetClientTimeoutMinutes); break;
                case "CanPlayAudio":                    Assert.AreEqual(worksheet.Bool("JsonValue"), model.InternetClientsCanPlayAudio); break;
                case "CanSubmitRoutes":                 Assert.AreEqual(worksheet.Bool("JsonValue"), model.InternetClientsCanSubmitRoutes); break;
                case "CanShowPictures":                 Assert.AreEqual(worksheet.Bool("JsonValue"), model.InternetClientsCanSeeAircraftPictures); break;
                case "AudioEnabled":                    Assert.AreEqual(worksheet.Bool("JsonValue"), model.IsAudioEnabled); break;
                case "CanShowPolarPlots":               Assert.AreEqual(worksheet.Bool("JsonValue"), model.InternetClientsCanSeePolarPlots); break;
                case "UseMarkerLabels":                 Assert.AreEqual(worksheet.Bool("JsonValue"), model.UseMarkerLabels); break;
                case "UseSvgGraphicsOnDesktop":         Assert.AreEqual(worksheet.Bool("JsonValue"), model.UseSvgGraphicsOnDesktop); break;
                case "UseSvgGraphicsOnMobile":          Assert.AreEqual(worksheet.Bool("JsonValue"), model.UseSvgGraphicsOnMobile); break;
                case "UseSvgGraphicsOnReports":         Assert.AreEqual(worksheet.Bool("JsonValue"), model.UseSvgGraphicsOnReports); break;
                default:                                throw new NotImplementedException();
            }
        }
    }
}
