using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Categories : BaseEntity
    {

        [Required(ErrorMessage = "Tên thể loại không được để trống")]
        [StringLength(100, ErrorMessage = "Tên thể loại tối đa 100 ký tự")]
        [Display(Name = "Tên thể loại")]
        public string? Name { get; set; }

        [StringLength(500)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        public ICollection<Books>? Books { get; set; }
    }
}
