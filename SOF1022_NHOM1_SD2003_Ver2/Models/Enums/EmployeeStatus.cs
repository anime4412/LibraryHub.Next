using System.ComponentModel.DataAnnotations;

namespace SOF1022_NHOM1_SD2003_Ver2.Models.Enums
{
    public enum EmployeeStatus
    {
        [Display(Name = "Đang làm việc")]
        Working = 1,

        [Display(Name = "Tạm ngưng")]
        Suspended = 2,

        [Display(Name = "Đã nghỉ việc")]
        Resigned = 3
    }
}