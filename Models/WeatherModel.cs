using System;

namespace GlobalTemp.Models
{
    public class WeatherModel
    {
        public int CityID { get; set; }
        public string CityNameFromUser { get; set; }
        public float CityTempVal { get; set; }
        public DateTime SunriseDT { get; set; }
        public DateTime SunsetDT { get; set; }
        public DateTime nowDT { get; set; }
        public string countryFlagUrl { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string ErrMsgToUser { get; set; }
    }
}