using System;
using System.Fabric;
using System.Fabric.Description;
using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using AzXmpp.Transport.Sockets;
using Microsoft.ServiceFabric.Services;

namespace AzXmpp.Transport
{
    /// <summary>
    /// Represents a TCP listener.
    /// </summary>
    internal class TcpCommunicationListener : IServiceCommunicationListener
    {
        private readonly TcpTransport _implementation;
        private readonly string _endpointName;

        private ServiceInitializationParameters _serviceInitializationParameters;
        private EndpointResourceDescription _endpoint;

        private ISocket _socket;

        /// <summary>
        /// Initializes a new instance of the <see cref="TcpCommunicationListener"/> class.
        /// </summary>
        /// <param name="implementation">The implementation.</param>
        /// <param name="endpointName">Name of the endpoint.</param>
        public TcpCommunicationListener(TcpTransport implementation, string endpointName)
        {
            _implementation = implementation;
            _endpointName = endpointName;
        }

        /// <summary>
        /// Initializes the listener.
        /// </summary>
        /// <param name="serviceInitializationParameters">The service initialization parameters.</param>
        /// <exception cref="System.InvalidOperationException">
        /// </exception>
        public void Initialize(ServiceInitializationParameters serviceInitializationParameters)
        {
            _serviceInitializationParameters = serviceInitializationParameters;
            _endpoint = _serviceInitializationParameters.CodePackageActivationContext.GetEndpoint(_endpointName);

            if (_endpoint == null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.ArgumentOutOfRange_EndpointDoesNotExist));
            }
            else if (_endpoint.Protocol != EndpointProtocol.Tcp)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.ArgumentOutOfRange_EndpointNotTCP));
            }
        }

        /// <summary>
        /// Asynchronously starts listening.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="Task{System.String}"/> that represents the asynchronous open operation.
        /// </returns>
        public Task<string> OpenAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            var socket = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.Tcp);
            socket.DualMode = true;

            socket.Bind(new IPEndPoint(IPAddress.IPv6Any, _endpoint.Port));
            socket.Listen(100);

            _socket = new NativeSocket(socket);

            return Task.FromResult(_socket.Identifier);
        }

        /// <summary>
        /// Asynchronously accepts a single socket connection.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="Task{ISocket}"/> that represents the asynchronous accept operation.
        /// </returns>
        public Task<ISocket> AcceptAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return AcceptSocketAsyncEventArgs.CheckOut().AcceptAsync(_socket, cancellationToken);
        }

        /// <summary>
        /// Aborts the listener.
        /// </summary>
        public void Abort()
        {
            _socket.Dispose();
        }

        /// <summary>
        /// Asynchronously closes the listener, waiting for any pending operations.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="System.Threading.CancellationToken.None" />.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous close operation.
        /// </returns>
        public Task CloseAsync(CancellationToken cancellationToken)
        {
            Abort();
            return Task.FromResult(0);
        }
    }
}
