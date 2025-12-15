using QL_ThuVIenHUIT_13.Helpers;
using QL_ThuVIenHUIT_13.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class SachController : Controller
    {
        private CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();

        // --- CÁC ACTION HIỂN THỊ (GIỮ NGUYÊN) ---
        public ActionResult Index(string tuKhoa = "")
        {
            var query = db.QLSACHes.Include(s => s.TACGIA).Include(s => s.BIASACH);
            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(s => s.TENSACH.Contains(tuKhoa) || s.TACGIA.TENTG.Contains(tuKhoa));
                ViewBag.TuKhoa = tuKhoa;
            }
            return View(query.OrderByDescending(s => s.NAMXB).ToList());
        }

        public ActionResult ChiTiet(string maSach)
        {
            if (string.IsNullOrEmpty(maSach)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var sach = db.QLSACHes.Include(s => s.BIASACH).Include(s => s.TACGIA)
                         .Include(s => s.NHAXUATBAN).Include(s => s.THELOAI)
                         .FirstOrDefault(x => x.MASACH == maSach);
            if (sach == null) return HttpNotFound();

            // Gợi ý sách cùng thể loại
            ViewBag.SanPhamLienQuan = db.QLSACHes.Include(s => s.BIASACH)
                                        .Where(x => x.MATHELOAI == sach.MATHELOAI && x.MASACH != maSach)
                                        .OrderBy(x => Guid.NewGuid()).Take(4).ToList();
            return View(sach);
        }

        public ActionResult QuanLySach()
        {
            var listSach = db.QLSACHes.Include(s => s.TACGIA).Include(s => s.THELOAI)
                                    .Include(s => s.NHAXUATBAN).Include(s => s.BIASACH)
                                    .OrderByDescending(s => s.MASACH).ToList();
            return View(listSach);
        }
        [HttpGet]
        public ActionResult ThemSach() { return View(); }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult ThemSach(QLSACH sach, string tenTacGia, string tenTheLoai, string tenNXB, HttpPostedFileBase fileAnh)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        sach.MATG = DataHelper.GetOrCreateTacGia(db, tenTacGia);
                        sach.MATHELOAI = DataHelper.GetOrCreateTheLoai(db, tenTheLoai);
                        sach.MAXB = DataHelper.GetOrCreateNXB(db, tenNXB);
                        sach.MASACH = DataHelper.GenerateNewID(db, "QLSACH", "MASACH", "S", 6);
                        sach.TINHTRANG = sach.SL;

                        db.QLSACHes.Add(sach);
                        db.SaveChanges();
                        if (fileAnh != null && fileAnh.ContentLength > 0)
                        {
                            string fileName = Path.GetFileName(fileAnh.FileName);
                            string uniqueFileName = DateTime.Now.Ticks.ToString() + "_" + fileName;
                            string path = Path.Combine(Server.MapPath("~/Images"), uniqueFileName);
                            fileAnh.SaveAs(path);

                            BIASACH bia = new BIASACH { MASACH = sach.MASACH, URL_ANH = uniqueFileName };
                            db.BIASACHes.Add(bia);
                            db.SaveChanges();
                        }

                        transaction.Commit();
                        TempData["Success"] = "Thêm sách thành công!";
                        return RedirectToAction("QuanLySach");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ViewBag.Error = "Lỗi thêm sách: " + ex.Message;
                    }
                }
            }
            return View(sach);
        }
        [HttpGet]
        public ActionResult CapNhat(string maSach)
        {
            if (string.IsNullOrEmpty(maSach)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            var sach = db.QLSACHes.Include(s => s.TACGIA).Include(s => s.THELOAI)
                                .Include(s => s.NHAXUATBAN).Include(s => s.BIASACH)
                                .FirstOrDefault(s => s.MASACH == maSach);
            if (sach == null) return HttpNotFound();
            return View(sach);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateInput(false)]
        public ActionResult CapNhat(QLSACH sach, string tenTacGia, string tenTheLoai, string tenNXB, HttpPostedFileBase fileAnh)
        {
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var sachInDb = db.QLSACHes.Include(s => s.BIASACH).FirstOrDefault(s => s.MASACH == sach.MASACH);
                        if (sachInDb != null)
                        {
                            sachInDb.TENSACH = sach.TENSACH;
                            sachInDb.NAMXB = sach.NAMXB;
                            sachInDb.MOTA = sach.MOTA;

                            sachInDb.MATG = DataHelper.GetOrCreateTacGia(db, tenTacGia);
                            sachInDb.MATHELOAI = DataHelper.GetOrCreateTheLoai(db, tenTheLoai);
                            sachInDb.MAXB = DataHelper.GetOrCreateNXB(db, tenNXB);
                            int oldTotal = sachInDb.SL ?? 0;
                            int newTotal = sach.SL ?? 0;
                            int diff = newTotal - oldTotal;

                            sachInDb.SL = newTotal;
                            sachInDb.TINHTRANG = (sachInDb.TINHTRANG ?? 0) + diff;

                            if (sachInDb.TINHTRANG < 0)
                            {
                                ViewBag.Error = "Không thể giảm tổng số lượng thấp hơn số sách đang được mượn!";
                                transaction.Rollback();
                                return View(sach);
                            }
                            if (fileAnh != null && fileAnh.ContentLength > 0)
                            {
                                string fileName = DateTime.Now.Ticks + "_" + Path.GetFileName(fileAnh.FileName);
                                string path = Path.Combine(Server.MapPath("~/Images"), fileName);
                                fileAnh.SaveAs(path);

                                var biaSach = db.BIASACHes.FirstOrDefault(b => b.MASACH == sachInDb.MASACH);
                                if (biaSach != null) biaSach.URL_ANH = fileName;
                                else db.BIASACHes.Add(new BIASACH { MASACH = sachInDb.MASACH, URL_ANH = fileName });
                            }

                            db.SaveChanges();
                            transaction.Commit();
                            TempData["Success"] = "Cập nhật thành công!";
                            return RedirectToAction("QuanLySach");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                    }
                }
            }
            return View(sach);
        }
        [HttpPost]
        public ActionResult XoaSach(string maSach)
        {
            try
            {
                var sach = db.QLSACHes.Find(maSach);
                if (sach == null) return HttpNotFound();
                bool dangMuon = db.CHITIETPMs.Any(ct => ct.MASACH == maSach && ct.PHIEUMUON.NgayDenHan == null);
                if (dangMuon)
                {
                    return Content("<script>alert('Sách đang được mượn, không thể xóa!'); window.location.href='/Sach/QuanLySach';</script>");
                }

                var bia = db.BIASACHes.FirstOrDefault(b => b.MASACH == maSach);
                if (bia != null) db.BIASACHes.Remove(bia);

                db.QLSACHes.Remove(sach);
                db.SaveChanges();
                return RedirectToAction("QuanLySach");
            }
            catch (Exception)
            {
                return Content("<script>alert('Lỗi rằng buộc dữ liệu (FK)!'); window.location.href='/Sach/QuanLySach';</script>");
            }
        }
    }
}