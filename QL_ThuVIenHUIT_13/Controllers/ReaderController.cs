using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using QL_ThuVIenHUIT_13.Models;

namespace QL_ThuVIenHUIT_13.Controllers
{
    public class ReaderController : Controller
    {
        // GET: Reader
        CNPM_DATABASE_THUVIENEntities db = new CNPM_DATABASE_THUVIENEntities();
        public ActionResult SignIn()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SigninSubmit(string Username, string Password,int Remember_me=0)
        {
            byte[] hashedPassword = HashPassword(Password);
            TAIKHOAN tk = db.TAIKHOANs.FirstOrDefault(x => x.USERNAME == Username && x.PASS == hashedPassword);
            if (tk == null)
            {
                Session["Error"] = "Thông tin đăng nhập không đúng!";
                return RedirectToAction("SignIn", "Reader");
            }
            Session["Username"] = tk.USERNAME;
            Session["User_info"] = null;
            if (tk.USERNAME.StartsWith("TH"))
            {
                Session["User_info"]=db.THETHUVIENs.FirstOrDefault(x=>x.MATHE==tk.USERNAME);
                DOCGIA dg=db.DOCGIAs.FirstOrDefault(x=>x.MADG==tk.USERNAME);
                Session["NameDisplay"]=dg.TENDG;
            }

            else if (tk.USERNAME.StartsWith("NV"))
            {
                QLNHANVIEN nv = db.QLNHANVIENs.FirstOrDefault(x => x.MANV == tk.USERNAME);
                Session["User_info"] = nv;
                Session["NameDisplay"] = nv.TENNV;
            }
            else
            {
                Session["NameDisplay"] = "Admin";
            } 
            // Lưu vào COOKIE (3 ngày)
            if (Remember_me == 1)
            {
                HttpCookie cookie = new HttpCookie("UserLogin");
                cookie["Username"] = tk.USERNAME;

                // Thời gian hết hạn
                cookie.Expires = DateTime.Now.AddDays(3);

                // Lưu xuống trình duyệt
                Response.Cookies.Add(cookie);

            }
            return RedirectToAction("Index", "Home");

        }

        public ActionResult Logout()
        {
            // Xóa SESSION
            Session.Clear();
            Session.Abandon();

            // Xóa COOKIE lưu đăng nhập
            if (Request.Cookies["Username"] != null)
            {
                HttpCookie cookie = new HttpCookie("Username");
                cookie.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Add(cookie);
            }
            return RedirectToAction("SignIn", "Reader");
        }


        public byte[] HashPassword(string Password) 
        {
            // Hash mật khẩu người dùng nhập bằng SHA256 (không salt)
            byte[] passwordBytes = Encoding.UTF8.GetBytes(Password);
            byte[] hashedBytes;

            using (SHA256 sha256 = SHA256.Create())
            {
                hashedBytes = sha256.ComputeHash(passwordBytes);
            }
            return hashedBytes;
        }
    }
}