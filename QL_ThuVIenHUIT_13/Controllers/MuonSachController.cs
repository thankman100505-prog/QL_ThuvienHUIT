using QL_ThuVIenHUIT_13.Models;
using QL_ThuVIenHUIT_13.Helpers; 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class MuonSachController : Controller
    {
        private CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();
        public ActionResult Index()
        {
            if (Session["DsSachMuon"] == null)
            {
                Session["DsSachMuon"] = new List<SachMuonTam>();
            }
            var listSach = db.QLSACHes
                             .Where(s => s.SL > 0)
                             .OrderBy(s => s.TENSACH) 
                             .Select(s => new {
                                 MASACH = s.MASACH,
                                 HIENTHI = s.TENSACH + " - " + s.TACGIA.TENTG + " (Còn: " + s.SL + ")"
                             })
                             .ToList();
            ViewBag.ListSach = new SelectList(listSach, "MASACH", "HIENTHI");

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
            var data = query.OrderBy(s => s.TENSACH)
                            .Take(20) 
                            .Select(s => new {
                                s.MASACH,
                                s.TENSACH,
                                s.SL,
                                TENTG = s.TACGIA.TENTG 
                            }).ToList();

            return Json(new { data = data }, JsonRequestBehavior.AllowGet);
        }

        // 3. THÊM SÁCH VÀO GIỎ
        [HttpPost]
        public JsonResult ThemSachAjax(string maSach)
        {
            try
            {
                var lst = Session["DsSachMuon"] as List<SachMuonTam>;
                if (lst == null) lst = new List<SachMuonTam>();

                var sach = db.QLSACHes.Find(maSach);
                if (sach == null) return Json(new { status = false, msg = "Không tìm thấy sách!" });

                // Kiểm tra tồn kho
                if (sach.SL < 1) return Json(new { status = false, msg = "Sách này đã hết hàng!" });

                // Kiểm tra sách đã có trong giỏ chưa
                var item = lst.FirstOrDefault(x => x.MaSach == maSach);
                if (item != null)
                {
                    // Nếu tổng số mượn > số tồn -> Báo lỗi
                    if (item.SoLuong >= sach.SL)
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
                        TienTheChan = 10000 // Giả định
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

        // 4. KIỂM TRA ĐỘC GIẢ (Xử lý Ajax)
        [HttpPost]
        public JsonResult KiemTraDocGia(string maThe)
        {
            var dg = db.THETHUVIENs.Find(maThe);
            if (dg != null)
            {
                // Kiểm tra hạn thẻ (Ví dụ: Nếu ngày hết hạn < hôm nay -> Hết hạn)
                bool conHan = dg.NGAYHETHAN >= DateTime.Now;
                string trangThai = conHan ? "" : " (ĐÃ HẾT HẠN)";

                return Json(new
                {
                    status = true,
                    ten = dg.DOCGIA.TENDG + trangThai,
                    han = dg.NGAYHETHAN.Value.ToString("dd/MM/yyyy")
                });
            }
            return Json(new { status = false, message = "Mã thẻ không tồn tại!" });
        }

        // 5. XÓA SÁCH KHỎI GIỎ
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

        // 6. LƯU PHIẾU MƯỢN (Submit Form)
        [HttpPost]
        public ActionResult LuuPhieu(string maThe)
        {
            var lst = Session["DsSachMuon"] as List<SachMuonTam>;
            if (lst == null || lst.Count == 0)
            {
                return Content("<script>alert('Giỏ sách trống!'); window.location.href='/MuonSach/Index';</script>");
            }
            string maNV = Session["MANV"] as string ?? "NV00001";

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // A. Tạo phiếu
                    PHIEUMUON pm = new PHIEUMUON();
                    pm.MAPM = DataHelper.GenerateNewID(db, "PHIEUMUON", "MAPM", "PM", 5);
                    pm.MATHE = maThe;
                    pm.MANV = maNV;
                    pm.NgayMuon = DateTime.Now;
                    pm.NgayDenHan = DateTime.Now.AddDays(7);

                    if (pm.NgayDenHan <= pm.NgayMuon) pm.NgayDenHan = pm.NgayMuon.Value.AddDays(1);

                    db.PHIEUMUONs.Add(pm);
                    db.SaveChanges(); // Lưu để có MAPM

                    // B. Tạo chi tiết & Trừ tồn kho
                    foreach (var item in lst)
                    {
                        CHITIETPM ct = new CHITIETPM();
                        ct.MAPM = pm.MAPM;
                        ct.MASACH = item.MaSach;
                        ct.SLMUON = item.SoLuong;
                        ct.TIENTHECHAN = item.TienTheChan * item.SoLuong;

                        db.CHITIETPMs.Add(ct);
                        var sach = db.QLSACHes.Find(item.MaSach);
                        if (sach != null)
                        {
                            sach.SL -= item.SoLuong;
                            sach.TINHTRANG = sach.SL; 
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();
                    Session["DsSachMuon"] = null;
                    Session["DsSachMuon"] = null; // Xóa giỏ
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
            if (string.IsNullOrEmpty(id)) return RedirectToAction("Index");
            var phieu = db.PHIEUMUONs.Include("THETHUVIEN.DOCGIA")
                                     .Include("QLNHANVIEN")
                                     .Include("CHITIETPMs.QLSACH")
                                     .FirstOrDefault(p => p.MAPM == id);

            if (phieu == null) return HttpNotFound();

            return View(phieu);
        }
    }
}