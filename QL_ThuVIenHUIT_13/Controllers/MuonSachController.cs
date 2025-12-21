using QL_ThuVIenHUIT_13.Models;
using QL_ThuVIenHUIT_13.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data.Entity;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class MuonSachController : Controller
    {
        private CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();

        public ActionResult Index()
        {
            if (Session["User_info"] == null) return RedirectToAction("SignIn", "User");

            if (Session["DsSachMuon"] == null) Session["DsSachMuon"] = new List<SachMuonTam>();
            var dsSach = db.QLSACHes.Include(s => s.TACGIA)
                                    .Where(s => s.TINHTRANG > 0)
                                    .OrderBy(s => s.TENSACH)
                                    .Take(15).ToList();
            return View(dsSach);
        }

        [HttpGet]
        public JsonResult TimKiemSach(string tuKhoa)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var data = db.QLSACHes.Where(s => (s.TENSACH.Contains(tuKhoa) || s.MASACH.Contains(tuKhoa)) && s.TINHTRANG > 0)
                            .Take(20)
                            .Select(s => new { s.MASACH, s.TENSACH, SL_TON = s.TINHTRANG, TENTG = s.TACGIA.TENTG })
                            .ToList();
            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult ThemSachAjax(string maSach)
        {
            var lst = Session["DsSachMuon"] as List<SachMuonTam> ?? new List<SachMuonTam>();
            var sach = db.QLSACHes.Find(maSach);
            if (sach == null || (sach.TINHTRANG ?? 0) < 1)
                return Json(new { status = false, msg = "Sách không khả dụng!" });

            var item = lst.FirstOrDefault(x => x.MaSach == maSach);
            if (item != null) item.SoLuong++;
            else lst.Add(new SachMuonTam { MaSach = sach.MASACH, TenSach = sach.TENSACH, SoLuong = 1, TienTheChan = 10000 });

            Session["DsSachMuon"] = lst;
            return Json(new { status = true, msg = "Đã thêm!" });
        }
        [HttpPost]
        public JsonResult KiemTraDocGia(string maThe)
        {
            try
            {
                var the = db.THETHUVIENs.Include(t => t.DOCGIA).FirstOrDefault(t => t.MATHE == maThe);
                if (the == null) return Json(new { status = false, message = "Mã thẻ không tồn tại!" });

                bool conHan = the.NGAYHETHAN >= DateTime.Now;
                return Json(new { status = true, ten = the.DOCGIA.TENDG, trangThai = conHan ? "Hợp lệ" : "Hết hạn", isValid = conHan });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, message = "Lỗi hệ thống: " + ex.Message });
            }
        }

        public ActionResult GetSelectedBooks()
        {
            return PartialView("_SelectedBooks");
        }
        [HttpPost]
        public JsonResult XoaSachAjax(string maSach)
        {
            var lst = Session["DsSachMuon"] as List<SachMuonTam>;
            if (lst != null)
            {
                var item = lst.FirstOrDefault(x => x.MaSach == maSach);
                if (item != null) lst.Remove(item);
                Session["DsSachMuon"] = lst;
            }
            return Json(new { status = true });
        }
        [HttpPost]
        public ActionResult LuuPhieu(string maThe)
        {
            var lst = Session["DsSachMuon"] as List<SachMuonTam>;
            if (lst == null || !lst.Any()) return Content("<script>alert('Giỏ trống!'); window.location.href='/MuonSach/Index';</script>");

            string maNV = (Session["User_info"] is QLNHANVIEN nv) ? nv.MANV : "ADMIN";
            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    PHIEUMUON pm = new PHIEUMUON
                    {
                        MAPM = DataHelper.GenerateNewID(db, "PHIEUMUON", "MAPM", "PM", 5),
                        MATHE = maThe,
                        MANV = maNV,
                        NgayMuon = DateTime.Now,
                        NgayDenHan = DateTime.Now.AddDays(7),
                        TINHTRANG = 1
                    };
                    db.PHIEUMUONs.Add(pm);
                    foreach (var item in lst)
                    {
                        db.CHITIETPMs.Add(new CHITIETPM
                        {
                            MAPM = pm.MAPM,
                            MASACH = item.MaSach,
                            SLMUON = item.SoLuong,
                            TIENTHECHAN = (decimal)(item.TienTheChan * item.SoLuong),
                            TINHTRANG = 0
                        });
                        var s = db.QLSACHes.Find(item.MaSach);
                        s.TINHTRANG -= item.SoLuong;
                    }
                    db.SaveChanges();
                    dbTransaction.Commit();
                    Session["DsSachMuon"] = null;
                    return RedirectToAction("ChiTietPhieu", new { id = pm.MAPM });
                }
                catch (Exception ex) { dbTransaction.Rollback(); return Content("Lỗi: " + ex.Message); }
            }
        }

        [HttpPost]
        public ActionResult ThemGioSach(string id, int soLuong = 1)
        {
            if (Session["UserName"] == null) return RedirectToAction("SignIn", "User");
            GioSach gs = (GioSach)Session["GioSach"] ?? new GioSach();
            if (gs.ThemSach(id, soLuong) == 1) Session["GioSach"] = gs;
            return RedirectToAction("ChiTiet", "Sach", new { maSach = id });
        }

        public ActionResult TrangGioSach()
        {
            if (Session["UserName"] == null) return RedirectToAction("SignIn", "User");
            return View((GioSach)Session["GioSach"] ?? new GioSach());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanMuon()
        {
            if (Session["UserName"] == null) return RedirectToAction("SignIn", "User");
            GioSach gs = (GioSach)Session["GioSach"];
            if (gs == null || !gs.List_Sach.Any()) return RedirectToAction("TrangGioSach");

            using (var dbTransaction = db.Database.BeginTransaction())
            {
                try
                {
                    PHIEUMUON pm = new PHIEUMUON
                    {
                        MAPM = DataHelper.GenerateNewID(db, "PHIEUMUON", "MAPM", "PM", 5),
                        MATHE = Session["UserName"].ToString(),
                        MANV = null, 
                        NgayMuon = DateTime.Now,
                        NgayDenHan = DateTime.Now.AddDays(7),
                        TINHTRANG = -1
                    };
                    db.PHIEUMUONs.Add(pm);
                    foreach (var item in gs.List_Sach)
                    {
                        db.CHITIETPMs.Add(new CHITIETPM
                        {
                            MAPM = pm.MAPM,
                            MASACH = item.MaSach,
                            SLMUON = item.soLuong,
                            TIENTHECHAN = (decimal)(item.TienTheChan * item.soLuong),
                            TINHTRANG = 0
                        });
                    }
                    db.SaveChanges();
                    dbTransaction.Commit();
                    Session["GioSach"] = null;
                    return RedirectToAction("ChiTietPhieu", new { id = pm.MAPM });
                }
                catch (Exception ex) { dbTransaction.Rollback(); return RedirectToAction("TrangGioSach"); }
            }
        }
        public ActionResult ChiTietPhieu(string id)
        {
            var phieu = db.PHIEUMUONs.Include(p => p.THETHUVIEN.DOCGIA).Include(p => p.QLNHANVIEN)
                          .Include("CHITIETPMs.QLSACH.TACGIA")
                          .FirstOrDefault(p => p.MAPM == id);
            return View(phieu);
        }

        public ActionResult SachDangMuon()
        {
            string id = Convert.ToString(Session["UserName"]);
            var phieuTra = db.PHIEUTRAs.Select(pt => pt.MAPM).ToList();
            var data = db.PHIEUMUONs.Include(pm => pm.CHITIETPMs).Include("CHITIETPMs.QLSACH.BIASACH")
                         .Where(pm => pm.MATHE == id && !phieuTra.Contains(pm.MAPM)).ToList();
            return View(data);
        }

        public ActionResult SachDaMuon()
        {
            string id = Convert.ToString(Session["UserName"]);
            var phieuTra = db.PHIEUTRAs.Select(pt => pt.MAPM).ToList();
            var data = db.PHIEUMUONs.Include(pm => pm.CHITIETPMs).Include(pm => pm.QLNHANVIEN)
                         .Include("CHITIETPMs.QLSACH.BIASACH").Include("CHITIETPMs.QLSACH.TACGIA")
                         .Where(pm => pm.MATHE == id && phieuTra.Contains(pm.MAPM)).ToList();
            return View(data);
        }
    }
}