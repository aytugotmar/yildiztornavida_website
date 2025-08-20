using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models.DataContext;

namespace WebApplication.Controllers
{
    public class ErrorController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();
        // GET: Error
        public ActionResult NotFound()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            //return View();
            Response.StatusCode = 404;                 // SEO ve doğru durum kodu
            Response.TrySkipIisCustomErrors = true;    // IIS’in kendi sayfasını bastır
            return View("NotFound");
        }
    }
}