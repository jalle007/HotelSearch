using Afonsoft.Amadeus;
using Afonsoft.Amadeus.referenceData;
using Hotels.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;

namespace Hotels.Domain
{
    /// <summary>
    /// Amadeus Search service
    /// </summary>
    public class Search
    {
        Amadeus amadeus;
        private IMemoryCache cache;

        public Search(IMemoryCache cache)
        {
            amadeus = Amadeus
            .Builder("96iSR2G1egfDyPT4ctmHTObA6YLjtDS5", "Qud0YqNJvL5AJuWH")
            .Build();

            this.cache = cache;
        }

        public List<CityInfo> GetLocation(string keyword)
        {
            var cities = new List<CityInfo>() ;
            cities = cache.Get<List<CityInfo>>(keyword);
            if (cities != null)
                return cities;

            var  locations = amadeus.ReferenceData.Locations.Get(Params
                .With("keyword", keyword)
                .And("subType", Locations.CITY));

            cities = new List<CityInfo>();
            foreach (var city in locations.ToList())
                cities.Add(new CityInfo() { 
                    City = city.name, 
                    CityCode = city.iataCode
                });

            //store in cache
            cache.Set(keyword, cities);

            return cities;
        }

        public List<ResultsViewModel> GetHotels(Models.Request request) //request - move models
        {
            var d1 = request.CheckIn.ToString("yyyy-MM-dd");
            var d2 = request.CheckOut.ToString("yyyy-MM-dd");

            var offers = amadeus.Shopping.HotelOffers.Get(Params
            .With("cityCode", request.CityCode)
            .And("radius", request.Radius)
            .And("checkInDate", d1)
            .And("checkInDate", d2)
            .And("adults", request.People)
            );
            
            var result = new List<ResultsViewModel>();

            foreach (var offer in offers)
            {
                result.Add(new ResultsViewModel()
                {
                    Name = offer.hotel.name,
                    Description = offer.hotel.description.text,
                    Image = offer.hotel.media[0].uri,
                    Ratting = offer.hotel.rating,
                    Price = offer.offers.Min(o=>o.price).total + " " + offer.offers.Min(o => o.price).currency
                });

            }

            return result;
        }

    }
}
