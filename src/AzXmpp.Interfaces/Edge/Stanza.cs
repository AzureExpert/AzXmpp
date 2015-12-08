using System;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace AzXmpp.Edge
{
    /// <summary>
    /// Represents information about a stanza.
    /// </summary>
    [DataContract(Name = "stanza", Namespace = Constants.NamespaceUri.Stanza)]
    public sealed class Stanza
    {
        /// <summary>
        /// Gets the received element.
        /// </summary>
        /// <value>
        /// The received element.
        /// </value>
        [DataMember(Name = "e", Order = 0)]
        public XElement Element
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a value indicating whether only the starting tag of the element was encountered.
        /// </summary>
        /// <value>
        ///   <c>true</c> if only the starting tag of the element was encountered; otherwise, <c>false</c>.
        /// </value>
        [DataMember(Name = "ot", Order = 1)]
        public bool OpeningTagOnly
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Stanza"/> class.
        /// </summary>
        private Stanza()
        {
        }

        /// <summary>
        /// Creates a <see cref="Stanza"/> where the <see cref="OpeningTagOnly"/> property
        /// is set to <c>true</c>.
        /// </summary>
        /// <param name="openingTag">The opening tag.</param>
        /// <returns>The resulting <see cref="Stanza"/>.</returns>
        public static Stanza FromOpeningTag(XElement openingTag)
        {
            if (openingTag == null) throw new ArgumentNullException(nameof(openingTag));
            return new Stanza()
            {
                Element = openingTag,
                OpeningTagOnly = true
            };
        }

        /// <summary>
        /// Creates a <see cref="Stanza"/> where the <see cref="OpeningTagOnly"/> property
        /// is set to <c>false</c>.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns>The resulting <see cref="Stanza"/>.</returns>
        public static Stanza FromFullElement(XElement element)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));
            return new Stanza()
            {
                Element = element
            };
        }
    }
}
