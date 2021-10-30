using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BakeryProject.Models
{
    public class DailyorderViewModel
    {
        public int Dailyorderid { get; set; }
        public int Customerid { get; set; }
        public string Customer { get; set; }
        public DateTime Orderdate { get; set; }
        public int Productid { get; set; }
        public string Product { get; set; }
        public int Quantity { get; set; }

        public int ExtraQuantity { get; set; }

        public int TotalQuantity { get; set; }

        public int BadQuantity { get; set; }
    }
}