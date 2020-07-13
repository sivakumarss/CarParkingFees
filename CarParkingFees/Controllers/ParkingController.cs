using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CarParkingFees.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Globalization;
using CarParkingFees.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace CarParkingFees.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ParkingController : ControllerBase
    {
        private readonly ILogger<ParkingController> _logger;

        public ParkingController(ILogger<ParkingController> logger)
        {
            _logger = logger;
        }



        [HttpGet]
        [Route("getParkingRates")]
        public List<Parking> GetParkingRates()
        {
            var rates = new List<Parking>();
            try
            {
                var array = Enum.GetValues(typeof(ParkingRate)).Cast<ParkingRate>().ToArray();
                foreach (var rate in array)
                {
                    var parkingRate = new Parking()
                    {
                        ParkingRateName = rate.ToString(),
                        TotalPrice = GetParkingPrice(rate).ToString("C", CultureInfo.CurrentCulture)
                    };
                    rates.Add(parkingRate);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} --Details:  {ex.InnerException}");
            }

            return rates;
        }


        [HttpPost]
        [Route("parkingFee")]

        public ActionResult<Parking> ParkingFee(ParkingModel model)
        {
            try
            {
                var parking = new Parking()
                {
                    ParkingRateName = ParkingRate.StandardRate.GetAttribute<ParkingRateInfoAttribute>().Name,
                    TotalPrice = GetParkingPrice(ParkingRate.StandardRate).ToString("C", CultureInfo.CurrentCulture)
                };

                var diffInTime = model.CarExitDateTime - model.CarEntryDateTime;

                //Check Hourly rates
                if (diffInTime.TotalMinutes < 60)
                {
                    parking.TotalPrice = HourlyRate.FirstHour.GetAttribute<ParkingRateInfoAttribute>().Price.ToString("C", CultureInfo.CurrentCulture);
                }
                else if (diffInTime.TotalMinutes > 60 && diffInTime.TotalMinutes < 120)
                {
                    parking.TotalPrice = HourlyRate.SecondHour.GetAttribute<ParkingRateInfoAttribute>().Price.ToString("C", CultureInfo.CurrentCulture);
                }
                else if (diffInTime.TotalMinutes > 120 && diffInTime.TotalMinutes < 180)
                {
                    parking.TotalPrice = HourlyRate.ThirdHour.GetAttribute<ParkingRateInfoAttribute>().Price.ToString("C", CultureInfo.CurrentCulture);
                }

                CheckEarlyBird(parking, model);
                CheckNightRate(parking, model);
                CheckWeekendRate(parking, model);
                

                return Ok(parking);
            }
            catch (Exception ex)
            {
                _logger.LogError($"{ex.Message} --Details:  {ex.InnerException}");
                return NotFound();
            }
        }

        private void CheckEarlyBird(Parking parking, ParkingModel model)
        {
            if(model.CarEntryDateTime.DayOfWeek != DayOfWeek.Saturday && model.CarEntryDateTime.DayOfWeek != DayOfWeek.Sunday)
            {
                var incomingStart = new TimeSpan(6,0,0);
                var incomingEnd = new TimeSpan(9,0,0);
                var outGoingStart = new TimeSpan(15, 30, 0);
                var outGoingEnd = new TimeSpan(23, 30, 0);
                var earlyBirdPrice = GetParkingPrice(ParkingRate.EarlyBirdRate);
                var price = parking.TotalPrice;

                if (model.CarEntryDateTime.TimeOfDay > incomingStart && model.CarEntryDateTime.TimeOfDay < incomingEnd  
                    && model.CarExitDateTime.TimeOfDay > outGoingStart && model.CarExitDateTime.TimeOfDay < outGoingEnd
                    && earlyBirdPrice < double.Parse(price.Replace('$', ' ')))
                {
                    parking.ParkingRateName = ParkingRate.EarlyBirdRate.GetAttribute<ParkingRateInfoAttribute>().Name;
                    parking.TotalPrice = earlyBirdPrice.ToString("C", CultureInfo.CurrentCulture);
                }
            }
        }

        private void CheckNightRate(Parking parking, ParkingModel model)
        {
            if (model.CarEntryDateTime.DayOfWeek != DayOfWeek.Saturday && model.CarEntryDateTime.DayOfWeek != DayOfWeek.Sunday)
            {
                var incomingStart = new TimeSpan(18, 0, 0);
                var incomingEnd = new TimeSpan(23, 59, 59);
                var outGoingStart = new TimeSpan(15, 30, 0);
                var outGoingEnd = new TimeSpan(23, 30, 0);
                TimeSpan ts24Hrs = TimeSpan.FromHours(24);

                var nightrate = GetParkingPrice(ParkingRate.NightRate);
                var price = parking.TotalPrice;

                if (model.CarEntryDateTime.TimeOfDay > incomingStart && model.CarEntryDateTime.TimeOfDay < incomingEnd
                   // && model.CarExitDateTime.TimeOfDay > outGoingStart.Add(ts24Hrs) && model.CarExitDateTime.TimeOfDay < outGoingEnd.Add(ts24Hrs)
                    && nightrate < double.Parse(price.Replace('$', ' ')))
                {
                    parking.ParkingRateName = ParkingRate.NightRate.GetAttribute<ParkingRateInfoAttribute>().Name;
                    parking.TotalPrice = GetParkingPrice(ParkingRate.NightRate).ToString("C", CultureInfo.CurrentCulture);
                }
            }
        }

        private void CheckWeekendRate(Parking parking, ParkingModel model)
        {
            if (model.CarEntryDateTime.DayOfWeek == DayOfWeek.Saturday || model.CarEntryDateTime.DayOfWeek == DayOfWeek.Sunday)
            {
                var weekendRate = GetParkingPrice(ParkingRate.WeekendRate);
                var price = parking.TotalPrice;

                if (weekendRate < double.Parse(price.Replace('$', ' ')))
                {
                    parking.ParkingRateName = ParkingRate.WeekendRate.GetAttribute<ParkingRateInfoAttribute>().Name;
                    parking.TotalPrice = GetParkingPrice(ParkingRate.WeekendRate).ToString("C", CultureInfo.CurrentCulture);
                }
            }
        }

        private double GetParkingPrice(ParkingRate rate)
        {
            double price = 0.0;
            switch (rate)
            {
                case ParkingRate.StandardRate:
                    price = ParkingRate.StandardRate.GetAttribute<ParkingRateInfoAttribute>().Price;
                    break;
                case ParkingRate.EarlyBirdRate:
                    price = ParkingRate.EarlyBirdRate.GetAttribute<ParkingRateInfoAttribute>().Price;
                    break;
                case ParkingRate.NightRate:
                    price = ParkingRate.NightRate.GetAttribute<ParkingRateInfoAttribute>().Price;
                    break;
                case ParkingRate.WeekendRate:
                    price = ParkingRate.WeekendRate.GetAttribute<ParkingRateInfoAttribute>().Price;
                    break;
                default:
                    break;
            }

            return price;
        }

    }
}
