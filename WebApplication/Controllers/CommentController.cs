using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebApplication.Filters;
using WebApplication.Models.DataContext;
using WebApplication.Models.Model;

namespace WebApplication.Controllers
{
    [AdminAuthorize(Roles = "Admin")]
    public class CommentController : Controller
    {
        private BlogDBContext dB = new BlogDBContext();

        // GET: Comment
        public ActionResult Index()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();

            // Onay bekleyen yorum sayısını hesapla
            ViewBag.PendingCount = dB.Comment.Count(x => !x.isApproved);

            // Tüm yorumları admin bilgileriyle birlikte getir
            var comments = dB.Comment
                .Include("Admin")
                .Include("Blog")
                .OrderByDescending(x => x.CreatedAt)
                .ToList(); // ToPagedList yerine ToList kullanıldı

            return View(comments);
        }

        // GET: Comment/Pending
        public ActionResult Pending()
        {
            ViewBag.SiteLogo = dB.ID.SingleOrDefault();

            // Onay bekleyen tüm yorumları getir
            var pendingComments = dB.Comment
                .Include("Admin")
                .Include("Blog")
                .Where(x => !x.isApproved)
                .OrderByDescending(x => x.CreatedAt)
                .ToList(); // ToPagedList yerine ToList kullanıldı

            return View(pendingComments);
        }

        // POST: Comment/Approve
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Approve(int id)
        {
            var comment = dB.Comment.Find(id);
            if (comment != null)
            {
                comment.isApproved = true;
                dB.SaveChanges();
                TempData["Message"] = "✅ Yorum onaylandı.";
            }
            else
            {
                TempData["Message"] = "❌ Yorum bulunamadı.";
            }

            return RedirectToAction("Pending");
        }

        // POST: Comment/Reject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reject(int id)
        {
            var comment = dB.Comment.Find(id);
            if (comment != null)
            {
                dB.Comment.Remove(comment);
                dB.SaveChanges();
                TempData["Message"] = "✅ Yorum reddedildi ve silindi.";
            }
            else
            {
                TempData["Message"] = "❌ Yorum bulunamadı.";
            }

            return RedirectToAction("Pending");
        }

        // POST: Comment/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            var comment = dB.Comment.Find(id);
            if (comment != null)
            {
                dB.Comment.Remove(comment);
                dB.SaveChanges();
                TempData["Message"] = "✅ Yorum silindi.";
            }
            else
            {
                TempData["Message"] = "❌ Yorum bulunamadı.";
            }

            return RedirectToAction("Index");
        }

        // POST: Comment/BulkApprove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkApprove(int[] commentIds)
        {
            if (commentIds != null && commentIds.Length > 0)
            {
                var comments = dB.Comment.Where(x => commentIds.Contains(x.CommentId)).ToList();
                foreach (var comment in comments)
                {
                    comment.isApproved = true;
                }
                dB.SaveChanges();
                TempData["Message"] = $"✅ {comments.Count} yorum onaylandı.";
            }

            return RedirectToAction("Pending");
        }

        // POST: Comment/BulkReject
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BulkReject(int[] commentIds)
        {
            if (commentIds != null && commentIds.Length > 0)
            {
                var comments = dB.Comment.Where(x => commentIds.Contains(x.CommentId)).ToList();
                dB.Comment.RemoveRange(comments);
                dB.SaveChanges();
                TempData["Message"] = $"✅ {comments.Count} yorum reddedildi ve silindi.";
            }

            return RedirectToAction("Pending");
        }
    }
}