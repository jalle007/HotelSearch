using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotels.Domain.Models
{
    public class Response
    {
        public Response()
        {
            //this.Request = new Request();
            this.Result = new List<string>();
        }
        //public Request Request { get; set; }
        public List<string> Result { get; set; }
    }
}
