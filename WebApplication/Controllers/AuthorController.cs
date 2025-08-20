using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Services.Description;
using WebApplication.Filters;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using WebApplication.Models.ViewModels;

namespace WebApplication.Controllers
{
    [AdminAuthorize(Roles = "Admin")]
    public class AuthorController : Controller
    {
        private BlogDBContext db = new BlogDBContext();

        // GET: Author
        public ActionResult Index()
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            var authorList = db.Author.ToList();
            return View(authorList);
        }

        // GET: Author/Details/5
        public ActionResult Details(int? id)
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Author author = db.Author.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            return View(author);
        }

        // GET: Author/Create
        public ActionResult Create()
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            return View();
        }

        // POST: Author/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Author author, HttpPostedFileBase ImageURL)
        {
            if (ModelState.IsValid)
            {
                if (ImageURL != null)
                {
                    WebImage img = new WebImage(ImageURL.InputStream);
                    FileInfo imginfo = new FileInfo(ImageURL.FileName);
                    string logoname = ImageURL.FileName;
                    img.Resize(400, 400);
                    img.Save("~/Uploads/Author/" + logoname);
                    author.ImageURL = "/Uploads/Author/" + logoname;
                }

                db.Author.Add(author);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(author);
        }

        // GET: Author/Edit/5
        public ActionResult Edit(int? id)
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var author = db.Author.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            return View(author);
        }

        // POST: Author/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(Author author, HttpPostedFileBase ImageURL)
        {
            var a = db.Author.Find(author.AuthorId); // id’yi modelden al
            if (a == null)
                return HttpNotFound();
            if (ModelState.IsValid)
            {
                if (ImageURL != null)
                {
                    if (System.IO.File.Exists(Server.MapPath(a.ImageURL)))
                    {
                        System.IO.File.Delete(Server.MapPath(a.ImageURL));
                    }
                    WebImage img = new WebImage(ImageURL.InputStream);
                    FileInfo imginfo = new FileInfo(ImageURL.FileName);

                    string imgname = ImageURL.FileName;
                    img.Resize(400, 400);
                    img.Save("~/Uploads/Author/" + imgname);
                    a.ImageURL = "/Uploads/Author/" + imgname;
                }
                a.Name = author.Name;
                a.Bio = author.Bio;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(a);
        }

        // GET: Author/Delete/5
        public ActionResult Delete(int? id)
        {
            ViewBag.SiteLogo = db.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Author author = db.Author.Find(id);
            if (author == null)
            {
                return HttpNotFound();
            }
            return View(author);
        }

        // POST: Author/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Author author = db.Author.Find(id);
            db.Author.Remove(author);
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
