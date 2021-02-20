using Hotels.Domain;
using Hotels.Domain.Models;
using Hotels.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Hotels.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly Search _search;

        public HomeController(ILogger<HomeController> logger, Search search)
        {
            _logger = logger;
            _search = search;
        }

        public IActionResult Index()
        {
            var model = new Request() {
                City = "", // "LONDON",
                CityCode = "LON",
                People = 1,
                Radius = 100,
                CheckIn = new System.DateTime(2021, 2, 22),
                CheckOut = new System.DateTime(2021, 2, 25)
            };

            return View(model);
        }

        public ActionResult GetLocation(string keyword)
        {
            var result = _search.GetLocation(keyword);
            return Ok(result);
        }

        [HttpPost]
        public PartialViewResult GetHotels(Request request)
        {
            //request.CityCode = "LON";
            var hotels = _search.GetHotels(request);

            return  PartialView("_SearchResults", hotels);
        }

        //[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        //public IActionResult Error()
        //{
        //    return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        //}
    }
}
