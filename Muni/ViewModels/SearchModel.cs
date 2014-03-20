using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace NexMuni.ViewModels
{
    public class SearchModel
    {
        public bool IsDataLoaded { get; set; }

        public void LoadData()
        {
            IsDataLoaded = true;
        }
    }
}
