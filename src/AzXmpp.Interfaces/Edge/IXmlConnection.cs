using System;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Actors;

namespace AzXmpp.Edge
{
    /// <summary>
    /// Represents any type of client.
    /// </summary>
    public interface IXmlConnection : IActor, IActorEventPublisher<IXmlConnectionEvents>
    {
        /// <summary>
        /// Asynchronously handles the stream starting or resetting.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous stream reset operation.
        /// </returns>
        Task OnStreamResetAsync();

        /// <summary>
        /// Asynchronously handles a stanza in the stream.
        /// </summary>
        /// <param name="stanza">The stanza.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous stanza received operation.
        /// </returns>
        Task<StanzaResult> OnStanzaReceivedAsync(Stanza stanza);

        /// <summary>
        /// Asynchronously handles an error.
        /// </summary>
        /// <param name="error">The error.</param>
        /// <returns>
        /// A <see cref="Task{StanzaResult}"/> that represents the asynchronous error occurred operation.
        /// </returns>
        Task<StanzaResult> OnErrorOccurredAsync(Exception error);
    }
}
