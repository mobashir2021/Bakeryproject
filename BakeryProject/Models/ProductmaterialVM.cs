using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BakeryProject.Models
{
    public class ProductmaterialVM
    {
        public int Productid { get; set; }
        public string Product { get; set; }
        public List<RawmaterialVM> lstmaterials { get; set; }

        public int Quantity { get; set; }

        public List<Productvm> products { get; set; }

        public int Orderid { get; set; }

        public string TotalRate { get; set; }

        public string WTotalRate { get; set; }

        public int ExtraQuantity { get; set; }
        public int Customerid { get; set; }

        public bool IsDispatchEnabled { get; set; }

        public int DeliveryInfoId { get; set; }

        public Customerviewmodel custObj { get; set; }

        public DeliveryInfo delObj { get; set; }

        public string BillNo { get; set; }
        public string Billdate { get; set; }
    }
}