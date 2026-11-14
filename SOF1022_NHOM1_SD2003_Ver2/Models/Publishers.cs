using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Publishers : BaseEntity
    {

        [Required(ErrorMessage = "Tên nhà xuất bản không được để trống")]
        [Display(Name = "Tên nhà xuất bản")]
        [StringLength(150)]
        public string? Name { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255)]
        public string? Address { get; set; }

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? Phone { get; set; }

        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Display(Name = "Website")]
        [Url(ErrorMessage = "Website không hợp lệ")]
        public string? Website { get; set; }

        public ICollection<Books>? Books { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; } = false;
    }
}
