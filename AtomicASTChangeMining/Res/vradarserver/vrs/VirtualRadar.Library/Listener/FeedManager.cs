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
using System.Linq;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Listener;
using VirtualRadar.Interface.Settings;

namespace VirtualRadar.Library.Listener
{
    /// <summary>
    /// The default implementation of <see cref="IFeedManager"/>.
    /// </summary>
    class FeedManager : IFeedManager
    {
        private int _NextCustomUniqueId =       1000000;
        private const int MaxCustomUniqueId =   1999999;

        /// <summary>
        /// True after <see cref="Initialise"/> has been called.
        /// </summary>
        private bool _Initialised;
        
        /// <summary>
        /// Locks the <see cref="Feeds"/> list.
        /// </summary>
        private object _SyncLock = new object();

        /// <summary>
        /// A list of custom feeds that were added before <see cref="Initialise"/> was called.
        /// </summary>
        private List<ICustomFeed> _PreinitialiseCustomFeeds = new List<ICustomFeed>();

        private IFeed[] _Feeds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFeed[] Feeds
        {
            get {
                IFeed[] result;
                lock(_SyncLock) result = _Feeds;
                return result;
            }
            private set {
                lock(_SyncLock) {
                    _Feeds = value;
                    _VisibleFeeds = value.Where(r => r.IsVisible).ToArray();
                }
            }
        }

        private IFeed[] _VisibleFeeds;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public IFeed[] VisibleFeeds
        {
            get {
                IFeed[] result;
                lock(_SyncLock) result = _VisibleFeeds;
                return result;
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<IFeed>> ConnectionStateChanged;

        /// <summary>
        /// Raises <see cref="ConnectionStateChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnConnectionStateChanged(EventArgs<IFeed> args)
        {
            EventHelper.Raise(ConnectionStateChanged, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler<EventArgs<Exception>> ExceptionCaught;

        /// <summary>
        /// Raises <see cref="ExceptionCaught"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnExceptionCaught(EventArgs<Exception> args)
        {
            EventHelper.Raise(ExceptionCaught, this, args);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public event EventHandler FeedsChanged;

        /// <summary>
        /// Raises <see cref="FeedsChanged"/>.
        /// </summary>
        /// <param name="args"></param>
        protected virtual void OnFeedsChanged(EventArgs args)
        {
            EventHelper.Raise(FeedsChanged, this, args);
        }

        /// <summary>
        /// Creates a new object.
        /// </summary>
        public FeedManager()
        {
            Feeds = new IFeed[0];
        }

        /// <summary>
        /// Finalises the object.
        /// </summary>
        ~FeedManager()
        {
            Dispose(false);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes of or finalises the object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if(disposing) {
                var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
                configurationStorage.ConfigurationChanged -= ConfigurationStorage_ConfigurationChanged;

                foreach(var feed in Feeds) {
                    DetachFeed(feed);
                    if(feed is ICustomFeed customFeed) {
                        customFeed.Disconnect();
                    } else {
                        feed.Dispose();
                    }
                }
                Feeds = new IFeed[0];
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Initialise()
        {
            if(_Initialised) throw new InvalidOperationException("The feed manager has already been initialised");

            var configurationStorage = Factory.ResolveSingleton<IConfigurationStorage>();
            var configuration = configurationStorage.Load();
            configurationStorage.ConfigurationChanged += ConfigurationStorage_ConfigurationChanged;

            var feeds = new List<IFeed>();
            foreach(var receiver in configuration.Receivers.Where(r => r.Enabled)) {
                CreateFeedForReceiver(receiver, configuration, feeds);
            }

            var justReceiverFeeds = new List<IFeed>(feeds);
            foreach(var mergedFeed in configuration.MergedFeeds.Where(r => r.Enabled)) {
                CreateFeedForMergedFeed(mergedFeed, justReceiverFeeds, feeds);
            }

            var firstClashingCustomFeed = _PreinitialiseCustomFeeds
                .FirstOrDefault(customFeed => feeds.Any(feed => feed.UniqueId == customFeed.UniqueId));
            if(firstClashingCustomFeed != null) {
                throw new FeedUniqueIdException($"A custom feed has used a unique ID of {firstClashingCustomFeed.UniqueId}. This ID is already in use.");
            }
            foreach(var customFeed in _PreinitialiseCustomFeeds) {
                AttachFeed(customFeed, feeds);
            }
            _PreinitialiseCustomFeeds.Clear();

            Feeds = feeds.ToArray();
            _Initialised = true;

            OnFeedsChanged(EventArgs.Empty);
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="customFeed"></param>
        public void AddCustomFeed(ICustomFeed customFeed)
        {
            if(customFeed == null) {
                throw new ArgumentNullException(nameof(customFeed));
            }

            if(customFeed.UniqueId == 0) {
                var uniqueId = System.Threading.Interlocked.Increment(ref _NextCustomUniqueId);
                if(uniqueId > MaxCustomUniqueId) {
                    throw new FeedUniqueIdException($"Cannot allocate any more unique custom feed identifiers, the limit of {MaxCustomUniqueId + 1} has been reached");
                }
                customFeed.SetUniqueId(uniqueId);
            }

            lock(_SyncLock) {
                if(!_Initialised) {
                    if(_PreinitialiseCustomFeeds.Any(r => r.UniqueId == customFeed.UniqueId)) {
                        throw new FeedUniqueIdException($"Cannot add a custom feed with a unique ID of {customFeed.UniqueId} - the ID is already in use");
                    }

                    _PreinitialiseCustomFeeds.Add(customFeed);
                } else {
                    if(_Feeds.Any(r => r.UniqueId == customFeed.UniqueId)) {
                        throw new FeedUniqueIdException($"Cannot add a custom feed with a unique ID of {customFeed.UniqueId} - the ID is already in use");
                    }

                    var feeds = new List<IFeed>(_Feeds);
                    AttachFeed(customFeed, feeds);
                    Feeds = feeds.ToArray();
                }
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="customFeed"></param>
        public void RemoveCustomFeed(ICustomFeed customFeed)
        {
            if(customFeed == null) {
                throw new ArgumentNullException(nameof(customFeed));
            }

            lock(_SyncLock) {
                if(!_Initialised) {
                    _PreinitialiseCustomFeeds.Remove(customFeed);
                } else {
                    var feeds = new List<IFeed>(_Feeds);
                    if(feeds.Remove(customFeed)) {
                        DetachFeed(customFeed);
                    }
                    Feeds = feeds.ToArray();
                }
            }
        }

        private void CreateFeedForReceiver(Receiver receiver, Configuration configuration, List<IFeed> feeds)
        {
            var feed = Factory.Resolve<IReceiverFeed>();
            feed.Initialise(receiver, configuration);
            AttachFeed(feed, feeds);
            feed.Listener.Connect();
        }

        private void CreateFeedForMergedFeed(MergedFeed mergedFeed, IEnumerable<IFeed> allReceiverPathways, List<IFeed> feeds)
        {
            var mergeFeeds = allReceiverPathways.Where(r => mergedFeed.ReceiverIds.Contains(r.UniqueId)).ToArray();
            var feed = Factory.Resolve<IMergedFeedFeed>();
            feed.Initialise(mergedFeed, mergeFeeds);
            AttachFeed(feed, feeds);
        }

        private void AttachFeed(IFeed feed, List<IFeed> feeds)
        {
            feed.ExceptionCaught += Feed_ExceptionCaught;
            feed.ConnectionStateChanged += Feed_ConnectionStateChanged;
            feeds.Add(feed);
        }

        /// <summary>
        /// Updates existing feeds, removes dead feeds and adds new feeds after a change in configuration.
        /// </summary>
        /// <param name="configurationStorage"></param>
        private void ApplyConfigurationChanges(IConfigurationStorage configurationStorage)
        {
            var configuration = configurationStorage.Load();
            var configReceiverSettings = configuration.Receivers;
            var configMergedFeedSettings = configuration.MergedFeeds;
            var existingFeeds = new List<IFeed>(Feeds);
            var feeds = new List<IFeed>(existingFeeds);

            for(var pass = 0;pass < 2;++pass) {
                var justReceiverFeeds = pass == 0 ? null : new List<IFeed>(feeds);

                if(pass == 0) {
                    foreach(var newReceiver in configReceiverSettings.Where(r => r.Enabled && !existingFeeds.Any(i => i.UniqueId == r.UniqueId))) {
                        CreateFeedForReceiver(newReceiver, configuration, feeds);
                    }
                } else {
                    foreach(var newMergedFeed in configMergedFeedSettings.Where(r => r.Enabled && !existingFeeds.Any(i => i.UniqueId == r.UniqueId))) {
                        CreateFeedForMergedFeed(newMergedFeed, justReceiverFeeds, feeds);
                    }
                }

                foreach(var feed in existingFeeds) {
                    var receiverConfig = configReceiverSettings.FirstOrDefault(r => r.UniqueId == feed.UniqueId);
                    if(receiverConfig != null && !receiverConfig.Enabled) receiverConfig = null;

                    var mergedFeedConfig = configMergedFeedSettings.FirstOrDefault(r => r.UniqueId == feed.UniqueId);
                    if(mergedFeedConfig != null && !mergedFeedConfig.Enabled) mergedFeedConfig = null;

                    if(receiverConfig != null) {
                        if(pass == 0 && feed is IReceiverFeed receiverFeed) {
                            receiverFeed.ApplyConfiguration(receiverConfig, configuration);
                        }
                    } else if(mergedFeedConfig != null) {
                        if(pass == 1 && feed is IMergedFeedFeed mergedFeedFeed) {
                            var mergeFeeds = justReceiverFeeds.Where(r => mergedFeedConfig.ReceiverIds.Contains(r.UniqueId)).ToList();
                            mergedFeedFeed.ApplyConfiguration(mergedFeedConfig, mergeFeeds);
                        }
                    } else if(feed is ICustomFeed _) {
                        ;
                    } else if(pass == 0) {
                        DetachFeed(feed);
                        feed.Dispose();
                        feeds.Remove(feed);
                    }
                }
            }

            Feeds = feeds.ToArray();
            OnFeedsChanged(EventArgs.Empty);
        }

        void DetachFeed(IFeed feed)
        {
            feed.ExceptionCaught -=         Feed_ExceptionCaught;
            feed.ConnectionStateChanged -=  Feed_ConnectionStateChanged;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="ignoreInvisibleFeeds"></param>
        /// <returns></returns>
        public IFeed GetByName(string name, bool ignoreInvisibleFeeds)
        {
            var result = Feeds.FirstOrDefault(r => (r.Name ?? "").Equals(name, StringComparison.CurrentCultureIgnoreCase));
            if(result != null && ignoreInvisibleFeeds && !result.IsVisible) {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="ignoreInvisibleFeeds"></param>
        /// <returns></returns>
        public IFeed GetByUniqueId(int id, bool ignoreInvisibleFeeds)
        {
            var result = Feeds.FirstOrDefault(r => r.UniqueId == id);
            if(result != null && ignoreInvisibleFeeds && !result.IsVisible) {
                result = null;
            }

            return result;
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Connect()
        {
            foreach(var feed in Feeds) {
                feed.Connect();
            }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public void Disconnect()
        {
            foreach(var feed in Feeds) {
                feed.Disconnect();
            }
        }

        /// <summary>
        /// Raised when a feed picks up an exception.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Feed_ExceptionCaught(object sender, EventArgs<Exception> args)
        {
            OnExceptionCaught(args);
        }

        /// <summary>
        /// Raised when the configuration has changed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void ConfigurationStorage_ConfigurationChanged(object sender, EventArgs args)
        {
            ApplyConfigurationChanges((IConfigurationStorage)sender);
        }

        /// <summary>
        /// Raised when a feed raises ConnectionStateChanged.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void Feed_ConnectionStateChanged(object sender, EventArgs args)
        {
            if(sender is IFeed feed) {
                OnConnectionStateChanged(new EventArgs<IFeed>(feed));
            }
        }
    }
}
