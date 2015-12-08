using System.Xml.Linq;

namespace AzXmpp.Edge
{
    internal static class Xmlns
    {
        public static class Attr
        {
            public static readonly XName Id = "id";
            public static readonly XName Type = "type";
            public static readonly XName Version = "version";
            public static readonly XName To = "to";
        }

        public static class Streams
        {
            public static readonly XNamespace Namespace = "http://etherx.jabber.org/streams";
            public static readonly XName Stream = Namespace + "stream";
            public static readonly XName Error = Namespace + "error";
            public static readonly XName Features = Namespace + "features";
        }

        public static class Client
        {
            public static readonly XNamespace Namespace = "jabber:client";
            public static readonly XName Iq = Namespace + "iq";
        }

        public static class IetfStreams
        {
            public static readonly XNamespace Namespace = "urn:ietf:params:xml:ns:xmpp-streams";
            public static readonly XName Text = Namespace + "text";
        }

        public static class IetfTls
        {
            public static readonly XNamespace Namespace = "urn:ietf:params:xml:ns:xmpp-tls";
            public static readonly XName StartTls = Namespace + "starttls";
            public static readonly XName Proceed = Namespace + "proceed";
            public static readonly XName Failure = Namespace + "failure";
            public static readonly XName Required = Namespace + "required";
        }

        public static class IetfSasl
        {
            public static readonly XNamespace Namespace = "urn:ietf:params:xml:ns:xmpp-sasl";
            public static readonly XName Mechanisms = Namespace + "mechanisms";
            public static readonly XName Mechanism = Namespace + "mechanism";
            public static readonly XName Auth = Namespace + "auth";
            public static readonly XName Challenge = Namespace + "challenge";
            public static readonly XName Response = Namespace + "response";
            public static readonly XName Failure = Namespace + "failure";
            public static readonly XName Success = Namespace + "success";
            public static readonly XName Abort = Namespace + "abort";
            public static readonly XName Text = Namespace + "text";
        }

        public static class IetfBind
        {
            public static readonly XNamespace Namespace = "urn:ietf:params:xml:ns:xmpp-bind";
            public static readonly XName Bind = Namespace + "bind";
            public static readonly XName Jid = Namespace + "jid";
        }

        public static class IetfStanzas
        {
            public static readonly XNamespace Namespace = "urn:ietf:params:xml:ns:xmpp-stanzas";
            public static readonly XName Unknown = Namespace + "unknown";
            public static readonly XName Text = Namespace + "text";
            public static readonly XName Error = Namespace + "error";
        }
    }
}
