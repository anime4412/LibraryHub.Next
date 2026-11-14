using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SOF1022_NHOM1_SD2003_Ver2.Models
{
    public class Permission
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } // "book.view", "book.create", "book.edit", "book.delete", "borrow.create", "borrow.confirm_return", "fine.view", "fine.create", "user.manage_employee", "user.manage_reader"

        [Required]
        [StringLength(100)]
        public string DisplayName { get; set; } // "Xem sách", "Thêm sách"... (tiếng Việt)

        [Required]
        [StringLength(50)]
        public string GroupName { get; set; } // "Quản lý sách", "Mượn trả", "Phạt", "Người dùng"

        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
        public virtual ICollection<UserPermission> UserPermissions { get; set; } = new List<UserPermission>();
    }
}
