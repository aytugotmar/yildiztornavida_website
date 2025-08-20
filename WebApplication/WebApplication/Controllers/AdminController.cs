using PagedList;
using PagedList.Mvc;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Security;
using System.Web.Services.Description;
using System.Xml.Linq;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;
using WebApplication.Models.ViewModels;

namespace WebApplication.Controllers
{
    public class AdminController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();

        // Bu metot, sadece Admin yetkisine sahip kullanıcılar tarafından erişilebilir.
        [AdminAuthorize(Roles = "Admin")]
        // GET: Admin
        public ActionResult Index()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var adminList = dB.Admin.ToList();
            
            // Onay bekleyen yorum sayısını hesapla
            ViewBag.PendingCommentCount = dB.Comment.Count(x => !x.isApproved);
            
            return View(adminList);
        }

        // Bu sayfa, giriş yapmış olsun veya olmasın, herkese açıktır.
        [AllowAnonymous]
        public ActionResult Login()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(Admin admin, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                var loggedInUser = dB.Admin.FirstOrDefault(x => x.Email == admin.Email && x.Password == admin.Password);

                if (loggedInUser != null)
                {
                    // Clear any stale anti-forgery cookies to avoid token mismatches after login
                    try
                    {
                        var antiCookieName = string.IsNullOrEmpty(AntiForgeryConfig.CookieName)
                            ? "__RequestVerificationToken" : AntiForgeryConfig.CookieName;
                        if (Request.Cookies[antiCookieName] != null)
                        {
                            var expired = new HttpCookie(antiCookieName, "") { Expires = DateTime.UtcNow.AddDays(-1), HttpOnly = true, Path = "/" };
                            Response.Cookies.Add(expired);
                        }
                        if (Request.Cookies["__RequestVerificationToken"] != null && antiCookieName != "__RequestVerificationToken")
                        {
                            var expired2 = new HttpCookie("__RequestVerificationToken", "") { Expires = DateTime.UtcNow.AddDays(-1), HttpOnly = true, Path = "/" };
                            Response.Cookies.Add(expired2);
                        }
                    }
                    catch { }

                    // Kullanıcının yetkisini Session'a kaydediyoruz
                    Session["Admin"] = loggedInUser.AdminId;
                    Session["Authority"] = loggedInUser.Authority;

                    // Forms Authentication ile giriş yap
                    FormsAuthentication.SetAuthCookie(loggedInUser.Email, false);

                    // Return URL varsa oraya yönlendir, yoksa yetkiye göre yönlendir
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    else
                    {
                        // Yetkiye göre doğru sayfaya yönlendirme yapıyoruz
                        if (loggedInUser.Authority == "Admin")
                        {
                            return RedirectToAction("Index");
                        }
                        else if (loggedInUser.Authority == "Editor")
                        {
                            return RedirectToAction("Index", "Blog"); // Editör için Blog sayfasına yönlendir
                        }
                        else if (loggedInUser.Authority == "Member")
                        {
                            return RedirectToAction("Index", "Home");
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "E-posta veya parola yanlış.");
                }
            }
            return View(admin);
        }

        [AdminAuthorize]
        public ActionResult Logout()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            Session.Clear();
            FormsAuthentication.SignOut();
            // Also clear anti-forgery cookies on logout
            try
            {
                var antiCookieName = string.IsNullOrEmpty(AntiForgeryConfig.CookieName)
                    ? "__RequestVerificationToken" : AntiForgeryConfig.CookieName;
                var expired = new HttpCookie(antiCookieName, "") { Expires = DateTime.UtcNow.AddDays(-1), HttpOnly = true, Path = "/" };
                Response.Cookies.Add(expired);
                if (antiCookieName != "__RequestVerificationToken")
                {
                    var expired2 = new HttpCookie("__RequestVerificationToken", "") { Expires = DateTime.UtcNow.AddDays(-1), HttpOnly = true, Path = "/" };
                    Response.Cookies.Add(expired2);
                }
            }
            catch { }
            return RedirectToAction("Login");
        }

        [AllowAnonymous]
        public ActionResult Signup()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public ActionResult Signup(Admin admin, string returnUrl = null)
        {
            if (ModelState.IsValid)
            {
                // E-posta adresinin daha önce kullanılıp kullanılmadığını kontrol et
                var existingUser = dB.Admin.FirstOrDefault(x => x.Email == admin.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("Email", "Bu e-posta adresi zaten kullanılmaktadır.");
                    return View(admin);
                }

                admin.Authority = "Member";
                dB.Admin.Add(admin);
                dB.SaveChanges();

                // Yeni kayıt olan kullanıcıyı otomatik giriş yaptır
                Session["Admin"] = admin.AdminId;
                Session["Authority"] = admin.Authority;
                FormsAuthentication.SetAuthCookie(admin.Email, false);

                // Return URL varsa oraya yönlendir, yoksa ana sayfaya
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                else
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            return View(admin);
        }
        [AllowAnonymous]
        public ActionResult ForgetPassword()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View();  
        }
        [AllowAnonymous]
        [HttpPost]
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı güvenlik önlemi
        public ActionResult ForgetPassword(string email)
        {
            var mail = dB.Admin.Where(x => x.Email == email).SingleOrDefault();
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            if (string.IsNullOrWhiteSpace(email))
            {
                TempData["Message"] = "⚠️ Lütfen tüm alanları doldurun!";
                return RedirectToAction("ForgetPassword");
            }

            try
            {
                Random rnd = new Random();
                int newpass = rnd.Next(10000000, 99999999); // 8 haneli rastgele şifre oluştur

                mail.Password = Convert.ToString(newpass); 
                dB.SaveChanges();

                WebMail.SmtpServer = "smtp.yandex.com";
                WebMail.EnableSsl = true;
                WebMail.UserName = ConfigurationManager.AppSettings["MailUser"];
                WebMail.Password = ConfigurationManager.AppSettings["MailPassword"];
                WebMail.SmtpPort = 587;

                WebMail.Send(
                    to: HttpUtility.HtmlEncode(email),
                    subject: "Yeni Şifreniz",
                    body: "Girdiğiniz Eposta: " + HttpUtility.HtmlEncode(email) + "<br>" +
                          newpass,
                    from: ConfigurationManager.AppSettings["MailUser"],
                    isBodyHtml: true
                );

                TempData["Message"] = "✅ Mail başarıyla iletildi eposta adresinizi kontrol ediniz!";
            }
            catch (Exception ex)
            {
                TempData["Message"] = "❌ Mail gönderilemedi: " + ex.Message;
            }

            // Redirect-After-Post: F5 ile tekrar mail gitmez
            return RedirectToAction("ForgetPassword");
        }
        // GET: Admin/Delete/5 -> Bu metot, silme onayı sayfasını gösterir.
        [AdminAuthorize(Roles = "Admin")]
        public ActionResult Delete(int id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var admin = dB.Admin.Find(id);
            if (admin == null)
            {
                return HttpNotFound();
            }
            return View(admin); // Onay sayfasını göster
        }

        // POST: Admin/Delete/5 -> Bu metot, onaylandıktan sonra silme işlemini yapar.
        [AdminAuthorize(Roles = "Admin")]
        [HttpPost, ActionName("Delete")] // URL'yi "Delete" olarak koru
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı güvenlik önlemi
        public ActionResult DeleteConfirmed(int id)
        {
            var admin = dB.Admin.Find(id);
            if (admin != null)
            {
                dB.Admin.Remove(admin);
                dB.SaveChanges();
                return RedirectToAction("Index");
            }
            return HttpNotFound();
        }

        [AdminAuthorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var admin = dB.Admin.Find(id);
            if (admin != null)
            {
                // Edit sayfasında da yetki dropdown menüsü için veriyi hazırlıyoruz
                var authorityRoles = new List<SelectListItem>
                {
                    new SelectListItem { Value = "Admin", Text = "Admin" },
                    new SelectListItem { Value = "Editor", Text = "Editör" },
                    new SelectListItem { Value = "Member", Text = "Üye" }
                };
                ViewBag.AuthorityRoles = new SelectList(authorityRoles, "Value", "Text", admin.Authority);

                return View(admin);
            }
            return HttpNotFound();
        }

        [AdminAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Edit(Admin admin)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingAdmin = dB.Admin.Find(admin.AdminId);
                    if (existingAdmin != null)
                    {
                        existingAdmin.Email = admin.Email;
                        existingAdmin.Password = admin.Password;
                        existingAdmin.Name = admin.Name;
                        existingAdmin.Surname = admin.Surname;
                        existingAdmin.Authority = admin.Authority;
                        dB.SaveChanges();
                        TempData["Message"] = "✅ Kullanıcı başarıyla güncellendi.";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        TempData["Message"] = "❌ Kullanıcı bulunamadı.";
                    }
                }
                catch (Exception ex)
                {
                    TempData["Message"] = $"❌ Hata oluştu: {ex.Message}";
                }
            }
            else
            {
                TempData["Message"] = "❌ Lütfen tüm alanları doğru şekilde doldurun.";
            }

            // Hata durumunda, ViewBag'i tekrar yükleyin ki dropdown menü görünmeye devam etsin
            var authorityRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Editor", Text = "Editör" },
                new SelectListItem { Value = "Member", Text = "Üye" }
            };
            ViewBag.AuthorityRoles = new SelectList(authorityRoles, "Value", "Text", admin.Authority);
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();

            return View(admin);
        }

        [AdminAuthorize(Roles = "Admin")]
        public ActionResult Details(int id)
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            var admin = dB.Admin.Find(id);
            if (admin != null)
            {
                return View(admin);
            }
            return HttpNotFound();
        }

        [AdminAuthorize(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            // Create sayfasında yetki dropdown menüsü için veriyi hazırlıyoruz
            var authorityRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Editor", Text = "Editör" },
                new SelectListItem { Value = "Member", Text = "Üye" }
            };
            ViewBag.AuthorityRoles = authorityRoles;
            return View();
        }

        [AdminAuthorize(Roles = "Admin")]
        [HttpPost]
        public ActionResult Create(Admin admin)
        {
            if (ModelState.IsValid)
            {
                dB.Admin.Add(admin);
                dB.SaveChanges();
                return RedirectToAction("Index");
            }
            // Hata durumunda, ViewBag'i tekrar yükleyin ki dropdown menü görünmeye devam etsin
            var authorityRoles = new List<SelectListItem>
            {
                new SelectListItem { Value = "Admin", Text = "Admin" },
                new SelectListItem { Value = "Editor", Text = "Editor" },
                new SelectListItem { Value = "Member", Text = "Member" }
            };
            ViewBag.AuthorityRoles = authorityRoles;
            return View(admin);
        }
    }
}
