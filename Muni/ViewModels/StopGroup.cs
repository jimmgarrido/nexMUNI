using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexMuni.ViewModels
{
    public class StopGroup
    {
        public StopGroup()
        {
            nearbyStopsList = new List<StopData>();
        }

        public List<StopData> nearbyStopsList { get; set; }    

    }
}
