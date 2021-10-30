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
    public class DeliveryInfoesController : Controller
    {
        private BakeryprojEntities db = new BakeryprojEntities();

        // GET: DeliveryInfoes
        public ActionResult Index()
        {
            return View(db.DeliveryInfoes.ToList());
        }

        // GET: DeliveryInfoes/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeliveryInfo deliveryInfo = db.DeliveryInfoes.Find(id);
            if (deliveryInfo == null)
            {
                return HttpNotFound();
            }
            return View(deliveryInfo);
        }

        // GET: DeliveryInfoes/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: DeliveryInfoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DeliveryInfoId,DeliveryBoyName,VehicleNo,WatsappNo,Username,Password,IsActive")] DeliveryInfo deliveryInfo)
        {
            if (ModelState.IsValid)
            {
                db.DeliveryInfoes.Add(deliveryInfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(deliveryInfo);
        }

        // GET: DeliveryInfoes/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeliveryInfo deliveryInfo = db.DeliveryInfoes.Find(id);
            if (deliveryInfo == null)
            {
                return HttpNotFound();
            }
            return View(deliveryInfo);
        }

        // POST: DeliveryInfoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "DeliveryInfoId,DeliveryBoyName,WatsappNo,VehicleNo,Username,Password,IsActive")] DeliveryInfo deliveryInfo)
        {
            if (ModelState.IsValid)
            {
                db.Entry(deliveryInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(deliveryInfo);
        }

        // GET: DeliveryInfoes/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeliveryInfo deliveryInfo = db.DeliveryInfoes.Find(id);
            if (deliveryInfo == null)
            {
                return HttpNotFound();
            }
            return View(deliveryInfo);
        }

        // POST: DeliveryInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            DeliveryInfo deliveryInfo = db.DeliveryInfoes.Find(id);
            db.DeliveryInfoes.Remove(deliveryInfo);
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
