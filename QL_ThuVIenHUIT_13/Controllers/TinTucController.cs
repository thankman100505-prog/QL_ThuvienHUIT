using QL_ThuVIenHUIT_13.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class TinTucController : Controller
    {
        CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();

        public ActionResult ChiTiet(string id)
        {
            var baiViet = db.TINTUCs.FirstOrDefault(x => x.Link == id);

            if (baiViet == null)
            {
                return HttpNotFound();
            }

            return View(baiViet);
        }
    }
}