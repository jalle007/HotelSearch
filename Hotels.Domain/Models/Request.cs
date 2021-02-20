using System;

namespace Hotels.Domain.Models
{
    public class Request
    {
        public string  City { get; set; }
        public string CityCode { get; set; }
        public int Radius { get; set; }
        public int People { get; set; }
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
    }
}
