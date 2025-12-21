using OfficeOpenXml;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
using OfficeOpenXml.Style;
using QL_ThuVIenHUIT_13.Helpers;
using QL_ThuVIenHUIT_13.Models;
using System;
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Web.Mvc;
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
                    var innerMessage = ex.InnerException != null && ex.InnerException.InnerException != null
                                       ? ex.InnerException.InnerException.Message
                                       : ex.Message;
                    ModelState.AddModelError("", "Lỗi: " + innerMessage);
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
            return RedirectToAction("Rules", "StaffManager");
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
        public ActionResult QLDocGia()
        {
            var dsDG = db.DOCGIAs.ToList();
            return View(dsDG);
        }

        public ActionResult ThemDocGia()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemDocGia(DOCGIA dg)
        {
            if (!string.IsNullOrEmpty(dg.SODT) && !Regex.IsMatch(dg.SODT, @"^0[0-9]{9}$"))
                ModelState.AddModelError("SODT", "Số điện thoại phải có 10 chữ số và bắt đầu bằng số 0.");

            if (!string.IsNullOrEmpty(dg.MAIL) && !Regex.IsMatch(dg.MAIL, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                ModelState.AddModelError("MAIL", "Địa chỉ Email không đúng định dạng.");
            if (db.DOCGIAs.Any(x => x.SODT == dg.SODT))
            {
                ModelState.AddModelError("SODT", "Số điện thoại này đã tồn tại trong hệ thống.");
            }

            if (db.DOCGIAs.Any(x => x.MAIL == dg.MAIL))
            {
                ModelState.AddModelError("MAIL", "Email này đã tồn tại trong hệ thống.");
            }
            if (ModelState.IsValid)
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        dg.MADG = SinhMaDocGia(); 
                        db.DOCGIAs.Add(dg);
                        db.SaveChanges();
                        var card = new THETHUVIEN
                        {
                            MATHE = "TH" + dg.MADG.Substring(2),
                            MADG = dg.MADG,
                            NGAYCAP = DateTime.Now,
                            NGAYHETHAN = DateTime.Now.AddYears(2),
                            TRANGTHAI = "Hoạt động"
                        };
                        db.THETHUVIENs.Add(card);
                        db.SaveChanges();

                        transaction.Commit();
                        TempData["Success"] = "Thêm độc giả thành công!";
                        return RedirectToAction("QLDocGia");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                    }
                }
            }

            return View(dg);
        }
        private string SinhMaDocGia()
        {
            var last_DocGiaID = db.DOCGIAs.OrderByDescending(x => x.MADG).Select(x => x.MADG).FirstOrDefault();
            int next = 1;
            if (last_DocGiaID != null) next = int.Parse(last_DocGiaID.Substring(2)) + 1;
            return "DG" + next.ToString("D5");
        }

        public ActionResult Xoa_DG(string id)
        {
            var dg = db.DOCGIAs.FirstOrDefault(x => x.MADG == id);
            if (dg == null) return HttpNotFound();
            return View(dg);
        }

        [HttpPost]
        public ActionResult Xoa_DG_Confi(string id)
        {
            try
            {
                var dg = db.DOCGIAs.Find(id);
                if (dg == null) return HttpNotFound();
                bool hasHistory = db.THETHUVIENs.Any(t => t.MADG == id &&
                                 (db.PHIEUMUONs.Any(p => p.MATHE == t.MATHE) ||
                                  db.PHIEU_MUONPHONG.Any(mp => mp.MATHE == t.MATHE)));

                if (hasHistory)
                {
                    var the = db.THETHUVIENs.FirstOrDefault(t => t.MADG == id);
                    if (the != null) the.TRANGTHAI = "Bị khóa";
                    db.SaveChanges();

                    TempData["Error"] = "Độc giả này đã có lịch sử giao dịch. Hệ thống đã chuyển trạng thái thẻ sang 'Bị khóa' thay vì xóa!";
                    return RedirectToAction("QLDocGia");
                }

                var theTG = db.THETHUVIENs.FirstOrDefault(t => t.MADG == id);
                if (theTG != null)
                {
                    var acc = db.TAIKHOANs.Find(theTG.MATHE);
                    if (acc != null) db.TAIKHOANs.Remove(acc);

                    db.THETHUVIENs.Remove(theTG);
                }

                db.DOCGIAs.Remove(dg);
                db.SaveChanges();

                TempData["Success"] = "Đã xóa độc giả và tài khoản liên quan.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi xóa: " + ex.Message;
            }
            return RedirectToAction("QLDocGia");
        }
        public ActionResult Sua_InformDG(string id)
        {
            var dg = db.DOCGIAs.FirstOrDefault(x => x.MADG == id);
            if (dg == null) return HttpNotFound();

            return View(dg);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Sua_InformDG(DOCGIA dg)
        {
            var old = db.DOCGIAs.FirstOrDefault(x => x.MADG == dg.MADG);
            if (old == null) return HttpNotFound();

            if (!string.IsNullOrEmpty(dg.SODT) && !Regex.IsMatch(dg.SODT, @"^0[0-9]{9}$"))
            {
                ModelState.AddModelError("SODT", "Số điện thoại phải có 10 chữ số và bắt đầu bằng số 0.");
            }
            if (!string.IsNullOrEmpty(dg.MAIL) && !Regex.IsMatch(dg.MAIL, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                ModelState.AddModelError("MAIL", "Email không đúng định dạng (ví dụ: abc@gmail.com).");
            }
            if (dg.SODT != old.SODT && db.DOCGIAs.Any(x => x.SODT == dg.SODT))
            {
                ModelState.AddModelError("SODT", "Số điện thoại này đã được sử dụng bởi người khác.");
            }
            if (dg.MAIL != old.MAIL && db.DOCGIAs.Any(x => x.MAIL == dg.MAIL))
            {
                ModelState.AddModelError("MAIL", "Email này đã được sử dụng bởi người khác.");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    old.TENDG = dg.TENDG;
                    old.KHOA = dg.KHOA;
                    old.LOP = dg.LOP;
                    old.DIACHI = dg.DIACHI;
                    old.SODT = dg.SODT;
                    old.MAIL = dg.MAIL;

                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật thông tin độc giả thành công!";
                    return RedirectToAction("QLDocGia");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi lưu dữ liệu: " + ex.Message);
                }
            }
            return View(dg);
        }
        public ActionResult QLPhongHop()
        {
            var ds = db.PHONGHOPs.ToList();
            return View(ds);
        }

        [HttpGet]
        public ActionResult ThemPhongHop() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ThemPhongHop(PHONGHOP ph)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    ph.MAPHONG = SinhMaPhongHop();
                    ph.TINHTRANG = 1;
                    db.PHONGHOPs.Add(ph);
                    db.SaveChanges();
                    TempData["Success"] = "Thêm phòng thành công!";
                    return RedirectToAction("QLPhongHop");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi: " + ex.Message);
                }
            }
            return View(ph);
        }

        [HttpGet]
        public ActionResult SuaPhongHop(string id)
        {
            var ph = db.PHONGHOPs.Find(id);
            if (ph == null) return HttpNotFound();
            return View(ph);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SuaPhongHop(PHONGHOP ph)
        {
            if (ModelState.IsValid)
            {
                var old = db.PHONGHOPs.Find(ph.MAPHONG);
                if (old != null)
                {
                    old.VITRI = ph.VITRI;
                    old.SL_NGUOITOIDA = ph.SL_NGUOITOIDA;
                    old.TINHTRANG = ph.TINHTRANG;
                    db.SaveChanges();
                    TempData["Success"] = "Cập nhật thành công!";
                    return RedirectToAction("QLPhongHop");
                }
            }
            return View(ph);
        }

        [HttpPost]
        public ActionResult XoaPhongHop(string id)
        {
            bool hasData = db.PHIEU_MUONPHONG.Any(m => m.MAPH == id);
            if (hasData)
            {
                TempData["Error"] = "Phòng đang có dữ liệu mượn, không thể xóa!";
            }
            else
            {
                var ph = db.PHONGHOPs.Find(id);
                if (ph != null)
                {
                    db.PHONGHOPs.Remove(ph);
                    db.SaveChanges();
                    TempData["Success"] = "Đã xóa phòng!";
                }
            }
            return RedirectToAction("QLPhongHop");
        }

        private string SinhMaPhongHop()
        {
            var lastID = db.PHONGHOPs.OrderByDescending(x => x.MAPHONG).Select(x => x.MAPHONG).FirstOrDefault();
            if (lastID == null) return "PH00001";
            int next = int.Parse(lastID.Substring(2)) + 1;
            return "PH" + next.ToString("D5");
        }
        public ActionResult DuyetMuonSach()
        {
            var dsChoDuyet = db.PHIEUMUONs
                               .Include(p => p.THETHUVIEN.DOCGIA)
                               .Where(p => p.TINHTRANG == -1)
                               .OrderByDescending(p => p.NgayMuon)
                               .ToList();
            return View(dsChoDuyet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanChoMuon(string id)
        {
            var pm = db.PHIEUMUONs.Include(p => p.CHITIETPMs).FirstOrDefault(p => p.MAPM == id);
            if (pm == null) return HttpNotFound();

            string maNV = (Session["User_info"] is QLNHANVIEN nv) ? nv.MANV : "ADMIN";

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    foreach (var ct in pm.CHITIETPMs)
                    {
                        var sach = db.QLSACHes.Find(ct.MASACH);
                        if (sach != null)
                        {
                            if ((sach.TINHTRANG ?? 0) < ct.SLMUON)
                            {
                                throw new Exception("Sách " + sach.TENSACH + " đã hết hàng, không thể duyệt!");
                            }
                            sach.TINHTRANG -= ct.SLMUON;
                        }
                    }
                    pm.TINHTRANG = 1;
                    pm.MANV = maNV;
                    pm.NgayMuon = DateTime.Now;
                    pm.NgayDenHan = DateTime.Now.AddDays(7);

                    db.SaveChanges();
                    transaction.Commit();
                    TempData["Success"] = "Đã duyệt phiếu mượn " + id + " thành công!";
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi duyệt phiếu: " + ex.Message;
                }
            }
            return RedirectToAction("DuyetMuonSach");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult TuChoiPhieuMuon(string id)
        {
            var pm = db.PHIEUMUONs.Find(id);
            if (pm != null)
            {
                pm.TINHTRANG = 2;
                db.SaveChanges();
                TempData["Success"] = "Đã từ chối phiếu mượn " + id;
            }
            return RedirectToAction("DuyetMuonSach");
        }
        public ActionResult TraSach(string maPM)
        {
            var dsDangMuon = db.PHIEUMUONs
                               .Include(p => p.THETHUVIEN.DOCGIA)
                               .Where(p => p.TINHTRANG == 1)
                               .OrderByDescending(p => p.NgayMuon)
                               .ToList();
            ViewBag.DsDangMuon = dsDangMuon;

            if (string.IsNullOrEmpty(maPM)) return View();
            var phieu = db.PHIEUMUONs
                          .Include(p => p.THETHUVIEN.DOCGIA)
                          .Include(p => p.CHITIETPMs.Select(ct => ct.QLSACH))
                          .FirstOrDefault(p => p.MAPM == maPM && p.TINHTRANG == 1);

            if (phieu == null)
            {
                ViewBag.Error = "Không tìm thấy phiếu mượn hợp lệ!";
                return View();
            }
            int soNgayTre = 0;
            decimal tienPhat = 0;

            if (DateTime.Now > phieu.NgayDenHan)
            {
                soNgayTre = (DateTime.Now - phieu.NgayDenHan.Value).Days;
                var ts = db.THAMSOes.FirstOrDefault(x => x.TENTHAMSO == "TienPhatMoiNgay");
                decimal donGiaPhat = ts != null ? (decimal)ts.GIATRI : 2000;
                tienPhat = soNgayTre * donGiaPhat;
            }

            ViewBag.SoNgayTre = soNgayTre;
            ViewBag.TienPhat = tienPhat;

            return View(phieu);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult XacNhanTraSach(string maPM, decimal tienPhat)
        {
            var pm = db.PHIEUMUONs.Include(p => p.CHITIETPMs).FirstOrDefault(p => p.MAPM == maPM);
            if (pm == null) return HttpNotFound();

            string maNV = (Session["User_info"] is QLNHANVIEN nv) ? nv.MANV : "ADMIN";

            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    PHIEUTRA pt = new PHIEUTRA();
                    pt.MAPT = DataHelper.GenerateNewID(db, "PHIEUTRA", "MAPT", "PT", 5);
                    pt.MAPM = maPM;
                    pt.MANV = maNV;
                    pt.NGAYTRA = DateTime.Now;
                    pt.TIENPHAT = tienPhat;
                    db.PHIEUTRAs.Add(pt);
                    pm.TINHTRANG = 2;

                    foreach (var ct in pm.CHITIETPMs)
                    {
                        var sach = db.QLSACHes.Find(ct.MASACH);
                        if (sach != null)
                        {
                            sach.TINHTRANG = (sach.TINHTRANG ?? 0) + ct.SLMUON;
                        }
                    }

                    db.SaveChanges();
                    transaction.Commit();

                    TempData["Success"] = "Trả sách thành công cho phiếu " + maPM;
                    return RedirectToAction("TraSach");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    TempData["Error"] = "Lỗi khi xử lý trả sách: " + ex.Message;
                    return RedirectToAction("TraSach", new { maPM = maPM });
                }
            }
        }
    }
}