using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using WebApplication.Models.ViewModels;


namespace WebApplication.Controllers
{
    public class ServicesController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();
        // GET: Services
        public ActionResult Index()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var servicesList = dB.Services.ToList();
            return View(servicesList);
        }
        public ActionResult Create()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View();
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Services service, HttpPostedFileBase ImageURL)
        {
            if (ModelState.IsValid)
            {
                if (ImageURL != null)
                {
                    WebImage img = new WebImage(ImageURL.InputStream);
                    FileInfo imginfo = new FileInfo(ImageURL.FileName);
                    string logoname = ImageURL.FileName;
                    img.Resize(800, 600);
                    img.Save("~/Uploads/Services/" + logoname);
                    service.ImageURL = "/Uploads/Services/" + logoname;
                }

                dB.Services.Add(service);
                dB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(service);
        }
        public ActionResult Edit(int? id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var service = dB.Services.Find(id);
            if (service == null)
            {
                return HttpNotFound();
            }
            return View(service);
        }
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Edit(int? id, Services service, HttpPostedFileBase ImageURL)
        {
            var s = dB.Services.Where(x => x.ServiceId == id).SingleOrDefault();
            if (ModelState.IsValid)
            {
                if (ImageURL != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(s.ImageURL)))
                    {
                        System.IO.File.Delete(Server.MapPath(s.ImageURL));
                    }
                    WebImage img = new WebImage(ImageURL.InputStream);
                    FileInfo imginfo = new FileInfo(ImageURL.FileName);

                    string imgname = ImageURL.FileName;
                    img.Resize(800, 600);
                    img.Save("~/Uploads/Services/" + imgname);
                    s.ImageURL = "/Uploads/Services/" + imgname;
                }
                s.Title = service.Title;
                s.Description = service.Description;
                dB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(service);
        }
        public ActionResult Delete(int? id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var s = dB.Services.Find(id);
            if (s == null)
            {
                return HttpNotFound();
            }
            dB.Services.Remove(s);
            dB.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}