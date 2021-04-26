using GlobalTemp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.RegularExpressions;

namespace GlobalTemp.Controllers
{
    public class HomeController : Controller
    {
        private static string _cityNotFoundMsg;
        private static string _cityNameInvalidMsg;
        private static string _networkComFailedMsg;

        private static string _weatherApiBaseUrl;
        private static string _weatherQueryFixedPart;
        private static int _weatherQueryFixedPartLen;
        private static string _weatherApiKey;
        private static UriBuilder _weatherUriBldr;

        private static string _countryApiBaseUrl;
        private static string _countryApiPath;
        private static string _countryApiEndpt;
        private static int _countryApiBaseUrlLen;

        private readonly IConfiguration _cfg;
        private static HttpClient _client;

        public HomeController(IConfiguration cfg)
        {
            _cfg = cfg;

            //
            // Bug Fix 2021-04-09
            // Using a static _client member to avoid making another connection at every user request coming to the controlller
            //
            if (_client == null)
            {
                _client = new HttpClient();
            }

            LoadSettings();
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async System.Threading.Tasks.Task<IActionResult> Index(WeatherModel weatherModel)
        {
            //
            // Get the model data coming from view
            //
            string cityNameFromUser = weatherModel.CityNameFromUser;

            //
            // Check for user typos
            //
            if ((string.IsNullOrWhiteSpace(cityNameFromUser)) || (Regex.IsMatch(cityNameFromUser, "^[a-zA-Z]+$") == false))
            {
                weatherModel.CityTemp = _cityNameInvalidMsg;
                return View(weatherModel);
            }

            int qParamIdx = _weatherUriBldr.Query.IndexOf("&q=");
            if (qParamIdx > -1)
            {
                _weatherUriBldr.Query = _weatherUriBldr.Query.Remove(qParamIdx);
            }
            _weatherUriBldr.Query += "&q=" + cityNameFromUser;

            HttpResponseMessage resp;
            try
            {
                //
                // Caution - Wrong usage detected in a code review with Jason and Khaled
                // Trying to read the final result in the same statement was causing this aync call to become SYNCHRONOUS
                // resp = _client.GetAsync(uriBldr.Uri).Result;
                //
                resp = await _client.GetAsync(_weatherUriBldr.Uri);
            }
            catch (Exception)
            {
                //
                // Network issues like a broken internet connection will cause execution to reach here
                //
                weatherModel.CityTemp = _networkComFailedMsg;
                return View(weatherModel);
            }

            if (resp.IsSuccessStatusCode == false)
            {
                //
                // Execution will reach here if the external API does not have this city in its database
                //
                weatherModel.CityTemp = _cityNotFoundMsg;
                return View(weatherModel);
            }

            //
            // Finally, we have some valid output for the view
            // Renamed the JsonReaderUtil class to WeatherDataReceiver after a code review with Khaled and Jason to improve code readability
            // The WeatherDataReceiver class is used solely to load the received raw JSON data into the members of a C# object
            //
            string dataAsRawJSON = resp.Content.ReadAsStringAsync().Result;
            Models.WeatherDataReceiver.Rootobject weatherData = 
                Newtonsoft.Json.JsonConvert.DeserializeObject<Models.WeatherDataReceiver.Rootobject>(
                    dataAsRawJSON,
                    new Newtonsoft.Json.JsonSerializerSettings
                    {
                        DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore,
                        NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
                    });

            //DateTime sunriseDT = DateTime.Parse(weatherData.sys.sunrise.ToString());
            //DateTime sunsetDT  = DateTime.Parse(weatherData.sys.sunset.ToString());
            string countryCode2LfromWeatherApi = weatherData.sys.country;

            //
            // Populate model with data bound for the view
            //
            weatherModel.CityName = weatherData.name;
            weatherModel.CityTemp = weatherData.main.temp.ToString() + " °F";


            // ------------------------------------------------------
            //
            //                      Call 2nd API
            //
            // ------------------------------------------------------

            _countryApiPath = "/alpha/" + countryCode2LfromWeatherApi;
            _countryApiEndpt = _countryApiBaseUrl + _countryApiPath;
            try
            {
                resp = await _client.GetAsync(_countryApiEndpt);
            }
            catch (Exception)
            {
                //
                // Network issues like a broken internet connection will cause execution to reach here
                //
                weatherModel.CityTemp = _networkComFailedMsg;
                return View(weatherModel);
            }

            if (resp.IsSuccessStatusCode == false)
            {
                //
                // Execution will reach here if the external API does not have this city in its database
                //
                weatherModel.CityTemp = _cityNotFoundMsg;
                return View(weatherModel);
            }

            dataAsRawJSON = resp.Content.ReadAsStringAsync().Result;
            Models.CountryDataReceiver.Rootobject ctryData = 
                Newtonsoft.Json.JsonConvert.DeserializeObject<Models.CountryDataReceiver.Rootobject>(
                    dataAsRawJSON,
                    new Newtonsoft.Json.JsonSerializerSettings 
                    { 
                        DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Ignore, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore 
                    });

            //
            // Populate model with data bound for the view
            //
            weatherModel.CountryName = ctryData.name;
            weatherModel.countryFlagUrl = ctryData.flag;

            return View(weatherModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private void LoadSettings()
        {
            if (string.IsNullOrEmpty(_weatherApiBaseUrl))
            {
                _weatherApiBaseUrl = _cfg.GetValue<string>("GT_API_BASE_URL");
            }

            if (string.IsNullOrEmpty(_weatherQueryFixedPart))
            {
                _weatherQueryFixedPart = _cfg.GetValue<string>("GT_API_QUERY_FIXED_PART");
            }

            if (string.IsNullOrEmpty(_weatherApiKey))
            {
                _weatherApiKey = _cfg.GetValue<string>("GT_API_KEY");
                _weatherUriBldr = new UriBuilder(_weatherApiBaseUrl);
                _weatherUriBldr.Query = _weatherQueryFixedPart;
                _weatherUriBldr.Query += "&" + _weatherApiKey;
                _weatherQueryFixedPartLen = _weatherUriBldr.Query.Length;
            }

            if (string.IsNullOrEmpty(_cityNameInvalidMsg))
            {
                _cityNameInvalidMsg = _cfg.GetValue<string>("GT_CITY_NAME_INVALID_MSG");
            }

            if (string.IsNullOrEmpty(_cityNotFoundMsg))
            {
                _cityNotFoundMsg = _cfg.GetValue<string>("GT_CITY_NOT_FOUND_MSG");
            }

            if (string.IsNullOrEmpty(_networkComFailedMsg))
            {
                _networkComFailedMsg = _cfg.GetValue<string>("GT_NETWORK_COMM_FAILED_MSG");
            }

            if (string.IsNullOrEmpty(_countryApiBaseUrl))
            {
                _countryApiBaseUrl = _cfg.GetValue<string>("GT_COUNTRY_API_BASE_URL");
                _countryApiBaseUrlLen = _countryApiBaseUrl.Length;
            }
        }
    }
}
