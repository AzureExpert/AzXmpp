using System;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AzXmpp.Transport
{
    /// <summary>
    /// Represents XML extensions.
    /// </summary>
    public static class XmlExtensions
    {
        /// <summary>
        /// Asynchronously writes this element to the specified <see cref="XmlWriter" />.
        /// </summary>
        /// <param name="element">The element to write.</param>
        /// <param name="writer">The writer to write to.</param>
        /// <param name="closeFinalTag">if set to <c>true</c> the final closing tag will be emitted.</param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous write to operation.
        /// </returns>
        /// <exception cref="System.ArgumentNullException"></exception>
        public static async Task WriteToAsync(this XElement element, XmlWriter writer, bool closeFinalTag = true)
        {
            if (element == null) throw new ArgumentNullException(nameof(element));

            var no = (XNode)element;
            do
            {
                switch (no.NodeType)
                {
                    case XmlNodeType.Element:
                        var e = (XElement)no;
                        await writer.WriteStartElementAsync(
                            e.GetPrefixOfNamespace(e.Name.Namespace),
                            e.Name.LocalName,
                            e.Name.NamespaceName);

                        for (var attr = e.FirstAttribute; attr != null; attr = attr.NextAttribute)
                        {
                            var namespaceName = attr.Name.NamespaceName ?? "";
                            var localName = attr.Name.LocalName;
                            await writer.WriteAttributeStringAsync(
                                e.GetPrefixOfNamespace(namespaceName),
                                attr.Name.LocalName,
                                (namespaceName.Length == 0 && localName == "xmlns")
                                    ? "http://www.w3.org/2000/xmlns/"
                                    : namespaceName,
                                attr.Value);
                        }

                        if (e.FirstNode != null)
                        {
                            no = e.FirstNode;
                            continue;
                        }
                        else if (closeFinalTag || e != element)
                        {
                            await writer.WriteEndElementAsync();
                        }
                        break;
                    case XmlNodeType.Text:
                        await writer.WriteStringAsync(((XText)no).Value);
                        break;
                    case XmlNodeType.CDATA:
                        await writer.WriteCDataAsync(((XCData)no).Value);
                        break;
                    case XmlNodeType.ProcessingInstruction:
                        var pi = (XProcessingInstruction)no;
                        await writer.WriteProcessingInstructionAsync(pi.Target, pi.Data);
                        break;
                    case XmlNodeType.Comment:
                        await writer.WriteCommentAsync(((XComment)no).Value);
                        break;
                }

                while (no != element && no == no.Parent.LastNode)
                {
                    if (closeFinalTag || no != element)
                        await writer.WriteEndElementAsync();
                    no = no.Parent;
                }

                no = no.NextNode;

            } while (no != null && no != element);
        }
    }
}
