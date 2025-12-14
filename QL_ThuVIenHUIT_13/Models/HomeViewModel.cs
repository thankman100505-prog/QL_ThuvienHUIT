using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QL_ThuVIenHUIT_13.Models;
namespace QL_ThuVIenHUIT_13.Models
{
    public class HomeViewModel
    {
        public List<TINTUC1> TinTucSuKien { get; set; }
        public List<TINTUC1> ThongBao { get; set; }
    }
}