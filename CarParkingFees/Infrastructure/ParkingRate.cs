using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace CarParkingFees.Infrastructure
{

    public enum ParkingRate
    {
        [ParkingRateInfo("Standard Rate", 20.00)]
        StandardRate = 0,

        [ParkingRateInfo("Early Bird", 13.00)]
        EarlyBirdRate = 1,

        [ParkingRateInfo("Night Rate", 6.50)]
        NightRate = 2,

        [ParkingRateInfo("Weekend Rate", 10.00)]
        WeekendRate = 3

    }


    public enum HourlyRate
    {
        [ParkingRateInfo("0-1 Hours", 5.00)]
        FirstHour = 0,

        [ParkingRateInfo("1-2 Hours", 10.00)]
        SecondHour = 1,

        [ParkingRateInfo("2-3 Hours", 15.00)]
        ThirdHour = 2,

        [ParkingRateInfo("3+ Hours", 20.00)]
        ThreePlusHour = 3
    }


    [AttributeUsage(AttributeTargets.All)]
    public class ParkingRateInfoAttribute : DescriptionAttribute
    {
        public ParkingRateInfoAttribute(string name, double price)
        {
            this.Name = name;
            this.Price = price;
        }
        public string Name { get; set; }
        public double Price { get; set; }
    }
}
