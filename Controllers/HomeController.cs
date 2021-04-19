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
        private static string _baseUrl;
        private static string _queryFixedPart;
        private static string _apiKey;
        private UriBuilder _uriBldr;
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
            string cityNameFromUser = weatherModel.CityNameFromUser;

            //
            // Check for user typos
            //
            if ((string.IsNullOrWhiteSpace(cityNameFromUser)) || (Regex.IsMatch(cityNameFromUser, "^[a-zA-Z]+$") == false))
            {
                weatherModel.cityTempToUser = _cityNameInvalidMsg;
                return View(weatherModel);
            }

            _uriBldr = new UriBuilder(_baseUrl);
            _uriBldr.Query = _queryFixedPart;
            _uriBldr.Query += "&" + _apiKey;
            _uriBldr.Query += "&q=" + cityNameFromUser;


            HttpResponseMessage resp;
            try
            {
                //
                // Caution - Wrong usage detected in a code review with Jason and Khaled
                // Trying to read the final result in the same statement was causing this aync call to become SYNCHRONOUS
                // resp = _client.GetAsync(uriBldr.Uri).Result;
                //
                resp = await _client.GetAsync(_uriBldr.Uri);
            }
            catch (Exception)
            {
                //
                // Network issues like a broken internet connection will cause execution to reach here
                //
                weatherModel.cityTempToUser = _networkComFailedMsg;
                return View(weatherModel);
            }

            if (resp.IsSuccessStatusCode == false)
            {
                //
                // Execution will reach here if the external API does not have this city in its database
                //
                weatherModel.cityTempToUser = _cityNotFoundMsg;
                return View(weatherModel);
            }

            //
            // Finally, we have some valid output for the view
            // Renamed the JsonReaderUtil class to WeatherDataReceiver after a code review with Khaled and Jason to improve code readability
            // The WeatherDataReceiver class is used solely to load the received raw JSON data into the members of a C# object
            //
            string dataAsRawJSON = resp.Content.ReadAsStringAsync().Result;
            Models.WeatherDataReceiver.Rootobject weatherData = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.WeatherDataReceiver.Rootobject>(dataAsRawJSON);
            weatherModel.cityTempToUser = weatherData.main.temp.ToString() + " °F";

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
            if (string.IsNullOrEmpty(_baseUrl))
            {
                _baseUrl = _cfg.GetValue<string>("GT_API_BASE_URL");
            }

            if (string.IsNullOrEmpty(_queryFixedPart))
            {
                _queryFixedPart = _cfg.GetValue<string>("GT_API_QUERY_FIXED_PART");
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                _apiKey = _cfg.GetValue<string>("GT_API_KEY");
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
        }
    }
}
