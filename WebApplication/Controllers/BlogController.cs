using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using WebApplication.Filters;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using WebApplication.Models.ViewModels;

namespace WebApplication.Controllers
{
    [AdminAuthorize(Roles = "Admin,Editor")]
    public class BlogController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();

        // GET: Blog
        public ActionResult Index()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            dB.Configuration.LazyLoadingEnabled = false;
            var blogList = dB.Blog.Include("Category").Include("Author").OrderByDescending(x => x.BlogId).ToList();
            return View(blogList);
        }

        public ActionResult Create()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            ViewBag.CategoryId = new SelectList(dB.Category, "CategoryId", "CategoryName");
            ViewBag.AuthorId = new SelectList(dB.Author, "AuthorId", "Name");

            return View();
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Create(Blog blog, HttpPostedFileBase CoverURL)
        {

            if (CoverURL != null)
            {
                WebImage img = new WebImage(CoverURL.InputStream);
                FileInfo imginfo = new FileInfo(CoverURL.FileName);
                string imgname = CoverURL.FileName;
                img.Resize(1920, 1080);
                img.Save("~/Uploads/Blog/" + imgname);
                blog.CoverURL = "/Uploads/Blog/" + imgname;
            }
            dB.Blog.Add(blog);
            dB.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int? id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var blog = dB.Blog.Where(x => x.BlogId == id)
                              .Include("Category")
                              .Include("Author")
                              .SingleOrDefault();
            if (blog == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryId = new SelectList(dB.Category, "CategoryId", "CategoryName", blog.CategoryId);
            ViewBag.AuthorId = new SelectList(dB.Author, "AuthorId", "Name", blog.AuthorId);
            return View(blog);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult Edit(int id, Blog blog, HttpPostedFileBase CoverURL)
        {
            // Veritabanından mevcut blog nesnesini alın.
            var existingBlog = dB.Blog.Where(x => x.BlogId == id).SingleOrDefault();

            if (existingBlog == null)
            {
                return HttpNotFound();
            }

            // Yalnızca yeni bir resim yüklenmişse bu bloğu çalıştırın.
            if (CoverURL != null && CoverURL.ContentLength > 0)
            {
                // Eski resim dosyasını silin.
                if (!string.IsNullOrEmpty(existingBlog.CoverURL) && System.IO.File.Exists(Server.MapPath(existingBlog.CoverURL)))
                {
                    System.IO.File.Delete(Server.MapPath(existingBlog.CoverURL));
                }

                // Yeni resmi kaydedin.
                WebImage img = new WebImage(CoverURL.InputStream);
                string imgName = Path.GetFileName(CoverURL.FileName);
                string path = Path.Combine(Server.MapPath("~/Uploads/Blog"), imgName);

                img.Resize(1920, 1080);
                img.Save(path);

                // Blog nesnesinin resim URL'sini yeni resimle güncelleyin.
                existingBlog.CoverURL = "/Uploads/Blog/" + imgName;
            }

            // Resim yüklenip yüklenmediğine bakılmaksızın diğer bilgileri güncelleyin.
            existingBlog.Title = blog.Title;
            existingBlog.Text = blog.Text;
            existingBlog.CategoryId = blog.CategoryId;
            existingBlog.AuthorId = blog.AuthorId;
            existingBlog.isHome = blog.isHome;
            existingBlog.UpdatedAt = DateTime.Now;

            // Tüm değişiklikleri veritabanına kaydedin.
            dB.SaveChanges();

            // İşlem başarılıysa Index sayfasına yönlendirin.
            return RedirectToAction("Index");
        }

        public ActionResult Delete(int? id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var blog = dB.Blog.Where(x => x.BlogId == id)
                              .Include("Category")
                              .Include("Author")
                              .SingleOrDefault();
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            var blog = dB.Blog.Find(id);
            if (blog != null)
            {
                // Resim dosyasını sil
                if (!string.IsNullOrEmpty(blog.CoverURL) && System.IO.File.Exists(Server.MapPath(blog.CoverURL)))
                {
                    System.IO.File.Delete(Server.MapPath(blog.CoverURL));
                }
                dB.Blog.Remove(blog);
                dB.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        public ActionResult Details(int? id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var blog = dB.Blog.Where(x => x.BlogId == id)
                              .Include("Category")
                              .Include("Author")
                              .SingleOrDefault();
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                dB.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}