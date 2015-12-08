using System;
using System.Fabric;
using System.Threading;
using Microsoft.ServiceFabric.Actors;

namespace AzXmpp.Edge
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (FabricRuntime fabricRuntime = FabricRuntime.Create())
                {
                    fabricRuntime.RegisterActor(typeof(Actors.UnboundClient));
                    fabricRuntime.RegisterActor(typeof(Actors.AuthenticationFeature));

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ActorEventSource.Current.ActorHostInitializationFailed(e);
                throw;
            }
        }
    }
}
