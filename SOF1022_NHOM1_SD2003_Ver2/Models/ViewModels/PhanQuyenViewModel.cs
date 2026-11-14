using System.Collections.Generic;
using SOF1022_NHOM1_SD2003_Ver2.Models;

namespace SOF1022_NHOM1_SD2003_Ver2.Models.ViewModels
{
    public class PhanQuyenViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public string EmployeeEmail { get; set; }
        public int? RoleId { get; set; }
        public List<Role> Roles { get; set; } = new List<Role>();
        public Dictionary<string, List<Permission>> PermissionGroups { get; set; } = new Dictionary<string, List<Permission>>();
        public HashSet<int> UserPermissionIds { get; set; } = new HashSet<int>();
        public List<int> SelectedPermissionIds { get; set; } = new List<int>();
        public bool HasReaderAccount { get; set; }
        public string? ReaderAccountEmail { get; set; }
    }
}
