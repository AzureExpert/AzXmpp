using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using AzXmpp.Xmpp.Features;
using Microsoft.ServiceFabric.Actors;

namespace AzXmpp.Edge.Actors
{
    [ActorService(Name = Constants.ActorNames.UnboundClient)]
    public class UnboundClient : Actor<UnboundClientState>, IXmlConnection
    {

        public override async Task OnActivateAsync()
        {
            if (State == null)
            {
                State = new UnboundClientState();

                var configPackage = Host.StatefulServiceInitializationParameters.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                var streamSection = configPackage.Settings.Sections["Stream"];
                var features = streamSection.Parameters["Features"].Value.Split(';');

                State.Features = new Dictionary<XName, ActorId>();
                foreach (var feature in features)
                {
                    var id = ActorId.NewId();
                    var actor = ActorProxy.Create<IStreamFeature>(id, Constants.ApplicationName, feature);
                    var el = await actor.CreateDescriptiveElementAsync();
                    State.Features.Add(el.Name, id);
                }
            }
        }

        public async Task<StanzaResult> OnStanzaReceivedAsync(Stanza stanza)
        {
            if (stanza.Element.Name == Xmlns.Streams.Stream)
            {
                if (State.HasDocumentTag)
                {
                    return new StanzaResult(StreamAction.Close, Stanza.FromFullElement(
                        new XElement(Xmlns.Streams.Error, new XElement(Xmlns.Client.Namespace + "bad-format"))));
                }
                else
                {
                    State.HasDocumentTag = true;
                    var element = new XElement(Xmlns.Streams.Stream,
                        new XAttribute(XNamespace.Xmlns + "stream", Xmlns.Streams.Stream.NamespaceName),
                        new XAttribute("xmlns", Xmlns.Client.Namespace.NamespaceName),
                        new XAttribute(Xmlns.Attr.Version, "1.0"));

                    var features = new XElement(Xmlns.Streams.Features);
                    element.Add(features);

                    foreach (var feat in State.Features.Values)
                    {
                        var actor = ActorProxy.Create<IStreamFeature>(feat, Constants.ApplicationName);
                        element.Add(await actor.CreateDescriptiveElementAsync());
                    }

                    return new StanzaResult(Stanza.FromOpeningTag(element));
                }
            }
            else
            {
                return null;
            }
        }

        public Task<StanzaResult> OnErrorOccurredAsync(Exception error)
        {
            throw new NotImplementedException();
        }

        public Task OnStreamResetAsync()
        {
            return Task.FromResult(0);
        }
    }

    [DataContract]
    public class UnboundClientState
    {
        [DataMember]
        public Dictionary<XName, ActorId> Features { get; set; }

        [DataMember]
        public bool HasDocumentTag { get; set; }
    }
}
