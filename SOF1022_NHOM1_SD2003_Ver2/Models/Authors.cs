using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Authors : BaseEntity
    {

        [Required(ErrorMessage = "Tên tác giả không được để trống")]
        [Display(Name = "Tên tác giả")]
        [StringLength(150)]
        public string? Name { get; set; }

        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Quốc tịch")]
        [StringLength(100)]
        public string? Nationality { get; set; }

        [Display(Name = "Tiểu sử")]
        [StringLength(1000)]
        [DataType(DataType.MultilineText)]
        public string? Biography { get; set; }

        public ICollection<Books>? Books { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; } = false;
    }
}
