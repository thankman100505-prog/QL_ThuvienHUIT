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
                string username = ck["Username"];
                TAIKHOAN tk = db.TAIKHOANs.FirstOrDefault(x => x.USERNAME == username);
                if (tk != null)
                {
                    Session["Username"] = tk.USERNAME;
                    Session["Role"] = tk.ROLE_ID;
                    Session["User_info"] = null;
                    if (tk.ROLE_ID == 3)
                    {
                        var theThuVien = db.THETHUVIENs.FirstOrDefault(x => x.MATHE == tk.USERNAME);

                        if (theThuVien != null)
                        {
                            Session["User_info"] = theThuVien;
                            DOCGIA dg = db.DOCGIAs.FirstOrDefault(x => x.MADG == theThuVien.MADG);

                            if (dg != null)
                            {
                                Session["NameDisplay"] = dg.TENDG;
                                Session["DocGia_Info"] = dg;
                            }
                        }
                    }
                    else if (tk.ROLE_ID == 2) 
                    {
                        QLNHANVIEN nv = db.QLNHANVIENs.FirstOrDefault(x => x.MANV == tk.USERNAME);
                        if (nv != null)
                        {
                            Session["User_info"] = nv;
                            Session["NameDisplay"] = nv.TENNV;
                        }
                    }
                    else
                    {
                        Session["NameDisplay"] = "Quản Trị Viên";
                    }
                }
            }
        }
    }
}
