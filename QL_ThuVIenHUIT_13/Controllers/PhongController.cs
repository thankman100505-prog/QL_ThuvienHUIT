using QL_ThuVIenHUIT_13.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class PhongController : Controller
    {
        private CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();

        // =======================
        // DANH SÁCH PHÒNG
        // =======================
        public ActionResult Index(string maPhong)
        {
            ViewBag.DsPhong = db.PHONGHOPs
                .Select(p => p.MAPHONG)
                .ToList();

            if (!string.IsNullOrEmpty(maPhong))
            {
                ViewBag.Phong = db.PHONGHOPs
                    .FirstOrDefault(p => p.MAPHONG == maPhong);
            }

            return View();
        }

        // =======================
        // ĐẶT PHÒNG
        // =======================
        public ActionResult DatPhong()
        {
            ViewBag.DsPhong = db.PHONGHOPs
                .Where(p => p.TINHTRANG == 1)
                .ToList();

            return View(new PHIEU_MUONPHONG());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DatPhong(PHIEU_MUONPHONG p)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.DsPhong = db.PHONGHOPs.Where(x => x.TINHTRANG == 1).ToList();
                return View(p);
            }

            p.MAPHIEU = TaoMaPhieu();
            p.TIENPHAT = 0;

            p.TRANGTHAI = 0; // chờ admin duyệt
            db.PHIEU_MUONPHONG.Add(p);
            db.SaveChanges();


            return RedirectToAction("ThanhCong");
        }

        public ActionResult ThanhCong()
        {
            return View();
        }

        public ActionResult LichSuDatPhong()
        {
            var ds = db.PHIEU_MUONPHONG
                .OrderByDescending(x => x.NGAYMUON)
                .ToList();

            return View(ds);
        }

        // =======================
        // XÁC NHẬN XÓA
        // =======================
        public ActionResult XoaDatPhong(string id)
        {
            var phieu = db.PHIEU_MUONPHONG.Find(id);
            if (phieu == null) return HttpNotFound();

            return View(phieu);
        }

        // =======================
        // XÁC NHẬN XÓA (POST)
        // =======================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanXoa(string id)
        {
            var phieu = db.PHIEU_MUONPHONG.Find(id);
            if (phieu == null) return HttpNotFound();

            db.PHIEU_MUONPHONG.Remove(phieu);
            db.SaveChanges();

            return RedirectToAction("LichSuDatPhong");
        }


        // =======================
        // HÀM DÙNG CHUNG
        // =======================
        private string TaoMaPhieu()
        {
            var last = db.PHIEU_MUONPHONG
                .OrderByDescending(x => x.MAPHIEU)
                .Select(x => x.MAPHIEU)
                .FirstOrDefault();

            if (last == null) return "MP00001";

            int so = int.Parse(last.Substring(2)) + 1;
            return "MP" + so.ToString("D5");
        }
    }
}
