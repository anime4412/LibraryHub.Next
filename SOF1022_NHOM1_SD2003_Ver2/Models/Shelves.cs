using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Shelves : BaseEntity
    {

        [Required(ErrorMessage = "Mã kệ không được để trống")]
        [Display(Name = "Mã kệ")]
        [StringLength(20)]
        public string? Code { get; set; }

        [StringLength(20)]
        public string? LocationCode { get; set; }

        [Required(ErrorMessage = "Tên kệ không được để trống")]
        [Display(Name = "Tên kệ")]
        [StringLength(100)]
        public string? Name { get; set; }

        [Display(Name = "Khu vực")]
        [StringLength(100)]
        public string? Area { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(300)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Sức chứa không được để trống")]
        [Display(Name = "Sức chứa tối đa")]
        [Range(1, 5000, ErrorMessage = "Sức chứa phải từ 1 đến 5,000 cuốn")]
        public int Capacity { get; set; } = 300;

        public ICollection<Books>? Books { get; set; }
    }
}
