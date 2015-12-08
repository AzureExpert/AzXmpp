using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml.Linq;
using AzXmpp.Xmpp.Features;
using Microsoft.ServiceFabric.Actors;

namespace AzXmpp.Edge.Actors
{
    /// <summary>
    /// Represents the authentication feature.
    /// </summary>
    [ActorService(Name = "Authentication")]
    public class AuthenticationFeature : Actor<AuthenticationFeatureState>, IStreamFeature
    {
        /// <summary>
        /// Asynchronously activates the actor.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous on activate operation.
        /// </returns>
        public override Task OnActivateAsync()
        {
            if (State == null)
            {
                var configPackage = Host.StatefulServiceInitializationParameters.CodePackageActivationContext.GetConfigurationPackageObject("Config");
                var streamSection = configPackage.Settings.Sections["Authentication"];
                var mechanisms = streamSection.Parameters["Mechanisms"].Value.Split(';');

                State.Mechanisms = new Dictionary<string, ActorId>(StringComparer.OrdinalIgnoreCase);
                foreach (var mech in mechanisms)
                {
                    State.Mechanisms.Add(mech, ActorId.NewId());
                }
            }

            return base.OnActivateAsync();
        }

        /// <summary>
        /// Creates the element that describes the feature to the client.
        /// </summary>
        /// <returns>
        /// The element that describes the feature to the client.
        /// </returns>
        public Task<XElement> CreateDescriptiveElementAsync()
        {
            var elem = new XElement(Xmlns.IetfSasl.Mechanisms);
            foreach (var mech in State.Mechanisms)
            {
                elem.Add(new XElement(Xmlns.IetfSasl.Mechanism, mech.Key.ToUpperInvariant()));
            }
            return Task.FromResult(elem);
        }
    }

    /// <summary>
    /// Represents the state for <see cref="AuthenticationFeature"/>.
    /// </summary>
    [DataContract]
    public class AuthenticationFeatureState
    {
        [DataMember]
        public Dictionary<string, ActorId> Mechanisms { get; set; }

        [DataMember]
        public ActorId ActiveMechanism { get; set; }
    }
}
