using System.Web;
using System.Web.Mvc;

namespace QL_ThuVIenHUIT_13
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
