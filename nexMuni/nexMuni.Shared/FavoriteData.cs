using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni
{
    class FavoriteData
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Routes { get; set; }
        public string Distance { get; set; }
        public string Tags { get; set; }

        public FavoriteData() { }

        public FavoriteData(string stopName, string routes, string _tags, string d)
        {
            Name = stopName;
            this.Tags = _tags;
            this.Routes = routes;
            Distance = d;
        }
    }
}
