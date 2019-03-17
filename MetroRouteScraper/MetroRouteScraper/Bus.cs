using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MetroRouteScraper
{
    public class Bus
    {
        public int Bus_number { get; set; }
        public int Driver_ID { get; set; }
        public string To_name { get; set; }
        public string From_name { get; set; }
        public string Bus_model { get; set; }
        public int Seats_available { get; set; }
        public int Miles { get; set; }
        public string Base_address { get; set; }
    }
}
