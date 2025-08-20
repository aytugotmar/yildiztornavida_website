using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using PagedList;
using PagedList.Mvc;

namespace WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();
        public ActionResult Search(string query, int Page = 1)
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (string.IsNullOrEmpty(query))
            {
                return View(new List<Blog>()); // boş liste döndür
            }

            var results = dB.Blog
                            .Include("Category")
                            .Include("Author")
                            .Where(b => b.Title.Contains(query) || b.Text.Contains(query))
                            .OrderByDescending(b => b.CreatedAt)
                            .ToPagedList(Page,10);

            ViewBag.Query = query; // aranan kelimeyi view'da göstermek için
            return View(results);   // Search.cshtml view'ına gönder
        }
        // GET: Home
        public ActionResult Index()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View();
        }
        public ActionResult PartialFooter()
        {
            ViewBag.Blog = dB.Blog.Include("Author").ToList().OrderByDescending(x => x.UpdatedAt > x.CreatedAt ? x.UpdatedAt : x.CreatedAt);
            ViewBag.Contact = dB.Contact.SingleOrDefault();
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return PartialView();
        }
        public ActionResult PartialHeader()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return PartialView();
        }
        public ActionResult PartialSlider()
        {
            var slider = dB.Blog.Include("Author").Where(x => x.isHome).ToList().OrderByDescending(x => x.UpdatedAt > x.CreatedAt ? x.UpdatedAt : x.CreatedAt);
            return PartialView(slider);
        }
        public ActionResult PartialWork()
        {
            var work = dB.Services.ToList().OrderByDescending(x => x.ServiceId);
            return PartialView(work);
        }
        // Home/About
        public ActionResult About()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View(dB.About_Us.SingleOrDefault());
        }
        // Home/Contact
        [HttpGet]
        public ActionResult Contact()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            // İlk açılışta sadece sayfayı göster
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Contact(string name, string email, string subject, string message)
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (string.IsNullOrWhiteSpace(name) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(subject) ||
                string.IsNullOrWhiteSpace(message))
            {
                TempData["Message"] = "⚠️ Lütfen tüm alanları doldurun!";
                return RedirectToAction("Contact");
            }

            try
            {
                WebMail.SmtpServer = "smtp.yandex.com";
                WebMail.EnableSsl = true;
                WebMail.UserName = ConfigurationManager.AppSettings["MailUser"];
                WebMail.Password = ConfigurationManager.AppSettings["MailPassword"];
                WebMail.SmtpPort = 587;

                WebMail.Send(
                    to: "yildiztornavidablog@gmail.com",
                    subject: subject,
                    body: "Gönderen Eposta: " + HttpUtility.HtmlEncode(email) + "<br>" + "Gönderen Ad Soyad: " + HttpUtility.HtmlEncode(name) + "<br>" +
                          HttpUtility.HtmlEncode(message),
                    from: ConfigurationManager.AppSettings["MailUser"],
                    isBodyHtml: true
                );

                TempData["Message"] = "✅ Mesajınız başarıyla iletildi!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Mesaj gönderilemedi: " + ex.Message;
            }

            // Redirect-After-Post: F5 ile tekrar mail gitmez
            return RedirectToAction("Contact");
        }
        // Home/Blog
        public ActionResult Blog(int Page=1)
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var blog = dB.Blog.Include("Category").Include("Author").OrderByDescending(x => x.UpdatedAt > x.CreatedAt ? x.UpdatedAt : x.CreatedAt).ToPagedList(Page, 10);
            return View(blog);
        }
        public ActionResult BlogbyCategory(int? id, int Page = 1)
        {
            // 1. Kategori ID'si null ise veya veritabanında böyle bir kategori yoksa 404 döndür
            if (id == null)
            {
                return HttpNotFound();
            }

            // Kategori nesnesini kontrol etmek için veritabanından çekiyoruz
            var category = dB.Category.Find(id.Value);
            if (category == null)
            {
                return HttpNotFound();
            }

            // Kategori adını ve açıklamasını ViewBag'e ekliyoruz
            ViewBag.CategoryName = category.CategoryName;
            ViewBag.CategoryDescription = category.Description;
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            ViewBag.CategoryId = id.Value; // Pagination için kategori ID'sini view'e gönderiyoruz

            // 2. Kategoriye ait blog yazılarını alıyoruz. Boş gelmesi sorun değil, view'de kontrol edeceğiz.
            var blog = dB.Blog
                .Include("Category")
                .Include("Author")
                .Where(x => x.CategoryId == id.Value)
                .OrderByDescending(x => x.UpdatedAt > x.CreatedAt ? x.UpdatedAt : x.CreatedAt)
                .ToPagedList(Page, 10);

            return View(blog);
        }
        public ActionResult BlogbyAuthor(int? id, int Page = 1)
        {
            // 1. Author ID'si null ise veya veritabanında böyle bir kategori yoksa 404 döndür
            if (id == null)
            {
                return HttpNotFound();
            }

            // Author nesnesini kontrol etmek için veritabanından çekiyoruz
            var author = dB.Author.Find(id.Value);
            if (author == null)
            {
                return HttpNotFound();
            }

            // Yazar adını ve açıklamasını ViewBag'e ekliyoruz
            ViewBag.AuthorName = author.Name;
            ViewBag.AuthorBio = author.Bio;
            ViewBag.AuthorImage = author.ImageURL;
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            ViewBag.AuthorId = id.Value; // Pagination için kategori ID'sini view'e gönderiyoruz

            // 2. Kategoriye ait blog yazılarını alıyoruz. Boş gelmesi sorun değil, view'de kontrol edeceğiz.
            var blog = dB.Blog
                .Include("Category")
                .Include("Author")
                .Where(x => x.AuthorId == id.Value)
                .OrderByDescending(x => x.UpdatedAt > x.CreatedAt ? x.UpdatedAt : x.CreatedAt)
                .ToPagedList(Page, 10);

            return View(blog);
        }
        public ActionResult PartialCategory()
        {
            var category = dB.Category.Include("Blogs").ToList().OrderBy(x => x.CategoryName);
            return PartialView(category);
        }
        public ActionResult PartialPosts()
        {
            var posts = dB.Blog.ToList().OrderByDescending(x => x.UpdatedAt > x.CreatedAt ? x.UpdatedAt : x.CreatedAt);
            return PartialView(posts);
        }
        // Home/BlogDetail
        public ActionResult BlogDetail(int id)
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var blog = dB.Blog.Include("Category").Include("Author").SingleOrDefault(x => x.BlogId == id);
            if (blog == null)
            {
                return HttpNotFound();
            }
            return View(blog);
        }
        public PartialViewResult SimilarPostPartial(int? currentPostId, int? categoryId)
        {
            if (currentPostId.HasValue && categoryId.HasValue)
            {
                // Veritabanından mevcut yazıyla aynı kategoride olanları sorgula
                var similarPosts = dB.Blog
                                       .Where(x => x.CategoryId == categoryId.Value && x.BlogId != currentPostId.Value)
                                       .OrderByDescending(x => x.CreatedAt) // En yeni yazıları al
                                       .Take(5) // İlk 5 tanesini al
                                       .ToList();

                return PartialView(similarPosts);
            }

            // Eğer gerekli parametreler gelmezse, boş veya başka bir liste döndürebilirsiniz.
            // Örneğin:
            return PartialView(new List<Blog>());
        }

        // Comments
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddComment(Comment comment, int blogId)
        {
            // Kullanıcının giriş yapıp yapmadığını kontrol et
            if (!User.Identity.IsAuthenticated || Session["Admin"] == null)
            {
                TempData["Message"] = "⚠️ Yorum yapmak için giriş yapmanız gerekmektedir.";
                return RedirectToAction("BlogDetail", new { id = blogId });
            }

            // Yorum metnini kontrol et
            if (string.IsNullOrWhiteSpace(comment.CommentText))
            {
                TempData["Message"] = "⚠️ Lütfen yorum metnini girin!";
                return RedirectToAction("BlogDetail", new { id = blogId });
            }

            try
            {
                // Oturum açmış kullanıcının bilgilerini al
                int adminId = (int)Session["Admin"];
                var currentUser = dB.Admin.Find(adminId);
                if (currentUser == null)
                {
                    TempData["Message"] = "❌ Kullanıcı bilgisi bulunamadı. Lütfen tekrar giriş yapın.";
                    return RedirectToAction("BlogDetail", new { id = blogId });
                }

                // Blog'un var olup olmadığını kontrol et
                var blog = dB.Blog.Find(blogId);
                if (blog == null)
                {
                    TempData["Message"] = "❌ Blog yazısı bulunamadı.";
                    return RedirectToAction("BlogDetail", new { id = blogId });
                }

                // Yeni yorum oluştur
                var newComment = new Comment
                {
                    CommentText = comment.CommentText.Trim(),
                    AdminId = currentUser.AdminId,
                    BlogId = blogId,
                    isApproved = false, // Varsayılan olarak onay bekliyor
                    CreatedAt = DateTime.Now
                };

                dB.Comment.Add(newComment);
                dB.SaveChanges();

                TempData["Message"] = "✅ Yorumunuz başarıyla gönderildi ve onay bekliyor.";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Yorum eklenirken bir hata oluştu: " + ex.Message;
            }

            return RedirectToAction("BlogDetail", new { id = blogId });
        }

        // Yorumları listelemek için partial view döndürecek action
        public PartialViewResult CommentList(int blogId)
        {
            // Onaylanmış ve ilgili bloga ait yorumları çek
            var comments = dB.Comment
                             .Include("Admin") // Yorum sahibinin bilgilerini almak için Admin tablosunu yükle
                             .Where(x => x.BlogId == blogId && x.isApproved)
                             .OrderByDescending(x => x.CreatedAt)
                             .ToList();

            return PartialView(comments);
        }
    }
}