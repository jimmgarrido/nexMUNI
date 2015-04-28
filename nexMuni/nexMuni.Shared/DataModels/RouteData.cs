using SQLite;
using System;
using System.Collections.Generic;
using System.Text;

namespace nexMuni.DataModels
{
    public class RouteData
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string Title { get; set; }
    }
}
