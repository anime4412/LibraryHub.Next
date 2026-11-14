using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Position
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên chức vụ không được để trống")]
        [StringLength(50, ErrorMessage = "Tên chức vụ tối đa 50 ký tự")]
        [Display(Name = "Tên chức vụ")]
        public string? Name { get; set; }

        [Display(Name = "Mô tả")]
        [StringLength(200, ErrorMessage = "Mô tả tối đa 200 ký tự")]
        public string? Description { get; set; }

        // Functional system permissions
        [Display(Name = "Quyền quản lý sách")]
        public bool CanManageBooks { get; set; } = false;

        [Display(Name = "Quyền quản lý người dùng")]
        public bool CanManageUsers { get; set; } = false;

        [Display(Name = "Quyền quản lý mượn trả")]
        public bool CanManageLoans { get; set; } = false;

        [Display(Name = "Quyền quản lý báo cáo")]
        public bool CanManageReports { get; set; } = false;

        [Display(Name = "Quyền quản lý hệ thống")]
        public bool CanManageSystem { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ICollection<Employee>? Employees { get; set; }
    }
}
