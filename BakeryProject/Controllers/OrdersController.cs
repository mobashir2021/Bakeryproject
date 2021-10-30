using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using BakeryProject.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;

namespace BakeryProject.Controllers
{
    public class OrdersController : Controller
    {
        BakeryprojEntities db = new BakeryprojEntities();
        // GET: Orders
        

        public ActionResult ViewTodayOrder()
        {
            List<DailyorderViewModel> lst = new List<DailyorderViewModel>();
            List<DailyOrder> temp = db.DailyOrders.ToList().Where(x => x.Orderdate.Value.Date == DateTime.Now.Date).ToList();
            List<Customer> cust = db.Customers.ToList();
            List<Product> prod = db.Products.ToList();
            foreach(var data in temp)
            {
                DailyorderViewModel vm = new DailyorderViewModel();
                vm.Dailyorderid = data.DailyOrderid;
                Customer customer = cust.Where(x => x.Customerid == data.Customerid.Value).First();
                if (customer != null)
                {
                    vm.Customerid = customer.Customerid;
                    vm.Customer = customer.Customer1;
                }
                Product pro = prod.Where(x => x.Productid == data.Productid.Value).First();
                if(pro != null)
                {
                    vm.Productid = pro.Productid;
                    vm.Product = pro.Product1;
                }
                vm.Quantity = data.Quantity.Value;
                vm.Orderdate = data.Orderdate.Value;
                lst.Add(vm);
            }
            ViewBag.Datetoday = DateTime.Now.Date.ToString("dd-MMM-yyyy");
            return View(lst);
        }

        [HttpGet]
        public ActionResult ProductionPlan()
        {
            ProductmaterialVM lst = new ProductmaterialVM();
            List<Productvm> productvms = new List<Productvm>();
            List<DailyOrder> temp = db.DailyOrders.ToList().Where(x => x.Orderdate.Value.Date == DateTime.Now.Date).ToList();
            List<Product> prod = db.Products.ToList();
            var result = temp.GroupBy(x => x.Productid.Value).ToDictionary(x => x.Key, x => x.ToList());
            foreach(var data in result)
            {
                Productvm vm = new Productvm();
                vm.Productid = data.Key;
                var templst = data.Value;
                Product obj = prod.Where(x => x.Productid == data.Key).FirstOrDefault();
                vm.Product = obj.Product1;
                vm.Quantity = templst.Sum(x => x.Quantity.Value);
                vm.ExtraQuantity = 0;
                productvms.Add(vm);
            }
            lst.products = productvms;
            return View(lst);
        }

        public ActionResult Rollout()
        {
            ProductmaterialVM obj = new ProductmaterialVM();
            obj.Productid = 0;
            List<Productvm> lst = new List<Productvm>();
            List<Dailyproduction> dps = db.Dailyproductions.ToList().Where(x => x.Productdate.Value.Date == DateTime.Now.Date).ToList();
            List<Product> prods = db.Products.ToList();
            foreach(var vals in dps)
            {
                Productvm temp = new Productvm();
                temp.Productid = vals.Productid.Value;
                Product pro = prods.Where(x => x.Productid == vals.Productid.Value).First();
                temp.Product = pro.Product1;
                temp.Dailyprodid = vals.Dailyproductionid;
                temp.Status = vals.ProductionStatus;
                temp.Quantity = vals.Quantity.Value;
                
                lst.Add(temp);
            }

            obj.products = lst;
            return View(obj);
        }

        public ActionResult RolloutProduct(int id)
        {
            Productvm vm = new Productvm();
            Dailyproduction dp = db.Dailyproductions.Find(id);
            Product pro = db.Products.Find(dp.Productid.Value);
            vm.Productid = pro.Productid;
            vm.Dailyprodid = dp.Dailyproductionid;
            vm.Status = dp.ProductionStatus;
            vm.Quantity = dp.Quantity.Value;
            return View(vm);
        }

        [HttpPost]
        public ActionResult RolloutSingle(Productvm vm)
        {
            Dailyproduction dp = db.Dailyproductions.Find(vm.Dailyprodid);
            dp.Goodproduct = vm.GoodQuantity;
            dp.BadProduct = vm.BadQuantity;
            dp.ProductionStatus = "RolledOut";
            db.Entry(dp).State = EntityState.Modified;
            db.SaveChanges();

            var materials = db.Rawmaterials.ToList();
            var stocks = db.RawmaterialStocks.ToList();
            var p = db.Productmaterials.Where(x => x.Productid.Value == vm.Productid).ToList();
            foreach(var data in p)
            {
                RawmaterialStock rm = stocks.Where(x => x.Rawmaterialid == data.Rawmaterialid.Value).First();
                int val = dp.Quantity.Value * Convert.ToInt32(data.Quantity);
                rm.Instock = (Convert.ToInt32(rm.Instock) - val).ToString();
                db.Entry(rm).State = EntityState.Modified;
                db.SaveChanges();
            }



            return RedirectToAction("Rollout");
        }

        [HttpPost]
        public ActionResult SubmitProductionPlan(ProductmaterialVM obj)
        {
            List<RawmaterialVM> lst = new List<RawmaterialVM>();
            List<Productmaterial> productmat = db.Productmaterials.ToList();
            List<Rawmaterial> rm = db.Rawmaterials.ToList();
            List<RawmaterialStock> stocks = db.RawmaterialStocks.ToList();
            List<Product> products = db.Products.ToList();
            bool isEnabled = true;
            foreach(var dd in obj.products)
            {
                Product proObj = products.Where(x => x.Productid == dd.Productid).First();
                var rw = productmat.Where(x => x.Productid.Value == dd.Productid).ToList();
                foreach(var data in rw)
                {
                    Session["Extra-" + proObj.Product1] = dd.Quantity.ToString() + "-" + dd.ExtraQuantity.ToString();
                    Rawmaterial material = rm.Where(x => x.Rawmaterialid == data.Rawmaterialid.Value).First();
                    RawmaterialStock stock = stocks.Where(x => x.Rawmaterialid.Value == material.Rawmaterialid).First();
                    RawmaterialVM temp = new RawmaterialVM();
                    temp.Rawmaterialid = data.Rawmaterialid.Value;
                    temp.Rawmaterial = material.Rawmaterial1;
                    temp.Units = material.Unit;
                    temp.Stock = Convert.ToInt32(stock.Instock);
                    int val = dd.Quantity + dd.ExtraQuantity;
                    int req = Convert.ToInt32(data.Quantity) * val;
                    temp.RequiredStock = req;
                    lst.Add(temp);
                }
            }
            List<RawmaterialVM> finallist = new List<RawmaterialVM>();
            var result = lst.GroupBy(x => x.Rawmaterialid).ToDictionary(x => x.Key, x => x.ToList());
            foreach(var lp in result)
            {
                RawmaterialVM temp = new RawmaterialVM();
                temp.Rawmaterialid = lp.Key;
                temp.Rawmaterial = lp.Value.First().Rawmaterial;
                temp.Units = lp.Value.First().Units;
                temp.Stock = lp.Value.Sum(x => x.Stock);
                temp.RequiredStock = lp.Value.Sum(x => x.RequiredStock);
                if(temp.Stock < temp.RequiredStock)
                {
                    isEnabled = false;
                }
                finallist.Add(temp);
            }
            ViewBag.IsBtnEnabled = isEnabled;
            return View(finallist);
        }

        [HttpPost]
        public ActionResult Forwarded()
        {
            List<ProductmaterialVM> lst = new List<ProductmaterialVM>();
            List<Product> products = db.Products.ToList();
            foreach(string values in Session.Keys)
            {
                if (values.ToString().StartsWith("Extra-"))
                {
                    ProductmaterialVM vm = new ProductmaterialVM();
                    string key = values.ToString().Replace("Extra-", "");
                    Product pro = products.Where(x => x.Product1.Trim().ToLower() == key.Trim().ToLower()).First();
                    Dailyproduction dp = new Dailyproduction();
                    dp.Productid = pro.Productid;
                    dp.Productdate = DateTime.Now;
                    dp.ProductionStatus = "InProgress";
                    var sess = Session[values].ToString();
                    string[] split = sess.Split('-');
                    dp.Quantity = Convert.ToInt32(split[0]) + Convert.ToInt32(split[1]);
                    db.Dailyproductions.Add(dp);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult Dispatch()
        {
            List<Customer> customers = db.Customers.ToList();
            
            ViewBag.Customer = new SelectList(customers, "Customerid", "Customer1");
            List<DeliveryInfo> delivery = db.DeliveryInfoes.ToList();
            ViewBag.Delivery = new SelectList(delivery, "DeliveryInfoId", "DeliveryBoyName");
            ProductmaterialVM obj = new ProductmaterialVM();
            obj.IsDispatchEnabled = false;
            List<Productvm> lst = new List<Productvm>();
            obj.products = lst;
            obj.Productid = 0;
            return View(obj);
        }

        [HttpPost]
        public ActionResult Dispatch(ProductmaterialVM obj, string dispatch, string Customerid, string DeliveryInfoId)
        {
            List<Customer> customers = db.Customers.ToList();
            ViewBag.Customer = new SelectList(customers, "Customerid", "Customer1");
            List<DeliveryInfo> delivery = db.DeliveryInfoes.ToList();
            ViewBag.Delivery = new SelectList(delivery, "DeliveryInfoId", "DeliveryBoyName");
            List<State> lststates = db.States.ToList();
            

            if (!string.IsNullOrEmpty(Customerid) && string.IsNullOrEmpty(dispatch))
            {
                ProductmaterialVM vm = new ProductmaterialVM();
                bool isEnable = true;
                List<Productvm> lst = new List<Productvm>();
                int custid = Convert.ToInt32(Customerid);
                List<Product> products = db.Products.ToList();
                Customer cust = db.Customers.Where(x => x.Customerid == custid).First();
                var dailyprod = db.Dailyproductions.ToList().Where(x => x.Productdate.Value.Date == DateTime.Now.Date).ToList();
                var dailyorder = db.DailyOrders.ToList().Where(x => x.Orderdate.Value.Date == DateTime.Now.Date).ToList();
                var result = (from dp in dailyprod
                              join dod in dailyorder on dp.Productid.Value equals dod.Productid.Value
                              select new { dp, dod }).ToList();

                foreach(var temp in result)
                {
                    if(temp.dod.Customerid.Value == custid)
                    {
                        Productvm pro = new Productvm();
                        pro.Productid = temp.dod.Productid.Value;
                        Product objpro = products.Where(x => x.Productid == pro.Productid).First();
                        pro.Product = objpro.Product1;
                        pro.GoodQuantity = temp.dp.Goodproduct.Value;
                        pro.BadQuantity = temp.dp.BadProduct.Value;
                        pro.Dailyprodid = temp.dp.Dailyproductionid;
                        if(temp.dp.ProductionStatus == "Dispatched")
                        {
                            isEnable = false;
                            pro.Status = "Dispatched";
                        }
                        else
                        {
                            pro.Status = "Not Dispatched";
                        }
                        pro.Quantity = temp.dod.Quantity.Value;
                        pro.Dailyorderid = temp.dod.DailyOrderid;
                        pro.DispatchedQuantity = 0;
                        lst.Add(pro);
                        
                    }
                }
                vm.products = lst;
                vm.Customerid = custid;
                vm.IsDispatchEnabled = isEnable;


                return View(vm);
            }
            else if(!string.IsNullOrEmpty(dispatch) && !string.IsNullOrEmpty(DeliveryInfoId) && !string.IsNullOrEmpty(Customerid))
            {
                
                bool isEnable = true;
                Customer cust = customers.Where(x => x.Customerid == Convert.ToInt32(Customerid)).First();
                DeliveryInfo info = delivery.Where(x => x.DeliveryInfoId == Convert.ToInt32(DeliveryInfoId)).First();
                Customerviewmodel customerviewmodel = new Customerviewmodel();
                customerviewmodel.Customerid = cust.Customerid;
                customerviewmodel.Customer1 = cust.Customer1;
                customerviewmodel.Code = cust.Code;
                customerviewmodel.Branch = cust.Branch;
                customerviewmodel.Address = cust.Address;
                customerviewmodel.GSTIN = cust.GSTIN;
                customerviewmodel.Mobileno = cust.Mobileno;
                customerviewmodel.Watsappno = cust.Watsappno;
                customerviewmodel.State = lststates.Where(x => x.StateId == cust.StateId).First().State1;


                obj.custObj = customerviewmodel;
                obj.delObj = info;
                foreach(var pro in obj.products)
                {
                    pro.Rate = "4.0";
                    pro.TotalRate = "542";
                }
                obj.TotalRate = "542";
                obj.WTotalRate = NumberToText.Convert(Convert.ToDecimal(542.0));

                //int custid = Convert.ToInt32(Customerid);
                //List<DeliveryInfo> lst = db.DeliveryInfoes.ToList();
                //int deliveryid = Convert.ToInt32(DeliveryInfoId);

                //foreach (var res in obj.products)
                //{
                //    Dailyproduction tds = db.Dailyproductions.Where(x => x.Dailyproductionid == res.Dailyprodid).First();
                //    tds.ProductionStatus = "Dispatched";
                //    tds.DispatchedQuantity = res.DispatchedQuantity;
                //    db.Entry(tds).State = EntityState.Modified;
                //    db.SaveChanges();

                //}
                //DispatchOrder order = new DispatchOrder();
                //order.Customerid = custid;
                //order.DeliveryId = deliveryid;
                //order.OrderDate = DateTime.Now;
                //order.Status = "Dispatched";
                //db.DispatchOrders.Add(order);
                //db.SaveChanges();

                //ProductmaterialVM vm = new ProductmaterialVM();

                //List<Productvm> lstpro = new List<Productvm>();
                //vm.products = lstpro;
                //vm.IsDispatchEnabled = false;


                //Generate Pdf
                var html = ToHtml("PrintableInvoice", new ViewDataDictionary(obj), ControllerContext);
                string fname = cust.Customerid.ToString() + "-" + DateTime.Now.Date.ToString("dd-MMM-yyyy");
                var fileName = BuildInvoiceFileName(fname, "pdf");

                BuildFileStream(html, fileName, MediaTypeNames.Application.Pdf, false);

                return View("Dispatch");
            }
            else
            {
                ProductmaterialVM vm = new ProductmaterialVM();

                List<Productvm> lstpro = new List<Productvm>();
                vm.products = lstpro;
                vm.IsDispatchEnabled = false;

                return View(vm);
            }
            
        }

        public ActionResult ExportPDFView()
        {
            ProductmaterialVM obj = new ProductmaterialVM();
            var html = ToHtml("PrintableInvoice", new ViewDataDictionary(obj), ControllerContext);
            var fileName = BuildInvoiceFileName("test", "pdf");

           BuildFileStream(html, fileName, MediaTypeNames.Application.Pdf, false);
            return RedirectToAction("Dispatch");
        }

        private string BuildInvoiceFileName(string value, string fileExtension)
        {
            return string.Format("Invoice_{0}.{1}", value, fileExtension);
        }

        public string ToHtml(string viewToRender, ViewDataDictionary viewData, ControllerContext controllerContext)
        {
            var result = ViewEngines.Engines.FindView(controllerContext, viewToRender, null);

            StringWriter output;
            using (output = new StringWriter())
            {
                var viewContext = new ViewContext(controllerContext, result.View, viewData, controllerContext.Controller.TempData, output);
                result.View.Render(viewContext, output);
                result.ViewEngine.ReleaseView(controllerContext, result.View);
            }

            return output.ToString();
        }

        private void BuildFileStream(string fileContent, string fileName, string contentType, bool showInline)
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(fileContent);

            using (var input = new MemoryStream(bytes))
            {
                var output = new MemoryStream(); // this MemoryStream is closed by FileStreamResult

                var document = new Document(PageSize.A4, 50, 50, 50, 50);
                HttpContext context = System.Web.HttpContext.Current;
                var writer = PdfWriter.GetInstance(document,  new FileStream(context.Server.MapPath("~/Invoices/") + fileName, FileMode.Create));
                writer.CloseStream = false;
                
                document.Open();
                

                var xmlWorker = XMLWorkerHelper.GetInstance();
                xmlWorker.ParseXHtml(writer, document, input, System.Text.Encoding.Default);
                document.Close();
                //output.Position = 0;

                //var cd = new ContentDisposition
                //{
                //    // for example foo.bak
                //    FileName = fileName,

                //    // always prompt the user for downloading, set to true if you want 
                //    // the browser to try to show the file inline
                //    Inline = showInline,
                //};
                //Response.AppendHeader("Content-Disposition", cd.ToString());
                

                ////return new FileStreamResult(output, "application/pdf");
                //return File(output, contentType);
            }
        }

        public PartialViewResult AddStockmodal(int id) 
        {
            
            Rawmaterial rw = db.Rawmaterials.Find(id);
            RawmaterialStock obj = db.RawmaterialStocks.Where(x => x.Rawmaterialid.Value == rw.Rawmaterialid).First();
            RawmaterialVM vm = new RawmaterialVM();
            vm.Rawmaterial = rw.Rawmaterial1;
            vm.RawmaterialStockid = obj.RawmaterialStockid;
            vm.Rawmaterialid = rw.Rawmaterialid;
            vm.Stock = 0;
            return PartialView(vm);
        }

        [HttpPost]
        public JsonResult SaveStockmodal(RawmaterialVM rawmaterials)
        {

            RawmaterialStock stock = db.RawmaterialStocks.Find(rawmaterials.RawmaterialStockid);
            int value = Convert.ToInt32(stock.Instock.Trim());
            stock.Instock = (value + rawmaterials.Stock).ToString();
            db.Entry(stock).State = EntityState.Modified;
            db.SaveChanges();
            return Json("success", JsonRequestBehavior.AllowGet);
        }



        public ActionResult AddStock(int id)
        {
            RawmaterialStock obj = db.RawmaterialStocks.Find(id);
            Rawmaterial rw = db.Rawmaterials.Find(obj.Rawmaterialid.Value);
            RawmaterialVM vm = new RawmaterialVM();
            vm.Rawmaterial = rw.Rawmaterial1;
            vm.RawmaterialStockid = obj.RawmaterialStockid;
            vm.Rawmaterialid = rw.Rawmaterialid;
            vm.Stock = 0;
            return View(vm);
        }

        [HttpPost]
        public ActionResult AddStock(RawmaterialVM obj)
        {
            RawmaterialStock stock = db.RawmaterialStocks.Find(obj.RawmaterialStockid);
            int value = Convert.ToInt32(stock.Instock.Trim());
            stock.Instock = (value + obj.Stock).ToString();
            db.Entry(stock).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}