using Hotels.Domain.Models;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Hotels.Domain
{
    /// <summary>
    /// Amadeus Search service
    /// </summary>
    public class SearchService
    {
        private const string baseUrl = "https://test.api.amadeus.com/v1/";
        private const string baseUrl2 = "https://test.api.amadeus.com/v2/";

        private const string key = "96iSR2G1egfDyPT4ctmHTObA6YLjtDS5";
        private const string secret = "Qud0YqNJvL5AJuWH";

        private IMemoryCache cache;

        public string token = "";


        public SearchService(IMemoryCache cache)
        {
            //get Token
            this.cache = cache;
            this.token = GetToken().Result;
        }

        public async Task<List<ResultsViewModel>> GetHotels(Request req)
        {
            var result = new List<ResultsViewModel>();

            var checkIn = WebUtility.UrlEncode( req.CheckIn.ToString("yyyy-MM-dd"));
            var checkOut = WebUtility.UrlEncode (req.CheckOut.ToString("yyyy-MM-dd"));


            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"),
                    $"{baseUrl2}shopping/hotel-offers?cityCode={req.CityCode}&radius={req.Radius}&checkInDate={checkIn}&checkOutDate={checkOut}&adults={req.People}"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(content);

                        JArray hotels = (JArray)json["data"];
                        foreach (JObject hotel in hotels) 
                        {
                            JArray offers = (JArray)hotel["offers"];

                            string Price = "";
                            float minPrice = float.MaxValue;
                            foreach (JObject offer in offers)
                            {
                                float p = float.Parse(offer["price"]["total"].ToString());
                                if (p < minPrice)
                                { 
                                    minPrice = p;
                                    Price = minPrice + " " + offer["price"]["currency"].ToString();
                                }
                            }

                            result.Add(new ResultsViewModel()
                            {
                                Name =hotel["hotel"]["name"].ToString(),
                                Description = hotel["hotel"]["description"]["text"].ToString(),
                                Image = hotel["hotel"]["media"][0]["uri"].ToString(),
                                Ratting = int.Parse (hotel["hotel"]["rating"].ToString()),
                                Price = Price.ToString()
                            });
                        }

                    }
                    else
                    {
                        return result;
                    }
                }

                
                return result;
            }

            return result;
        }

        public async Task<List<CityInfo>> GetLocation(string keyword)
        {
            var cities = new List<CityInfo>();
            //Check cache first
            cities = cache.Get<List<CityInfo>>(keyword);
            if (cities != null)
                return cities;

            cities = new List<CityInfo>();
            //keyword = "london";

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("GET"),
                    $"{baseUrl}reference-data/locations?keyword={keyword}&subType=CITY"))
                {
                    request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var json = JObject.Parse(content);

                        JArray jArray = (JArray)json["data"];
                        foreach (JObject item in jArray)
                        {
                            cities.Add(new CityInfo()
                            {
                                City = item.GetValue("name").ToString(),
                                CityCode = item.GetValue("iataCode").ToString()
                            });
                        }

                    }
                    else
                    {
                        return null;
                    }
                }

                //store in cache
                cache.Set(keyword, cities);

                return cities;
            }
        }

       

        // Get and store token in cache
        public async Task<string> GetToken()
        {
            string token = GetCacheToken();
            if (token != "")
                return token;

            using (var client = new HttpClient())
            {
                using (var request = new HttpRequestMessage(new HttpMethod("POST"), baseUrl + "security/oauth2/token"))
                {
                    request.Content = new StringContent($"grant_type=client_credentials&client_id={key}&client_secret={secret}");
                    request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

                    var response = await client.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var obj = JObject.Parse(content);

                        token = (string)obj["access_token"];
                        cache.Set("token", token);
                        var expires_in = (int)obj["expires_in"];
                        var expiryDate = DateTime.Now.AddSeconds(expires_in - 10);
                        cache.Set("tokenExpiry", expiryDate);

                        return token;
                    }
                    else
                    {
                        return "";
                        //error
                    }
                }
            }



            //var requestTokenModel = new RequestTokenModel()
            //{
            //    grant_type = "client_credentials",
            //    client_id = key,
            //    client_secret = secret
            //};


        }

        // Check cache for token
        private string GetCacheToken()
        {
            string token = "";

            var expiryDate = cache.Get<DateTime?>("tokenExpiry");

            if (expiryDate != null && expiryDate > DateTime.Now)
                token = cache.Get("token").ToString();
            if (token != "")
                return token;

            return token;
        }
    
    }
}
