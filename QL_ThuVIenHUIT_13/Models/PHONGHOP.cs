using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QL_ThuVIenHUIT_13.Models
{
    [Table("PHONGHOP")]
    public partial class PHONGHOP
    {
        [Key]
        [StringLength(10)]
        public string MAPHONG { get; set; }

        [StringLength(100)]
        public string TENPHONG { get; set; }

        public int? SL_NGUOITOIDA { get; set; }

        [StringLength(100)]
        public string VITRI { get; set; }

        public int? TINHTRANG { get; set; }

        [StringLength(255)]
        public string MOTA { get; set; }

        public virtual ICollection<PHIEU_MUONPHONG> PHIEU_MUONPHONG { get; set; }
    }
}
