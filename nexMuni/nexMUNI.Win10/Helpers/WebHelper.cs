using System;
using NextBus;

namespace NexMuni.Helpers
{
    public class WebHelper
    {
        public static NextBusClient Client
        {
            get
            {
                return lazyInstance.Value;
            }
        }

        static readonly Lazy<NextBusClient> lazyInstance = new Lazy<NextBusClient>(() => new NextBusClient());

        private WebHelper() { }
    }
}
