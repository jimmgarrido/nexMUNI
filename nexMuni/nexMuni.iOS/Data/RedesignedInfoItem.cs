using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Foundation;
using Newtonsoft.Json;
using UIKit;

namespace NexMuni.iOS.Data
{
    public class RedesignedInfoItem
    {
        public string Id { get; set; }

        [JsonProperty(PropertyName = "train_id")]
        public int TrainId { get; set; }

    }
}