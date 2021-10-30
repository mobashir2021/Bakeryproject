using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BakeryProject.Models
{
    public class RawmaterialVM
    {
        public int Productmaterialid { get; set; }
        public int Rawmaterialid { get; set; }

        public string Rawmaterial { get; set; }

        public string Quantity { get; set; }

        public string Units { get; set; }

        public int Productid { get; set; }

        public int Stock { get; set; }

        public int RawmaterialStockid { get; set; }

        public int RequiredStock { get; set; }



    }
}