using QL_ThuVIenHUIT_13.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class PhongController : Controller
    {
        CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();
        public ActionResult Index(string maPhong = "")
        {
            ViewBag.DsPhong = db.PHONGHOPs.Select(p => p.MAPHONG).ToList();

            if (!string.IsNullOrEmpty(maPhong))
            {
                ViewBag.Phong = db.PHONGHOPs.FirstOrDefault(p => p.MAPHONG == maPhong);
            }

            return View();
        }
        public ActionResult DatPhong()
        {
            ViewBag.DsPhong = db.PHONGHOPs.Where(p => p.TINHTRANG == 1).ToList();
            return View();
        }
        [HttpPost]
        public ActionResult XemPhieu(string MATHE, string MAPH, DateTime NGAYMUON, TimeSpan GIOMUON, int SL_NGUOITHAMGIA, string MUCDICH, TimeSpan GIOTRA)
        {
            PHIEU_MUONPHONG p = new PHIEU_MUONPHONG
            {
                MATHE = MATHE,
                MAPH = MAPH,
                NGAYMUON = NGAYMUON,
                GIOMUON = GIOMUON,
                GIOTRA = GIOTRA,
                SL_NGUOITHAMGIA = SL_NGUOITHAMGIA,
                MUCDICH = MUCDICH
            };

            return View(p);
        }
        [HttpPost]
        public ActionResult XacNhan(PHIEU_MUONPHONG p)
        {
            var phong = db.PHONGHOPs.FirstOrDefault(x => x.MAPHONG == p.MAPH);
            if (phong == null) return View("XemPhieu", p);
            if (p.GIOTRA <= p.GIOMUON)
            {
                ViewBag.Loi = "Giờ trả phải sau giờ mượn!";
                return View("XemPhieu", p);
            }
            if (p.SL_NGUOITHAMGIA > phong.SL_NGUOITOIDA)
            {
                ViewBag.Loi = "Số người tham gia vượt quá sức chứa (" + phong.SL_NGUOITOIDA + ")!";
                return View("XemPhieu", p);
            }
            bool maTheTonTai = db.THETHUVIENs.Any(t => t.MATHE == p.MATHE);
            if (!maTheTonTai)
            {
                ViewBag.Loi = "Mã thẻ thư viện không tồn tại!";
                return View("XemPhieu", p);
            }
            bool biTrung = db.PHIEU_MUONPHONG.Any(x =>
                x.MAPH == p.MAPH &&
                x.NGAYMUON == p.NGAYMUON &&
                p.GIOMUON < x.GIOTRA &&
                p.GIOTRA > x.GIOMUON
            );

            if (biTrung)
            {
                ViewBag.Loi = "Phòng đã bị đặt trong khung giờ này!";
                return View("XemPhieu", p);
            }
            p.MAPHIEU = TaoMaPhieu();
            p.TIENPHAT = 0;

            try
            {
                db.PHIEU_MUONPHONG.Add(p);
                db.SaveChanges();
                return View("ThanhCong");
            }
            catch (Exception)
            {
                ViewBag.Loi = "Có lỗi xảy ra khi lưu dữ liệu!";
                return View("XemPhieu", p);
            }
        }

        public ActionResult ThanhCong()
        {
            return View();
        }
        private string TaoMaPhieu()
        {
            var last = db.PHIEU_MUONPHONG
                .OrderByDescending(x => x.MAPHIEU)
                .Select(x => x.MAPHIEU)
                .FirstOrDefault();

            if (last == null) return "MP00001";

            string maSoStr = last.Trim().Substring(2);
            int so;
            if (int.TryParse(maSoStr, out so))
            {
                so++;
                return "MP" + so.ToString("D5");
            }
            return "MP" + Guid.NewGuid().ToString().Substring(0, 5);
        }
        public ActionResult LichSuDatPhong()
        {
            var ds = db.PHIEU_MUONPHONG.OrderByDescending(x => x.NGAYMUON).ToList();
            return View(ds);
        }
        public ActionResult XoaDatPhong(string id)
        {
            var phieu = db.PHIEU_MUONPHONG.FirstOrDefault(x => x.MAPHIEU == id);
            if (phieu == null) return HttpNotFound();

            db.PHIEU_MUONPHONG.Remove(phieu);
            db.SaveChanges();

            return RedirectToAction("LichSuDatPhong");
        }
        public ActionResult SuaDatPhong(string id)
        {
            var phieu = db.PHIEU_MUONPHONG.FirstOrDefault(x => x.MAPHIEU == id);
            if (phieu == null) return HttpNotFound();

            return View(phieu);
        }
        [HttpPost]
        public ActionResult SuaDatPhong(PHIEU_MUONPHONG p)
        {
            var phieuGoc = db.PHIEU_MUONPHONG.FirstOrDefault(x => x.MAPHIEU == p.MAPHIEU);
            if (phieuGoc == null) return HttpNotFound();
            DateTime now = DateTime.Now;
            if (phieuGoc.NGAYMUON != null && phieuGoc.GIOTRA != null)
            {
                DateTime ketThucCu = phieuGoc.NGAYMUON.Value.Date + phieuGoc.GIOTRA.Value;
                if (ketThucCu <= now)
                {
                    ViewBag.Loi = "Phiếu này đã hoàn tất, không thể chỉnh sửa!";
                    return View(p);
                }
            }
            if (p.GIOTRA <= p.GIOMUON)
            {
                ViewBag.Loi = "Giờ trả phải sau giờ mượn!";
                return View(p);
            }
            bool biTrung = db.PHIEU_MUONPHONG.Any(x =>
                x.MAPHIEU != p.MAPHIEU &&
                x.MAPH == phieuGoc.MAPH &&
                x.NGAYMUON == p.NGAYMUON &&
                p.GIOMUON < x.GIOTRA &&
                p.GIOTRA > x.GIOMUON
            );

            if (biTrung)
            {
                ViewBag.Loi = "Giờ cập nhật bị trùng với phiếu khác!";
                return View(p);
            }
            phieuGoc.NGAYMUON = p.NGAYMUON;
            phieuGoc.GIOMUON = p.GIOMUON;
            phieuGoc.GIOTRA = p.GIOTRA;
            phieuGoc.SL_NGUOITHAMGIA = p.SL_NGUOITHAMGIA;
            phieuGoc.MUCDICH = p.MUCDICH;

            db.SaveChanges();

            return RedirectToAction("LichSuDatPhong");
        }

        //Quản lý phòng
    }
}