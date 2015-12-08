
using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AzXmpp.Edge.Xmpp
{
    /// <summary>
    /// Represents a Jabber identifier.
    /// </summary>
    [DataContract(Name = "jid", Namespace = Constants.NamespaceUri.Spec)]
    public struct Jid : IFormattable, IDeserializationCallback
    {
        [IgnoreDataMember]
        private int _hashCode;
        [DataMember(Name = "node", Order = 0)]
        private string _node;
        [DataMember(Name = "domain", Order = 1)]
        private string _domain;
        [DataMember(Name = "res", Order = 2)]
        private string _resource;
        [IgnoreDataMember]
        private string _full;

        /// <summary>
        /// Gets the node of the JID.
        /// </summary>
        /// <value>
        /// The node of the JID.
        /// </value>
        public string Node
        {
            get { return _node; }
        }

        /// <summary>
        /// Gets the domain of the JID.
        /// </summary>
        /// <value>
        /// The domain of the JID.
        /// </value>
        public string Domain
        {
            get { return _domain; }
        }

        /// <summary>
        /// Gets the resource of the JID.
        /// </summary>
        /// <value>
        /// The resource of the JID.
        /// </value>
        public string Resource
        {
            get { return _resource; }
        }

        /// <summary>
        /// Gets the bare JID (node and domain).
        /// </summary>
        /// <value>
        /// The bare JID (node and domain).
        /// </value>
        public string Bare
        {
            get { return ToString("b"); }
        }

        /// <summary>
        /// Gets the full JID (node, domain and resource).
        /// </summary>
        /// <value>
        /// The  full JID (node, domain and resource).
        /// </value>
        public string Full
        {
            get { return _full; }
        }

        /// <summary>
        /// Gets a value indicating whether this JID is empty.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this JID is empty; otherwise, <c>false</c>.
        /// </value>
        public bool IsEmpty
        {
            get { return _full == null; }
        }

        /// <summary>
        /// Gets a value indicating whether this value is a domain JID.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this value is a domain JID; otherwise, <c>false</c>.
        /// </value>
        public bool IsDomain
        {
            get { return _full != null && _domain != null && _node == null; }
        }

        /// <summary>
        /// Gets a value indicating whether this value is a full JID.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this value is a full JID; otherwise, <c>false</c>.
        /// </value>
        public bool IsFull
        {
            get { return _full != null && _domain != null && _node == null && _resource != null; }
        }

        /// <summary>
        /// Gets a value indicating whether this value is a bare JID.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this value is a bare JID; otherwise, <c>false</c>.
        /// </value>
        public bool IsBare
        {
            get { return _full != null && _domain != null && _node == null && _resource == null; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Jid" /> struct.
        /// </summary>
        /// <param name="domain">The domain.</param>
        public Jid(string domain)
        {
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));
            if (domain.Any(IsInvalid)) throw new ArgumentOutOfRangeException(nameof(domain));

            _node = null;
            _domain = _full = domain;
            _resource = null;
            _hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(_full);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Jid"/> struct.
        /// </summary>
        /// <param name="node">The node (user).</param>
        /// <param name="domain">The domain.</param>
        public Jid(string node, string domain)
        {
            if (string.IsNullOrEmpty(node)) throw new ArgumentNullException(nameof(node));
            if (node.Any(IsInvalid)) throw new ArgumentOutOfRangeException(nameof(node));
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));
            if (domain.Any(IsInvalid)) throw new ArgumentOutOfRangeException(nameof(domain));

            _node = node;
            _domain = domain;
            _resource = null;
            _full = null;
            _hashCode = 0;

            _full = ToBestJid();
            _hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(_full);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Jid"/> struct.
        /// </summary>
        /// <param name="node">The node (user).</param>
        /// <param name="domain">The domain.</param>
        public Jid(string node, string domain, string resource)
        {
            if (string.IsNullOrEmpty(node)) throw new ArgumentNullException(nameof(node));
            if (node.Any(IsInvalid)) throw new ArgumentOutOfRangeException(nameof(node));
            if (string.IsNullOrEmpty(domain)) throw new ArgumentNullException(nameof(domain));
            if (domain.Any(IsInvalid)) throw new ArgumentOutOfRangeException(nameof(domain));
            if (!string.IsNullOrEmpty(resource) && resource.Any(IsInvalid))
                throw new ArgumentOutOfRangeException(nameof(resource));

            _node = node;
            _domain = domain;
            _resource = resource;
            _full = null;
            _hashCode = 0;

            _full = ToBestJid();
            _hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(_full);
        }

        /// <summary>
        /// Converts the string representation of a JID to the parsed JID.
        /// </summary>
        /// <param name="value">A string containing a JID to convert.</param>
        /// <returns>The parsed JID.</returns>
        public static Jid Parse(string value)
        {
            Jid result;
            if (!TryParse(value, out result))
                throw new FormatException();
            return result;
        }

        /// <summary>
        /// Converts the string representation of a JID to the parsed JID.
        /// A return value indicates whether the conversion succeeded. 
        /// </summary>
        /// <param name="value">A string containing a JID to convert.</param>
        /// <param name="result">
        /// When this method returns, contains the parsed JID of the value contained in <paramref name="value"/>,
        /// if the conversion succeeded or empty if the conversion failed. The conversion fails if the
        /// parameter is <c>null</c> or <see cref="string.Empty"/> or is not of the correct format. This
        /// parameter is passed uninitialized.
        /// </param>
        /// <returns><c>true</c> if <paramref name="value"/> was converted successfully; otherwise, <c>false</c>.</returns>
        public static bool TryParse(string value, out Jid result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = default(Jid);
                return false;
            }
            var length = value.Length;
            var i = 0;

            var sb = new StringBuilder();
            for (; i < length; i++)
            {
                var c = value[i];
                if (c == '@' && sb.Length != 0)
                    break;
                else if (!IsValid(c))
                {
                    result = default(Jid);
                    return false;
                }
                else sb.Append(c);
            }

            var node = sb.ToString();
            if (i == length)
            {
                result = new Jid(node);
                return true;
            }
            sb.Clear();

            for (; i < length; i++)
            {
                var c = value[i];
                if (c == '/' && sb.Length != 0)
                    break;
                else if (!IsValid(c))
                {
                    result = default(Jid);
                    return false;
                }
                else sb.Append(c);
            }
            sb.Clear();

            var domain = sb.ToString();
            if (i == length)
            {
                result = new Jid(node, domain);
                return true;
            }

            for (; i < length; i++)
            {
                var c = value[i];
                if (!IsValid(c))
                {
                    result = default(Jid);
                    return false;
                }
                else sb.Append(c);
            }

            result = new Jid(node, domain, sb.ToString());
            return true;
        }

        /// <summary>
        /// Determines whether the specified character is valid in any part of a JID.
        /// </summary>
        /// <param name="c">The character to validate.</param>
        /// <returns>A value indicating whether the character is valid.</returns>
        private static bool IsValid(char c)
        {
            var i = (int)c;
            if (i <= 0x20) return false;
            switch (i)
            {
                case 0x22:
                case 0x26:
                case 0x27:
                case 0x2F:
                case 0x3A:
                case 0x3C:
                case 0x3E:
                case 0x40:
                case 0x7F:
                case 0xFFFE:
                case 0xFFFF:
                    return false;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Determines whether the specified character is invalid in any part of a JID.
        /// </summary>
        /// <param name="c">The character to validate.</param>
        /// <returns>A value indicating whether the character is invalid.</returns>
        private static bool IsInvalid(char c)
        {
            return !IsValid(c);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Full;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format)
        {
            return ToString(format, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format)) format = "f";
            switch (format)
            {
                case "f": return ToBestJid();
                case "b": return ToBareJid();
                case "n": return Node;
                case "d": return Domain;
                case "r": return Resource;
                default: throw new ArgumentOutOfRangeException(nameof(format));
            }
        }

        private string ToBareJid()
        {
            if (_domain == null) return null;
            if (_node == null) return _domain;
            return string.Concat(_node, "@", _domain);
        }

        private string ToBestJid()
        {
            if (_domain == null) return null;
            if (_node == null) return _domain;
            if (_resource == null) return string.Concat(_node, "@", _domain);
            return string.Concat(_node, "@", _domain, "/", _resource);
        }

        /// <summary>
        /// Called when deserialization is complete.
        /// </summary>
        /// <param name="sender">The sender.</param>
        void IDeserializationCallback.OnDeserialization(object sender)
        {
            _full = ToBestJid();
            _hashCode = StringComparer.OrdinalIgnoreCase.GetHashCode(_full);
        }
    }
}
