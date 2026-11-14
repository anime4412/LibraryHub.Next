using System;
using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Validation
{
    public class ValidBirthDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Ngày sinh không được để trống");
            }

            if (!(value is DateTime ngaySinh))
            {
                return new ValidationResult("Ngày sinh không hợp lệ");
            }

            // Không được lớn hơn ngày hiện tại
            if (ngaySinh.Date > DateTime.Today)
            {
                return new ValidationResult("Ngày sinh không được lớn hơn ngày hiện tại");
            }

            // Tuổi tối thiểu 18
            int age = DateTime.Today.Year - ngaySinh.Year;
            if (ngaySinh.Date > DateTime.Today.AddYears(-age))
            {
                age--;
            }

            if (age < 18)
            {
                return new ValidationResult("Nhân viên phải đủ 18 tuổi tại thời điểm vào làm.");
            }

            return ValidationResult.Success;
        }
    }
}