using System.Runtime.Serialization;

namespace AzXmpp.Edge
{
    /// <summary>
    /// Represents the result of performing a stream action.
    /// </summary>
    [DataContract(Name = "result", Namespace = Constants.NamespaceUri.Stanza)]
    public class StanzaResult
    {
        /// <summary>
        /// Gets action that should be performed against the stream.
        /// </summary>
        /// <value>
        /// The action that should be performed against the stream.
        /// </value>
        [DataMember(Name = "a", Order = 0)]
        public StreamAction StreamAction { get; private set; }

        /// <summary>
        /// Gets the response stanza.
        /// </summary>
        /// <value>
        /// The response stanza.
        /// </value>
        [DataMember(Name = "r", Order = 1)]
        public Stanza Response { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StanzaResult"/> class.
        /// </summary>
        public StanzaResult()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StanzaResult" /> class.
        /// </summary>
        /// <param name="response">The response stanza.</param>
        public StanzaResult(Stanza response)
        {
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StanzaResult"/> class.
        /// </summary>
        /// <param name="streamAction">The action to perform on the stream after sending the stanza.</param>
        /// <param name="response">The response to send on the connection.</param>
        public StanzaResult(StreamAction streamAction, Stanza response)
        {
            StreamAction = streamAction;
            Response = response;
        }
    }
}
