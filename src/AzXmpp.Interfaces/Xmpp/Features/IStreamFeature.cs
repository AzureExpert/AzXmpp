using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.ServiceFabric.Actors;

namespace AzXmpp.Xmpp.Features
{
    /// <summary>
    /// Represents a stream feature.
    /// </summary>
    public interface IStreamFeature : IActor
    {
        /// <summary>
        /// Creates the element that describes the feature to the client.
        /// </summary>
        /// <returns>The element that describes the feature to the client.</returns>
        Task<XElement> CreateDescriptiveElementAsync();


    }
}
