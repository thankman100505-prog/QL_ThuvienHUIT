using QL_ThuVIenHUIT_13.Models;
using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class AdminPhongController : Controller
    {
        CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();

        // GET: AdminPhong/ChoDuyet
        public ActionResult ChoDuyet()
        {
            var ds = db.PHIEU_MUONPHONG
                       .Include(p => p.PHONGHOP)
                       .Include(p => p.THETHUVIEN)
                       .Where(p => p.TINHTRANG == 0)
                       .ToList();

            return View(ds);
        }

        [HttpPost]
        public ActionResult Duyet(string id)
        {
            var phieu = db.PHIEU_MUONPHONG.Find(id);
            if (phieu == null) return HttpNotFound();

            phieu.TINHTRANG = 1;
            phieu.GHICHU_NV = "Đã duyệt";

            db.SaveChanges();
            return RedirectToAction("ChoDuyet");
        }

        [HttpPost]
        public ActionResult TuChoi(string id, string ghiChu)
        {
            var phieu = db.PHIEU_MUONPHONG.Find(id);
            if (phieu == null) return HttpNotFound();

            phieu.TINHTRANG = 2; 
            phieu.TRANGTHAI = 2;

            phieu.GHICHU_NV = ghiChu;
            phieu.NGAYDUYET = DateTime.Now; 

            db.SaveChanges();
            return RedirectToAction("ChoDuyet");
        }
    }
}
