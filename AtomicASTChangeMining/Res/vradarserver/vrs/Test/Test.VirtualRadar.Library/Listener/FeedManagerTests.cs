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
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using InterfaceFactory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Test.Framework;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Network;

namespace Test.VirtualRadar.Library.Listener
{
    [TestClass]
    public class FeedManagerTests
    {
        #region CustomFeed
        private class CustomFeed : ICustomFeed
        {
            public int UniqueId { get; private set; }

            public string Name => "Custom Feed";

            private Mock<IAircraftList> _AircraftList = TestUtilities.CreateMockInstance<IAircraftList>();
            public IAircraftList AircraftList => _AircraftList.Object;

            public bool IsVisible { get; set; } = true;

            public ConnectionStatus ConnectionStatus { get; set; }

            public event EventHandler ConnectionStateChanged;

            public void RaiseConnectionStateChanged() => ConnectionStateChanged?.Invoke(this, EventArgs.Empty);

            public event EventHandler<EventArgs<Exception>> ExceptionCaught;

            public void RaiseExceptionCaught(EventArgs<Exception> args) => ExceptionCaught?.Invoke(this, args);

            public int CallCount_Connect;

            public void Connect()
            {
                ++CallCount_Connect;
            }

            public int CallCount_Disconnect;

            public void Disconnect()
            {
                ++CallCount_Disconnect;
            }

            public int CallCount_Dispose;

            public void Dispose()
            {
                ++CallCount_Dispose;
            }

            public void SetUniqueId(int uniqueId) => UniqueId = uniqueId;
        }
        #endregion

        #region TestContext, Fields, TestInitialise, TestCleanup
        public TestContext TestContext { get; set; }

        private IClassFactory _SnapshotFactory;
        private IFeedManager _Manager;
        private List<Mock<IReceiverFeed>> _CreatedReceiverFeeds;
        private List<Mock<IMergedFeedFeed>> _CreatedMergedFeedFeeds;
        private List<Mock<IListener>> _CreatedListeners;
        private Dictionary<MergedFeed, List<IFeed>> _MergedFeedFeeds;
        private Configuration _Configuration;
        private Mock<IConfigurationStorage> _ConfigurationStorage;
        private Receiver _Receiver1;
        private Receiver _Receiver2;
        private Receiver _Receiver3;
        private Receiver _Receiver4;
        private MergedFeed _MergedFeed1;
        private MergedFeed _MergedFeed2;
        private EventRecorder<EventArgs<Exception>> _ExceptionCaughtRecorder;
        private EventRecorder<EventArgs> _FeedsChangedRecorder;
        private EventRecorder<EventArgs<IFeed>> _ConnectionStateChangedRecorder;

        [TestInitialize]
        public void TestInitialise()
        {
            _SnapshotFactory = Factory.TakeSnapshot();

            _Manager = Factory.ResolveNewInstance<IFeedManager>();

            _Receiver1 = new Receiver() { UniqueId = 1, Name = "First", DataSource = DataSource.Port30003, ConnectionType = ConnectionType.TCP, Address = "127.0.0.1", Port = 30003 };
            _Receiver2 = new Receiver() { UniqueId = 2, Name = "Second", DataSource = DataSource.Beast, ConnectionType = ConnectionType.COM, ComPort = "COM1", BaudRate = 19200, DataBits = 8, StopBits = StopBits.One };
            _Receiver3 = new Receiver() { UniqueId = 3, Name = "Third", ReceiverUsage = ReceiverUsage.HideFromWebSite, };
            _Receiver4 = new Receiver() { UniqueId = 4, Name = "Fourth", ReceiverUsage = ReceiverUsage.MergeOnly, };
            _MergedFeed1 = new MergedFeed() { UniqueId = 5, Name = "M1", ReceiverIds = { 1, 2 }, ReceiverUsage = ReceiverUsage.Normal, };
            _MergedFeed2 = new MergedFeed() { UniqueId = 6, Name = "M2", ReceiverIds = { 3, 4 }, ReceiverUsage = ReceiverUsage.HideFromWebSite, };
            _Configuration = new Configuration() {
                Receivers = { _Receiver1, _Receiver2, _Receiver3, _Receiver4 },
                MergedFeeds = { _MergedFeed1, _MergedFeed2 },
            };
            _ConfigurationStorage = TestUtilities.CreateMockSingleton<IConfigurationStorage>();
            _ConfigurationStorage.Setup(r => r.Load()).Returns(_Configuration);

            _CreatedListeners = new List<Mock<IListener>>();
            _CreatedReceiverFeeds = new List<Mock<IReceiverFeed>>();
            _CreatedMergedFeedFeeds = new List<Mock<IMergedFeedFeed>>();
            _MergedFeedFeeds = new Dictionary<MergedFeed,List<IFeed>>();

            Factory.Register<IReceiverFeed>(() => {
                var feed = TestUtilities.CreateMockInstance<IReceiverFeed>();
                var listener = TestUtilities.CreateMockInstance<IListener>();
                _CreatedListeners.Add(listener);

                feed.Setup(r => r.Initialise(It.IsAny<Receiver>(), It.IsAny<Configuration>())).Callback((Receiver rcvr, Configuration conf) => {
                    feed.Setup(i => i.UniqueId).Returns(rcvr.UniqueId);
                    feed.Setup(i => i.Name).Returns(rcvr.Name);
                    feed.Setup(i => i.Listener).Returns(listener.Object);
                    feed.Setup(i => i.IsVisible).Returns(rcvr.ReceiverUsage == ReceiverUsage.Normal);
                });

                _CreatedReceiverFeeds.Add(feed);
                return feed.Object;
            });

            Factory.Register<IMergedFeedFeed>(() => {
                var feed = TestUtilities.CreateMockInstance<IMergedFeedFeed>();
                var listener = TestUtilities.CreateMockInstance<IListener>();
                _CreatedListeners.Add(listener);

                feed.Setup(r => r.Initialise(It.IsAny<MergedFeed>(), It.IsAny<IEnumerable<IFeed>>())).Callback((MergedFeed mfeed, IEnumerable<IFeed> feeds) => {
                    feed.Setup(i => i.UniqueId).Returns(mfeed.UniqueId);
                    feed.Setup(i => i.Name).Returns(mfeed.Name);
                    feed.Setup(i => i.Listener).Returns(listener.Object);
                    feed.Setup(i => i.IsVisible).Returns(mfeed.ReceiverUsage == ReceiverUsage.Normal);

                    if(_MergedFeedFeeds.ContainsKey(mfeed)) _MergedFeedFeeds[mfeed] = feeds.ToList();
                    else                                    _MergedFeedFeeds.Add(mfeed, feeds.ToList());
                });

                feed.Setup(r => r.ApplyConfiguration(It.IsAny<MergedFeed>(), It.IsAny<IEnumerable<IFeed>>())).Callback((MergedFeed mfeed, IEnumerable<IFeed> feeds) => {
                    if(_MergedFeedFeeds.ContainsKey(mfeed)) _MergedFeedFeeds[mfeed] = feeds.ToList();
                    else                                    _MergedFeedFeeds.Add(mfeed, feeds.ToList());
                });

                _CreatedMergedFeedFeeds.Add(feed);
                return feed.Object;
            });

            _ExceptionCaughtRecorder = new EventRecorder<EventArgs<Exception>>();
            _FeedsChangedRecorder = new EventRecorder<EventArgs>();
            _ConnectionStateChangedRecorder = new EventRecorder<EventArgs<IFeed>>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Factory.RestoreSnapshot(_SnapshotFactory);
            _Manager.Dispose();
        }

        void OnlyUseTwoReceiversAndNoMergedFeed()
        {
            _Configuration.Receivers.Remove(_Receiver3);
            _Configuration.Receivers.Remove(_Receiver4);
            _Configuration.MergedFeeds.Clear();

            _Receiver3 = _Receiver4 = null;
            _MergedFeed1 = _MergedFeed2 = null;
        }
        #endregion

        #region Constructors and Properties
        [TestMethod]
        public void Constructor_Initialises_To_Known_Value_And_Properties_Work()
        {
            _Manager.Dispose();
            _Manager = Factory.ResolveNewInstance<IFeedManager>();

            Assert.AreEqual(0, _Manager.Feeds.Length);
            Assert.AreEqual(0, _Manager.VisibleFeeds.Length);
        }
        #endregion

        #region Initialise
        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Initialise_Throws_If_Called_Twice()
        {
            _Manager.Initialise();
            _Manager.Initialise();
        }

        [TestMethod]
        public void Initialise_Causes_Manager_To_Create_And_Initialise_ReceiverPathways_For_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Manager.Initialise();

            Assert.AreEqual(2, _CreatedReceiverFeeds.Count);
            Assert.AreEqual(0, _CreatedMergedFeedFeeds.Count);
            _CreatedReceiverFeeds[0].Verify(r => r.Initialise(_Receiver1, _Configuration), Times.Once());
            _CreatedReceiverFeeds[1].Verify(r => r.Initialise(_Receiver2, _Configuration), Times.Once());
        }

        [TestMethod]
        public void Initialise_Causes_Manager_To_Create_And_Initialise_ReceiverPathways_For_MergedFeeds()
        {
            _Manager.Initialise();

            Assert.AreEqual(4, _CreatedReceiverFeeds.Count);
            Assert.AreEqual(2, _CreatedMergedFeedFeeds.Count);
            _CreatedMergedFeedFeeds[0].Verify(r => r.Initialise(_MergedFeed1, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
            _CreatedMergedFeedFeeds[1].Verify(r => r.Initialise(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed1].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedReceiverFeeds[0].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedReceiverFeeds[1].Object));

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[3].Object));
        }

        [TestMethod]
        public void Initialise_Does_Not_Create_Pathways_For_Disabled_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Receiver1.Enabled = false;
            _Manager.Initialise();

            Assert.AreEqual(1, _CreatedReceiverFeeds.Count);
            _CreatedReceiverFeeds[0].Verify(r => r.Initialise(_Receiver2, _Configuration), Times.Once());
        }

        [TestMethod]
        public void Initialise_Does_Not_Create_Pathways_For_Disabled_MergedFeeds()
        {
            _MergedFeed1.Enabled = false;
            _Manager.Initialise();

            Assert.AreEqual(1, _CreatedMergedFeedFeeds.Count);
            _CreatedMergedFeedFeeds[0].Verify(r => r.Initialise(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
        }

        [TestMethod]
        public void Initialise_Exposes_Created_Feeds_In_Feeds_Property()
        {
            _Manager.Initialise();

            Assert.AreEqual(6, _Manager.Feeds.Length);
            Assert.AreSame(_CreatedReceiverFeeds[0].Object, _Manager.Feeds[0]);
            Assert.AreSame(_CreatedReceiverFeeds[1].Object, _Manager.Feeds[1]);
            Assert.AreSame(_CreatedReceiverFeeds[2].Object, _Manager.Feeds[2]);
            Assert.AreSame(_CreatedReceiverFeeds[3].Object, _Manager.Feeds[3]);
            Assert.AreSame(_CreatedMergedFeedFeeds[0].Object, _Manager.Feeds[4]);
            Assert.AreSame(_CreatedMergedFeedFeeds[1].Object, _Manager.Feeds[5]);
        }

        [TestMethod]
        public void Initialise_Exposes_Visible_Feeds_In_VisibleFeeds_Property()
        {
            // Receivers 3 & 4 should not be visible, neither should merged feed 2
            _Manager.Initialise();

            Assert.AreEqual(3, _Manager.VisibleFeeds.Length);
            Assert.IsFalse(_Manager.VisibleFeeds.Any(r => r.UniqueId == _Receiver3.UniqueId));
            Assert.IsFalse(_Manager.VisibleFeeds.Any(r => r.UniqueId == _Receiver4.UniqueId));
            Assert.IsFalse(_Manager.VisibleFeeds.Any(r => r.UniqueId == _MergedFeed2.UniqueId));
        }

        [TestMethod]
        public void Initialise_Hooks_ExceptionCaught_On_Pathways()
        {
            _Manager.Initialise();
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var exception = new InvalidOperationException();

            // Pathway created from receiver
            _CreatedReceiverFeeds[0].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));
            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);

            // Pathway created from merged feed
            _CreatedMergedFeedFeeds[0].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));
            Assert.AreEqual(2, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void Initialise_Raises_FeedsChanged()
        {
            _Manager.FeedsChanged += _FeedsChangedRecorder.Handler;
            _Manager.Initialise();

            Assert.AreEqual(1, _FeedsChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _FeedsChangedRecorder.Sender);
        }

        [TestMethod]
        public void Initialise_Connects_Listeners()
        {
            _Receiver1.AutoReconnectAtStartup = true;
            _Receiver2.AutoReconnectAtStartup = false;

            _Manager.Initialise();

            _CreatedListeners[0].Verify(r => r.Connect(), Times.Once());
            _CreatedListeners[1].Verify(r => r.Connect(), Times.Once());

            // Merged feeds are not connected - they don't have any connection state, they always report connected
            _CreatedListeners[4].Verify(r => r.Connect(), Times.Never());
            _CreatedListeners[5].Verify(r => r.Connect(), Times.Never());
        }
        #endregion

        #region AddCustomFeed
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddCustomFeed_Throws_When_Passed_Null()
        {
            _Manager.AddCustomFeed(null);
        }

        [TestMethod]
        public void AddCustomFeed_Adds_The_Feed_If_Called_After_Initialise()
        {
            _Manager.Initialise();

            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);

            Assert.IsTrue(_Manager.Feeds.Contains(customFeed));
            Assert.IsTrue(_Manager.VisibleFeeds.Contains(customFeed));
        }

        [TestMethod]
        public void AddCustomFeed_Does_Not_Destroy_Existing_Feeds()
        {
            _Manager.Initialise();

            Assert.AreEqual(6, _Manager.Feeds.Length);

            _Manager.AddCustomFeed(new CustomFeed());

            Assert.AreEqual(7, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void AddCustomFeed_Adds_The_Feed_If_Called_Before_Initialise()
        {
            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);

            Assert.AreEqual(0, _Manager.Feeds.Length);
            Assert.AreEqual(0, _Manager.VisibleFeeds.Length);

            _Manager.Initialise();

            Assert.IsTrue(_Manager.Feeds.Contains(customFeed));
            Assert.IsTrue(_Manager.VisibleFeeds.Contains(customFeed));
        }

        [TestMethod]
        public void AddCustomFeed_Assigns_UniqueId_If_Zero()
        {
            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);
            Assert.AreNotEqual(0, customFeed.UniqueId);
        }

        [TestMethod]
        public void AddCustomFeed_Will_Not_Overwrite_NonZero_UniqueId()
        {
            var customFeed = new CustomFeed();
            customFeed.SetUniqueId(10203040);
            _Manager.AddCustomFeed(customFeed);

            Assert.AreEqual(10203040, customFeed.UniqueId);
        }

        [TestMethod]
        [ExpectedException(typeof(FeedUniqueIdException))]
        public void AddCustomFeed_Will_Not_Allow_Duplicate_UniqueId_When_Called_Before_Initialise()
        {
            var customFeed1 = new CustomFeed();
            var customFeed2 = new CustomFeed();
            customFeed1.SetUniqueId(100);
            customFeed2.SetUniqueId(100);
            _Manager.AddCustomFeed(customFeed1);
            _Manager.AddCustomFeed(customFeed2);
        }

        [TestMethod]
        [ExpectedException(typeof(FeedUniqueIdException))]
        public void AddCustomFeed_Will_Not_Allow_Clash_With_Configured_UniqueIds()
        {
            var customFeed = new CustomFeed();
            customFeed.SetUniqueId(_Receiver1.UniqueId);
            _Manager.AddCustomFeed(customFeed);
            _Manager.Initialise();
        }

        [TestMethod]
        [ExpectedException(typeof(FeedUniqueIdException))]
        public void AddCustomFeed_Will_Not_Allow_Duplicate_UniqueId_When_Called_After_Initialise()
        {
            _Manager.Initialise();

            var customFeed1 = new CustomFeed();
            var customFeed2 = new CustomFeed();
            customFeed1.SetUniqueId(100);
            customFeed2.SetUniqueId(100);
            _Manager.AddCustomFeed(customFeed1);
            _Manager.AddCustomFeed(customFeed2);
        }

        [TestMethod]
        public void AddCustomFeed_Will_Not_Assign_The_Same_UniqueId_Twice()
        {
            var customFeed1 = new CustomFeed();
            var customFeed2 = new CustomFeed();
            _Manager.AddCustomFeed(customFeed1);
            _Manager.AddCustomFeed(customFeed2);

            Assert.AreNotEqual(customFeed1.UniqueId, customFeed2.UniqueId);
        }

        [TestMethod]
        public void AddCustomFeed_Hooks_Feeds_In_Initialise_When_Called_Before_Initialise()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);

            customFeed.RaiseConnectionStateChanged();
            Assert.AreEqual(0, _ConnectionStateChangedRecorder.CallCount);
            customFeed.RaiseExceptionCaught(new EventArgs<Exception>(new NotImplementedException()));
            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);

            _Manager.Initialise();

            customFeed.RaiseConnectionStateChanged();
            Assert.AreEqual(1, _ConnectionStateChangedRecorder.CallCount);
            customFeed.RaiseExceptionCaught(new EventArgs<Exception>(new NotImplementedException()));
            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void AddCustomFeed_Hooks_Feeds_When_Called_After_Initialise()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _Manager.Initialise();

            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);

            customFeed.RaiseConnectionStateChanged();
            Assert.AreEqual(1, _ConnectionStateChangedRecorder.CallCount);

            customFeed.RaiseExceptionCaught(new EventArgs<Exception>(new NotImplementedException()));
            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
        }
        #endregion

        #region RemoveCustomFeed
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveCustomFeed_Throws_When_Passed_Null()
        {
            _Manager.RemoveCustomFeed(null);
        }

        [TestMethod]
        public void RemoveCustomFeed_Ignores_Attempts_To_Remove_Feeds_That_Have_Not_Been_Added()
        {
            _Manager.RemoveCustomFeed(new CustomFeed());
        }

        [TestMethod]
        public void RemoveCustomFeed_Removes_Feeds_When_Called_Before_Initialise()
        {
            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);

            _Manager.RemoveCustomFeed(customFeed);

            _Manager.Initialise();
            Assert.IsFalse(_Manager.Feeds.Contains(customFeed));
            Assert.IsFalse(_Manager.VisibleFeeds.Contains(customFeed));
        }

        [TestMethod]
        public void RemoveCustomFeed_Removes_Feeds_When_Called_After_Initialise()
        {
            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);
            _Manager.Initialise();

            _Manager.RemoveCustomFeed(customFeed);

            Assert.IsFalse(_Manager.Feeds.Contains(customFeed));
            Assert.IsFalse(_Manager.VisibleFeeds.Contains(customFeed));
        }

        [TestMethod]
        public void RemoveCustomFeed_Does_Not_Return_UniqueId_To_Pool()
        {
            var customFeed1 = new CustomFeed();
            _Manager.AddCustomFeed(customFeed1);
            _Manager.RemoveCustomFeed(customFeed1);

            var customFeed2 = new CustomFeed();
            _Manager.AddCustomFeed(customFeed2);

            Assert.AreNotEqual(customFeed1.UniqueId, customFeed2.UniqueId);
        }

        [TestMethod]
        public void RemoveCustomFeed_Detaches_Events()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);
            _Manager.Initialise();
            _Manager.RemoveCustomFeed(customFeed);

            customFeed.RaiseConnectionStateChanged();
            Assert.AreEqual(0, _ConnectionStateChangedRecorder.CallCount);

            customFeed.RaiseExceptionCaught(new EventArgs<Exception>(new NotImplementedException()));
            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }
        #endregion

        #region ConfigurationChanged
        [TestMethod]
        public void ConfigurationChanged_Updates_Existing_ReceiverPathways_For_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Manager.Initialise();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedReceiverFeeds[0].Verify(r => r.ApplyConfiguration(_Receiver1, _Configuration), Times.Once());
            _CreatedReceiverFeeds[1].Verify(r => r.ApplyConfiguration(_Receiver2, _Configuration), Times.Once());
        }

        [TestMethod]
        public void ConfigurationChanged_Updates_Existing_ReceiverPathways_For_Merged_Feeds()
        {
            _Manager.Initialise();
            _MergedFeedFeeds.Clear();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedMergedFeedFeeds[0].Verify(r => r.ApplyConfiguration(_MergedFeed1, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
            _CreatedMergedFeedFeeds[1].Verify(r => r.ApplyConfiguration(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed1].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedReceiverFeeds[0].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed1].Contains(_CreatedReceiverFeeds[1].Object));

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[3].Object));
        }

        [TestMethod]
        public void ConfigurationChanged_Determines_Existing_ReceiverPathways_By_UniqueId_For_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Manager.Initialise();
            _Receiver1.Name = "New Name";

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedReceiverFeeds[0].Verify(r => r.ApplyConfiguration(_Receiver1, _Configuration), Times.Once());
        }

        [TestMethod]
        public void ConfigurationChanged_Determines_Existing_ReceiverPathways_By_UniqueId_For_MergedFeeds()
        {
            _Manager.Initialise();
            _MergedFeed1.Name = "New Name";

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedMergedFeedFeeds[0].Verify(r => r.ApplyConfiguration(_MergedFeed1, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
        }

        [TestMethod]
        public void ConfigurationChanged_Disposes_Of_Old_ReceiverPathways()
        {
            _Manager.Initialise();
            _Configuration.Receivers.RemoveAt(0);
            _Configuration.MergedFeeds.RemoveAt(0);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedReceiverFeeds[0].Verify(r => r.Dispose(), Times.Once());
            _CreatedMergedFeedFeeds[0].Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ConfigurationChanged_Identifies_Old_ReceiverPathways_By_UniqueId()
        {
            _Manager.Initialise();
            _Receiver1.Name = "New Name";
            _MergedFeed1.Name = "Another New Name";

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedReceiverFeeds[0].Verify(r => r.Dispose(), Times.Never());
            _CreatedMergedFeedFeeds[0].Verify(r => r.Dispose(), Times.Never());
        }

        [TestMethod]
        public void ConfigurationChanged_Unhooks_Disposed_ReceiverPathways()
        {
            _Manager.Initialise();
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _Configuration.Receivers.RemoveAt(0);
            _Configuration.MergedFeeds.RemoveAt(0);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedReceiverFeeds[0].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(new Exception()));
            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);

            _CreatedMergedFeedFeeds[0].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(new Exception()));
            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }

        [TestMethod]
        public void ConfigurationChanged_Disposes_Of_ReceiverPathways_That_Have_Been_Disabled()
        {
            _Manager.Initialise();
            _Receiver1.Enabled = false;
            _MergedFeed1.Enabled = false;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedReceiverFeeds[0].Verify(r => r.Dispose(), Times.Once());
            _CreatedMergedFeedFeeds[0].Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void ConfigurationChanged_Creates_New_Pathways_For_New_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Configuration.Receivers.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.Receivers.Add(_Receiver2);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedReceiverFeeds[1].Verify(r => r.Initialise(_Receiver2, _Configuration), Times.Once());
            _CreatedReceiverFeeds[1].Verify(r => r.ApplyConfiguration(_Receiver2, _Configuration), Times.Never());
        }

        [TestMethod]
        public void ConfigurationChanged_Connects_New_Receivers()
        {
            foreach(var reconnect in new bool[] { true, false }) {
                TestCleanup();
                TestInitialise();

                OnlyUseTwoReceiversAndNoMergedFeed();
                _Configuration.Receivers.RemoveAt(1);
                _Manager.Initialise();
                _Receiver2.AutoReconnectAtStartup = reconnect;
                _Configuration.Receivers.Add(_Receiver2);

                _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

                _CreatedListeners[1].Verify(r => r.Connect(), Times.Once());
            }
        }

        [TestMethod]
        public void ConfigurationChanged_Does_Not_Create_New_Feeds_For_Disabled_New_Receivers()
        {
            OnlyUseTwoReceiversAndNoMergedFeed();
            _Configuration.Receivers.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.Receivers.Add(_Receiver2);
            _Receiver2.Enabled = false;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void ConfigurationChanged_Creates_New_Feeds_For_New_MergedFeeds()
        {
            _Configuration.MergedFeeds.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.MergedFeeds.Add(_MergedFeed2);

            _MergedFeedFeeds.Clear();
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedMergedFeedFeeds[1].Verify(r => r.Initialise(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Once());
            _CreatedMergedFeedFeeds[1].Verify(r => r.ApplyConfiguration(_MergedFeed2, It.IsAny<IEnumerable<IFeed>>()), Times.Never());

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[3].Object));
        }

        [TestMethod]
        public void ConfigurationChanged_Creates_New_Feeds_For_Disabled_New_MergedFeeds()
        {
            _Configuration.MergedFeeds.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.MergedFeeds.Add(_MergedFeed2);
            _MergedFeed2.Enabled = false;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(5, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void ConfigurationChanged_Creates_New_Merged_Feeds_After_All_Receivers_Have_Been_Created()
        {
            _Configuration.Receivers.Remove(_Receiver3);
            _Configuration.Receivers.Remove(_Receiver4);
            _Configuration.MergedFeeds.Clear();
            _Manager.Initialise();

            _Configuration.Receivers.Add(_Receiver3);
            _Configuration.Receivers.Add(_Receiver4);
            _Configuration.MergedFeeds.Add(_MergedFeed2);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(2, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[2].Object));
            Assert.IsTrue(_MergedFeedFeeds[_MergedFeed2].Contains(_CreatedReceiverFeeds[3].Object));
        }

        [TestMethod]
        public void ConfigurationChanged_Creates_New_Merged_Feeds_After_All_Receivers_Have_Been_Deleted()
        {
            _Configuration.MergedFeeds.Clear();
            _Manager.Initialise();

            _Configuration.Receivers.Remove(_Receiver4);
            _Configuration.MergedFeeds.Add(_MergedFeed2);

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _MergedFeedFeeds[_MergedFeed2].Count);
            Assert.AreSame(_CreatedReceiverFeeds[2].Object, _MergedFeedFeeds[_MergedFeed2][0]);
        }

        [TestMethod]
        public void ConfigurationChanged_Hooks_ExceptionCaught_On_New_Receivers()
        {
            _Configuration.Receivers.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.Receivers.Add(_Receiver2);
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            var exception = new InvalidOperationException();
            _CreatedReceiverFeeds[1].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void ConfigurationChanged_Hooks_ExceptionCaught_On_New_Merged_Feeds()
        {
            _Configuration.MergedFeeds.RemoveAt(1);
            _Manager.Initialise();
            _Configuration.MergedFeeds.Add(_MergedFeed2);
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            var exception = new InvalidOperationException();
            _CreatedMergedFeedFeeds[1].Raise(r => r.ExceptionCaught += null, new EventArgs<Exception>(exception));

            Assert.AreEqual(1, _ExceptionCaughtRecorder.CallCount);
            Assert.AreSame(_Manager, _ExceptionCaughtRecorder.Sender);
            Assert.AreSame(exception, _ExceptionCaughtRecorder.Args.Value);
        }

        [TestMethod]
        public void ConfigurationChanged_Reflects_Changes_In_Feeds_Property()
        {
            _Manager.Initialise();

            _Receiver1.Enabled = false;
            _MergedFeed1.Enabled = false;
            _Configuration.Receivers.Add(new Receiver() { UniqueId = 100, Name = "New Receiver", Port = 10001 });
            _Configuration.MergedFeeds.Add(new MergedFeed() { UniqueId = 101, Name = "New Merged Feed", ReceiverIds = { 3, 4, 100 } });
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(6, _Manager.Feeds.Length);
            for(var i = 0;i < 4;++i) {
                var stillExists = i != 0;
                Assert.AreEqual(stillExists, _Manager.Feeds.Contains(_CreatedReceiverFeeds[i].Object));
            }
            for(var i = 0;i < 2;++i) {
                var stillExists = i != 0;
                Assert.AreEqual(stillExists, _Manager.Feeds.Contains(_CreatedMergedFeedFeeds[i].Object));
            }
        }

        [TestMethod]
        public void ConfigurationChanged_Reflects_Changes_In_VisibleFeeds_Property()
        {
            _Manager.Initialise();

            _Configuration.Receivers.Add(new Receiver() { UniqueId = 100, Name = "New Receiver", Port = 10001 });

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(4, _Manager.VisibleFeeds.Length);
        }

        [TestMethod]
        public void ConfigurationChanged_Raises_FeedsChanged()
        {
            _Manager.Initialise();
            _Manager.FeedsChanged += _FeedsChangedRecorder.Handler;

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _FeedsChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _FeedsChangedRecorder.Sender);
        }

        [TestMethod]
        public void ConfigurationChanged_Preserves_Custom_Feeds()
        {
            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);
            _Manager.Initialise();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(1, _Manager.Feeds.Count(r => Object.ReferenceEquals(r, customFeed)));
            Assert.AreEqual(1, _Manager.VisibleFeeds.Count(r => Object.ReferenceEquals(r, customFeed)));
        }
        #endregion

        #region Dispose
        [TestMethod]
        public void Dispose_Disposes_Of_All_Pathways()
        {
            _Manager.Initialise();

            _Manager.Dispose();

            _CreatedReceiverFeeds[0].Verify(r => r.Dispose(), Times.Once());
            _CreatedReceiverFeeds[1].Verify(r => r.Dispose(), Times.Once());
        }

        [TestMethod]
        public void Dispose_Unhooks_All_Pathways()
        {
            _Manager.Initialise();
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            _Manager.Dispose();
            var args = new EventArgs<Exception>(new Exception());
            _CreatedReceiverFeeds[0].Raise(r => r.ExceptionCaught += null, args);
            _CreatedReceiverFeeds[1].Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
            Assert.AreEqual(0, _ConnectionStateChangedRecorder.CallCount);
        }

        [TestMethod]
        public void Dispose_Unhooks_ConfigurationManager()
        {
            _Manager.Initialise();
            _Manager.Dispose();

            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            Assert.AreEqual(0, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void Dispose_Clears_Down_Feeds_Property()
        {
            _Manager.Initialise();
            _Manager.Dispose();
            Assert.AreEqual(0, _Manager.Feeds.Length);
        }

        [TestMethod]
        public void Dispose_Clears_Down_VisibleFeeds_Property()
        {
            _Manager.Initialise();
            _Manager.Dispose();
            Assert.AreEqual(0, _Manager.VisibleFeeds.Length);
        }

        [TestMethod]
        public void Dispose_Can_Be_Called_Before_Initialise()
        {
            _Manager.Dispose();
        }

        [TestMethod]
        public void Dispose_Can_Be_Called_Twice()
        {
            _Manager.Dispose();
            _Manager.Dispose();
        }

        [TestMethod]
        public void Dispose_Does_Not_Dispose_Custom_Feeds()
        {
            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);
            _Manager.Initialise();

            _Manager.Dispose();

            Assert.AreEqual(0, customFeed.CallCount_Dispose);
        }

        [TestMethod]
        public void Dispose_Disconnects_Custom_Feeds()
        {
            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);
            _Manager.Initialise();

            _Manager.Dispose();

            Assert.AreEqual(1, customFeed.CallCount_Disconnect);
        }

        [TestMethod]
        public void Dispose_Unhooks_Custom_Feeds()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.ExceptionCaught += _ExceptionCaughtRecorder.Handler;

            var customFeed = new CustomFeed();
            _Manager.AddCustomFeed(customFeed);
            _Manager.Initialise();

            _Manager.Dispose();

            customFeed.RaiseConnectionStateChanged();
            Assert.AreEqual(0, _ConnectionStateChangedRecorder.CallCount);

            customFeed.RaiseExceptionCaught(new EventArgs<Exception>(new NotImplementedException()));
            Assert.AreEqual(0, _ExceptionCaughtRecorder.CallCount);
        }
        #endregion

        #region ConnectionStateChanged
        [TestMethod]
        public void ConnectionStateChanged_Raised_When_Feed_Raises_Event()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.Initialise();

            _CreatedReceiverFeeds[0].Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);
            Assert.AreEqual(1, _ConnectionStateChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _ConnectionStateChangedRecorder.Sender);
            Assert.AreSame(_CreatedReceiverFeeds[0].Object, _ConnectionStateChangedRecorder.Args.Value);

            _CreatedMergedFeedFeeds[0].Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);
            Assert.AreEqual(2, _ConnectionStateChangedRecorder.CallCount);
            Assert.AreSame(_Manager, _ConnectionStateChangedRecorder.Sender);
            Assert.AreSame(_CreatedMergedFeedFeeds[0].Object, _ConnectionStateChangedRecorder.Args.Value);
        }

        [TestMethod]
        public void ConnectionStateChanged_Not_Raised_When_Listener_Raises_Event_After_Removal_By_Configuration_Change()
        {
            _Manager.ConnectionStateChanged += _ConnectionStateChangedRecorder.Handler;
            _Manager.Initialise();
            _Configuration.Receivers.RemoveAt(1);
            _ConfigurationStorage.Raise(r => r.ConfigurationChanged += null, EventArgs.Empty);

            _CreatedListeners[1].Raise(r => r.ConnectionStateChanged += null, EventArgs.Empty);
            Assert.AreEqual(0, _ConnectionStateChangedRecorder.CallCount);
        }
        #endregion

        #region GetByName
        [TestMethod]
        public void GetByName_Returns_Null_If_Passed_Null()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByName(null, false));
        }

        [TestMethod]
        public void GetByName_Returns_Feed_If_Passed_Matching_Name()
        {
            _Manager.Initialise();
            Assert.AreSame(_CreatedReceiverFeeds[0].Object, _Manager.GetByName(_Receiver1.Name, false));
        }

        [TestMethod]
        public void GetByName_Can_Ignore_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByName(_Receiver3.Name, ignoreInvisibleFeeds: true));
        }

        [TestMethod]
        public void GetByName_Can_Return_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNotNull(_Manager.GetByName(_Receiver3.Name, ignoreInvisibleFeeds: false));
        }

        [TestMethod]
        public void GetByName_Is_Case_Insensitive()
        {
            _Manager.Initialise();
            Assert.AreSame(_CreatedReceiverFeeds[0].Object, _Manager.GetByName(_Receiver1.Name.ToLowerInvariant(), false));
            Assert.AreSame(_CreatedReceiverFeeds[0].Object, _Manager.GetByName(_Receiver1.Name.ToUpperInvariant(), false));
        }

        [TestMethod]
        public void GetByName_Returns_Null_If_Not_Found()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByName("DOES NOT EXIST", false));
        }
        #endregion

        #region GetByUniqueId
        [TestMethod]
        public void GetByUniqueId_Returns_Pathway_With_Matching_UniqueId()
        {
            _Manager.Initialise();
            Assert.AreSame(_CreatedReceiverFeeds[0].Object, _Manager.GetByUniqueId(_Receiver1.UniqueId, false));
        }

        [TestMethod]
        public void GetByUniqueId_Returns_Null_If_No_Record_Matches()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByUniqueId(_Manager.Feeds.Select(r => r.UniqueId).Max() + 1, false));
        }

        [TestMethod]
        public void GetByUniqueId_Can_Ignore_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNull(_Manager.GetByUniqueId(_Receiver3.UniqueId, ignoreInvisibleFeeds: true));
        }

        [TestMethod]
        public void GetByUniqueId_Can_Return_Invisible_Feeds()
        {
            _Manager.Initialise();
            Assert.IsNotNull(_Manager.GetByUniqueId(_Receiver3.UniqueId, ignoreInvisibleFeeds: false));
        }
        #endregion

        #region Connect
        [TestMethod]
        public void Connect_Passes_The_Call_Through_To_Feeds()
        {
            _Manager.Initialise();

            // Can't use Verify as that will count the connect from Initialise as well and it gets a bit messy
            var seenReceiverFeedConnect = false;
            var seenMergedFeedConnect = false;
            _CreatedReceiverFeeds[0].Setup(r => r.Connect()).Callback(() => seenReceiverFeedConnect = true);
            _CreatedMergedFeedFeeds[0].Setup(r => r.Connect()).Callback(() => seenMergedFeedConnect = true);

            _Manager.Connect();

            Assert.IsTrue(seenReceiverFeedConnect);
            Assert.IsTrue(seenMergedFeedConnect);
        }
        #endregion

        #region Disconnect
        [TestMethod]
        public void Disconnect_Passes_The_Call_Through_To_Feeds()
        {
            _Manager.Initialise();
            _Manager.Disconnect();

            _CreatedReceiverFeeds[0].Verify(r => r.Disconnect(), Times.Once());
            _CreatedMergedFeedFeeds[0].Verify(r => r.Disconnect(), Times.Once());
        }
        #endregion
    }
}
