using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using QL_ThuVIenHUIT_13.Models;

namespace QL_ThuVIenHUIT_13
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        protected void Session_Start()
        {
            CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();
            HttpCookie ck = HttpContext.Current.Request.Cookies["UserLogin"];

            if (ck != null)
            {
                Session["Username"] = ck["Username"];
                string Username = ck["Username"];
                TAIKHOAN tk = db.TAIKHOANs.FirstOrDefault(x => x.USERNAME == Username);
                Session["User_info"] = null;
                if (tk.USERNAME.StartsWith("TH"))
                {
                    Session["User_info"] = db.THETHUVIENs.FirstOrDefault(x => x.MATHE == tk.USERNAME);
                    DOCGIA dg = db.DOCGIAs.FirstOrDefault(x => x.MADG == tk.USERNAME);
                    Session["NameDisplay"] = dg.TENDG;
                }

                else if (tk.USERNAME.StartsWith("NV"))
                {
                    QLNHANVIEN nv = db.QLNHANVIENs.FirstOrDefault(x => x.MANV == tk.USERNAME);
                    Session["User_info"] = nv;
                    Session["NameDisplay"] = nv.TENNV;
                }
                else
                {
                    Session["NameDisplay"] = "Admin";
                }
            }
        }
    }
}
