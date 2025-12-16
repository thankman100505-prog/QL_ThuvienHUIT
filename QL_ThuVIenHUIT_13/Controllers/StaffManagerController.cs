using QL_ThuVIenHUIT_13.Helpers;
using QL_ThuVIenHUIT_13.Models;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using OfficeOpenXml;
using OfficeOpenXml.Style;
namespace QL_ThuVIenHUIT_13.Controllers
{
    public class StaffManagerController : Controller
    {
        private CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();
        public ActionResult Index(string search = "")
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");

            var query = db.QLNHANVIENs.AsQueryable();
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(nv => nv.TENNV.Contains(search) || nv.MANV.Contains(search));
                ViewBag.Search = search;
            }
            return View(query.OrderByDescending(nv => nv.MANV).ToList());
        }
        [HttpGet]
        public ActionResult Create()
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(QLNHANVIEN nv)
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");

            if (ModelState.IsValid)
            {
                if (db.QLNHANVIENs.Any(x => x.SDIENTHOAI == nv.SDIENTHOAI))
                {
                    ModelState.AddModelError("SDIENTHOAI", "Số điện thoại này đã tồn tại!");
                    return View(nv);
                }
                if (db.QLNHANVIENs.Any(x => x.MAIL == nv.MAIL))
                {
                    ModelState.AddModelError("MAIL", "Email này đã tồn tại!");
                    return View(nv);
                }

                try
                {
                    nv.MANV = DataHelper.GenerateNewID(db, "QLNHANVIEN", "MANV", "NV", 5);
                    if (string.IsNullOrEmpty(nv.CHUCVU)) nv.CHUCVU = "Thủ thư";

                    db.QLNHANVIENs.Add(nv);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm nhân viên thành công! Mật khẩu mặc định: 12345";
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                }
            }
            return View(nv);
        }
        [HttpGet]
        public ActionResult Edit(string id)
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");
            var nv = db.QLNHANVIENs.Find(id);
            if (nv == null) return HttpNotFound();
            return View(nv);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(QLNHANVIEN nv)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existNV = db.QLNHANVIENs.Find(nv.MANV);
                    if (existNV != null)
                    {
                        existNV.TENNV = nv.TENNV;
                        existNV.NGSINH = nv.NGSINH;
                        existNV.CHUCVU = nv.CHUCVU;
                        existNV.SDIENTHOAI = nv.SDIENTHOAI;
                        existNV.MAIL = nv.MAIL;
                        db.SaveChanges();
                        TempData["Success"] = "Cập nhật thành công!";
                        return RedirectToAction("Index");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Error = "Lỗi cập nhật: " + ex.Message;
                }
            }
            return View(nv);
        }
        [HttpPost]
        public ActionResult Delete(string id)
        {
            if (Session["Role"]?.ToString() != "1") return Content("Không có quyền!");

            string currentAdmin = Session["User_info"] is QLNHANVIEN admin ? admin.MANV : "";
            if (id == currentAdmin) return Content("<script>alert('Không thể tự xóa chính mình!'); window.location.href='/StaffManager/Index';</script>");

            try
            {
                var nv = db.QLNHANVIENs.Find(id);
                if (nv == null) return HttpNotFound();

                bool hasData = db.PHIEUMUONs.Any(p => p.MANV == id) || db.PHIEUTRAs.Any(p => p.MANV == id);
                if (hasData) return Content("<script>alert('Nhân viên này đã có dữ liệu nghiệp vụ, không thể xóa!'); window.location.href='/StaffManager/Index';</script>");

                var acc = db.TAIKHOANs.Find(id);
                if (acc != null) db.TAIKHOANs.Remove(acc);

                db.QLNHANVIENs.Remove(nv);
                db.SaveChanges();

                TempData["Success"] = "Đã xóa nhân viên " + id;
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return Content("<script>alert('Lỗi: " + ex.Message + "'); window.location.href='/StaffManager/Index';</script>");
            }
        }
        [HttpGet]
        public ActionResult Rules()
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");
            var maxSach = db.THAMSOes.Find("SoSachToiDa");
            var maxNgay = db.THAMSOes.Find("SoNgayMuon");
            var tienPhat = db.THAMSOes.Find("TienPhat");
            ViewBag.SoSachToiDa = maxSach != null ? maxSach.GIATRI : 5;
            ViewBag.SoNgayMuon = maxNgay != null ? maxNgay.GIATRI : 7;
            ViewBag.TienPhat = tienPhat != null ? tienPhat.GIATRI : 5000;

            return View();
        }
        [HttpPost]
        public ActionResult UpdateRules(int SoSachToiDa, int SoNgayMuon, int TienPhat)
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var p1 = db.THAMSOes.Find("SoSachToiDa");
                    var p2 = db.THAMSOes.Find("SoNgayMuon");
                    var p3 = db.THAMSOes.Find("TienPhat");

                    if (p1 != null) p1.GIATRI = SoSachToiDa;
                    if (p2 != null) p2.GIATRI = SoNgayMuon;
                    if (p3 != null) p3.GIATRI = TienPhat;

                    db.SaveChanges();
                    transaction.Commit();
                    TempData["Success"] = "Đã cập nhật quy định thành công!";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi: " + ex.Message;
                }
            }
            return RedirectToAction("Rules","StaffManager");
        }
        public ActionResult Stats(DateTime? fromDate, DateTime? toDate, int? year, int? quarter, int? month)
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");

            DateTime startDate, endDate;
            string timeLabel = "";
            bool isCustomDate = false;

            if (fromDate.HasValue && toDate.HasValue)
            {
                startDate = fromDate.Value;
                endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                timeLabel = $"{startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}";
                isCustomDate = true;

                year = null; quarter = null; month = null;
            }
            else
            {
                int selectedYear = year ?? DateTime.Now.Year;

                if (month.HasValue && month.Value > 0)
                {
                    startDate = new DateTime(selectedYear, month.Value, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    timeLabel = $"Tháng {month}/{selectedYear}";
                    quarter = null;
                }
                else if (quarter.HasValue && quarter.Value > 0)
                {
                    int startMonth = (quarter.Value - 1) * 3 + 1;
                    startDate = new DateTime(selectedYear, startMonth, 1);
                    endDate = startDate.AddMonths(3).AddDays(-1);
                    timeLabel = $"Quý {quarter}/{selectedYear}";
                    month = null;
                }
                else
                {
                    startDate = new DateTime(selectedYear, 1, 1);
                    endDate = new DateTime(selectedYear, 12, 31);
                    timeLabel = $"Năm {selectedYear}";
                }
                endDate = endDate.Date.AddDays(1).AddTicks(-1);
            }

            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");
            ViewBag.SelectedYear = year;
            ViewBag.SelectedQuarter = quarter;
            ViewBag.SelectedMonth = month;
            ViewBag.TimeLabel = timeLabel;
            ViewBag.IsCustomDate = isCustomDate;

            ViewBag.TongSach = db.QLSACHes.Sum(s => s.SL) ?? 0;
            ViewBag.TongDocGia = db.DOCGIAs.Count();
            ViewBag.LuotMuonMoi = db.PHIEUMUONs.Count(p => p.NgayMuon >= startDate && p.NgayMuon <= endDate);
            ViewBag.DoanhThuPhat = db.PHIEUTRAs.Where(pt => pt.NGAYTRA >= startDate && pt.NGAYTRA <= endDate).Sum(pt => pt.TIENPHAT) ?? 0;

            var chartLabels = new System.Collections.Generic.List<string>();
            var chartData = new System.Collections.Generic.List<int>();

            var rawData = db.PHIEUMUONs
                .Where(p => p.NgayMuon >= startDate && p.NgayMuon <= endDate)
                .GroupBy(p => new { Y = p.NgayMuon.Value.Year, M = p.NgayMuon.Value.Month })
                .Select(g => new { Year = g.Key.Y, Month = g.Key.M, Count = g.Count() })
                .ToList();

            DateTime iterator = startDate;
            int safetyCount = 0;
            while (iterator <= endDate && safetyCount < 24)
            {
                string label = iterator.ToString("MM/yyyy");
                chartLabels.Add(label);

                var dataItem = rawData.FirstOrDefault(x => x.Year == iterator.Year && x.Month == iterator.Month);
                chartData.Add(dataItem != null ? dataItem.Count : 0);

                iterator = iterator.AddMonths(1);
                safetyCount++;
            }

            ViewBag.ChartLabelsStr = Newtonsoft.Json.JsonConvert.SerializeObject(chartLabels);
            ViewBag.ChartDataStr = string.Join(",", chartData);

            var topBooks = db.CHITIETPMs
                .Where(ct => ct.PHIEUMUON.NgayMuon >= startDate && ct.PHIEUMUON.NgayMuon <= endDate)
                .GroupBy(ct => ct.MASACH)
                .Select(g => new { MaSach = g.Key, TongMuon = g.Sum(x => x.SLMUON) })
                .OrderByDescending(x => x.TongMuon).Take(5).ToList();

            ViewBag.TopSach = (from t in topBooks
                               join s in db.QLSACHes on t.MaSach equals s.MASACH
                               select new TopBookViewModel { TenSach = s.TENSACH, TacGia = s.TACGIA.TENTG, LuotMuon = t.TongMuon }).ToList();

            return View();
        }
        public ActionResult ExportToExcel(DateTime? fromDate, DateTime? toDate, int? year, int? quarter, int? month)
        {
            if (Session["Role"]?.ToString() != "1") return RedirectToAction("Index", "Home");

            DateTime startDate, endDate;
            string timeLabel = "";

            if (fromDate.HasValue && toDate.HasValue)
            {
                startDate = fromDate.Value;
                endDate = toDate.Value.Date.AddDays(1).AddTicks(-1);
                timeLabel = $"Tùy chỉnh ({startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy})";
            }
            else
            {
                int selectedYear = year ?? DateTime.Now.Year;
                if (month.HasValue && month.Value > 0)
                {
                    startDate = new DateTime(selectedYear, month.Value, 1);
                    endDate = startDate.AddMonths(1).AddDays(-1);
                    timeLabel = $"Tháng {month}/{selectedYear}";
                }
                else if (quarter.HasValue && quarter.Value > 0)
                {
                    int startMonth = (quarter.Value - 1) * 3 + 1;
                    startDate = new DateTime(selectedYear, startMonth, 1);
                    endDate = startDate.AddMonths(3).AddDays(-1);
                    timeLabel = $"Quý {quarter}/{selectedYear}";
                }
                else
                {
                    startDate = new DateTime(selectedYear, 1, 1);
                    endDate = new DateTime(selectedYear, 12, 31);
                    timeLabel = $"Năm {selectedYear}";
                }
                endDate = endDate.Date.AddDays(1).AddTicks(-1);
            }

            var data = db.PHIEUMUONs
                .Include(p => p.THETHUVIEN.DOCGIA)
                .Include(p => p.QLNHANVIEN)
                .Where(p => p.NgayMuon >= startDate && p.NgayMuon <= endDate)
                .OrderByDescending(p => p.NgayMuon)
                .ToList();

            using (ExcelPackage package = new ExcelPackage())
            {
                var ws = package.Workbook.Worksheets.Add("BaoCao");

                ws.Cells["A1:F1"].Style.Font.Bold = true;
                ws.Cells["A1:F1"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                ws.Cells["A1:F1"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                ws.Cells["A1:F1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                ws.Cells["A1"].Value = "Mã Phiếu";
                ws.Cells["B1"].Value = "Độc Giả";
                ws.Cells["C1"].Value = "Nhân Viên Duyệt";
                ws.Cells["D1"].Value = "Ngày Mượn";
                ws.Cells["E1"].Value = "Hạn Trả";
                ws.Cells["F1"].Value = "Trạng Thái";

                int row = 2;
                foreach (var item in data)
                {
                    ws.Cells[string.Format("A{0}", row)].Value = item.MAPM;
                    ws.Cells[string.Format("B{0}", row)].Value = item.THETHUVIEN.DOCGIA.TENDG;
                    ws.Cells[string.Format("C{0}", row)].Value = item.QLNHANVIEN != null ? item.QLNHANVIEN.TENNV : "Chưa duyệt";

                    ws.Cells[string.Format("D{0}", row)].Value = item.NgayMuon;
                    ws.Cells[string.Format("D{0}", row)].Style.Numberformat.Format = "dd/MM/yyyy";

                    ws.Cells[string.Format("E{0}", row)].Value = item.NgayDenHan;
                    ws.Cells[string.Format("E{0}", row)].Style.Numberformat.Format = "dd/MM/yyyy";

                    string status = "Đang mượn";
                    if (item.TINHTRANG == 1) status = "Đã trả";
                    else if (item.TINHTRANG == -1) status = "Chờ duyệt";
                    else if (item.TINHTRANG == 2) status = "Đã hủy";
                    ws.Cells[string.Format("F{0}", row)].Value = status;

                    row++;
                }

                ws.Cells[string.Format("A{0}", row + 1)].Value = "Tổng cộng: " + data.Count + " phiếu mượn.";
                ws.Cells[string.Format("A{0}", row + 1)].Style.Font.Italic = true;

                ws.Cells.AutoFitColumns();

                var stream = new MemoryStream();
                package.SaveAs(stream);

                string fileName = "BaoCao_" + DateTime.Now.ToString("yyyyMMdd_HHmm") + ".xlsx";
                string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                stream.Position = 0;
                return File(stream, contentType, fileName);
            }
        }
    }
}