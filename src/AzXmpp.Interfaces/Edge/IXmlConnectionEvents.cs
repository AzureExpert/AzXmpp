using Microsoft.ServiceFabric.Actors;

namespace AzXmpp.Edge
{
    /// <summary>
    /// Represents the events an XML connection can raise.
    /// </summary>
    public interface IXmlConnectionEvents : IActorEvents
    {
        /// <summary>
        /// Writes the specified stanza to the connection.
        /// </summary>
        /// <param name="stanza">The stanza.</param>
        void WriteStanza(Stanza stanza);
    }
}
