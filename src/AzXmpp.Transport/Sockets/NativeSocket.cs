using System;
using System.Fabric;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace AzXmpp.Transport.Sockets
{
    /// <summary>
    /// Represents a <see cref="ISocket"/> that invokes a <see cref="Socket"/> directly.
    /// </summary>
    sealed class NativeSocket : ISocket
    {
        private readonly Socket _socket;
        private readonly string _identifier;
        private volatile int _isDisposed;

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="ReceiveAsync(DataSocketAsyncEventArgs)" /> call will time out.
        /// </summary>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public int ReceiveTimeout
        {
            get { return _socket.ReceiveTimeout; }

            set { _socket.ReceiveTimeout = value; }
        }

        /// <summary>
        /// Gets or sets a value that specifies the amount of time after which a synchronous <see cref="SendAsync(DataSocketAsyncEventArgs)" /> call will time out.
        /// </summary>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public int SendTimeout
        {
            get { return _socket.SendTimeout; }

            set { _socket.SendTimeout = value; }
        }

        /// <summary>Gets or sets a <see cref="T:System.Boolean" /> value that specifies whether the stream <see cref="T:System.Net.Sockets.Socket" /> is using the Nagle algorithm.</summary>
        /// <returns>false if the <see cref="T:System.Net.Sockets.Socket" /> uses the Nagle algorithm; otherwise, true. The default is false.</returns>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the <see cref="T:System.Net.Sockets.Socket" />. See the Remarks section for more information. </exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        /// </PermissionSet>
        public bool NoDelay
        {
            get { return _socket.NoDelay; }
            set { _socket.NoDelay = value; }
        }

        /// <summary>Gets the unique identifier for the socket.</summary>
        /// <value>The unique identifier for the socket.</value>
        public string Identifier
        {
            get { return _identifier; }
        }

        /// <summary>
        /// Gets a value indicating whether this socket is connected.
        /// </summary>
        /// <value>
        /// <c>true</c> if this socket is connected; otherwise, <c>false</c>.
        /// </value>
        public bool IsConnected
        {
            get { return _socket.Connected; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NativeSocket"/> class.
        /// </summary>
        /// <param name="socket">The socket.</param>
        public NativeSocket(Socket socket)
        {
            _socket = socket;
            _socket.UseOnlyOverlappedIO = true;
            _socket.Blocking = false;

            IPEndPoint endpoint = null;
            if (_socket.Connected && _socket.RemoteEndPoint != null)
            {
                endpoint = _socket.RemoteEndPoint as IPEndPoint;
                if (endpoint == null)
                {
                    _identifier = string.Format(CultureInfo.InvariantCulture, "net://{0}/", _socket.RemoteEndPoint);
                }
                else
                {
                    _identifier = string.Format(CultureInfo.InvariantCulture,
                        "tcp://{0}:{1}/",
                        FabricRuntime.GetNodeContext().IPAddressOrFQDN,
                        endpoint.Port);
                }
            }
            else if (_socket.IsBound)
            {
                endpoint = _socket.LocalEndPoint as IPEndPoint;
                if (endpoint == null)
                {
                    _identifier = string.Format(CultureInfo.InvariantCulture, "net://{0}/", _socket.LocalEndPoint);
                }
                else
                {
                    _identifier = string.Format(CultureInfo.InvariantCulture,
                        "tcp://{0}:{1}/",
                        FabricRuntime.GetNodeContext().IPAddressOrFQDN,
                        endpoint.Port);
                }
            }
            else
            {
                _identifier = "net://?/";
            }
        }

        /// <summary>Begins an asynchronous operation to accept an incoming connection attempt.</summary>
        /// <returns>Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation.Returns false if the I/O operation completed synchronously. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.</returns>
        /// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
        /// <exception cref="T:System.ArgumentException">An argument is not valid. This exception occurs if the buffer provided is not large enough. The buffer must be at least 2 * (sizeof(SOCKADDR_STORAGE + 16) bytes. This exception also occurs if multiple buffers are specified, the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.BufferList" /> property is not null.</exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">An argument is out of range. The exception occurs if the <see cref="P:System.Net.Sockets.SocketAsyncEventArgs.Count" /> is less than 0.</exception>
        /// <exception cref="T:System.InvalidOperationException">An invalid operation was requested. This exception occurs if the accepting <see cref="T:System.Net.Sockets.Socket" /> is not listening for connections or the accepted socket is bound. You must call the <see cref="M:System.Net.Sockets.Socket.Bind(System.Net.EndPoint)" /> and <see cref="M:System.Net.Sockets.Socket.Listen(System.Int32)" /> method before calling the <see cref="M:System.Net.Sockets.Socket.AcceptAsync(System.Net.Sockets.SocketAsyncEventArgs)" /> method.This exception also occurs if the socket is already connected or a socket operation was already in progress using the specified <paramref name="e" /> parameter. </exception>
        /// <exception cref="T:System.Net.Sockets.SocketException">An error occurred when attempting to access the socket. See the Remarks section for more information. </exception>
        /// <exception cref="T:System.NotSupportedException">Windows XP or later is required for this method.</exception>
        /// <exception cref="T:System.ObjectDisposedException">The <see cref="T:System.Net.Sockets.Socket" /> has been closed. </exception>
        public bool AcceptAsync(AcceptSocketAsyncEventArgs e)
        {
            return _socket.AcceptAsync(e);
        }

        /// <summary>
        /// Begins an asynchronous request to receive data from a connected <see cref="T:System.Net.Sockets.Socket" /> object.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        public bool ReceiveAsync(DataSocketAsyncEventArgs e)
        {
            return _socket.ReceiveAsync(e);
        }

        /// <summary>
        /// Sends data asynchronously to a connected <see cref="T:System.Net.Sockets.Socket" /> object.
        /// </summary>
        /// <param name="e">The <see cref="T:System.Net.Sockets.SocketAsyncEventArgs" /> object to use for this asynchronous socket operation.</param>
        /// <returns>
        /// Returns true if the I/O operation is pending. The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will be raised upon completion of the operation. Returns false if the I/O operation completed synchronously. In this case, The <see cref="E:System.Net.Sockets.SocketAsyncEventArgs.Completed" /> event on the <paramref name="e" /> parameter will not be raised and the <paramref name="e" /> object passed as a parameter may be examined immediately after the method call returns to retrieve the result of the operation.
        /// </returns>
        public bool SendAsync(DataSocketAsyncEventArgs e)
        {
            return _socket.SendAsync(e);
        }

        /// <summary>
        /// Disables sends and receives on a <see cref="T:System.Net.Sockets.Socket" />.
        /// </summary>
        /// <param name="how">One of the <see cref="T:System.Net.Sockets.SocketShutdown" /> values that specifies the operation that will no longer be allowed.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        public void Shutdown(SocketShutdown how)
        {
            _socket.Shutdown(how);
        }

        /// <summary>
        /// Closes the <see cref="T:System.Net.Sockets.Socket" /> connection and releases all associated resources with a specified timeout to allow queued data to be sent.
        /// </summary>
        /// <param name="timeout">Wait up to <paramref name="timeout" /> seconds to send any remaining data, then close the socket.</param>
        /// <PermissionSet>
        ///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
        ///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
        /// </PermissionSet>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void Close(int timeout)
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0 && _socket.Handle != IntPtr.Zero)
            {
                try
                {
                    _socket.LingerState = new LingerOption(true, 0);
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch (Exception e)
                {
                    if (e is ObjectDisposedException) return;
                }

                try
                {
                    // Socket.Close calls Socket.Dispose.
                    _socket.Close(timeout);
                }
                catch (Exception) { }
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Close(0);
        }
    }
}
