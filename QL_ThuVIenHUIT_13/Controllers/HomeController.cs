using Microsoft.Win32;
using QL_ThuVIenHUIT_13.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class HomeController : Controller
    {
        CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();
        public ActionResult HuongDan_DK_The()
        {
            return View();
        }
        public ActionResult HuongDan_SD_PhongHop()
        {
            return View();
        }
        public ActionResult Index()
        {
            HomeViewModel model = new HomeViewModel();
            model.TinTucSuKien = db.TINTUC1
                                   .Where(x => x.LoaiTin == 1 && x.HienThi == true)
                                   .OrderByDescending(x => x.NgayDang)
                                   .ToList();

            model.ThongBao = db.TINTUC1
                               .Where(x => x.LoaiTin == 2 && x.HienThi == true)
                               .OrderByDescending(x => x.NgayDang)
                               .ToList();

            return View(model);
        }
        public ActionResult About()
        {
            ViewBag.Message = "Giới thiệu về hệ thống thư viện.";
            return View();
        }
    }
}