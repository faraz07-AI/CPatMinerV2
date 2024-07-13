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
using System.Net;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.BaseStation;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.StandingData;
using VirtualRadar.Interface.WebSite;

namespace Test.VirtualRadar.WebSite.ApiControllers
{
    [TestClass]
    public class FeedsControllerTests : ControllerTests
    {
        private Mock<IFeedManager> _FeedManager;
        private List<Mock<IFeed>> _VisibleFeeds;
        private Mock<IBaseStationAircraftList> _AircraftList;
        private Mock<IPolarPlotter> _PolarPlotter;
        private List<PolarPlotSlice> _Slices;
        private Mock<IAircraftListJsonBuilder> _AircraftListJsonBuilder;
        private AircraftListJson _AircraftListJson;
        private AircraftListJsonBuilderArgs _ActualAircraftListJsonBuilderArgs;
        private bool? _ActualAircraftListJsonBuilderIgnoreInvisibleFeeds;
        private bool? _ActualAircraftListJsonBuilderFallbackToDefault;
        private Mock<IFlightSimulatorAircraftList> _FlightSimulatorAircraftList;
        private Mock<ITileServerSettingsManager> _TileServerSettingsManager;

        protected override void ExtraInitialise()
        {
            _Configuration.InternetClientSettings.CanShowPolarPlots = true;

            _Slices = new List<PolarPlotSlice>();

            _PolarPlotter = TestUtilities.CreateMockInstance<IPolarPlotter>();
            _PolarPlotter.Setup(r => r.TakeSnapshot()).Returns(_Slices);

            _AircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
            _AircraftList.SetupGet(r => r.PolarPlotter).Returns(_PolarPlotter.Object);

            _FlightSimulatorAircraftList = TestUtilities.CreateMockImplementation<IFlightSimulatorAircraftList>();
            _TileServerSettingsManager = TestUtilities.CreateMockSingleton<ITileServerSettingsManager>();

            _AircraftListJson = new AircraftListJson();
            _ActualAircraftListJsonBuilderArgs = null;

            _AircraftListJsonBuilder = TestUtilities.CreateMockImplementation<IAircraftListJsonBuilder>();
            _AircraftListJsonBuilder
                .Setup(r => r.Build(It.IsAny<AircraftListJsonBuilderArgs>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .Returns((AircraftListJsonBuilderArgs args, bool ignoreInvisibleFeeds, bool fallbackToDefault) => {
                    _ActualAircraftListJsonBuilderArgs = args;
                    _ActualAircraftListJsonBuilderIgnoreInvisibleFeeds = ignoreInvisibleFeeds;
                    _ActualAircraftListJsonBuilderFallbackToDefault = fallbackToDefault;
                    return _AircraftListJson;
                });

            _FeedManager = TestUtilities.CreateMockSingleton<IFeedManager>();
            _VisibleFeeds = new List<Mock<IFeed>>();
            _FeedManager.Setup(r => r.GetByUniqueId(It.IsAny<int>(), It.IsAny<bool>())).Returns((IFeed)null);
            _FeedManager.SetupGet(r => r.VisibleFeeds).Returns(() => {
                return _VisibleFeeds.Select(r => r.Object).ToArray();
            });
        }

        private Mock<IFeed> CreateFeed(int uniqueId = 1, string name = "My Feed", bool hasPlotter = true, bool isVisible = true, bool hasDistinctAircraftList = false)
        {
            var result = TestUtilities.CreateMockInstance<IFeed>();
            result.SetupGet(r => r.UniqueId).Returns(uniqueId);
            result.SetupGet(r => r.Name).Returns(name);
            result.SetupGet(r => r.IsVisible).Returns(isVisible);

            if(hasDistinctAircraftList) {
                var aircraftList = TestUtilities.CreateMockInstance<IBaseStationAircraftList>();
                aircraftList.SetupGet(r => r.PolarPlotter).Returns(hasPlotter ? TestUtilities.CreateMockInstance<IPolarPlotter>().Object : null);
                result.SetupGet(r => r.AircraftList).Returns(aircraftList.Object);
            } else {
                if(!hasPlotter) {
                    _AircraftList.SetupGet(r => r.PolarPlotter).Returns((IPolarPlotter)null);
                }
                result.SetupGet(r => r.AircraftList).Returns(_AircraftList.Object);
            }

            return result;
        }

        private PolarPlotSlice CreatePolarPlotSlice(int lowAltitude, int highAltitude, params PolarPlot[] points)
        {
            var result = new PolarPlotSlice() {
                AltitudeLower =     lowAltitude,
                AltitudeHigher =    highAltitude,
            };
            foreach(var point in points) {
                result.PolarPlots.Add(point.Angle, point);
            }

            return result;
        }

        private Mock<IFeed> ConfigureGetFeedById(Mock<IFeed> feed)
        {
            _FeedManager.Setup(r => r.GetByUniqueId(feed.Object.UniqueId, true)).Returns(feed.Object);

            return feed;
        }

        private AircraftListJsonBuilderArgs ExpectedAircraftListJsonBuilderArgs(
            int feedId = -1,
            double? latitude = null,
            double? longitude = null,
            long previousDataVersion = -1,
            bool resendTrails = false,
            int selectedAircraftID = -1,
            long serverTimeTicks = -1,
            bool isFlightSimulator = false,
            AircraftListJsonBuilderFilter filter = null,
            TrailType trailType = TrailType.None,
            string sortColumn1 = null,
            bool sortAscending1 = false,
            string sortColumn2 = null,
            bool sortAscending2 = false,
            IEnumerable<int> previousAircraft = null
        )
        {
            var result = new AircraftListJsonBuilderArgs() {
                AircraftList =          isFlightSimulator ? _FlightSimulatorAircraftList.Object : null,
                BrowserLatitude =       latitude,
                BrowserLongitude =      longitude,
                Filter =                filter,
                IsFlightSimulatorList = isFlightSimulator,
                PreviousDataVersion =   previousDataVersion,
                ResendTrails =          resendTrails,
                SelectedAircraftId =    selectedAircraftID,
                ServerTimeTicks =       serverTimeTicks,
                SourceFeedId =          feedId,
                TrailType =             trailType,
            };

            if(sortColumn1 == null) {
                result.SortBy.Add(new KeyValuePair<string, bool>(AircraftComparerColumn.FirstSeen, false));
            } else {
                result.SortBy.Add(new KeyValuePair<string, bool>(sortColumn1, sortAscending1));
                if(sortColumn2 != null) {
                    result.SortBy.Add(new KeyValuePair<string, bool>(sortColumn2, sortAscending2));
                }
            }

            if(previousAircraft != null) {
                result.PreviousAircraft.AddRange(previousAircraft);
            }

            return result;
        }

        private void AssertBuilderArgsAreEqual(AircraftListJsonBuilderArgs expected, AircraftListJsonBuilderArgs actual)
        {
            if(expected == null && actual == null) {
                return;
            }

            if(expected == null && actual != null) {
                Assert.Fail("Expected null args but actual was not null (builder was called when it should not have been)");
            }
            if(expected != null && actual == null) {
                Assert.Fail("Expected args but actual was null (builder was not called when it should have been)");
            }

            Assert.AreSame (expected.AircraftList,          actual.AircraftList);
            Assert.AreEqual(expected.BrowserLatitude,       actual.BrowserLatitude);
            Assert.AreEqual(expected.BrowserLongitude,      actual.BrowserLongitude);
            Assert.AreEqual(expected.IsFlightSimulatorList, actual.IsFlightSimulatorList);
            Assert.AreEqual(expected.PreviousDataVersion,   actual.PreviousDataVersion);
            Assert.AreEqual(expected.ResendTrails,          actual.ResendTrails);
            Assert.AreEqual(expected.SelectedAircraftId,    actual.SelectedAircraftId);
            Assert.AreEqual(expected.ServerTimeTicks,       actual.ServerTimeTicks);
            Assert.AreEqual(expected.SourceFeedId,          actual.SourceFeedId);
            Assert.AreEqual(expected.TrailType,             actual.TrailType);

            AssertFiltersAreEqual(expected.Filter, actual.Filter);
            AssertSortColumnsAreEqual(expected.SortBy, actual.SortBy);
            AssertPreviousAircraftAreEqual(expected.PreviousAircraft, actual.PreviousAircraft);

            Assert.IsTrue(_ActualAircraftListJsonBuilderIgnoreInvisibleFeeds.Value);
            Assert.IsTrue(_ActualAircraftListJsonBuilderFallbackToDefault.Value);
        }

        private void AssertFiltersAreEqual(AircraftListJsonBuilderFilter expected, AircraftListJsonBuilderFilter actual)
        {
            if(expected == null && actual == null) {
                return;
            }

            if(expected == null && actual != null) {
                Assert.Fail("Expected null filter but actual was not null (filters were configured when they should not have been)");
            }
            if(expected != null && actual == null) {
                Assert.Fail("Expected filter but actual was null (filters were not configured when they should have been)");
            }

            Assert.AreEqual(expected.Airport,                   actual.Airport);
            Assert.AreEqual(expected.Altitude,                  actual.Altitude);
            Assert.AreEqual(expected.Callsign,                  actual.Callsign);
            Assert.AreEqual(expected.Distance,                  actual.Distance);
            Assert.AreEqual(expected.EngineType,                actual.EngineType);
            Assert.AreEqual(expected.Icao24,                    actual.Icao24);
            Assert.AreEqual(expected.Icao24Country,             actual.Icao24Country);
            Assert.AreEqual(expected.IsInteresting,             actual.IsInteresting);
            Assert.AreEqual(expected.IsMilitary,                actual.IsMilitary);
            Assert.AreEqual(expected.MustTransmitPosition,      actual.MustTransmitPosition);
            Assert.AreEqual(expected.Operator,                  actual.Operator);
            Assert.AreEqual(expected.OperatorIcao,              actual.OperatorIcao);
            Assert.AreEqual(expected.PositionWithin,            actual.PositionWithin);
            Assert.AreEqual(expected.Registration,              actual.Registration);
            Assert.AreEqual(expected.Species,                   actual.Species);
            Assert.AreEqual(expected.Squawk,                    actual.Squawk);
            Assert.AreEqual(expected.Type,                      actual.Type);
            Assert.AreEqual(expected.UserTag,                   actual.UserTag);
            Assert.AreEqual(expected.WakeTurbulenceCategory,    actual.WakeTurbulenceCategory);
        }

        private void AssertSortColumnsAreEqual(List<KeyValuePair<string, bool>> expected, List<KeyValuePair<string, bool>> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);

            for(var i = 0;i < expected.Count;++i) {
                var expectedSortColumn = expected[i].Key;
                var actualSortColumn = actual[i].Key;

                var expectedAscending = expected[i].Value;
                var actualAscending = actual[i].Value;

                Assert.AreEqual(expectedSortColumn, actualSortColumn, $"Expected sort column {expectedSortColumn}, actual was {actualSortColumn} for index {i}");
                Assert.AreEqual(expectedAscending, actualAscending, $"Expected ascending {expectedAscending}, actual was {actualAscending} for index {i}");
            }
        }

        private void AssertPreviousAircraftAreEqual(List<int> expected, List<int> actual)
        {
            Assert.AreEqual(expected.Count, actual.Count);
            Assert.IsFalse(expected.Except(actual).Any());
        }

        private static List<string> AircraftComparerColumns()
        {
            var result = new List<string>();

            foreach(var field in typeof(AircraftComparerColumn).GetFields().Where(r => r.FieldType == typeof(string) && r.IsLiteral)) {
                result.Add((string)field.GetValue(null));
            }

            return result;
        }

        #region GetFeeds
        [TestMethod]
        public void FeedsController_GetFeeds_Returns_All_Visible_Feeds()
        {
            _VisibleFeeds.Add(CreateFeed(uniqueId: 1, name: "First", hasPlotter: true, hasDistinctAircraftList: true));
            _VisibleFeeds.Add(CreateFeed(uniqueId: 2, name: "Second", hasPlotter: false, hasDistinctAircraftList: true));

            var feeds = Get("/api/3.00/feeds").Json<FeedJson[]>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(2, feeds.Length);

            var feed1 = feeds.Single(r => r.UniqueId == 1);
            Assert.AreEqual("First", feed1.Name);
            Assert.AreEqual(true, feed1.HasPolarPlot);

            var feed2 = feeds.Single(r => r.UniqueId == 2);
            Assert.AreEqual("Second", feed2.Name);
            Assert.AreEqual(false, feed2.HasPolarPlot);
        }

        [TestMethod]
        public void FeedsController_GetFeeds_Returns_Empty_Array_If_All_Feeds_Are_Invisible()
        {
            var feeds = Get("/api/3.00/feeds").Json<FeedJson[]>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(0, feeds.Length);
        }
        #endregion

        #region GetFeed
        [TestMethod]
        public void FeedsController_GetFeed_Returns_Feed_If_Known()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, name: "Feed", hasPlotter: true, isVisible: true));

            var feed = Get("/api/3.00/feeds/1").Json<FeedJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(1, feed.UniqueId);
            Assert.AreEqual("Feed", feed.Name);
            Assert.AreEqual(true, feed.HasPolarPlot);
        }

        [TestMethod]
        public void FeedsController_GetFeed_Returns_Null_If_ID_Is_Unknown()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1));

            var feed = Get("/api/3.00/feeds/2").Json<FeedJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.IsNull(feed);
        }

        [TestMethod]
        public void FeedsController_GetFeed_Returns_Null_If_Feed_Is_Invisible()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, isVisible: false));

            var feed = Get("/api/3.00/feeds/1").Json<FeedJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.IsNull(feed);
        }
        #endregion

        #region GetPolarPlot
        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_Object()
        {
            ConfigureGetFeedById(CreateFeed());

            var json = Get("/api/3.00/feeds/polar-plot/1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual(1, json.FeedId);
            Assert.IsNotNull(json.Slices);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_Object_For_Version_2_Route()
        {
            ConfigureGetFeedById(CreateFeed());

            var json = Get("/PolarPlot.json?feedId=1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.IsNotNull(json);
            Assert.AreEqual(1, json.FeedId);
            Assert.IsNotNull(json.Slices);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_Slices()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 2));
            _Slices.Add(CreatePolarPlotSlice(100, 199, new PolarPlot() { Angle = 1, Altitude = 150, Distance = 7, Latitude = 10.1, Longitude = 11.2 }));

            var json = Get("/api/3.00/feeds/polar-plot/2").Json<PolarPlotsJson>();

            Assert.AreEqual(2, json.FeedId);
            Assert.AreEqual(1, json.Slices.Count);

            var slice = json.Slices[0];
            Assert.AreEqual(100, slice.StartAltitude);
            Assert.AreEqual(199, slice.FinishAltitude);
            Assert.AreEqual(1, slice.Plots.Count);
            Assert.AreEqual(10.1F, slice.Plots[0].Latitude);
            Assert.AreEqual(11.2F, slice.Plots[0].Longitude);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_No_Slices_If_Invalid_ID_Supplied()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 2));

            var json = Get("/api/3.00/feeds/polar-plot/1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_No_Slices_If_Feed_Is_Invisible()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, isVisible: false));

            var json = Get("/api/3.00/feeds/polar-plot/1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_No_Slices_If_Feed_Has_No_Aircraft_List()
        {
            var feed = CreateFeed(uniqueId: 1);
            feed.SetupGet(r => r.AircraftList).Returns((IBaseStationAircraftList)null);
            ConfigureGetFeedById(feed);

            var json = Get("/api/3.00/feeds/polar-plot/1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_No_Slices_If_Feed_Has_No_Plotter()
        {
            ConfigureGetFeedById(CreateFeed(uniqueId: 1, hasPlotter: false));

            var json = Get("/api/3.00/feeds/polar-plot/1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_No_Slices_If_Prohibited_By_Configuration()
        {
            _Configuration.InternetClientSettings.CanShowPolarPlots = false;
            _RemoteIpAddress = "1.2.3.4";

            ConfigureGetFeedById(CreateFeed(uniqueId: 1));
            _Slices.Add(CreatePolarPlotSlice(100, 199, new PolarPlot() { Angle = 1, Altitude = 150, Distance = 7, Latitude = 10.1, Longitude = 11.2 }));

            var json = Get("/api/3.00/feeds/polar-plot/1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(0, json.Slices.Count);
        }

        [TestMethod]
        public void FeedsController_GetPolarPlot_Returns_Slices_If_Prohibited_By_Configuration_But_Accessed_Locally()
        {
            _Configuration.InternetClientSettings.CanShowPolarPlots = false;
            _RemoteIpAddress = "192.168.0.1";

            ConfigureGetFeedById(CreateFeed(uniqueId: 1));
            _Slices.Add(CreatePolarPlotSlice(100, 199, new PolarPlot() { Angle = 1, Altitude = 150, Distance = 7, Latitude = 10.1, Longitude = 11.2 }));

            var json = Get("/api/3.00/feeds/polar-plot/1").Json<PolarPlotsJson>();

            Assert.AreEqual(HttpStatusCode.OK, _Context.ResponseHttpStatusCode);
            Assert.AreEqual(1, json.FeedId);
            Assert.AreEqual(1, json.Slices.Count);
        }
        #endregion

        #region AircraftList
        [TestMethod]
        public void FeedsController_AircraftList_Returns_Default_Aircraft_List_V2_POST()
        {
            Post("AircraftList.json");

            var expected = ExpectedAircraftListJsonBuilderArgs();
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Returns_Default_Aircraft_List_V2_GET()
        {
            Get("AircraftList.json");

            var expected = ExpectedAircraftListJsonBuilderArgs();
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Returns_Default_Aircraft_List_V3()
        {
            Post("/api/3.00/feeds/aircraft-list");

            var expected = ExpectedAircraftListJsonBuilderArgs();
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Feed_V2_POST()
        {
            Post("AircraftList.json?feed=7");

            var expected = ExpectedAircraftListJsonBuilderArgs(feedId: 7);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Feed_V2_GET()
        {
            Get("AircraftList.json?feed=7");

            var expected = ExpectedAircraftListJsonBuilderArgs(feedId: 7);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Feed_V3()
        {
            Post("/api/3.00/feeds/aircraft-list/7");

            var expected = ExpectedAircraftListJsonBuilderArgs(feedId: 7);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Browser_Location_V2_POST()
        {
            Post("AircraftList.json?lat=1.2&lng=3.4");

            var expected = ExpectedAircraftListJsonBuilderArgs(latitude: 1.2, longitude: 3.4);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Browser_Location_V2_GET()
        {
            Get("AircraftList.json?lat=1.2&lng=3.4");

            var expected = ExpectedAircraftListJsonBuilderArgs(latitude: 1.2, longitude: 3.4);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Browser_Location_V3()
        {
            PostForm("/api/3.00/feeds/aircraft-list", new [,] {
                { "Latitude",  "1.2" },
                { "Longitude", "3.4" },
            });

            var expected = ExpectedAircraftListJsonBuilderArgs(latitude: 1.2, longitude: 3.4);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Identifies_Internet_Clients_V2_POST()
        {
            _RemoteIpAddress = "127.0.0.1";
            Post("AircraftList.json");
            Assert.AreEqual(false, _ActualAircraftListJsonBuilderArgs.IsInternetClient);

            TestCleanup();
            TestInitialise();

            _RemoteIpAddress = "1.2.3.4";
            Post("AircraftList.json");
            Assert.AreEqual(true, _ActualAircraftListJsonBuilderArgs.IsInternetClient);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Identifies_Internet_Clients_V2_GET()
        {
            _RemoteIpAddress = "127.0.0.1";
            Get("AircraftList.json");
            Assert.AreEqual(false, _ActualAircraftListJsonBuilderArgs.IsInternetClient);

            TestCleanup();
            TestInitialise();

            _RemoteIpAddress = "1.2.3.4";
            Get("AircraftList.json");
            Assert.AreEqual(true, _ActualAircraftListJsonBuilderArgs.IsInternetClient);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Identifies_Internet_Clients_V3()
        {
            _RemoteIpAddress = "127.0.0.1";
            Post("/api/3.00/feeds/aircraft-list");
            Assert.AreEqual(false, _ActualAircraftListJsonBuilderArgs.IsInternetClient);

            TestCleanup();
            TestInitialise();

            _RemoteIpAddress = "1.2.3.4";
            Post("/api/3.00/feeds/aircraft-list");
            Assert.AreEqual(true, _ActualAircraftListJsonBuilderArgs.IsInternetClient);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Last_DataVersion_V2_POST()
        {
            Post("AircraftList.json?ldv=12");

            var expected = ExpectedAircraftListJsonBuilderArgs(previousDataVersion: 12);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Last_DataVersion_V2_GET()
        {
            Get("AircraftList.json?ldv=12");

            var expected = ExpectedAircraftListJsonBuilderArgs(previousDataVersion: 12);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Last_DataVersion_V3()
        {
            PostForm("/api/3.00/feeds/aircraft-list", new [,] {
                { "LastDataVersion", "12" },
            });

            var expected = ExpectedAircraftListJsonBuilderArgs(previousDataVersion: 12);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_ServerTimeTicks_V2_POST()
        {
            Post("AircraftList.json?stm=12");

            var expected = ExpectedAircraftListJsonBuilderArgs(serverTimeTicks: 12);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_ServerTimeTicks_V2_GET()
        {
            Get("AircraftList.json?stm=12");

            var expected = ExpectedAircraftListJsonBuilderArgs(serverTimeTicks: 12);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_ServerTimeTicks_V3()
        {
            PostForm("/api/3.00/feeds/aircraft-list", new [,] {
                { "ServerTicks", "12" },
            });

            var expected = ExpectedAircraftListJsonBuilderArgs(serverTimeTicks: 12);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Force_Resend_Of_Trails_V2_POST()
        {
            Post("AircraftList.json?refreshTrails=1");

            var expected = ExpectedAircraftListJsonBuilderArgs(resendTrails: true);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Force_Resend_Of_Trails_V2_GET()
        {
            Get("AircraftList.json?refreshTrails=1");

            var expected = ExpectedAircraftListJsonBuilderArgs(resendTrails: true);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Force_Resend_Of_Trails_V3()
        {
            PostForm("/api/3.00/feeds/aircraft-list", new [,] {
                { "ResendTrails", "true" },
            });

            var expected = ExpectedAircraftListJsonBuilderArgs(resendTrails: true);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_SelectedAircraft_V2_POST()
        {
            Post("AircraftList.json?selAc=8");

            var expected = ExpectedAircraftListJsonBuilderArgs(selectedAircraftID: 8);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_SelectedAircraft_V2_GET()
        {
            Get("AircraftList.json?selAc=8");

            var expected = ExpectedAircraftListJsonBuilderArgs(selectedAircraftID: 8);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_SelectedAircraft_V3()
        {
            PostForm("/api/3.00/feeds/aircraft-list", new [,] {
                { "SelectedAircraft", "8" },
            });

            var expected = ExpectedAircraftListJsonBuilderArgs(selectedAircraftID: 8);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_FSX_AircraftList_V2_POST()
        {
            Post("flightsimlist.json");

            var expected = ExpectedAircraftListJsonBuilderArgs(isFlightSimulator: true);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_FSX_AircraftList_V2_GET()
        {
            Get("flightsimlist.json");

            var expected = ExpectedAircraftListJsonBuilderArgs(isFlightSimulator: true);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_FSX_AircraftList_V3()
        {
            PostForm("/api/3.00/feeds/aircraft-list", new [,] {
                { "FlightSimulator", "true" },
            });

            var expected = ExpectedAircraftListJsonBuilderArgs(isFlightSimulator: true);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Ignores_Feed_For_FSX_V2_POST()
        {
            Post("FLIGHTSimList.json?feed=7");

            var expected = ExpectedAircraftListJsonBuilderArgs(isFlightSimulator: true, feedId: -1);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Ignores_Feed_For_FSX_V2_GET()
        {
            Get("FLIGHTSimList.json?feed=7");

            var expected = ExpectedAircraftListJsonBuilderArgs(isFlightSimulator: true, feedId: -1);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Ignores_Feed_For_FSX_V3()
        {
            PostForm("/api/3.00/feeds/aircraft-list/7", new [,] {
                { "FlightSimulator", "true" },
            });

            var expected = ExpectedAircraftListJsonBuilderArgs(isFlightSimulator: true, feedId: -1);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListFilter$")]
        public void FeedsController_AircraftList_Accepts_Filter_Requests_V2_POST()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var key = HttpUtility.UrlEncode(worksheet.String("V2Key"));
            var value = HttpUtility.UrlEncode(worksheet.EString("V2Value"));

            if(!String.IsNullOrEmpty(key)) {
                var queryString = $"{key}={value}";
                FeedsController_AircraftList_Accepts_Filter_Requests_Worker(worksheet, $"AircraftList.json?{queryString}", useGet: false);
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListFilter$")]
        public void FeedsController_AircraftList_Accepts_Filter_Requests_V2_GET()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var key = HttpUtility.UrlEncode(worksheet.String("V2Key"));
            var value = HttpUtility.UrlEncode(worksheet.EString("V2Value"));

            if(!String.IsNullOrEmpty(key)) {
                var queryString = $"{key}={value}";
                FeedsController_AircraftList_Accepts_Filter_Requests_Worker(worksheet, $"AircraftList.json?{queryString}", useGet: true);
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListFilter$")]
        public void FeedsController_AircraftList_Accepts_Filter_Requests_V3()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var filterJson = worksheet.String("V3Json");

            if(!String.IsNullOrEmpty(filterJson)) {
                JsonBody(new {
                    Filters = new GetAircraftListFilter[] {
                        JsonConvert.DeserializeObject<GetAircraftListFilter>(filterJson)
                    }
                });
                FeedsController_AircraftList_Accepts_Filter_Requests_Worker(worksheet, "/api/3.00/feeds/aircraft-list", useGet: false);
            }
        }

        private void FeedsController_AircraftList_Accepts_Filter_Requests_Worker(ExcelWorksheetData worksheet, string url, bool useGet)
        {
            if(useGet) {
                Get(url);
            } else {
                Post(url);
            }

            AircraftListJsonBuilderFilter jsonFilter = null;

            var filterPropertyName = worksheet.String("FilterProperty");
            if(filterPropertyName != null) {
                jsonFilter = new AircraftListJsonBuilderFilter();

                var propertyInfo = typeof(AircraftListJsonBuilderFilter).GetProperty(filterPropertyName);
                var filter = (Filter)Activator.CreateInstance(propertyInfo.PropertyType);
                filter.Condition = worksheet.ParseEnum<FilterCondition>("Condition");
                filter.ReverseCondition = worksheet.Bool("ReverseCondition");

                if(filter is FilterBool filterBool) {
                    filterBool.Value = worksheet.Bool("FilterValue");
                } else if(filter is FilterEnum<EngineType> filterEngineType) {
                    filterEngineType.Value = worksheet.ParseEnum<EngineType>("FilterValue");
                } else if(filter is FilterEnum<Species> filterSpecies) {
                    filterSpecies.Value = worksheet.ParseEnum<Species>("FilterValue");
                } else if(filter is FilterEnum<WakeTurbulenceCategory> filterWakeTurblenceCategory) {
                    filterWakeTurblenceCategory.Value = worksheet.ParseEnum<WakeTurbulenceCategory>("FilterValue");
                } else if(filter is FilterRange<double> filterDoubleRange) {
                    filterDoubleRange.LowerValue = worksheet.NDouble("LowerValue");
                    filterDoubleRange.UpperValue = worksheet.NDouble("UpperValue");
                } else if(filter is FilterRange<int> filterIntRange) {
                    filterIntRange.LowerValue = worksheet.NInt("LowerValue");
                    filterIntRange.UpperValue = worksheet.NInt("UpperValue");
                } else if(filter is FilterString filterString) {
                    filterString.Value = worksheet.EString("FilterValue");
                } else {
                    Assert.Fail($"Need code to fill {filter.GetType().Name} filters");
                }

                propertyInfo.SetValue(jsonFilter, filter);
            }

            var expected = ExpectedAircraftListJsonBuilderArgs(filter: jsonFilter);
            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListFilterName$")]
        public void FeedsController_AircraftList_Filters_Ignore_Invalid_Conditions_V2_POST()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            Test_FeedsController_AircraftList_Filters_Ignore_Invalid_Conditions_V2(worksheet, useGet: false);
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListFilterName$")]
        public void FeedsController_AircraftList_Filters_Ignore_Invalid_Conditions_V2_GET()
        {
            var worksheet = new ExcelWorksheetData(TestContext);
            Test_FeedsController_AircraftList_Filters_Ignore_Invalid_Conditions_V2(worksheet, useGet: true);
        }

        private void Test_FeedsController_AircraftList_Filters_Ignore_Invalid_Conditions_V2(ExcelWorksheetData worksheet, bool useGet)
        {
            var propertyName = HttpUtility.UrlEncode(worksheet.String("FilterProperty"));
            var queryStringKey = HttpUtility.UrlEncode(worksheet.EString("QueryStringKey"));

            var property = typeof(AircraftListJsonBuilderFilter).GetProperty(propertyName);
            var filterType = property.PropertyType;

            var isStringFilter = typeof(FilterString).IsAssignableFrom(filterType);
            var isIntRangeFilter = typeof(FilterRange<int>).IsAssignableFrom(filterType);
            var isDoubleRangeFilter = typeof(FilterRange<double>).IsAssignableFrom(filterType);
            var isEnumFilter = filterType.IsGenericType && filterType.GetGenericTypeDefinition() == typeof(FilterEnum<>);
            var isBoolFilter = typeof(FilterBool).IsAssignableFrom(filterType);

            var queryStringValue = "1";

            var maybeValidConditions = new string[] { "L", "U", "C", "E", "Q", "S", "LN", "UN", "CN", "EN", "QN", "SN", };
            var alwaysInvalidConditions = new string[] { "NL", "NU", "NC", "NE", "NQ", "NS", "LZ", "UZ", "CZ", "EZ", "QZ", "SZ", "Z", "ZZ", };

            foreach(var condition in maybeValidConditions.Concat(alwaysInvalidConditions)) {
                var queryString = $"{queryStringKey}{condition}={queryStringValue}";
                TestCleanup();
                TestInitialise();

                var conditionIsValid = false;
                switch(condition) {
                    case "L":
                    case "LN":
                    case "U":
                    case "UN":  conditionIsValid = isIntRangeFilter || isDoubleRangeFilter; break;
                    case "C":
                    case "CN":
                    case "E":
                    case "EN":
                    case "S":
                    case "SN":  conditionIsValid = isStringFilter; break;
                    case "Q":
                    case "QN":  conditionIsValid = isStringFilter | isBoolFilter | isEnumFilter; break;
                }

                if(useGet) {
                    Get($"AircraftList.json?{queryString}");
                } else {
                    Post($"AircraftList.json?{queryString}");
                }
                var filterUsed = _ActualAircraftListJsonBuilderArgs.Filter;
                var actualFilter = filterUsed == null ? null : property.GetValue(_ActualAircraftListJsonBuilderArgs.Filter, null);

                if(conditionIsValid) {
                    Assert.IsNotNull(actualFilter, $"Request for {queryString} should have filled {propertyName} but it did not");
                } else {
                    Assert.IsNull(actualFilter, $"Request for {queryString} should not have filled {propertyName} but it did");
                }
            }
        }

        [TestMethod]
        [DataSource("Data Source='WebSiteTests.xls';Provider=Microsoft.Jet.OLEDB.4.0;Persist Security Info=False;Extended Properties='Excel 8.0'",
                    "AircraftListFilterName$")]
        public void FeedsController_AircraftList_Filters_Ignore_Invalid_Conditions_V3()
        {
            var worksheet = new ExcelWorksheetData(TestContext);

            var propertyName = HttpUtility.UrlEncode(worksheet.String("FilterProperty"));
            var fieldName = HttpUtility.UrlEncode(worksheet.String("V3FieldName"));

            var property = typeof(AircraftListJsonBuilderFilter).GetProperty(propertyName);
            var filterType = property.PropertyType;

            var isStringFilter = typeof(FilterString).IsAssignableFrom(filterType);
            var isIntRangeFilter = typeof(FilterRange<int>).IsAssignableFrom(filterType);
            var isDoubleRangeFilter = typeof(FilterRange<double>).IsAssignableFrom(filterType);
            var isEnumFilter = filterType.IsGenericType && filterType.GetGenericTypeDefinition() == typeof(FilterEnum<>);
            var isBoolFilter = typeof(FilterBool).IsAssignableFrom(filterType);

            foreach(FilterCondition condition in Enum.GetValues(typeof(FilterCondition))) {
                var filter = new GetAircraftListFilter() {
                    Field = (GetAircraftListFilterField)Enum.Parse(typeof(GetAircraftListFilterField), fieldName),
                    Condition = condition,
                    Value = isStringFilter || isEnumFilter ? "1" : null,
                    Is = isBoolFilter ? true : (bool?)null,
                    From = isIntRangeFilter || isDoubleRangeFilter ? 1 : (double?)null,
                    To = isIntRangeFilter || isDoubleRangeFilter ? 2 : (double?)null,
                };

                TestCleanup();
                TestInitialise();

                var conditionIsValid = false;
                switch(condition) {
                    case FilterCondition.Between:       conditionIsValid = isIntRangeFilter || isDoubleRangeFilter; break;
                    case FilterCondition.Contains:
                    case FilterCondition.EndsWith:
                    case FilterCondition.StartsWith:    conditionIsValid = isStringFilter; break;
                    case FilterCondition.Equals:        conditionIsValid = isStringFilter || isEnumFilter || isBoolFilter; break;
                    case FilterCondition.Missing:       conditionIsValid = isIntRangeFilter || isDoubleRangeFilter || isEnumFilter || isBoolFilter; break;
                }

                PostJson("/api/3.00/feeds/aircraft-list", new {
                    Filters = new GetAircraftListFilter[] {
                        filter
                    }
                });
                var filterUsed = _ActualAircraftListJsonBuilderArgs.Filter;
                var actualFilter = filterUsed == null ? null : property.GetValue(_ActualAircraftListJsonBuilderArgs.Filter, null);

                if(conditionIsValid) {
                    Assert.IsNotNull(actualFilter, $"Request for {fieldName} {condition} should have filled {propertyName} but it did not");
                } else {
                    Assert.IsNull(actualFilter, $"Request for {fieldName} {condition} should not have filled {propertyName} but it did");
                }
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Filters_Accept_Full_Set_Of_Bounds_V2_POST()
        {
            Post($"AircraftList.json?FNBnd=1.2&FSBnd=3.4&FWBnd=5.6&FEBnd=7.8");

            Assert.AreEqual(1.2, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.First.Latitude);
            Assert.AreEqual(5.6, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.First.Longitude);
            Assert.AreEqual(3.4, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.Second.Latitude);
            Assert.AreEqual(7.8, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.Second.Longitude);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Filters_Accept_Full_Set_Of_Bounds_V2_GET()
        {
            Get($"AircraftList.json?FNBnd=1.2&FSBnd=3.4&FWBnd=5.6&FEBnd=7.8");

            Assert.AreEqual(1.2, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.First.Latitude);
            Assert.AreEqual(5.6, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.First.Longitude);
            Assert.AreEqual(3.4, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.Second.Latitude);
            Assert.AreEqual(7.8, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.Second.Longitude);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Filters_Accept_Full_Set_Of_Bounds_V3()
        {
            var filter = new GetAircraftListFilter() {
                Field = GetAircraftListFilterField.PositionBounds,
                North = 1.2,
                South = 3.4,
                West = 5.6,
                East = 7.8,
            };
            PostJson("/api/3.00/feeds/aircraft-list", new {
                Filters = new GetAircraftListFilter[] {
                    filter
                }
            });

            Assert.AreEqual(1.2, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.First.Latitude);
            Assert.AreEqual(5.6, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.First.Longitude);
            Assert.AreEqual(3.4, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.Second.Latitude);
            Assert.AreEqual(7.8, _ActualAircraftListJsonBuilderArgs.Filter.PositionWithin.Second.Longitude);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Filters_Ignore_Partial_Set_Of_Bounds_V2_POST()
        {
            Post($"AircraftList.json?FNBnd=1.2&FWBnd=5.6");

            Assert.IsNull(_ActualAircraftListJsonBuilderArgs.Filter);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Filters_Ignore_Partial_Set_Of_Bounds_V2_GET()
        {
            Get($"AircraftList.json?FNBnd=1.2&FWBnd=5.6");

            Assert.IsNull(_ActualAircraftListJsonBuilderArgs.Filter);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Filters_Ignore_Partial_Set_Of_Bounds_V3()
        {
            PostJson("/api/3.00/feeds/aircraft-list", new {
                Filters = new GetAircraftListFilter[] {
                    new GetAircraftListFilter() {
                        Field = GetAircraftListFilterField.PositionBounds,
                        North = 1.2,
                        West = 5.6,
                    }
                }
            });

            Assert.IsNull(_ActualAircraftListJsonBuilderArgs.Filter);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Trail_Format_V2_POST()
        {
            foreach(var queryStringValue in new string[] { "F", "FA", "FS", "S", "SA", "SS" }) {
                TestCleanup();
                TestInitialise();

                var trailType = TrailType.None;
                switch(queryStringValue) {
                    case "F":   trailType = TrailType.Full; break;
                    case "FA":  trailType = TrailType.FullAltitude; break;
                    case "FS":  trailType = TrailType.FullSpeed; break;
                    case "S":   trailType = TrailType.Short; break;
                    case "SA":  trailType = TrailType.ShortAltitude; break;
                    case "SS":  trailType = TrailType.ShortSpeed; break;
                }

                var expected = ExpectedAircraftListJsonBuilderArgs(trailType: trailType);
                Post($"AircraftList.json?trFmt={queryStringValue}");

                AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Trail_Format_V2_GET()
        {
            foreach(var queryStringValue in new string[] { "F", "FA", "FS", "S", "SA", "SS" }) {
                TestCleanup();
                TestInitialise();

                var trailType = TrailType.None;
                switch(queryStringValue) {
                    case "F":   trailType = TrailType.Full; break;
                    case "FA":  trailType = TrailType.FullAltitude; break;
                    case "FS":  trailType = TrailType.FullSpeed; break;
                    case "S":   trailType = TrailType.Short; break;
                    case "SA":  trailType = TrailType.ShortAltitude; break;
                    case "SS":  trailType = TrailType.ShortSpeed; break;
                }

                var expected = ExpectedAircraftListJsonBuilderArgs(trailType: trailType);
                Get($"AircraftList.json?trFmt={queryStringValue}");

                AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Trail_Format_Is_Case_Insensitive_V2_POST()
        {
            foreach(var upperCase in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                var queryString = "trfmt=fa";
                if(upperCase) {
                    queryString = queryString.ToUpperInvariant();
                }

                var expected = ExpectedAircraftListJsonBuilderArgs(trailType: TrailType.FullAltitude);
                Post($"AircraftList.json?{queryString}");

                AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Trail_Format_Is_Case_Insensitive_V2_GET()
        {
            foreach(var upperCase in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                var queryString = "trfmt=fa";
                if(upperCase) {
                    queryString = queryString.ToUpperInvariant();
                }

                var expected = ExpectedAircraftListJsonBuilderArgs(trailType: TrailType.FullAltitude);
                Get($"AircraftList.json?{queryString}");

                AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Trail_Format_V3()
        {
            foreach(TrailType trailType in Enum.GetValues(typeof(TrailType))) {
                TestCleanup();
                TestInitialise();

                var expected = ExpectedAircraftListJsonBuilderArgs(trailType: trailType);
                PostJson("/api/3.00/feeds/aircraft-list", new {
                    TrailType = trailType,
                });

                AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Single_Sort_Column_V2_POST()
        {
            foreach(var aircraftComparerColumn in AircraftComparerColumns()) {
                foreach(var sortOrder in new string[] { "asc", "desc" }) {
                    TestCleanup();
                    TestInitialise();

                    var expected = ExpectedAircraftListJsonBuilderArgs(
                        sortColumn1: aircraftComparerColumn,
                        sortAscending1: sortOrder == "asc"
                    );
                    Post($"AircraftList.json?sortBy1={aircraftComparerColumn}&sortOrder1={sortOrder}");

                    AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
                }
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Single_Sort_Column_V2_GET()
        {
            foreach(var aircraftComparerColumn in AircraftComparerColumns()) {
                foreach(var sortOrder in new string[] { "asc", "desc" }) {
                    TestCleanup();
                    TestInitialise();

                    var expected = ExpectedAircraftListJsonBuilderArgs(
                        sortColumn1: aircraftComparerColumn,
                        sortAscending1: sortOrder == "asc"
                    );
                    Get($"AircraftList.json?sortBy1={aircraftComparerColumn}&sortOrder1={sortOrder}");

                    AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
                }
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Single_Sort_Column_V3()
        {
            foreach(var aircraftComparerColumn in AircraftComparerColumns()) {
                foreach(var ascending in new bool[] { true, false }) {
                    TestCleanup();
                    TestInitialise();

                    var expected = ExpectedAircraftListJsonBuilderArgs(
                        sortColumn1: aircraftComparerColumn,
                        sortAscending1: ascending
                    );
                    PostJson("/api/3.00/feeds/aircraft-list", new {
                        SortBy = new object [] {
                            new { Col = aircraftComparerColumn, Asc = ascending },
                        }
                    });

                    AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
                }
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Two_Sort_Columns_V2_POST()
        {
            foreach(var aircraftComparerColumn in AircraftComparerColumns()) {
                foreach(var sortOrder in new string[] { "asc", "desc" }) {
                    TestCleanup();
                    TestInitialise();

                    var expected = ExpectedAircraftListJsonBuilderArgs(
                        sortColumn1: AircraftComparerColumn.Altitude,
                        sortAscending1: true,

                        sortColumn2: aircraftComparerColumn,
                        sortAscending2: sortOrder == "asc"
                    );
                    Post($"AircraftList.json?sortBy1={AircraftComparerColumn.Altitude}&sortOrder1=ASC" +
                         $"&sortBy2={aircraftComparerColumn}&sortOrder2={sortOrder}"
                    );

                    AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
                }
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Two_Sort_Columns_V2_GET()
        {
            foreach(var aircraftComparerColumn in AircraftComparerColumns()) {
                foreach(var sortOrder in new string[] { "asc", "desc" }) {
                    TestCleanup();
                    TestInitialise();

                    var expected = ExpectedAircraftListJsonBuilderArgs(
                        sortColumn1: AircraftComparerColumn.Altitude,
                        sortAscending1: true,

                        sortColumn2: aircraftComparerColumn,
                        sortAscending2: sortOrder == "asc"
                    );
                    Get($"AircraftList.json?sortBy1={AircraftComparerColumn.Altitude}&sortOrder1=ASC" +
                        $"&sortBy2={aircraftComparerColumn}&sortOrder2={sortOrder}"
                    );

                    AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
                }
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Two_Sort_Columns_V3()
        {
            foreach(var aircraftComparerColumn in AircraftComparerColumns()) {
                foreach(var ascending in new bool[] { true, false }) {
                    TestCleanup();
                    TestInitialise();

                    var expected = ExpectedAircraftListJsonBuilderArgs(
                        sortColumn1: AircraftComparerColumn.Altitude,
                        sortAscending1: true,

                        sortColumn2: aircraftComparerColumn,
                        sortAscending2: ascending
                    );
                    PostJson("/api/3.00/feeds/aircraft-list", new {
                        SortBy = new object [] {
                            new { Col = AircraftComparerColumn.Altitude },
                            new { Col = aircraftComparerColumn, Asc = ascending },
                        }
                    });

                    AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
                }
            }
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Single_Known_Aircraft_V2_POST()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
            });

            PostForm($"AircraftList.json", new string[,] {
                { "icaos", "4008f6" }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Single_Known_Aircraft_V2_GET()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
            });

            _Context.RequestHeadersDictionary["X-VirtualRadarServer-AircraftIds"] = "4196598";
            Get($"AircraftList.json");

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Single_Known_Aircraft_V3()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
            });

            PostJson("/api/3.00/feeds/aircraft-list", new {
                PreviousAircraft = new string [] {
                    "4008f6",
                }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Many_Known_Aircraft_Icaos_V2_POST()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
                0xA88CC0,
            });

            PostForm($"AircraftList.json", new string[,] {
                { "icaos", "4008f6-A88CC0" }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Many_Known_Aircraft_Ids_V2_POST()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
                1,
                0xABCDEF,
                0x7FFFFFFF,
            });

            PostForm($"AircraftList.json", new string[,] {
                { "ids", "4008f6-1-ABCDEF-7FFFFFFF" }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Many_Known_Aircraft_V2_GET()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
                0xA88CC0,
            });

            _Context.RequestHeadersDictionary["X-VirtualRadarServer-AircraftIds"] = "4196598,11046080";
            Get($"AircraftList.json");

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Specify_Many_Known_Aircraft_V3()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
                0xA88CC0,
            });

            PostJson("/api/3.00/feeds/aircraft-list", new {
                PreviousAircraft = new string [] {
                    "4008f6", "A88CC0",
                }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Ignores_Invalid_Known_Aircraft_V2_POST()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs();

            PostForm($"AircraftList.json", new string[,] {
                { "icaos", "ANDREW" }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Ignores_Invalid_Known_Aircraft_V2_GET()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs();

            _Context.RequestHeadersDictionary["X-VirtualRadarServer-AircraftIds"] = $"ANDREW";
            Get($"AircraftList.json");

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Can_Ignores_Invalid_Known_Aircraft_V3()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs();

            PostJson("/api/3.00/feeds/aircraft-list", new {
                PreviousAircraft = new string [] {
                    "ANDREW",
                }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_V2_POST_Can_Post_Known_Aircraft_Icaos_And_QueryString_Values()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x4008f6,
            }, serverTimeTicks: 12);

            PostForm($"AircraftList.json?stm=12", new string[,] {
                { "icaos", "4008f6" }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_V2_POST_Can_Post_Known_Aircraft_Ids_And_QueryString_Values()
        {
            var expected = ExpectedAircraftListJsonBuilderArgs(previousAircraft: new int[] {
                0x1,
            }, serverTimeTicks: 12);

            PostForm($"AircraftList.json?stm=12", new string[,] {
                { "ids", "1" }
            });

            AssertBuilderArgsAreEqual(expected, _ActualAircraftListJsonBuilderArgs);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Sets_ServerConfigChanged_If_Changed_V2_POST()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);

            var aircraftList = Post($"AircraftList.json?stm={configLastLoadedTicks - 1}").Json<AircraftListJson>();

            Assert.IsTrue(aircraftList.ServerConfigChanged);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Sets_ServerConfigChanged_If_Changed_V2_GET()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);

            var aircraftList = Get($"AircraftList.json?stm={configLastLoadedTicks - 1}").Json<AircraftListJson>();

            Assert.IsTrue(aircraftList.ServerConfigChanged);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Sets_ServerConfigChanged_If_Tile_Server_Settings_Downloaded_V2_GET()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var tileSettingsDownloaded = configLastLoaded.AddMilliseconds(1);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);
            _TileServerSettingsManager.Setup(r => r.LastDownloadUtc).Returns(tileSettingsDownloaded);

            var aircraftList = Get($"AircraftList.json?stm={configLastLoadedTicks}").Json<AircraftListJson>();

            Assert.IsTrue(aircraftList.ServerConfigChanged);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Sets_ServerConfigChanged_If_Changed_V3()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);

            var aircraftList = PostJson("/api/3.00/feeds/aircraft-list", new {
                ServerTicks = configLastLoadedTicks - 1,
            })
            .Json<AircraftListJson>();

            Assert.IsTrue(aircraftList.ServerConfigChanged);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Sets_ServerConfigChanged_If_Tile_Server_Settings_Downloaded_V3()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var tileSettingsDownloaded = configLastLoaded.AddMilliseconds(1);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);
            _TileServerSettingsManager.Setup(r => r.LastDownloadUtc).Returns(tileSettingsDownloaded);

            var aircraftList = PostJson("/api/3.00/feeds/aircraft-list", new {
                ServerTicks = configLastLoadedTicks,
            })
            .Json<AircraftListJson>();

            Assert.IsTrue(aircraftList.ServerConfigChanged);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Clears_ServerConfigChanged_If_Not_Changed_V2_POST()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);

            var aircraftList = Post($"AircraftList.json?stm={configLastLoadedTicks + 1}").Json<AircraftListJson>();

            Assert.IsFalse(aircraftList.ServerConfigChanged);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Clears_ServerConfigChanged_If_Not_Changed_V2_GET()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);

            var aircraftList = Get($"AircraftList.json?stm={configLastLoadedTicks + 1}").Json<AircraftListJson>();

            Assert.IsFalse(aircraftList.ServerConfigChanged);
        }

        [TestMethod]
        public void FeedsController_AircraftList_Clears_ServerConfigChanged_If_Not_Changed_V3()
        {
            var configLastLoaded = new DateTime(1, 7, 29);
            var configLastLoadedTicks = JavascriptHelper.ToJavascriptTicks(configLastLoaded);

            _SharedConfiguration.Setup(r => r.GetConfigurationChangedUtc()).Returns(configLastLoaded);

            var aircraftList = PostJson("/api/3.00/feeds/aircraft-list", new {
                ServerTicks = configLastLoadedTicks + 1,
            })
            .Json<AircraftListJson>();

            Assert.IsFalse(aircraftList.ServerConfigChanged);
        }
        #endregion
    }
}
