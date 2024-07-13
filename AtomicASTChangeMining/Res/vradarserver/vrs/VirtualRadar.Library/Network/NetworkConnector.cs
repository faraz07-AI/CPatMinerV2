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
using System.Net.Sockets;
using System.Text;
using System.Threading;
using InterfaceFactory;
using VirtualRadar.Interface;
using VirtualRadar.Interface.Network;
using VirtualRadar.Interface.Settings;
using VirtualRadar.Interop;

namespace VirtualRadar.Library.Network
{
    /// <summary>
    /// The default implementation of the <see cref="NetworkConnector"/>.
    /// </summary>
    class NetworkConnector : Connector, INetworkConnector
    {
        #region Private class - SocketState
        /// <summary>
        /// The class that carries state in the BeginConnect call for active connections
        /// and the BeginAccept call for passive connections.
        /// </summary>
        class SocketState : WaitState
        {
            public Socket Socket { get; set; }

            public SocketState(Socket socket) : base()
            {
                Socket = socket;
            }
        }
        #endregion

        #region Public fields
        /// <summary>
        /// The number of milliseconds the connector waits before abandoning an attempt to connect.
        /// </summary>
        public static readonly int ConnectTimeout = 30000;

        /// <summary>
        /// The number of milliseconds the connector waits before retrying the attempt to connect.
        /// </summary>
        public static readonly int RetryConnectTimeout = 5000;

        /// <summary>
        /// The number of milliseconds of inactivity before the connection times out.
        /// </summary>
        public static readonly int DefaultIdleTimeout = 60000;

        /// <summary>
        /// The number of milliseconds to wait before forcibly halting the connect thread when disconnecting.
        /// </summary>
        public static readonly int DisconnectTimeout = 2000;

        /// <summary>
        /// The number of milliseconds between checks on every connection to ensure that they are still
        /// in the ESTABLISHED state.
        /// </summary>
        public static readonly int EstablishedCheckTimeout = 20000;

        /// <summary>
        /// The number of milliseconds that the remote side has to send authentication before the connection is
        /// abandoned.
        /// </summary>
        public static readonly int AuthenticationTimeout = 10000;
        #endregion

        #region Fields
        /// <summary>
        /// True if the connector is in the closed state, false if it is in the open state.
        /// </summary>
        private bool _Closed = true;

        /// <summary>
        /// The object that decides whether incoming connections can be accepted.
        /// </summary>
        private IAccessFilter _AccessFilter;

        /// <summary>
        /// The heartbeat service that we hooked.
        /// </summary>
        private IHeartbeatService _HeartbeatService;

        /// <summary>
        /// The service that reports on the state of TCP connections for us.
        /// </summary>
        private ITcpConnectionStateService _TcpConnectionService;

        /// <summary>
        /// The date and time that the connections were checked for idle. This is a belt and braces check
        /// that operates independently of the socket timeouts. However, it is only used when keep-alive
        /// packets are not used.
        /// </summary>
        private DateTime _LastIdleCheck;

        /// <summary>
        /// The date and time that the connection states were last checked to ensure they were established.
        /// This is a belt and braces check that operates independently of both keep-alive and idle timeout
        /// checks. It is not configurable by the user.
        /// </summary>
        private DateTime _LastEstablishedCheck;
        #endregion

        #region Behavioural properties
        protected override bool PassiveModeSupported        { get { return true; } }
        protected override bool ActiveModeSupported         { get { return true; } }
        protected override bool SingleConnectionSupported   { get { return true; } }
        protected override bool MultiConnectionSupported    { get { return true; } }
        #endregion

        #region Properties
        /// <summary>
        /// See interface docs.
        /// </summary>
        public string Address { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int Port { get; set; }

        private bool _UseKeepAlive;
        /// <summary>
        /// See interface docs.
        /// </summary>
        public bool UseKeepAlive
        {
            get { return _UseKeepAlive; }
            set { _UseKeepAlive = _IsMono ? false : value; }
        }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public int IdleTimeout { get; set; }

        /// <summary>
        /// See interface docs.
        /// </summary>
        public Access Access { get; set; }
        #endregion

        #region Ctor
        /// <summary>
        /// Creates a new object.
        /// </summary>
        public NetworkConnector() : base()
        {
            UseKeepAlive = true;
            IdleTimeout = DefaultIdleTimeout;
            _LastIdleCheck = DateTime.UtcNow;
            _LastEstablishedCheck = _LastIdleCheck;

            _TcpConnectionService = Factory.Resolve<ITcpConnectionStateService>();
            _HeartbeatService = Factory.ResolveSingleton<IHeartbeatService>();
            _HeartbeatService.FastTick += HeartbeatService_FastTick ;
        }
        #endregion

        #region Dispose
        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if(disposing) {
                if(_HeartbeatService != null) _HeartbeatService.FastTick -= HeartbeatService_FastTick;
                _HeartbeatService = null;
            }
        }
        #endregion

        #region DoEstablishIntent
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoEstablishIntent()
        {
            if(IsPassive) {
                Intent = String.Format("Listen to port {0} for {1} connection{2}", Port, IsSingleConnection ? "single" : "many",  IsSingleConnection ? "" : "s");
            } else {
                Intent = String.Format("Establish connection with {0}:{1}", Address, Port);
            }
        }
        #endregion

        #region GetConnection
        /// <summary>
        /// Returns the connection that was current as-at the time the call is made.
        /// </summary>
        /// <returns></returns>
        private SocketConnection GetConnection()
        {
            return GetFirstConnection() as SocketConnection;
        }
        #endregion

        #region DoEstablishConnection, CloseConnection
        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoEstablishConnection()
        {
            _Closed = false;
            if(IsPassive) {
                PassiveModeStartListening();
            } else {
                ActiveModeReconnect(ConnectionStatus.Connecting, pauseBeforeConnect: false);
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        protected override void DoCloseConnection()
        {
            _Closed = true;
            WaitForEstablishConnectionThreadToFinish(DisconnectTimeout);
            AbandonAllConnections();
            ConnectionStatus = ConnectionStatus.Disconnected;
        }
        #endregion

        #region ActiveModeReconnect, ActiveModeConnect, ActiveModeDisconnect
        private bool _InActiveModeReconnect;
        /// <summary>
        /// Keeps attempting to connect in active mode until a connection is established.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="pauseBeforeConnect"></param>
        private void ActiveModeReconnect(ConnectionStatus status, bool pauseBeforeConnect)
        {
            var inActiveModeReconnect = false;
            lock(_SyncLock) {
                inActiveModeReconnect = _InActiveModeReconnect;
                if(!inActiveModeReconnect) _InActiveModeReconnect = true;
            }
            if(!inActiveModeReconnect) {
                try {
                    while(ConnectionStatus != ConnectionStatus.Connected && !_Closed) {
                        if(pauseBeforeConnect) {
                            PauseWhileTestingClosed(RetryConnectTimeout);
                        }
                        pauseBeforeConnect = true;

                        try {
                            ConnectionStatus = status;
                            ActiveModeConnect();
                        } catch(Exception ex) {
                            OnExceptionCaught(new EventArgs<Exception>(ex));
                        }
                    }
                } finally {
                    _InActiveModeReconnect = false;
                }
            }
        }

        /// <summary>
        /// Waits for the number of milliseconds passed across, but can return early if _Closed is set.
        /// </summary>
        /// <param name="pauseMilliseconds"></param>
        private void PauseWhileTestingClosed(int pauseMilliseconds)
        {
            const int microSleepMilliseconds = 100;

            var countSleeps = Math.Max(1, pauseMilliseconds / microSleepMilliseconds);
            for(var i = 0;i < countSleeps && !_Closed;++i) {
                Thread.Sleep(microSleepMilliseconds);
            }
        }

        /// <summary>
        /// Connects to the remote machine in active mode.
        /// </summary>
        private void ActiveModeConnect()
        {
            var ipAddress = ResolveAddress(Address);
            var endPoint = new IPEndPoint(ipAddress, Port);
            var socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Some operating systems may not support all options
            /*
            try {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);
            } catch {}
            try {
                socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
            } catch {}
             */

            SetSocketKeepAliveAndTimeouts(socket);

            using(var state = new SocketState(socket)) {
                socket.BeginConnect(endPoint, ActiveModeCompleteConnect, state);

                var timeout = ConnectTimeout;
                if(Authentication != null) timeout += AuthenticationTimeout;

                if(!state.WaitHandle.WaitOne(timeout)) {
                    try {
                        socket.Close(1000);
                    } catch { ; }
                    try {
                        ((IDisposable)socket).Dispose();
                    } catch { ; }
                }
            }
        }

        /// <summary>
        /// Called on a background thread when the socket established by <see cref="ActiveModeConnect"/>
        /// connects, or when something has gone wrong.
        /// </summary>
        /// <param name="asyncResult"></param>
        private void ActiveModeCompleteConnect(IAsyncResult asyncResult)
        {
            try {
                var state = (SocketState)asyncResult.AsyncState;
                var socket = state.Socket;
                var waitHandle = state.WaitHandle;

                try {
                    socket.EndConnect(asyncResult);

                    SocketConnection connection = new SocketConnection(this, socket, ConnectionStatus.Disconnected);
                    RegisterConnection(connection, raiseConnectionEstablished: true, mirrorConnectionState: true);
                    connection.ConnectionStatus = ConnectionStatus.Connected;

                    var authentication = Authentication;
                    if(authentication != null) {
                        var abandonConnection = true;
                        try {
                            SendAuthentication(connection, authentication);
                            RecordMiscellaneousActivity("Sent authentication");
                            abandonConnection = false;
                        } catch {
                            abandonConnection = true;
                        }

                        if(abandonConnection) {
                            connection.Abandon();
                        }
                    }
                } catch(ThreadAbortException) {
                    ;
                } catch(Exception ex) {
                    OnExceptionCaught(new EventArgs<Exception>(ex));
                } finally {
                    if(waitHandle != null) {
                        waitHandle.Set();
                    }
                }
            } catch(ThreadAbortException) {
                ;
            } catch {
                // Don't let exceptions bubble out of this
                ;
            }
        }
        #endregion

        #region PassiveModeStartListening, CreatePassiveModeListeningSocket, CreateNewPassiveConnection
        private bool _InPassiveModeStartListening;
        /// <summary>
        /// Starts the passive mode listener.
        /// </summary>
        private void PassiveModeStartListening()
        {
            var inPassiveModeStartListening = false;
            lock(_SyncLock) {
                inPassiveModeStartListening = _InPassiveModeStartListening;
                if(!inPassiveModeStartListening) _InPassiveModeStartListening = true;
            }

            if(!inPassiveModeStartListening) {
                ConnectionStatus = ConnectionStatus.Connecting;

                var access = Access;
                _AccessFilter = access == null ? null : Factory.Resolve<IAccessFilter>();
                if(_AccessFilter != null) {
                    _AccessFilter.Initialise(access);
                }

                Socket socket = null;
                try {
                    while(socket == null && !_Closed) {
                        try {
                            socket = CreatePassiveModeListeningSocket();
                            ConnectionStatus = ConnectionStatus.Waiting;
                            RecordMiscellaneousActivity("Listening for incoming connections on port {0}", Port);

                            while(!_Closed) {
                                using(var state = new SocketState(socket)) {
                                    socket.BeginAccept(PassiveModeAccept, state);
                                    while(!state.WaitHandle.WaitOne(250)) {
                                        if(_Closed) break;
                                    }
                                }
                            }
                        } catch(Exception ex) {
                            OnExceptionCaught(new EventArgs<Exception>(ex));
                        }

                        if(socket == null && !_Closed) {
                            PauseWhileTestingClosed(RetryConnectTimeout);
                        }
                    }
                } finally {
                    AbandonAllConnections();
                    if(socket != null) {
                        try {
                            socket.Close();
                            ((IDisposable)socket).Dispose();
                        } catch {
                            ;
                        }
                    }

                    _InPassiveModeStartListening = false;
                    ConnectionStatus = ConnectionStatus.Disconnected;
                }
            }
        }

        private void PassiveModeAccept(IAsyncResult asyncResult)
        {
            try {
                var state = (SocketState)asyncResult.AsyncState;
                var listeningSocket = state.Socket;
                var waitHandle = state.WaitHandle;

                try {
                    var socket = listeningSocket.EndAccept(asyncResult);
                    CreateNewPassiveConnection(socket);
                } catch(ThreadAbortException) {
                    ;
                } catch(Exception ex) {
                    OnExceptionCaught(new EventArgs<Exception>(ex));
                } finally {
                    if(waitHandle != null) {
                        waitHandle.Set();
                    }
                }
            } catch {
                ; // Do not allow exceptions to bubble out of this
            }
        }

        /// <summary>
        /// Creates a listening socket.
        /// </summary>
        /// <returns></returns>
        private Socket CreatePassiveModeListeningSocket()
        {
            var result = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            var endPoint = new IPEndPoint(IPAddress.Any, Port);
            result.Bind(endPoint);
            result.Listen(200);

            return result;
        }

        /// <summary>
        /// Creates a socket for an incoming connection.
        /// </summary>
        /// <param name="socket"></param>
        private void CreateNewPassiveConnection(Socket socket)
        {
            if(socket != null) {
                var connection = new SocketConnection(this, socket, ConnectionStatus.Waiting);
                var address = socket.RemoteEndPoint as IPEndPoint;

                var abandonConnection = false;
                if(address == null || address.Address == null)                          abandonConnection = true;
                else if(IsSingleConnection && GetConnections().Length != 0)             abandonConnection = true;
                if(!abandonConnection && _AccessFilter != null && !_AccessFilter.Allow(address.Address)) {
                    abandonConnection = true;
                    RecordMiscellaneousActivity("Rejected connection from {0}, did not match access rules", address);
                }

                if(!abandonConnection) {
                    try {
                        SetSocketKeepAliveAndTimeouts(socket);
                    } catch(Exception ex) {
                        abandonConnection = true;
                        OnExceptionCaught(new EventArgs<Exception>(ex));
                        RecordMiscellaneousActivity("Abandoning incoming passive connection due to previous exception");
                    }
                }

                var authentication = Authentication;
                if(!abandonConnection && authentication != null) {
                    var authenticationResponse = GetAuthenticationResponse(connection, authentication);
                    abandonConnection = !authentication.GetResponseIsValid(authenticationResponse);
                    if(abandonConnection) {
                        RecordMiscellaneousActivity("Rejecting connection from {0}, authentication failed or was not completed within {1} seconds", address, AuthenticationTimeout / 1000);
                    }
                }

                if(abandonConnection) {
                    connection.Abandon();
                } else {
                    RegisterConnection(connection, raiseConnectionEstablished: true, mirrorConnectionState: false);
                    connection.ConnectionStatus = ConnectionStatus.Connected;
                    ConnectionStatus = ConnectionStatus.Connected;
                }
            }
        }
        #endregion

        #region SendAuthentication, GetAuthenticationResponse
        /// <summary>
        /// Send the bytes over the connection.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="authentication"></param>
        private void SendAuthentication(SocketConnection connection, IConnectorAuthentication authentication)
        {
            var bytes = authentication.SendAuthentication();
            var socket = connection.Socket;
            if(socket != null) {
                socket.Send(bytes);
            }
        }

        /// <summary>
        /// Reads bytes off the connection until the authentication object says we've got enough.
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="authentication"></param>
        /// <returns></returns>
        private byte[] GetAuthenticationResponse(SocketConnection connection, IConnectorAuthentication authentication)
        {
            var result = new byte[0];

            var readBuffer = new byte[512];
            var finished = false;
            while(!finished) {
                var socket = connection.Socket;
                var bytesRead = socket == null ? 0 : socket.Receive(readBuffer);
                finished = bytesRead == 0;
                if(!finished) {
                    var offset = result.Length;
                    Array.Resize(ref result, result.Length + bytesRead);
                    finished = result.Length > authentication.MaximumResponseLength;
                    if(!finished) {
                        Array.ConstrainedCopy(readBuffer, 0, result, offset, bytesRead);
                        finished = authentication.GetResponseIsComplete(result);
                    }
                }
            }

            return result;
        }
        #endregion

        #region AbandonAllConnections, ConnectionAbandoned
        /// <summary>
        /// Closes all connections. Works for both active and passive modes.
        /// </summary>
        private void AbandonAllConnections()
        {
            if(!_Closed) _Closed = true;

            foreach(var connection in GetConnections()) {
                connection.Abandon();
            }
        }

        /// <summary>
        /// See base docs.
        /// </summary>
        /// <param name="connection"></param>
        internal override void ConnectionAbandoned(IConnection connection)
        {
            DeregisterConnection(connection, raiseConnectionClosed: true, stopMirroringConnectionState: !IsPassive);

            if(!_Closed) {
                if(!IsPassive) ActiveModeReconnect(ConnectionStatus.Reconnecting, pauseBeforeConnect: true);
                else           ConnectionStatus = ConnectionStatus.Waiting;
            }
        }
        #endregion

        #region Helper methods - ResolveAddress, SetSocketKeepAliveAndTimeouts
        /// <summary>
        /// Resolves the address passed across.
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        private static IPAddress ResolveAddress(string address)
        {
            var ipAddresses = Dns.GetHostAddresses(address);
            var result = ipAddresses == null || ipAddresses.Length == 0 ? null : ipAddresses[0];
            if(result == null) {
                throw new InvalidOperationException($"Cannot resolve {address}");
            }

            return result;
        }

        /// <summary>
        /// Sets the keep-alive and timeout options on the socket.
        /// </summary>
        /// <param name="socket"></param>
        private void SetSocketKeepAliveAndTimeouts(Socket socket)
        {
            if(!UseKeepAlive) {
                socket.ReceiveTimeout = IdleTimeout;
                socket.SendTimeout = IdleTimeout;
            } else {
                socket.IOControl(IOControlCode.KeepAliveValues, new TcpKeepAlive() {
                    OnOff = 1,
                    KeepAliveTime = 10 * 1000,
                    KeepAliveInterval = 1 * 1000
                }.BuildBuffer(), null);
            }
        }
        #endregion

        #region Broken connection checks
        /// <summary>
        /// A belt-and-braces check that runs alongside the Socket library timeouts to
        /// ensure that idle connections are disconnected if nothing happens on them
        /// for a given period of time.
        /// </summary>
        /// <param name="now"></param>
        private void TestAndAbandonIdleConnections(DateTime now)
        {
            var threshold = _LastIdleCheck.AddMilliseconds(IdleTimeout);
            if(now >= threshold) {
                _LastIdleCheck = now;

                foreach(SocketConnection connection in GetConnections()) {
                    if(connection.IsIdle(now)) {
                        RecordMiscellaneousActivity("Background thread check has determined that connection {0} is idle - abandoning it", connection.Description ?? "<ANONYMOUS>");
                        connection.Abandon();
                    }
                }
            }
        }

        /// <summary>
        /// Checks that every connection that has been up for a reasonable period
        /// of time is in the ESTABLISHED state. If any are found that are not
        /// then they are abandoned.
        /// </summary>
        /// <param name="now"></param>
        private void TestAndAbandonBadStateConnections(DateTime now)
        {
            var threshold = _LastEstablishedCheck.AddMilliseconds(EstablishedCheckTimeout);
            if(now >= threshold) {
                _LastEstablishedCheck = now;
                _TcpConnectionService.RefreshTcpConnectionStates();

                foreach(SocketConnection connection in GetConnections()) {
                    var connectionThreshold = connection.Created.AddMilliseconds(EstablishedCheckTimeout);
                    if(now >= connectionThreshold) {
                        if(!_TcpConnectionService.IsRemoteConnectionEstablished(connection.LocalEndPoint, connection.RemoteEndPoint)) {
                            RecordMiscellaneousActivity("Abandoning connection {0} as it is no longer in the ESTABLISHED state", connection.Description ?? "<ANONYMOUS>");
                            connection.Abandon();
                        }
                    }
                }
            }
        }
        #endregion

        #region Event handlers
        /// <summary>
        /// Called roughly once a second.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void HeartbeatService_FastTick(object sender, EventArgs e)
        {
            try {
                var now = DateTime.UtcNow;

                if(!UseKeepAlive && IdleTimeout > 0) {
                    TestAndAbandonIdleConnections(now);
                }

                TestAndAbandonBadStateConnections(now);
            } catch(Exception ex) {
                OnExceptionCaught(new EventArgs<Exception>(ex));
            }
        }
        #endregion
    }
}
