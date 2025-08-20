using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using WebApplication.Filters;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using WebApplication.Models.ViewModels;

namespace WebApplication.Controllers
{
    [AdminAuthorize(Roles = "Admin")]
    public class ContactController : Controller
    {
        private BlogDBContext db = new BlogDBContext();

        // GET: Contact
        public ActionResult Index()
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            var contactList = db.Contact.ToList();
            return View(contactList);
        }

        // GET: Contact/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contact.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // GET: Contact/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Contact contact = db.Contact.Find(id);
            if (contact == null)
            {
                return HttpNotFound();
            }
            return View(contact);
        }

        // POST: Contact/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ContactId,Adress,Fax,Tel,Email,Instagram,Facebook,Tiktok,Youtube,X,Linkedin")] Contact contact)
        {
            if (ModelState.IsValid)
            {
                db.Entry(contact).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(contact);
        }
    }
}
