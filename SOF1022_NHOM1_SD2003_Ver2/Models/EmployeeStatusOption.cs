using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class EmployeeStatusOption
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên trạng thái không được để trống")]
        [StringLength(50, ErrorMessage = "Tên trạng thái tối đa 50 ký tự")]
        [Display(Name = "Tên trạng thái")]
        public string? Name { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(200, ErrorMessage = "Mô tả tối đa 200 ký tự")]
        public string? Description { get; set; }

        [Display(Name = "Thứ tự hiển thị")]
        public int DisplayOrder { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Employee>? Employees { get; set; }
    }
}
