namespace AzXmpp.Xmpp
{
    /// <summary>
    /// Represents the different XMPP error codes.
    /// </summary>
    public enum XmppErrorCode
    {
        /// <summary>
        /// The error condition is not one of those defined by the other conditions in this list.
        /// </summary>
        Unknown,
        /// <summary>
        /// The stream was not recognized.
        /// </summary>
        UnrecognizedStream,
        /// <summary>
        /// The entity has sent XML that cannot be processed.
        /// </summary>
        BadFormat,
        /// <summary>
        /// The entity has sent a namespace prefix that is unsupported.
        /// </summary>
        BadNamespacePrefix,
        /// <summary>
        /// The server is closing the active stream for this entity because a new stream has been initiated that conflicts with the existing stream. 
        /// </summary>
        Conflict,
        /// <summary>
        /// The entity has not generated any traffic over the stream for some period of time. 
        /// </summary>
        ConnectionTimeout,
        /// <summary>
        /// The value of the 'to' attribute provided by the initiating entity in the stream header corresponds to a hostname that is no longer hosted by the server.
        /// </summary>
        HostGone,
        /// <summary>
        /// The value of the 'to' attribute provided by the initiating entity in the stream header does not correspond to a hostname that is hosted by the server.
        /// </summary>
        HostUnknown,
        /// <summary>
        /// A stanza sent between two servers lacks a 'to' or 'from' attribute (or the attribute has no value).
        /// </summary>
        ImproperAddressing,
        /// <summary>
        /// The server has experienced a misconfiguration or an otherwise-undefined internal error that prevents it from servicing the stream. 
        /// </summary>
        InternalServerError,
        /// <summary>
        /// The JID or hostname provided in a 'from' address does not match an authorized JID.
        /// </summary>
        InvalidFrom,
        /// <summary>
        /// The stream ID or dialback ID is invalid or does not match an ID previously provided.
        /// </summary>
        InvalidID,
        /// <summary>
        /// The streams namespace name is something other than the required standard namespace.
        /// </summary>
        InvalidNamespace,
        /// <summary>
        /// The entity has sent invalid XML over the stream to a server that performs validation.
        /// </summary>
        InvalidXml,
        /// <summary>
        /// The entity has attempted to send data before the stream has been authenticated, or otherwise is not authorized to perform an action related to stream negotiation. 
        /// </summary>
        NotAuthorized,
        /// <summary>
        /// The entity has violated some local service policy.
        /// </summary>
        PolicyViolation,
        /// <summary>
        /// The server is unable to properly connect to a remote entity that is required for authentication or authorization. 
        /// </summary>
        RemoteConnectionFailed,
        /// <summary>
        /// The server lacks the system resources necessary to service the stream.
        /// </summary>
        ResourceConstraint,
        /// <summary>
        /// The entity has attempted to send restricted XML features.
        /// </summary>
        RestrictedXml,
        /// <summary>
        /// The server will not provide service to the initiating entity but is redirecting traffic to another host.
        /// </summary>
        SeeOtherHost,
        /// <summary>
        /// The server is being shut down and all active streams are being closed.
        /// </summary>
        SystemShutdown,
        /// <summary>
        /// The initiating entity has encoded the stream in an encoding that is not supported by the server.
        /// </summary>
        UnsupportedEncoding,
        /// <summary>
        /// The initiating entity has sent a first-level child of the stream that is not supported by the server. 
        /// </summary>
        UnsupportedStanzaType,
        /// <summary>
        /// The value of the 'version' attribute provided by the initiating entity in the stream header specifies a version of XMPP that is not supported by the server.
        /// </summary>
        UnsupportedVersion,
        /// <summary>
        /// The initiating entity has sent XML that is not well-formed.
        /// </summary>
        XmlNotWellFormed,
        /// <summary>
        /// The required entity has sent a required feature that is not supported.
        /// </summary>
        RequiredFeatureUnknown,
        /// <summary>
        /// Authentication with the host failed.
        /// </summary>
        AuthenticationFailed,
        /// <summary>
        /// Authentication was aborted.
        /// </summary>
        AuthenticationAborted,
        /// <summary>
        /// The account is disabled.
        /// </summary>
        AccountDisabled,
        /// <summary>
        /// The credentials have expired.
        /// </summary>
        CredentialsExpired,
        /// <summary>
        /// Encryption is required.
        /// </summary>
        EncryptionRequired,
        /// <summary>
        /// The impersonation identity is incorrect.
        /// </summary>
        InvalidImpersonation,
        /// <summary>
        /// The authentication mechanism is invalid.
        /// </summary>
        InvalidMechanism,
        /// <summary>
        /// The request is malformed.
        /// </summary>
        MalformedRequest,
        /// <summary>
        /// The authentication mechanism is too weak.
        /// </summary>
        MechanismTooWeak,
        /// <summary>
        /// Authentication is temporarily unavailable.
        /// </summary>
        TemporaryAuthFailure,
        /// <summary>
        /// Negotiating TLS failed.
        /// </summary>
        TlsFailure,
        /// <summary>
        /// A stanza error occurred.
        /// </summary>
        Stanza
    }
}
