using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Ajax.Utilities;

namespace QL_ThuVIenHUIT_13.Models
{
    public class GioSach
    {
        CNPM_DATABASE_THUVIENEntities data = new CNPM_DATABASE_THUVIENEntities();
        public List<Sach_GioSach> List_Sach = new List<Sach_GioSach>();
        //Phương thức cho giỏ hàng
        
        ///Tính tổng số lượng sản phẩm trong giỏ hàng
        public int TongSL()
        {
            return List_Sach.Sum(x => x.soLuong);
        }

        public double TongTC()
        {

            return List_Sach.Sum(x => (double)x.TienTheChan * x.soLuong);
        }

        public int ThemSach(string id, int soLuong)
        {
            Sach_GioSach item = new Sach_GioSach();
            item = List_Sach.Find(x => x.MaSach == id);
            if (item != null)
            {
                item.soLuong += soLuong;
            }
            else
            {
                Sach_GioSach sp = new Sach_GioSach(id, soLuong);
                if (sp == null)
                    return -1;
                List_Sach.Add(sp);

            }

            return 1;
        }

        public int XoaSach(string id)
        {
            Sach_GioSach item = new Sach_GioSach();
            item = List_Sach.FirstOrDefault(x => x.MaSach == id);
            if (item != null)//nếu sản phẩm đã có trong giỏ
            {
                List_Sach.Remove(item);
                return 1;//Xóa thành công!
            }
            else
            {
                return -1;//Xóa không thành công!
            }

        }

        public int CapNhatSL(string id, int thaotac)
        {
            Sach_GioSach item = new Sach_GioSach();
            item = List_Sach.FirstOrDefault(x => x.MaSach == id);
            if (item == null)
                return -1; //thực hiện thao tác thất bại!
            else
            {
                if (thaotac == 1)
                {
                    if (item.soLuong < 3)
                        item.soLuong++;
                }
                else if (thaotac == 2)
                {
                    if (item.soLuong == 1)
                        List_Sach.Remove(item);
                    else
                        item.soLuong--;

                }
                return 1;
            }
        }
    }
}