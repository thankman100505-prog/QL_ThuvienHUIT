using System;
using System.Linq;
using QL_ThuVIenHUIT_13.Models;

namespace QL_ThuVIenHUIT_13.Helpers
{
    public static class DataHelper
    {
        // 1. Hàm sinh mã tự động 
        public static string GenerateNewID(CNPM_DATABASE_THUVIENEntities1 db, string tableName, string colName, string prefix, int len)
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
        public static string GetOrCreateTacGia(CNPM_DATABASE_THUVIENEntities1 db, string tenTG)
        {
            tenTG = tenTG.Trim();
            var tg = db.TACGIA.FirstOrDefault(t => t.TENTG.ToLower() == tenTG.ToLower());
            if (tg != null) return tg.MATG;

            var newTG = new TACGIA();
            newTG.MATG = GenerateNewID(db, "TACGIA", "MATG", "TG", 5);
            newTG.TENTG = tenTG;
            newTG.QUOCTICH = "Việt Nam";
            newTG.NGAYSINH = DateTime.Now.AddYears(-20);

            db.TACGIA.Add(newTG);
            db.SaveChanges();
            return newTG.MATG;
        }

        // 3. Hàm xử lý Thể Loại
        public static string GetOrCreateTheLoai(CNPM_DATABASE_THUVIENEntities1 db, string tenTL)
        {
            tenTL = tenTL.Trim();
            var tl = db.THELOAI.FirstOrDefault(t => t.TENTHELOAI.ToLower() == tenTL.ToLower());
            if (tl != null) return tl.MATHELOAI;

            var newTL = new THELOAI();
            newTL.MATHELOAI = GenerateNewID(db, "THELOAI", "MATHELOAI", "TL", 5);
            newTL.TENTHELOAI = tenTL;
            newTL.MOTA = "Mới cập nhật";

            db.THELOAI.Add(newTL);
            db.SaveChanges();
            return newTL.MATHELOAI;
        }

        // 4. Hàm xử lý NXB
        public static string GetOrCreateNXB(CNPM_DATABASE_THUVIENEntities1 db, string tenNXB)
        {
            tenNXB = tenNXB.Trim();
            var nxb = db.NHAXUATBAN.FirstOrDefault(n => n.TENNXB.ToLower() == tenNXB.ToLower());
            if (nxb != null) return nxb.MAXB;

            var newNXB = new NHAXUATBAN();
            newNXB.MAXB = GenerateNewID(db, "NHAXUATBAN", "MAXB", "NXB", 4);
            newNXB.TENNXB = tenNXB;
            newNXB.DCHI = "Đang cập nhật";

            db.NHAXUATBAN.Add(newNXB);
            db.SaveChanges();
            return newNXB.MAXB;
        }
    }
}