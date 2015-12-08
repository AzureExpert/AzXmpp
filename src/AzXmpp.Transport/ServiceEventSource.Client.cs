using System.Diagnostics.Tracing;

namespace AzXmpp.Transport
{
    internal sealed partial class ServiceEventSource
    {
        [Event(100, Level = EventLevel.Error, Message = "Client {0} failed to connect: {1}.")]
        public void ClientConnectionFailure(string client, string exception)
        {
            WriteEvent(100, client, exception);
        }
    }
}
