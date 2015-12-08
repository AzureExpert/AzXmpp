using System.Xml.Serialization;

namespace AzXmpp.Edge
{
    /// <summary>
    /// Represents the actions that can be performed against a stream.
    /// </summary>
    public enum StreamAction
    {
        /// <summary>
        /// Continues normal processing on the stream.
        /// </summary>
        [XmlEnum("go")]
        Continue = 0,
        /// <summary>
        /// Closes the connection after sending pending data.
        /// </summary>
        [XmlEnum("close")]
        Close = 1,
        /// <summary>
        /// Aborts the connection without sending pending data.
        /// </summary>
        [XmlEnum("abort")]
        Abort = 2,
        /// <summary>
        /// Resets the XML stream.
        /// </summary>
        [XmlEnum("reset")]
        Reset = 3,
        /// <summary>
        /// Initiates a TLS session on the connection.
        /// </summary>
        [XmlEnum("tls")]
        StartTls = 101
    }
}