using System;
using System.ComponentModel.DataAnnotations;

namespace QL_ThuVIenHUIT_13.Models
{
    public class Registration
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [Display(Name = "Họ và tên")]
        public string HoTen { get; set; }

        [Display(Name = "Mã số SV / CMND / CCCD")]
        public string CMND { get; set; } 

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ liên hệ")]
        public string DiaChi { get; set; }

        [Display(Name = "Đối tượng (Sinh viên/Giảng viên)")]
        public string DoiTuong { get; set; }

        [Display(Name = "Khoa / Đơn vị công tác")]
        public string Khoa { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập Email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0\d{9}$", ErrorMessage = "SĐT phải bắt đầu bằng 0 và có 10 số")]
        [Display(Name = "Số điện thoại")]
        public string DienThoai { get; set; }

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải ít nhất 6 ký tự")]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu")]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [Display(Name = "Nhập lại mật khẩu")]
        public string NhapLaiMatKhau { get; set; }
    }
}