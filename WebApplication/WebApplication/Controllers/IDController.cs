using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using WebApplication.Filters;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using WebApplication.Models.ViewModels;

namespace WebApplication.Controllers
{
    [AdminAuthorize(Roles = "Admin")]
    public class IDController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();
        // GET: ID
        public ActionResult Index()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var idList = dB.ID.ToList();
            return View(idList);
        }

        // GET: ID/Edit/5
        public ActionResult Edit(int id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var ID = dB.ID.Where(x => x.IdentityId == id).SingleOrDefault();
            return View(ID);
        }

        //POST: ID/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, ID identity, HttpPostedFileBase LogoURL)
        {
            if (ModelState.IsValid)
            {
                var k = dB.ID.Where(x => x.IdentityId == id).SingleOrDefault();
                if (LogoURL != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(k.LogoURL)))
                    {
                        System.IO.File.Delete(Server.MapPath(k.LogoURL));
                    }
                    WebImage img = new WebImage(LogoURL.InputStream);
                    FileInfo imginfo = new FileInfo(LogoURL.FileName);

                    string logoname = LogoURL.FileName;
                    img.Resize(250, 250);
                    img.Save("~/Uploads/ID/" + logoname);
                    k.LogoURL = "/Uploads/ID/" + logoname;
                }
                k.Title = identity.Title;
                k.Keywords = identity.Keywords;
                k.Description = identity.Description;
                k.Slogan = identity.Slogan;
                dB.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(identity);
        }
    }
}
