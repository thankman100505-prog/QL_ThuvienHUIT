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

        [ChildActionOnly]
        public ActionResult _SidebarPartial()
        {
            return PartialView(db.THELOAIs.ToList());
        }

        public ActionResult Index(string tuKhoa = "")
        {
            var query = db.QLSACHes
                          .Include(s => s.TACGIA)
                          .Include(s => s.BIASACH);

            if (!string.IsNullOrEmpty(tuKhoa))
            {
                query = query.Where(s => s.TENSACH.Contains(tuKhoa) ||
                                         s.TACGIA.TENTG.Contains(tuKhoa));
                ViewBag.TuKhoa = tuKhoa;
            }

            return View(query.OrderByDescending(s => s.NAMXB).ToList());
        }

        public ActionResult LocTheLoai(string maTL)
        {
            if (string.IsNullOrEmpty(maTL)) return RedirectToAction("Index");

            var listSach = db.QLSACHes
                             .Include(s => s.TACGIA)
                             .Include(s => s.BIASACH)
                             .Where(s => s.MATHELOAI == maTL)
                             .OrderByDescending(s => s.NAMXB)
                             .ToList();

            return View("Index", listSach);
        }
        public ActionResult ChiTiet(string maSach)
        {
            if (string.IsNullOrEmpty(maSach)) return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);

            var sach = db.QLSACHes
                         .Include(s => s.BIASACH)
                         .Include(s => s.TACGIA)
                         .Include(s => s.NHAXUATBAN)
                         .Include(s => s.THELOAI)
                         .FirstOrDefault(x => x.MASACH == maSach);

            if (sach == null) return HttpNotFound();
            ViewBag.SanPhamLienQuan = db.QLSACHes
                                        .Include(s => s.BIASACH)
                                        .Where(x => x.MATHELOAI == sach.MATHELOAI && x.MASACH != maSach)
                                        .OrderBy(x => Guid.NewGuid()) 
                                        .Take(4) 
                                        .ToList();

            return View(sach);
        }
        public ActionResult QuanLySach()
        {
            var listSach = db.QLSACHes.Include(s => s.TACGIA)
                                    .Include(s => s.THELOAI)
                                    .Include(s => s.NHAXUATBAN)
                                    .Include(s => s.BIASACH)
                                    .OrderByDescending(s => s.MASACH)
                                    .ToList();
            return View(listSach);
        }

       
        [HttpGet]
        public ActionResult ThemSach()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
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
                            string path = Path.Combine(Server.MapPath("~/Images"), fileName);
                            fileAnh.SaveAs(path);
                            BIASACH bia = new BIASACH();
                            bia.MASACH = sach.MASACH;
                            bia.URL_ANH = fileName;

                            db.BIASACHes.Add(bia);
                            db.SaveChanges(); 
                        }

                        transaction.Commit(); 
                        return RedirectToAction("Index");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ViewBag.Error = "Lỗi: " + ex.Message;
                    }
                }
            }
            return View(sach);
        }
        [HttpGet]
        public ActionResult CapNhat(string maSach)
        {
            if (string.IsNullOrEmpty(maSach))
            {
                return new HttpStatusCodeResult(System.Net.HttpStatusCode.BadRequest);
            }
            var sach = db.QLSACHes.Include(s => s.TACGIA)
                                .Include(s => s.THELOAI)
                                .Include(s => s.NHAXUATBAN)
                                .Include(s => s.BIASACH)
                                .FirstOrDefault(s => s.MASACH == maSach);

            if (sach == null)
            {
                return HttpNotFound();
            }

            return View(sach);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult CapNhat(QLSACH sach, string tenTacGia, string tenTheLoai, string tenNXB, HttpPostedFileBase fileAnh)
        {
            if (string.IsNullOrWhiteSpace(tenTacGia)) ModelState.AddModelError("", "Vui lòng nhập tên Tác giả.");
            if (string.IsNullOrWhiteSpace(tenTheLoai)) ModelState.AddModelError("", "Vui lòng nhập tên Thể loại.");
            if (string.IsNullOrWhiteSpace(tenNXB)) ModelState.AddModelError("", "Vui lòng nhập tên Nhà xuất bản.");

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
                            sachInDb.MATG = Helpers.DataHelper.GetOrCreateTacGia(db, tenTacGia);
                            sachInDb.MATHELOAI = Helpers.DataHelper.GetOrCreateTheLoai(db, tenTheLoai);
                            sachInDb.MAXB = Helpers.DataHelper.GetOrCreateNXB(db, tenNXB);
                            int oldSL = sachInDb.SL ?? 0;
                            int newSL = sach.SL ?? 0;
                            sachInDb.SL = newSL;
                            sachInDb.MOTA = sach.MOTA;
                            sachInDb.TINHTRANG = (sachInDb.TINHTRANG ?? 0) + (newSL - oldSL);

                            if (sachInDb.TINHTRANG < 0)
                            {
                                ModelState.AddModelError("SL", "Số lượng tồn kho không thể âm (do đang có người mượn).");
                                transaction.Rollback();
                                return View(sachInDb);
                            }
                            if (fileAnh != null && fileAnh.ContentLength > 0)
                            {
                                string fileName = System.IO.Path.GetFileName(fileAnh.FileName);
                                string path = System.IO.Path.Combine(Server.MapPath("~/Images"), fileName);
                                fileAnh.SaveAs(path);

                                var biaSach = db.BIASACHes.FirstOrDefault(b => b.MASACH == sachInDb.MASACH);
                                if (biaSach != null) biaSach.URL_ANH = fileName;
                                else db.BIASACHes.Add(new Models.BIASACH { MASACH = sachInDb.MASACH, URL_ANH = fileName });
                            }

                            db.SaveChanges();
                            transaction.Commit();
                            return RedirectToAction("Index");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        string message = ex.Message;
                        if (ex.InnerException != null)
                        {
                            message += " | Chi tiết: " + ex.InnerException.Message;
                            if (ex.InnerException.InnerException != null)
                            {
                                message += " | Gốc rễ: " + ex.InnerException.InnerException.Message;
                            }
                        }
                        ViewBag.Error = "Lỗi hệ thống: " + message;
                    }
                }
            }
            var currentBook = db.QLSACHes.Include(s => s.TACGIA).Include(s => s.BIASACH).FirstOrDefault(s => s.MASACH == sach.MASACH);
            ViewBag.TenTacGia = tenTacGia;
            ViewBag.TenTheLoai = tenTheLoai;
            ViewBag.TenNXB = tenNXB;

            return View(currentBook ?? sach);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XoaSach(string maSach)
        {
            try
            {
                var sach = db.QLSACHes.Find(maSach);
                if (sach == null) return HttpNotFound();

                var bia = db.BIASACHes.FirstOrDefault(b => b.MASACH == maSach);
                if (bia != null)
                {
                    db.BIASACHes.Remove(bia);
                }
                db.QLSACHes.Remove(sach);
                db.SaveChanges();
                return RedirectToAction("QuanLySach");
            }
            catch (Exception)
            {
                return Content("<script>alert('Không thể xóa sách này vì đang có trong phiếu mượn!'); window.location.href='/Sach/QuanLySach';</script>");
            }
        }
    }
}