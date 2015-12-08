using System;
using System.Diagnostics;
using System.Fabric;
using System.Threading;

namespace AzXmpp.Transport
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                using (FabricRuntime fabricRuntime = FabricRuntime.Create())
                {
                    fabricRuntime.RegisterServiceType("TcpTransportType", typeof(TcpTransport));

                    ServiceEventSource.Current.ServiceTypeRegistered(Process.GetCurrentProcess().Id, typeof(TcpTransport).Name);

                    Thread.Sleep(Timeout.Infinite);
                }
            }
            catch (Exception e)
            {
                ServiceEventSource.Current.ServiceHostInitializationFailed(e);
                throw;
            }
        }
    }
}
