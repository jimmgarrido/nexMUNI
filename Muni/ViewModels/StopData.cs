using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NexMuni.ViewModels
{
    public class StopData
    {
        public string Name { get; set; }
        public string Routes { get; set; }

        public StopData(){}

        public StopData(string stopName )
        {
            Name = stopName;
        }
    }
}
