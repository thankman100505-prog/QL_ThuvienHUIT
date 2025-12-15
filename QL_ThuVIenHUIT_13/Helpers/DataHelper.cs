using QL_ThuVIenHUIT_13.Models;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;

namespace QL_ThuVIenHUIT_13.Helpers
{
    public static class DataHelper
    {
        // 1. Hàm sinh mã tự động 
        public static string GenerateNewID(CNPM_DATABASE_THUVIENEntities db, string tableName, string colName, string prefix, int len)
        {
            string query = $"SELECT TOP 1 {colName} FROM {tableName} ORDER BY {colName} DESC";
            var lastId = db.Database.SqlQuery<string>(query).FirstOrDefault();

            if (string.IsNullOrEmpty(lastId))
            {
                return prefix + new string('0', len) + "1";
            }

            string numberPart = lastId.Substring(prefix.Length);
            int nextNumber = int.Parse(numberPart) + 1;

            return prefix + nextNumber.ToString("D" + len);
        }

        // 2. Hàm xử lý Tác Giả
        public static string GetOrCreateTacGia(CNPM_DATABASE_THUVIENEntities db, string tenTG)
        {
            tenTG = tenTG.Trim();
            var tg = db.TACGIAs.FirstOrDefault(t => t.TENTG.ToLower() == tenTG.ToLower());
            if (tg != null) return tg.MATG;

            var newTG = new TACGIA();
            newTG.MATG = GenerateNewID(db, "TACGIA", "MATG", "TG", 5);
            newTG.TENTG = tenTG;
            newTG.QUOCTICH = "Việt Nam";
            newTG.NGAYSINH = DateTime.Now.AddYears(-20);

            db.TACGIAs.Add(newTG);
            db.SaveChanges();
            return newTG.MATG;
        }

        // 3. Hàm xử lý Thể Loại
        public static string GetOrCreateTheLoai(CNPM_DATABASE_THUVIENEntities db, string tenTL)
        {
            tenTL = tenTL.Trim();
            var tl = db.THELOAIs.FirstOrDefault(t => t.TENTHELOAI.ToLower() == tenTL.ToLower());
            if (tl != null) return tl.MATHELOAI;

            var newTL = new THELOAI();
            newTL.MATHELOAI = GenerateNewID(db, "THELOAI", "MATHELOAI", "TL", 5);
            newTL.TENTHELOAI = tenTL;
            newTL.MOTA = "Mới cập nhật";

            db.THELOAIs.Add(newTL);
            db.SaveChanges();
            return newTL.MATHELOAI;
        }

        // 4. Hàm xử lý NXB
        public static string GetOrCreateNXB(CNPM_DATABASE_THUVIENEntities db, string tenNXB)
        {
            tenNXB = tenNXB.Trim();
            var nxb = db.NHAXUATBANs.FirstOrDefault(n => n.TENNXB.ToLower() == tenNXB.ToLower());
            if (nxb != null) return nxb.MAXB;

            var newNXB = new NHAXUATBAN();
            newNXB.MAXB = GenerateNewID(db, "NHAXUATBAN", "MAXB", "NXB", 4);
            newNXB.TENNXB = tenNXB;
            newNXB.DCHI = "Đang cập nhật";

            db.NHAXUATBANs.Add(newNXB);
            db.SaveChanges();
            return newNXB.MAXB;
        }
        public static string GenerateRandomPassword(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static void SendEmailToUser(string toEmail, string newPassword)
        {
            string fromEmail = "loc375682@gmail.com";
            string passwordApp = "xhnt vcwb wcvu jukq";

            MailMessage mail = new MailMessage();
            mail.To.Add(toEmail);
            mail.From = new MailAddress(fromEmail);
            mail.Subject = "[Thư viện HUIT] - Cấp lại mật khẩu mới";
            mail.Body = $"Chào bạn,\n\nMật khẩu mới để đăng nhập vào hệ thống thư viện của bạn là: {newPassword}\n\nVui lòng đăng nhập và đổi lại mật khẩu ngay.";
            mail.IsBodyHtml = false;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com");
            smtp.EnableSsl = true;
            smtp.Port = 587;
            smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
            smtp.Credentials = new NetworkCredential(fromEmail, passwordApp);

            smtp.Send(mail);
        }
        public static byte[] HashPassword(string Password)
        {
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