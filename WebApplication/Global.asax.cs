using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Helpers;
using System.Web.Routing;
using System.Text;

namespace WebApplication
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            // Force a fresh anti-forgery cookie name to avoid old/stale cookies causing mismatches
            AntiForgeryConfig.CookieName = "XSRF-TOKEN";
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        protected void Application_BeginRequest()
        {
            // Set UTF-8 encoding for each request
            if (HttpContext.Current != null && HttpContext.Current.Response != null)
            {
                try
                {
                    HttpContext.Current.Response.Charset = "utf-8";
                }
                catch
                {
                    // Ignore encoding errors
                }
            }
        }
    }
}
