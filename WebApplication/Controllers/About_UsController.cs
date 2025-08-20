using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication.Filters;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using WebApplication.Models.ViewModels;

namespace WebApplication.Controllers
{
    [AdminAuthorize(Roles = "Admin")]
    public class About_UsController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();
        // GET: About_Us
        public ActionResult Index()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var aboutUsList = dB.About_Us.ToList();
            return View(aboutUsList);
        }
        public ActionResult Edit(int id) 
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var h = dB.About_Us.Where(x => x.AboutId == id).FirstOrDefault();
            return View(h);
        }

        //POST: About_Us/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, About_Us about)
        {
            if (ModelState.IsValid)
            {
                var h = dB.About_Us.Where(x => x.AboutId == id).SingleOrDefault();
                h.Text = about.Text;
                dB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(about);
        }

    }
}