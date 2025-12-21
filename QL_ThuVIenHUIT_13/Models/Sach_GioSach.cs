using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace QL_ThuVIenHUIT_13.Models
{
    public class Sach_GioSach
    {
        public string MaSach { get; set; }
        public string TenSach { get; set; }
        public string TacGia {  get; set; }
        public string HinhAnh { get; set; }
        public decimal TienTheChan { get; set; }
        private int _soluong;
        public int soLuong //Chỉ cho phép mượn tối đa một lần 3 cuốn giống nhau
        {
            get
            { 
                return _soluong;
            } 
            set
            {
                if (value <= 0)
                    _soluong = 1;
                if (value >= 3)
                    _soluong = 3;
                else
                    _soluong = value;
            }
        }
 

        CNPM_DATABASE_THUVIENEntities data = new CNPM_DATABASE_THUVIENEntities();
      
        public Sach_GioSach() { }
        public Sach_GioSach(string ma, int sl)
        {
            QLSACH sp = new QLSACH();
            sp = data.QLSACHes.Include("TACGIA").Include("BIASACH").FirstOrDefault(x => x.MASACH == ma);
            if (sp != null)
            {
                MaSach = sp.MASACH;
                TenSach = sp.TENSACH;
                TacGia = sp.TACGIA.TENTG;
                HinhAnh = sp.BIASACH != null ? sp.BIASACH.URL_ANH : null;
                soLuong = sl;
                TienTheChan = sl * 10000;
            }
        }
    }
}