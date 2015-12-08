using System;
using System.Threading;
using System.Threading.Tasks;
using AzXmpp.Transport.Sockets;
using Microsoft.ServiceFabric.Services;

namespace AzXmpp.Transport
{
    public class TcpTransport : StatelessService
    {
        private TcpCommunicationListener _listener;

        protected override ICommunicationListener CreateCommunicationListener()
        {
            return _listener = new TcpCommunicationListener(this, "ServiceEndpoint");
        }

        protected override async Task RunAsync(CancellationToken cancellationToken)
        {
            ServiceEventSource.Current.ServiceRunning();
            while (!cancellationToken.IsCancellationRequested)
            {
                ISocket socket;
                try
                {
                    socket = await _listener.AcceptAsync(cancellationToken);
                }
                catch
                {
                    continue;
                }

                try
                {
                    var client = new XmlClient(socket, cancellationToken);
                    ThreadPool.QueueUserWorkItem(cli => ((XmlClient)cli).Open(), client);
                }
                catch (Exception e)
                {
                    ServiceEventSource.Current.ClientConnectionFailure(socket.Identifier, e.ToString());
                }
            }
        }
    }
}
