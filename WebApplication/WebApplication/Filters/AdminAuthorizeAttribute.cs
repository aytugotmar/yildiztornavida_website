using System.Linq;
using System.Web.Mvc;
using System.Web.Routing;
using WebApplication.Models.Model;
using WebApplication.Models.DataContext;

namespace WebApplication.Filters
{
    public class AdminAuthorizeAttribute : AuthorizeAttribute
    {
        public new string Roles { get; set; } // "Admin,Editor" gibi roller alacak

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
        {
            // Eğer oturum yoksa, yetkilendirme başarısız
            if (httpContext.Session["Admin"] == null)
            {
                return false;
            }

            // Session'dan AdminId'yi al ve veritabanından Admin objesini çek
            int adminId = (int)httpContext.Session["Admin"];
            var dB = new WebApplication.Models.DataContext.BlogDBContext();
            var currentAdmin = dB.Admin.Find(adminId);

            if (currentAdmin == null)
            {
                return false;
            }

            // Bu sayfa için bir rol belirtilmemişse, giriş yapan herkes erişebilir
            if (string.IsNullOrEmpty(Roles))
            {
                return true;
            }

            // Sayfa için belirtilen rolleri virgülle ayırarak diziye çevir
            var requiredRoles = Roles.Split(',').Select(r => r.Trim()).ToList();

            // Giriş yapan kullanıcının yetkisi, gerekli rollerden biri mi kontrol et
            if (requiredRoles.Contains(currentAdmin.Authority))
            {
                return true;
            }

            // Gerekli role sahip değilse, yetkilendirme başarısız
            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            // Kullanıcı yetkisizse giriş sayfasına yönlendir
            filterContext.Result = new RedirectToRouteResult(
                new RouteValueDictionary
                {
                    {"controller", "Admin"},
                    {"action", "Login"}
                });
        }
    }
}