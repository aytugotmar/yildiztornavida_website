using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;

namespace WebApplication.Controllers
{
    public class ProfileController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();

        // GET: Profile/Edit - Member ve Editor rolleri için kendi profillerini düzenleyebilir
        public ActionResult Edit()
        {
            ViewBag.ID = dB.ID.SingleOrDefault();
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            // Kullanıcı giriş yapmış mı kontrol et
            if (!User.Identity.IsAuthenticated || Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            int adminId = (int)Session["Admin"];
            var admin = dB.Admin.Find(adminId);

            if (admin == null)
            {
                return HttpNotFound();
            }

            // Admin ise admin paneline yönlendir
            if (admin.Authority == "Admin")
            {
                return RedirectToAction("Edit", "Admin", new { id = adminId });
            }

            // Member ve Editor rolleri için profil düzenleme sayfasını göster
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View(admin);
        }

        // POST: Profile/Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Admin admin)
        {
            // Kullanıcı giriş yapmış mı kontrol et
            if (!User.Identity.IsAuthenticated || Session["Admin"] == null)
            {
                return RedirectToAction("Login", "Admin");
            }

            int currentAdminId = (int)Session["Admin"];
            var currentAdmin = dB.Admin.Find(currentAdminId);

            if (currentAdmin == null)
            {
                return HttpNotFound();
            }

            // Sadece kendi profilini düzenleyebilir
            if (admin.AdminId != currentAdminId)
            {
                TempData["Message"] = "❌ Sadece kendi profilinizi düzenleyebilirsiniz.";
                return RedirectToAction("Edit");
            }

            // Admin ise admin paneline yönlendir
            if (currentAdmin.Authority == "Admin")
            {
                return RedirectToAction("Edit", "Admin", new { id = currentAdminId });
            }

            // Member ve Editor rolleri için profil düzenleme işlemini yap

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAdmin = dB.Admin.Find(admin.AdminId);
                    if (existingAdmin != null)
                    {
                        // Sadece temel bilgileri güncelle, rolü değiştirme
                        existingAdmin.Name = admin.Name;
                        existingAdmin.Surname = admin.Surname;
                        existingAdmin.Email = admin.Email;
                        
                        // Şifre değiştirilmek isteniyorsa
                        if (!string.IsNullOrEmpty(admin.Password))
                        {
                            existingAdmin.Password = admin.Password;
                        }

                        // Authority değiştirilmesin, mevcut rol korunsun
                        // existingAdmin.Authority değiştirilmiyor

                        dB.SaveChanges();
                        TempData["Message"] = "✅ Profiliniz başarıyla güncellendi.";
                        return RedirectToAction("Edit");
                    }
                    else
                    {
                        TempData["Message"] = "❌ Profil bulunamadı.";
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

            ViewBag.SiteLogo = dB.ID.SingleOrDefault();
            return View(admin);
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
