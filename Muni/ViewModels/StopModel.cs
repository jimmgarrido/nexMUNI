using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace Muni.ViewModels
{
    public class StopModel
    {
        public StopGroup Stops { get; set; }
        public StopGroup Stop2 { get; set; }
        public StopGroup Stop3 { get; set; }
        public StopGroup Stop4 { get; set; }
        public StopGroup Stop5 { get; set; }

        public bool IsDataLoaded { get; set; }

        public void LoadData()
        {
            //Load data

            Stops = CreateStopGroup();

            IsDataLoaded = true;

        }

        private StopGroup CreateStopGroup()
        {
            StopGroup data = new StopGroup();
            //data.Title = "Bus Stop";

            data.StopsList.Add(new StopData { StopName = "Geneva & Mission" });
            data.StopsList.Add(new StopData { StopName = "Broad & Plymouth" });
            data.StopsList.Add(new StopData { StopName = "Mission & Sickles " });

            return data;
        }

    }
}
