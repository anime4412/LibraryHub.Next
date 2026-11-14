using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SOF1022_NHOM1_SD2003_Ver2.Validation;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Employee : IValidatableObject
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống")]
        [StringLength(20, ErrorMessage = "Mã nhân viên tối đa 20 ký tự")]
        [Display(Name = "Mã nhân viên")]
        public string? EmployeeId { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống")]
        [StringLength(100, ErrorMessage = "Họ tên tối đa 100 ký tự")]
        [Display(Name = "Họ và tên")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100, ErrorMessage = "Email tối đa 100 ký tự")]
        [Display(Name = "Email")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(200, ErrorMessage = "Địa chỉ tối đa 200 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "CCCD/CMND không được để trống")]
        [RegularExpression(@"^\d{9}$|^\d{12}$", ErrorMessage = "CCCD/CMND phải gồm 9 hoặc 12 số")]
        [Display(Name = "CCCD/CMND")]
        public string? CCCD { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn giới tính")]
        [Display(Name = "Giới tính")]
        public Gender Gender { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày sinh")]
        [ValidBirthDate(ErrorMessage = "Ngày sinh không hợp lệ. Nhân viên phải từ 18 tuổi trở lên và ngày sinh không được lớn hơn ngày hiện tại")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "Ngày vào làm không được để trống")]
        [DataType(DataType.Date)]
        [Display(Name = "Ngày vào làm")]
        [ValidHireDate(ErrorMessage = "Ngày vào làm không hợp lệ. Ngày vào làm phải từ năm 2000 đến hiện tại")]
        public DateTime HireDate { get; set; }

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [Display(Name = "Chức vụ")]
        public int? PositionId { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Lương phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Lương")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Trạng thái không được để trống")]
        [Display(Name = "Trạng thái")]
        public int? StatusId { get; set; }

        // Leave tracking fields
        [Display(Name = "Lý do nghỉ")]
        [StringLength(500, ErrorMessage = "Lý do nghỉ tối đa 500 ký tự")]
        public string? LeaveReason { get; set; }

        [Display(Name = "Ngày bắt đầu nghỉ")]
        public DateTime? LeaveStartDate { get; set; }

        [Display(Name = "Số ngày nghỉ không phép")]
        public int UnexcusedDaysCount { get; set; } = 0;

        [Display(Name = "Ngày nghỉ không phép gần nhất")]
        public DateTime? LastUnexcusedDate { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; } = false;

        // Role for permission system
        public int? RoleId { get; set; }

        // Navigation properties
        public virtual Position? Position { get; set; }
        public virtual EmployeeStatusOption? EmployeeStatusOption { get; set; }
        public virtual Role? Role { get; set; }
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();

        // Computed property for Status enum
        public EmployeeStatus Status
        {
            get
            {
                return StatusId switch
                {
                    1 => EmployeeStatus.Working,
                    2 => EmployeeStatus.Suspended,
                    3 => EmployeeStatus.Resigned,
                    _ => EmployeeStatus.Working
                };
            }
        }   

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (HireDate < DateOfBirth)
            {
                yield return new ValidationResult(
                    "Ngày vào làm không được nhỏ hơn ngày sinh.",
                    new[] { nameof(HireDate) }
                );
            }

            if (HireDate < DateOfBirth.AddYears(18))
            {
                yield return new ValidationResult(
                    "Nhân viên phải đủ 18 tuổi tại thời điểm vào làm.",
                    new[] { nameof(HireDate) }
                );
            }

            // Validate salary matches position
            if (PositionId.HasValue && Position != null)
            {
                decimal expectedSalary = Position.Name switch
                {
                    "Thủ thư" => 5000000,
                    "Nhân viên mượn trả" => 5000000,
                    "Admin" => 15000000,
                    _ => 0
                };

                if (expectedSalary > 0 && Salary != expectedSalary)
                {
                    yield return new ValidationResult(
                        $"Lương phải là {expectedSalary:N0} VNĐ cho chức vụ {Position.Name}.",
                        new[] { nameof(Salary) }
                    );
                }
            }
        }
    }

    public enum Gender
    {
        Male = 1,
        Female = 2,
        Other = 3
    }
}
