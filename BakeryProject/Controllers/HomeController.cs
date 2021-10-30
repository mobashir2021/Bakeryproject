using BakeryProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Twilio;
using Twilio.Rest.Api.V2010.Account;


namespace BakeryProject.Controllers
{
    public class HomeController : Controller
    {
        string accSid = "ACaa0d3f4c0b6a4ca0a0689977521620c1";
        string authToken = "b79fc9d05e7b04b1af70daffd603dffa";
        string fromwatsapp = "whatsapp:+14155238886";
        string custmobileno = "";
        BakeryprojEntities db = new BakeryprojEntities();
        public ActionResult Index()
        {
            ViewBag.Message = "";
            return View();
        }

        [HttpPost]
        public ActionResult SendRequest()
        {
            //string custmobileno = cust.Customermobileno.Contains("+91") ? "whatsapp:" + cust.Customermobileno : "whatsapp:+91" + cust.Customermobileno;
            List<Customer> lst = db.Customers.ToList();
            foreach(var cust in lst)
            {
                OrderPlaced order = new OrderPlaced();
                order.Customerid = cust.Customerid;
                db.OrderPlaceds.Add(order);
                db.SaveChanges();

                custmobileno = "whatsapp:" + cust.Watsappno;
                TwilioClient.Init(accSid, authToken);
                MessageResource.Create(
                    body: "Place Order now \n http://abc.com/Home/SendOrder/" + order.OrderPlacedid.ToString() ,
                    from: new Twilio.Types.PhoneNumber(fromwatsapp),
                    to: new Twilio.Types.PhoneNumber(custmobileno)
                    );
            }
            
            ViewBag.Message = "Message Sent to Customers";
            return View("Index");
        }

        public ActionResult SendOrder()
        {
            var id = RouteData.Values["id"].ToString();
            List<Product> lst = db.Products.ToList();
            ProductmaterialVM vm = new ProductmaterialVM();
            List<Productvm> pro = new List<Productvm>();
            vm.Orderid = Convert.ToInt32(id);
            foreach (var temp in lst)
            {
                Productvm obj = new Productvm();
                
                obj.Productid = temp.Productid;
                obj.Product = temp.Product1;
                obj.Quantity = 0;
                pro.Add(obj);
            }
            vm.products = pro;

            return View(vm);
        }

        [HttpPost]
        public ActionResult SendOrderPost(ProductmaterialVM vm)
        {
            OrderPlaced order = db.OrderPlaceds.Where(x => x.OrderPlacedid == vm.Orderid).FirstOrDefault();
            if(order != null)
            {
                
                foreach(var data in vm.products)
                {
                    DailyOrder daily = new DailyOrder();
                    daily.Customerid = order.Customerid.Value;
                    daily.Orderdate = DateTime.Now;
                    daily.Productid = data.Productid;
                    daily.Quantity = data.Quantity;
                    db.DailyOrders.Add(daily);
                    db.SaveChanges();
                }
                TempData["data"] = "Your Order has been placed successfully";
                return RedirectToAction("Result");
            }
            else
            {
                TempData["data"] = "Your Order failed";
                return RedirectToAction("Result");
            }

            
        }

        public ActionResult Result()
        {
            ViewBag.Message = TempData["data"].ToString();
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}