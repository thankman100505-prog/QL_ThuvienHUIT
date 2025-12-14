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
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        // GET: /Registration/Register
        [HttpGet]
        public ActionResult Register()
        {
            return View(new Registration());
        }

        // POST: /Registration/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(Registration model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // 1) Sinh mã độc giả CHAR(7)
            string madg = GenerateMadg();

            // 2) Tạo đối tượng DOCGIA (mapping đúng DB)
            var docgia = new DOCGIA
            {
                MADG = madg,
                TENDG = madg,
                KHOA = model.Khoa,
                LOP = model.DoiTuong,
                DIACHI = model.DiaChi,
                SODT = model.DienThoai,
                MAIL = model.Email
            };

            

            // Thêm độc giả
            db.DOCGIAs.Add(docgia);

            // 3) Tạo thẻ thư viện (CHAR(7))
            string mathe = GenerateMathe();

            var the = new THETHUVIEN
            {
                MATHE = mathe,
                MADG = madg,
                NGAYCAP = DateTime.Now.Date,
                NGAYHETHAN = DateTime.Now.Date.AddYears(4),
                TRANGTHAI = "Hoạt động"
            };

            db.THETHUVIENs.Add(the);

            // 5) Lưu database
            db.SaveChanges();

            TempData["SuccessMessage"] = "Đăng ký thành công. Mã thẻ: " + mathe;
            return RedirectToAction("RegisterSuccess", new { id = mathe });
        }

        // GET: /Registration/RegisterSuccess
        public ActionResult RegisterSuccess(string id)
        {
            ViewBag.Mathe = id;
            return View();
        }

        // Sinh mã độc giả: DG00001 (CHAR(7))
        private string GenerateMadg()
        {
            int count = db.DOCGIAs.Count() + 1;
            return "DG" + count.ToString("00000");
        }

        // Sinh mã thẻ thư viện: TH00001 (CHAR(7))
        private string GenerateMathe()
        {
            int count = db.THETHUVIENs.Count() + 1;
            return "TH" + count.ToString("00000");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}