using System;
using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Validation
{
    public class ValidHireDateAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value == null)
            {
                return new ValidationResult("Ngày vào làm không được để trống");
            }

            if (!(value is DateTime ngayVaoLam))
            {
                return new ValidationResult("Ngày vào làm không hợp lệ");
            }

            // Không được lớn hơn hôm nay
            if (ngayVaoLam.Date > DateTime.Today)
            {
                return new ValidationResult("Ngày vào làm không được lớn hơn ngày hiện tại");
            }

            // Không được quá cũ (tùy bạn chỉnh)
            if (ngayVaoLam.Year < 2000)
            {
                return new ValidationResult("Ngày vào làm không hợp lệ");
            }

            return ValidationResult.Success;
        }
    }
}