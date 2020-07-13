using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CarParkingFees.Models
{
    public class ParkingModel
    {
        public DateTime CarEntryDateTime { get; set; }
        public DateTime CarExitDateTime { get; set; }
    }

    public class Parking
    {
        public string ParkingRateName { get; set; }
        public string TotalPrice { get; set; }

    }

}
