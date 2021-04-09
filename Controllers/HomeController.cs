using GlobalTemp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;

namespace GlobalTemp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private Microsoft.Extensions.Configuration.IConfiguration _cfg;
        private static HttpClient _client { get; set; }
        private UriBuilder uriBldr;

        public HomeController(ILogger<HomeController> logger, Microsoft.Extensions.Configuration.IConfiguration cfg)
        {
            _logger = logger;
            _cfg = cfg;

            //
            // Bug Fix 2021-04-09
            // Using a static _client member to avoid making another connection at every user request coming to the controlller
            //
            if ( _client == null )
            {
                _client = new HttpClient();
            }

            string baseUrl = _cfg.GetValue<string>("weatherBaseUrl");
            uriBldr = new UriBuilder(baseUrl);
            uriBldr.Query = _cfg.GetValue<string>("weatherQueryFixedPart");
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Index(WeatherModel weather)
        {
            string cityName = weather.CityName;
            uriBldr.Query = uriBldr.Query.Substring(1) + "&q=" + cityName;

            HttpResponseMessage resp = _client.GetAsync(uriBldr.Uri).Result;
            
            try
            {
                resp.EnsureSuccessStatusCode();
            }
            catch (Exception)
            {
                weather.ReportText = "I'm sorry, no city by that name exists on the surface of this planet.";
                return View(weather);
            }

            string dataAsJSON = resp.Content.ReadAsStringAsync().Result;

            Models.JsonReaderUtil.Rootobject jsonReader = JsonConvert.DeserializeObject<Models.JsonReaderUtil.Rootobject>(dataAsJSON);

            weather.ReportText = jsonReader.main.temp.ToString() + " °F";

            return View(weather);
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
    }
}
