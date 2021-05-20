using System;

namespace GlobalTemp.Models
{
    public class CityModel
    {
        public int CityID { get; set; }
        public string CityNameFromUser { get; set; }
        public float TemperatureVal { get; set; }
        public DateTime SunriseDT { get; set; }
        public DateTime SunsetDT { get; set; }
        public DateTime DateTimeNow { get; set; }
        public string countryFlagUrl { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string CurrencyCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyName { get; set; }
        public float CurrencyVal { get; set; }

        public string ErrMsgToUser { get; set; }

        public string FeatureCtrlClientTime { get; set; }
    }
}