using Microsoft.AspNetCore.Identity;
using SOF1022_NHOM1_SD2003_Ver2.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Readers
    {
        public int Id { get; set; }

        [StringLength(20)]
        public string? ReaderCode { get; set; }

        [Required]
        [StringLength(150)]
        [RegularExpression(@"^[a-zA-ZÀ-Ỹà-ỹ\s]+$", ErrorMessage = "Họ tên không được chứa ký tự số hoặc ký tự đặc biệt")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập ngày sinh")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^(03|05|07|08|09)\d{8}$", ErrorMessage = "Số điện thoại phải bắt đầu bằng 03, 05, 07, 08, 09 và có đúng 10 chữ số")]
        public string? Phone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [StringLength(500)]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập CCCD/CMND")]
        [RegularExpression(@"^\d{9}$|^\d{12}$", ErrorMessage = "CCCD/CMND phải có đúng 9 hoặc 12 chữ số")]
        [StringLength(20)]
        public string? IdentityNumber { get; set; }

        public DateTime RegisterDate { get; set; } = DateTime.Now;

        public DateTime ExpirationDate { get; set; }

        public DateTime? LastLoginDate { get; set; }

        public int ConsecutiveLoginDays { get; set; } = 1;

        public DateTime? LastStreakDate { get; set; }

        [DataType(DataType.ImageUrl)]
        public string? Avatar { get; set; }

        // Profile customization fields
        public string? CoverImageUrl { get; set; } // URL for custom cover background
        public string? Bio { get; set; } // Short custom biography/quote
        public string? ActiveBadge { get; set; } // Current visible reader title/badge
        public string? FullPageBackgroundUrl { get; set; } // URL for full-page background (static/animated)
        public string? FeaturedBookIds { get; set; } // Comma-separated list of featured book IDs (max 4)
        public bool IsCommentBlocked { get; set; } = false; // Block user from commenting due to reports
        public DateTime? CommentBlockedUntil { get; set; } // Optional: temporary block expiration

        public int MaxBorrowBooks { get; set; } = 5;

        // Thêm mới
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(256,
        MinimumLength = 6,
        ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [NotMapped]
        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare("Password",
        ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [DataType(DataType.Password)]
        public string? ConfirmPassword { get; set; }

        public string Role { get; set; } = "Reader";


        public bool IsActive { get; set; } = true;

        public bool EmailConfirmed { get; set; } = false;

        public string? EmailConfirmationToken { get; set; }

        public DateTime? EmailConfirmationSentDate { get; set; }

        public bool ForcePasswordChange { get; set; } = false;

        public ReadersType ReadersType { get; set; } = ReadersType.Student;

        public Genders Genders { get; set; } = Genders.Other;

        public ICollection<BorrowSlips>? BorrowSlips { get; set; }

        public ICollection<Reservations>? Reservations { get; set; }

        // Soft-delete flag
        public bool IsDeleted { get; set; } = false;

        // Onboarding tracking
        public bool IsNameSet { get; set; } = false;

        // Verification tracking - user has provided valid phone and address
        public bool IsVerified { get; set; } = false;
    }
}
