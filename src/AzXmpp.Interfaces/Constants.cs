namespace AzXmpp
{
    /// <summary>
    /// Represents application constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Gets the application name.
        /// </summary>
        public const string ApplicationName = "fabric:/AzXmpp";

        /// <summary>
        /// Represents the service names for the actors.
        /// </summary>
        public static class ActorNames
        {
            /// <summary>
            /// Gets the actor name for an unbound client.
            /// </summary>
            public const string UnboundClient = "UnboundClient";
        }

        public static class NamespaceUri
        {
            /// <summary>
            /// The namespace for stanza-related data contracts.
            /// </summary>
            public const string Stanza = "urn:azxmpp:stanza";

            /// <summary>
            /// The namespace for specification-relation data contracts.
            /// </summary>
            public const string Spec = "urn:azxmpp:rfc";
        }
    }
}
