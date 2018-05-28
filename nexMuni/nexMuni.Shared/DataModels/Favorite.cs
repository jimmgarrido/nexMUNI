using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    [Table("FavoriteData")]
    public class Favorite
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Routes { get; set; }
        public string Tags { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public Favorite() { }

        public Favorite(string stopName, string routes, string _tags, string d)
        {
            Name = stopName;
            this.Tags = _tags;
            this.Routes = routes;
        }
    }
}
