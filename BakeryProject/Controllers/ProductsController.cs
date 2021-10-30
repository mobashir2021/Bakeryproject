using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using BakeryProject.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace BakeryProject.Controllers
{
    public class ProductsController : Controller
    {
        private BakeryprojEntities db = new BakeryprojEntities();

        // GET: Products
        public ActionResult Index()
        {
            return View(db.Products.ToList());
        }

        // GET: Products/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create()
        {

            return View();
        }

        public ActionResult GetRawmaterial(int id)
        {
            ProductmaterialVM vm = new ProductmaterialVM();
            Product prod = db.Products.Where(x => x.Productid == id).First();
            vm.Product = prod.Product1;
            vm.Productid = prod.Productid;
            List<RawmaterialVM> raw = new List<RawmaterialVM>();
            List<Rawmaterial> rawmaterials = db.Rawmaterials.ToList();
            var lst = db.Productmaterials.Where(x => x.Productid.Value == id).ToList();
            foreach(var temp in lst)
            {
                RawmaterialVM rm = new RawmaterialVM();
                Rawmaterial mat = rawmaterials.Where(x => x.Rawmaterialid == temp.Rawmaterialid.Value).First();
                rm.Productmaterialid = temp.Productmaterialid;
                rm.Productid = id;
                rm.Rawmaterial = mat.Rawmaterial1;
                rm.Rawmaterialid = mat.Rawmaterialid;
                rm.Units = mat.Unit;
                rm.Quantity = temp.Quantity;
                raw.Add(rm);
            }

            vm.lstmaterials = raw;
            return View(vm);
        }

        public ActionResult DeleteRawmaterial(int id)
        {
            return View();
        }

        [HttpPost]
        public ActionResult savedata(int id, string propertyName, string value)
        {
            var status = false;
            var message = "";

            //Update data to database 
            
            var mat = db.Productmaterials.Find(id);
            object updateValue = value;
            bool isValid = true;
            //if (propertyName == "Rawmaterial1")
            //{
            //    int newRoleID = 0;
            //    if (int.TryParse(value, out newRoleID))
            //    {
            //        updateValue = newRoleID;
            //        //Update value field
            //        value = db.Rawmaterials.Where(a => a.Rawmaterialid == newRoleID).First().Rawmaterialid.ToString();
            //    }
            //    else
            //    {
            //        isValid = false;
            //    }

            //}
            if (mat != null)
            {
                if (propertyName == "Rawmaterialid")
                {
                    db.Entry(mat).Property(propertyName).CurrentValue = Convert.ToInt32(value);
                }
                else
                {
                    db.Entry(mat).Property(propertyName).CurrentValue = value;
                }
                
                db.SaveChanges();
                status = true;
            }
            else
            {
                message = "Error!";
            }
            

            var response = new { value = value, status = status, message = message };
            JObject o = JObject.FromObject(response);
            return Content(o.ToString());
        }

        public ActionResult GetDropdown(int id)
        {
            //Allowed response content format
            //{'E':'Letter E','F':'Letter F','G':'Letter G', 'selected':'F'}
            int selectedRoleID = 0;
            StringBuilder sb = new StringBuilder();
            
                var roles = db.Rawmaterials.OrderBy(a => a.Rawmaterial1).ToList();
                foreach (var item in roles)
                {
                    sb.Append(string.Format("'{0}':'{1}',", item.Rawmaterialid, item.Rawmaterial1));
                }

                selectedRoleID = db.Productmaterials.Where(a => a.Productmaterialid == id).First().Rawmaterialid.Value;

            

            sb.Append(string.Format("'selected': '{0}'", selectedRoleID));
            string final = "{" + sb.ToString() + "}";


            return Content(final );
            
            //var result = new JsonResult
            //{
            //    Data = JsonConvert.DeserializeObject(sb.ToString())
            //};
            //return result;
        }

        public PartialViewResult Newrawmaterial(string productid)
        {
            
            List<Rawmaterial> rawmaterial = db.Rawmaterials.ToList();
            
            ViewBag.Rawmaterial = new SelectList(rawmaterial, "Rawmaterialid", "Rawmaterial1");
            RawmaterialVM vm = new RawmaterialVM();
            vm.Productid = Convert.ToInt32(productid);
            return PartialView(vm);
        }

        [HttpPost]
        public ActionResult Newrawmaterial(RawmaterialVM raw)
        {
            Productmaterial obj = new Productmaterial();
            obj.Productid = raw.Productid;
            obj.Rawmaterialid = raw.Rawmaterialid;
            obj.Quantity = raw.Stock.ToString();
            db.Productmaterials.Add(obj);
            db.SaveChanges();

            return RedirectToAction("Index");
        }

        // POST: Products/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Productid,Product1,Unit,Baseprice,HSNCode,AltQty,CGST,SGST")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Productid,Product1,Unit,Baseprice,HSNCode,AltQty,CGST,SGST")] Product product)
        {
            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
