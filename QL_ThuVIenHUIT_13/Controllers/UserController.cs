using QL_ThuVIenHUIT_13.Models;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using QL_ThuVIenHUIT_13.Helpers;
namespace QL_ThuVIenHUIT_13.Controllers
{
    public class UserController : Controller
    {
        readonly CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SigninSubmit(string Username, string Password, int Remember_me = 0)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                Session["Error"] = "Vui lòng nhập đầy đủ thông tin!";
                return RedirectToAction("SignIn");
            }
            byte[] hashedPassword = DataHelper.HashPassword(Password);
            var tk = db.TAIKHOANs.FirstOrDefault(x => x.USERNAME == Username && x.PASS == hashedPassword);

            if (tk == null)
            {
                Session["Error"] = "Tài khoản hoặc mật khẩu không chính xác!";
                return RedirectToAction("SignIn");
            }
            Session.Clear();
            Session["Username"] = tk.USERNAME;
            Session["Role"] = tk.ROLE_ID;
            switch (tk.ROLE_ID)
            {
                case 1:
                    Session["NameDisplay"] = "Quản Trị Viên (Admin)";
                    Session["User_info"] = tk;
                    break;

                case 2: 
                    var nhanvien = db.QLNHANVIENs.FirstOrDefault(x => x.MANV == tk.USERNAME);
                    if (nhanvien != null)
                    {
                        Session["NameDisplay"] = nhanvien.TENNV;
                        Session["User_info"] = nhanvien;
                    }
                    break;

                case 3:
                    var theThuVien = db.THETHUVIENs.FirstOrDefault(x => x.MATHE == tk.USERNAME);

                    if (theThuVien != null)
                    { 
                        if (theThuVien.TRANGTHAI != "Hoạt động")
                        {
                            Session.Abandon();
                            Session["Error"] = "Thẻ thư viện của bạn đã bị khóa hoặc hết hạn. Vui lòng liên hệ thủ thư.";
                            return RedirectToAction("SignIn");
                        }
                        var docGia = db.DOCGIAs.FirstOrDefault(x => x.MADG == theThuVien.MADG);
                        Session["NameDisplay"] = docGia != null ? docGia.TENDG : "Độc giả";
                        Session["User_info"] = docGia;
                        Session["MaThe"] = tk.USERNAME;
                    }
                    break;

                default:
                    Session["NameDisplay"] = "User";
                    break;
            }
            if (Remember_me == 1)
            {
                HttpCookie cookie = new HttpCookie("UserLogin");
                cookie["Username"] = tk.USERNAME;
                cookie["Role"] = tk.ROLE_ID.ToString();
                cookie.Expires = DateTime.Now.AddDays(3);
                Response.Cookies.Add(cookie);
            }
            if (tk.ROLE_ID == 1 || tk.ROLE_ID == 2)
            {
                return RedirectToAction("Index", "Home");
            }
            return RedirectToAction("Index", "Home");
        }

        // GET: User/Register
        public ActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterSubmit(Registration model)
        {
            if (!ModelState.IsValid)
            {
                return View("Register", model); 
            }
            if (model.MatKhau != model.NhapLaiMatKhau)
            {
                ModelState.AddModelError("NhapLaiMatKhau", "Mật khẩu xác nhận không khớp!");
                return View("Register", model);
            }

            try
            {
                var maTheParam = new System.Data.SqlClient.SqlParameter
                {
                    ParameterName = "@MATHE_OUT",
                    SqlDbType = System.Data.SqlDbType.Char,
                    Size = 7,
                    Direction = System.Data.ParameterDirection.Output
                };

                var parameters = new object[]
                {
            new System.Data.SqlClient.SqlParameter("@TENDG", model.HoTen),
            new System.Data.SqlClient.SqlParameter("@KHOA", (object)model.Khoa ?? DBNull.Value),
            new System.Data.SqlClient.SqlParameter("@LOP", (object)model.CMND ?? DBNull.Value),
            new System.Data.SqlClient.SqlParameter("@DIACHI", model.DiaChi),
            new System.Data.SqlClient.SqlParameter("@SODT", model.DienThoai),
            new System.Data.SqlClient.SqlParameter("@MAIL", model.Email),
            maTheParam
                };

                db.Database.ExecuteSqlCommand("EXEC SP_REGISTER_DOCGIA @TENDG, @KHOA, @LOP, @DIACHI, @SODT, @MAIL, @MATHE_OUT OUT", parameters);

                string newMaThe = maTheParam.Value.ToString();
                var tk = db.TAIKHOANs.FirstOrDefault(x => x.USERNAME == newMaThe);
                if (tk != null)
                {
                    tk.PASS = DataHelper.HashPassword(model.MatKhau);
                    db.SaveChanges();
                }

                ViewBag.Mathe = newMaThe;
                return View("RegisterSuccess", "User");
            }
            catch (Exception ex)
            {
                ViewBag.Error = ex.Message;
                if (ex.InnerException != null)
                {
                    ViewBag.Error += " (" + ex.InnerException.Message + ")";
                }
                return View("Register", model);
            }
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            if (Request.Cookies["UserLogin"] != null)
            {
                HttpCookie cookie = new HttpCookie("UserLogin");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }

            return RedirectToAction("SignIn", "User");
        }
        public ActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPasswordSubmit(string Email)
        {
            string usernameFound = null;
            var docGia = db.DOCGIAs.FirstOrDefault(x => x.MAIL == Email);
            if (docGia != null)
            {
                var theThuVien = db.THETHUVIENs.FirstOrDefault(x => x.MADG == docGia.MADG);

                if (theThuVien != null)
                {
                    usernameFound = theThuVien.MATHE;
                }
            }
            if (usernameFound == null)
            {
                var nhanVien = db.QLNHANVIENs.FirstOrDefault(x => x.MAIL == Email);
                if (nhanVien != null)
                {
                    usernameFound = nhanVien.MANV;
                }
            }
            if (usernameFound == null)
            {
                ViewBag.Error = "Email này chưa được đăng ký hoặc chưa được cấp thẻ thư viện!";
                return View("ForgotPassword");
            }
            var tk = db.TAIKHOANs.FirstOrDefault(x => x.USERNAME == usernameFound);
            if (tk == null)
            {
                ViewBag.Error = "Tài khoản hệ thống đang bị khóa hoặc không tồn tại.";
                return View("ForgotPassword");
            }
            try
            {
                string newPass = DataHelper.GenerateRandomPassword(6);
                tk.PASS = DataHelper.HashPassword(newPass);
                db.SaveChanges();

                DataHelper.SendEmailToUser(Email, newPass);

                TempData["Success"] = "Mật khẩu mới đã gửi về Email. Vui lòng kiểm tra!";
                return RedirectToAction("SignIn", "User");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi gửi mail: " + ex.Message;
                return View("ForgotPassword");
            }
        }
        // GET: Hiển thị form đổi mật khẩu
        public ActionResult ChangePassword()
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("SignIn", "User");
            }
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePasswordSubmit(string CurrentPassword, string NewPassword, string ConfirmNewPassword)
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("SignIn", "User");
            }
            if (string.IsNullOrEmpty(CurrentPassword) || string.IsNullOrEmpty(NewPassword) || string.IsNullOrEmpty(ConfirmNewPassword))
            {
                ViewBag.Error = "Vui lòng nhập đầy đủ thông tin!";
                return View("ChangePassword");
            }

            if (NewPassword != ConfirmNewPassword)
            {
                ViewBag.Error = "Mật khẩu xác nhận không khớp!";
                return View("ChangePassword");
            }
            string username = Session["Username"].ToString();
            byte[] hashedCurrentPass = DataHelper.HashPassword(CurrentPassword);

            var tk = db.TAIKHOANs.FirstOrDefault(x => x.USERNAME == username && x.PASS == hashedCurrentPass);

            if (tk == null)
            {
                ViewBag.Error = "Mật khẩu hiện tại không chính xác!";
                return View("ChangePassword");
            }
            try
            {
                tk.PASS = DataHelper.HashPassword(NewPassword);
                db.SaveChanges();

                TempData["Success"] = "Đổi mật khẩu thành công! Vui lòng đăng nhập lại.";
                Session.Clear();
                Session.Abandon();

                return RedirectToAction("SignIn", "User");
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi hệ thống: " + ex.Message;
                return View("ChangePassword");
            }
        }
        public ActionResult ProfileUser()   
        {
            if (Session["Username"] == null)
            {
                return RedirectToAction("SignIn", "User");
            }

            string username = Session["Username"].ToString();
            int roleId = Convert.ToInt32(Session["Role"]);
            UserProfileVM model = new UserProfileVM();
            model.RoleName = roleId == 3 ? "Độc giả" : "Nhân viên";
            if (roleId == 3) 
            {
                var the = db.THETHUVIENs.FirstOrDefault(x => x.MATHE == username);
                if (the != null)
                {
                    var dg = db.DOCGIAs.FirstOrDefault(x => x.MADG == the.MADG);
                    if (dg != null)
                    {
                        model.MaSo = the.MATHE;
                        model.HoTen = dg.TENDG;
                        model.SDT = dg.SODT;
                        model.Email = dg.MAIL;
                        model.DiaChi = dg.DIACHI;
                        model.Khoa = dg.KHOA;
                        model.Lop = dg.LOP;
                        model.NgayHetHan = the.NGAYHETHAN;
                    }
                }
            }
            else 
            {
                var nv = db.QLNHANVIENs.FirstOrDefault(x => x.MANV == username);
                if (nv != null)
                {
                    model.MaSo = nv.MANV;
                    model.HoTen = nv.TENNV;
                    model.NgaySinh = nv.NGSINH;
                    model.SDT = nv.SDIENTHOAI;
                    model.Email = nv.MAIL;
                    model.ChucVu = nv.CHUCVU;
                }
                else if (username == "admin") 
                {
                    model.MaSo = "ADMIN";
                    model.HoTen = "Quản trị viên hệ thống";
                    model.ChucVu = "Super Admin";
                }
            }

            return View(model);
        }
        public ActionResult EditProfile()
        {
            if (Session["Username"] == null) return RedirectToAction("SignIn", "User");
            var result = ProfileUser() as ViewResult;
            var model = result.Model as UserProfileVM;
            return View(model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditProfileSubmit(UserProfileVM model)
        {
            if (Session["Username"] == null) return RedirectToAction("SignIn", "User");
            if (string.IsNullOrEmpty(model.HoTen) || string.IsNullOrEmpty(model.SDT) || string.IsNullOrEmpty(model.Email))
            {
                ViewBag.Error = "Vui lòng không để trống Họ tên, SĐT hoặc Email.";
                return View("EditProfile", model);
            }

            string username = Session["Username"].ToString();
            int roleId = Convert.ToInt32(Session["Role"]);

            try
            {
                if (roleId == 3)
                {

                    var the = db.THETHUVIENs.FirstOrDefault(x => x.MATHE == username);
                    if (the != null)
                    {
                        var dg = db.DOCGIAs.FirstOrDefault(x => x.MADG == the.MADG);
                        if (dg != null)
                        {
                            dg.TENDG = model.HoTen;
                            dg.DIACHI = model.DiaChi;
                            dg.SODT = model.SDT;
                            dg.MAIL = model.Email;
                            Session["NameDisplay"] = dg.TENDG;
                        }
                    }
                }
                else 
                {
                    var nv = db.QLNHANVIENs.FirstOrDefault(x => x.MANV == username);
                    if (nv != null)
                    {
                        nv.TENNV = model.HoTen;
                        nv.SDIENTHOAI = model.SDT;
                        nv.MAIL = model.Email;
                        Session["NameDisplay"] = nv.TENNV;
                    }
                }
                db.SaveChanges();
                TempData["Success"] = "Cập nhật thông tin thành công!";
                return RedirectToAction("ProfileUser", "User"); 
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Lỗi cập nhật: " + ex.Message;
                return View("EditProfile", model);
            }
        }
    }
}