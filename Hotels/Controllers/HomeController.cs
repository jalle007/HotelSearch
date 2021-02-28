using Hotels.Domain;
using Hotels.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Hotels.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly SearchService _search;

        public HomeController(ILogger<HomeController> logger, SearchService search)
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
                CheckIn = new System.DateTime(2021, 3, 3),
                CheckOut = new System.DateTime(2021, 3, 8)
            };
            return View(model);
        }

        public ActionResult GetLocation(string keyword)
        {
            var result = _search.GetLocation(keyword).Result;
            return Ok(result);
        }

        [HttpPost]
        public PartialViewResult GetHotels(Request request)
        {
            //request.CityCode = "LON";
            var hotels = _search.GetHotels(request).Result;

            return  PartialView("_SearchResults", hotels);
        }

       
    }
}
