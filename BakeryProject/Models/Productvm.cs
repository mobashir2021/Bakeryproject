using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BakeryProject.Models
{
    public class Productvm
    {
        public int Productid { get; set; }

        public int SRNO { get; set; }

        public string HSNCode { get; set; }
        public string Product { get; set; }

        public string Rate { get; set; }
        public string TotalRate { get; set; }

        public int Quantity { get; set; }

        public int ExtraQuantity { get; set; }

        public int GoodQuantity { get; set; }
        public int BadQuantity { get; set; }
        public int Dailyprodid { get; set; }
        public string Status { get; set; }

        public bool IsRolledout { get; set; }

        public int DispatchedQuantity { get; set; }
        public int Dailyorderid { get; set; }
    }
}