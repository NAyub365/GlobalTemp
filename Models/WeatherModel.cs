namespace GlobalTemp.Models
{
    public class WeatherModel
    {
        public int CityID { get; set; }
        public string CityNameFromUser { get; set; }
        public float CityTempVal { get; set; }
        public string SunriseTime { get; set; }
        public string SunsetTime { get; set; }
        public string countryFlagUrl { get; set; }
        public string CityName { get; set; }
        public string CountryName { get; set; }
        public string ErrMsgToUser { get; set; }
    }
}