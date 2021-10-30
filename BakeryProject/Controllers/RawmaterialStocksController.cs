using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BakeryProject.Models;

namespace BakeryProject.Controllers
{
    public class RawmaterialStocksController : Controller
    {
        private BakeryprojEntities db = new BakeryprojEntities();

        // GET: RawmaterialStocks
        public ActionResult Index()
        {
            List<RawmaterialVM> lst = new List<RawmaterialVM>();
            var temp = db.RawmaterialStocks.ToList();
            var rw = db.Rawmaterials.ToList();
            foreach(var data in temp)
            {
                RawmaterialVM vm = new RawmaterialVM();
                Rawmaterial obj = rw.Where(x => x.Rawmaterialid == data.Rawmaterialid.Value).FirstOrDefault();
                vm.RawmaterialStockid = data.RawmaterialStockid;
                if(obj != null)
                {
                    vm.Rawmaterial = obj.Rawmaterial1;
                    vm.Rawmaterialid = obj.Rawmaterialid;
                    vm.Units = obj.Unit;
                    vm.Stock = Convert.ToInt32(data.Instock);

                }
                lst.Add(vm);
            }
            return View(lst);
        }

       

        // GET: RawmaterialStocks/Create
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

        public ActionResult Create()
        {
            List<Rawmaterial> rawmaterial = db.Rawmaterials.ToList();

            ViewBag.Rawmaterial = new SelectList(rawmaterial, "Rawmaterialid", "Rawmaterial1");
            RawmaterialVM vm = new RawmaterialVM();
            
            return View(vm);
        }

        [HttpPost]
        public ActionResult Create(RawmaterialVM vm)
        {
            RawmaterialStock obj = new RawmaterialStock();
            obj.Rawmaterialid = vm.Rawmaterialid;
            obj.Instock = Convert.ToString(vm.Stock);
            db.RawmaterialStocks.Add(obj);
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
