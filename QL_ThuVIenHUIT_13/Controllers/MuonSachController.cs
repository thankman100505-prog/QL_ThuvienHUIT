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
            if (Session["User_info"] == null || Session["Role"] == null)
                return RedirectToAction("SignIn", "User");

            if (Session["DsSachMuon"] == null)
            {
                Session["DsSachMuon"] = new List<SachMuonTam>();
            }
            return View();
        }
        [HttpGet]
        public JsonResult TimKiemSach(string tuKhoa)
        {
            db.Configuration.ProxyCreationEnabled = false;
            var query = db.QLSACHes.AsQueryable();

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(s => s.TENSACH.Contains(tuKhoa) || s.MASACH.Contains(tuKhoa));
            }
            var data = query.Where(s => s.TINHTRANG > 0)
                            .OrderBy(s => s.TENSACH)
                            .Take(20)
                            .Select(s => new {
                                s.MASACH,
                                s.TENSACH,
                                SL_TON = s.TINHTRANG, 
                                TENTG = s.TACGIA.TENTG
                            }).ToList();

            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult ThemSachAjax(string maSach)
        {
            try
            {
                var lst = Session["DsSachMuon"] as List<SachMuonTam>;
                if (lst == null) lst = new List<SachMuonTam>();

                var sach = db.QLSACHes.Find(maSach);
                if (sach == null) return Json(new { status = false, msg = "Không tìm thấy sách!" });
                if ((sach.TINHTRANG ?? 0) < 1) return Json(new { status = false, msg = "Sách này đã hết!" });
                var item = lst.FirstOrDefault(x => x.MaSach == maSach);
                if (item != null)
                {
                    if (item.SoLuong >= sach.TINHTRANG)
                    {
                        return Json(new { status = false, msg = "Số lượng mượn vượt quá tồn kho!" });
                    }
                    item.SoLuong++;
                }
                else
                {
                    lst.Add(new SachMuonTam
                    {
                        MaSach = sach.MASACH,
                        TenSach = sach.TENSACH,
                        SoLuong = 1,
                        TienTheChan = 0 
                    });
                }

                Session["DsSachMuon"] = lst;
                return Json(new { status = true, msg = "Đã thêm thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { status = false, msg = ex.Message });
            }
        }
        [HttpPost]
        public JsonResult KiemTraDocGia(string maThe)
        {
            var the = db.THETHUVIENs.Include(t => t.DOCGIA).FirstOrDefault(t => t.MATHE == maThe);
            if (the != null)
            {
                bool conHan = the.NGAYHETHAN >= DateTime.Now;
                return Json(new
                {
                    status = true,
                    ten = the.DOCGIA.TENDG,
                    trangThai = conHan ? "Hợp lệ" : "Hết hạn",
                    isValid = conHan
                });
            }
            return Json(new { status = false, message = "Mã thẻ không tồn tại!" });
        }
        public ActionResult XoaSach(string maSach)
        {
            var lst = Session["DsSachMuon"] as List<SachMuonTam>;
            if (lst != null)
            {
                var item = lst.FirstOrDefault(x => x.MaSach == maSach);
                if (item != null) lst.Remove(item);
            }
            return RedirectToAction("Index");
        }
        [HttpPost]
        public ActionResult LuuPhieu(string maThe)
        {
            var lst = Session["DsSachMuon"] as List<SachMuonTam>;
            if (lst == null || lst.Count == 0)
            {
                return Content("<script>alert('Chưa chọn sách nào!'); window.location.href='/MuonSach/Index';</script>");
            }
            string maNV = "";
            if (Session["User_info"] is QLNHANVIEN nv) maNV = nv.MANV;
            else maNV = "ADMIN";

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    PHIEUMUON pm = new PHIEUMUON();
                    pm.MAPM = DataHelper.GenerateNewID(db, "PHIEUMUON", "MAPM", "PM", 5);
                    pm.MATHE = maThe;
                    pm.MANV = maNV;
                    pm.NgayMuon = DateTime.Now;
                    pm.NgayDenHan = DateTime.Now.AddDays(7);
                    pm.TINHTRANG = 0; 

                    db.PHIEUMUONs.Add(pm);
                    db.SaveChanges(); 
                    foreach (var item in lst)
                    {

                        CHITIETPM ct = new CHITIETPM();
                        ct.MAPM = pm.MAPM;
                        ct.MASACH = item.MaSach;
                        ct.SLMUON = item.SoLuong;
                        ct.TIENTHECHAN = item.TienTheChan;
                        ct.TINHTRANG = 0; 

                        db.CHITIETPMs.Add(ct);
                        var sach = db.QLSACHes.Find(item.MaSach);
                        if (sach != null)
                        {
                            sach.TINHTRANG = (sach.TINHTRANG ?? 0) - item.SoLuong;
                            if (sach.TINHTRANG < 0) throw new Exception("Sách " + sach.TENSACH + " không đủ số lượng!");
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    Session["DsSachMuon"] = null;
                    return RedirectToAction("ChiTietPhieu", new { id = pm.MAPM });
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    return Content("<script>alert('Lỗi: " + ex.Message + "'); window.location.href='/MuonSach/Index';</script>");
                }
            }
        }

        public ActionResult ChiTietPhieu(string id)
        {
            var phieu = db.PHIEUMUONs.Include("THETHUVIEN.DOCGIA")
                                     .Include("QLNHANVIEN")
                                     .Include("CHITIETPMs.QLSACH")
                                     .FirstOrDefault(p => p.MAPM == id);
            return View(phieu);
        }
    }
}