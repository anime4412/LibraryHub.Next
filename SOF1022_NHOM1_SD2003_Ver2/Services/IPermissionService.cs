using System.Collections.Generic;
using System.Threading.Tasks;

namespace SOF1022_NHOM1_SD2003_Ver2.Services
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(int employeeId, string permissionCode);
        Task<List<string>> GetPermissionCodesAsync(int employeeId);
    }
}
