using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace QL_ThuVIenHUIT_13.Models
{
    public class Registration
    {
        [Display(Name = "CMND/CCCD/Thẻ SV")]
        [Required(ErrorMessage = "CMND/CCCD/Thẻ SV bắt buộc")]
        public string CMND { get; set; }

        [Display(Name = "Địa chỉ")]
        public string DiaChi { get; set; }

        [Display(Name = "Đối tượng")]
        public string DoiTuong { get; set; } // Sinh viên, CBGV, Khác

        [Display(Name = "Khoa")]
        public string Khoa { get; set; }   // NULL nếu là độc giả bên ngoài

        [Display(Name = "Lớp")]
        public string Lop { get; set; }    // NULL nếu là độc giả bên ngoài

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Display(Name = "Điện thoại")]
        public string DienThoai { get; set; }

        [Display(Name = "Ghi chú")]
        public string GhiChu { get; set; }

        [Display(Name = "Mật khẩu")]
        [DataType(DataType.Password)]
        public string MatKhau { get; set; }

        [Display(Name = "Nhập lại mật khẩu")]
        [DataType(DataType.Password)]
        [Compare("MatKhau", ErrorMessage = "Mật khẩu nhập lại không khớp")]
        public string NhapLaiMatKhau { get; set; }

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
