using System;
using System.ComponentModel.DataAnnotations;

namespace QL_ThuVIenHUIT_13.Models
{
    public class UserProfileVM
    {
        [Display(Name = "Mã số")]
        public string MaSo { get; set; } // Mã thẻ hoặc Mã nhân viên

        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Display(Name = "Ngày sinh")]
        public DateTime? NgaySinh { get; set; }

        [Display(Name = "Số điện thoại")]
        public string SDT { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Khoa")]
        public string Khoa { get; set; }

        [Display(Name = "Lớp")]
        public string Lop { get; set; }

        [Display(Name = "Ngày hết hạn thẻ")]
        public DateTime? NgayHetHan { get; set; }

        [Display(Name = "Chức vụ")]
        public string ChucVu { get; set; }

        public string RoleName { get; set; }
    }
}