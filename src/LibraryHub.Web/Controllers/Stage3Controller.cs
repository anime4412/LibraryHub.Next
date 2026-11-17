using System.Security.Claims;
using LibraryHub.Application;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using LibraryHub.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web.Controllers;

public sealed class Stage3Controller(LibraryDbContext db, IPermissionService permissions) : Controller
{
    [Authorize]
    [HttpGet("/notifications")]
    public async Task<IActionResult> Notifications()
    {
        var accountId = AccountId();
        var items = await db.Notifications.AsNoTracking()
            .Where(x => x.RecipientAccountId == accountId)
            .OrderByDescending(x => x.CreatedAt)
            .Take(200)
            .ToListAsync(HttpContext.RequestAborted);
        return View(new NotificationCenterViewModel(items, items.Count(x => !x.IsRead)));
    }

    [Authorize]
    [HttpGet("/support")]
    public async Task<IActionResult> Support(int? ticketId)
    {
        var readerId = await ReaderIdAsync();
        if (!readerId.HasValue) return Forbid();
        var (rows, total) = await LoadTicketsAsync(readerId.Value, null, null, 1, 100);
        var selected = await LoadSelectedTicketMessagesAsync(
            ticketId.HasValue ? rows.SingleOrDefault(x => x.Ticket.Id == ticketId.Value) : rows.FirstOrDefault());
        if (selected is not null) await MarkTicketReadAsync(selected.Ticket, false);
        return View(new SupportCenterViewModel(rows, selected, false, null, null, 1, 100, total));
    }

    [Authorize(Policy = "InternalOnly")]
    [HttpGet("/admin/readers")]
    public async Task<IActionResult> Readers(string? q, ReaderType? type, bool? verified, int page = 1)
    {
        if (!await HasAny(PermissionCodes.ReaderView, PermissionCodes.ReaderEdit)) return Forbid();
        page = Math.Max(1, page);
        const int pageSize = 20;
        var query = db.Readers.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.FullName.Contains(q) || x.Email.Contains(q) || x.ReaderCode.Contains(q));
        if (type.HasValue) query = query.Where(x => x.Type == type);
        if (verified.HasValue) query = query.Where(x => x.IsVerified == verified);
        var total = await query.CountAsync(HttpContext.RequestAborted);
        var items = await query.OrderBy(x => x.FullName).Skip((page - 1) * pageSize).Take(pageSize)
            .ToListAsync(HttpContext.RequestAborted);
        return View(new AdminReadersViewModel(new(items, page, pageSize, total), q, type, verified));
    }

    [Authorize(Policy = "InternalOnly")]
    [HttpGet("/admin/employees")]
    public async Task<IActionResult> Employees(string? q, EmploymentStatus? status, int page = 1)
    {
        if (!await HasAny(PermissionCodes.EmployeeView, PermissionCodes.EmployeeCreate, PermissionCodes.EmployeeEdit))
            return Forbid();
        page = Math.Max(1, page);
        const int pageSize = 20;
        var query = db.Employees.AsNoTracking().Where(x => !x.IsDeleted);
        if (!string.IsNullOrWhiteSpace(q))
            query = query.Where(x => x.FullName.Contains(q) || x.Email.Contains(q) || x.EmployeeCode.Contains(q));
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var total = await query.CountAsync(HttpContext.RequestAborted);
        var employees = await (
            from employee in query
            join position in db.Positions.AsNoTracking() on employee.PositionId equals position.Id into positions
            from position in positions.DefaultIfEmpty()
            join role in db.SecurityRoles.AsNoTracking() on employee.RoleId equals role.Id into roles
            from role in roles.DefaultIfEmpty()
            orderby employee.FullName
            select new AdminEmployeeRow(employee, position, role))
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(HttpContext.RequestAborted);
        return View(new AdminEmployeesViewModel(
            new(employees, page, pageSize, total),
            await db.Positions.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            await db.EmployeeStatusOptions.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.DisplayOrder).ToListAsync(),
            await db.SecurityRoles.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name).ToListAsync(),
            q, status));
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpGet("/admin/rbac")]
    public async Task<IActionResult> Rbac()
    {
        var permissionItems = await db.Permissions.AsNoTracking().OrderBy(x => x.GroupName).ThenBy(x => x.Code)
            .ToListAsync(HttpContext.RequestAborted);
        var roleItems = await db.SecurityRoles.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.Name)
            .ToListAsync(HttpContext.RequestAborted);
        var roleAssignments = await db.RolePermissions.AsNoTracking()
            .Where(x => x.Effect == PermissionEffect.Allow).ToListAsync(HttpContext.RequestAborted);
        var employees = await db.Employees.AsNoTracking().Where(x => !x.IsDeleted).OrderBy(x => x.FullName)
            .ToListAsync(HttpContext.RequestAborted);
        var overrides = await db.UserPermissions.AsNoTracking().ToListAsync(HttpContext.RequestAborted);
        return View(new AdminRbacViewModel(
            permissionItems,
            roleItems.Select(role => new RolePermissionRow(
                role,
                roleAssignments.Where(x => x.RoleId == role.Id).Select(x => x.PermissionId).ToHashSet())).ToList(),
            employees.Select(employee => new EmployeePermissionRow(
                employee,
                overrides.Where(x => x.EmployeeId == employee.Id)
                    .ToDictionary(x => x.PermissionId, x => x.Effect))).ToList()));
    }

    [Authorize(Policy = "InternalOnly")]
    [HttpGet("/admin/support")]
    public async Task<IActionResult> AdminSupport(string? q, SupportTicketStatus? status, int? ticketId, int page = 1)
    {
        if (!await HasAny(PermissionCodes.SupportClaim)) return Forbid();
        page = Math.Max(1, page);
        const int pageSize = 20;
        var (rows, total) = await LoadTicketsAsync(null, q, status, page, pageSize);
        var selected = await LoadSelectedTicketMessagesAsync(
            ticketId.HasValue ? rows.SingleOrDefault(x => x.Ticket.Id == ticketId.Value) : rows.FirstOrDefault());
        if (selected is not null) await MarkTicketReadAsync(selected.Ticket, true);
        return View("Support", new SupportCenterViewModel(rows, selected, true, q, status, page, pageSize, total));
    }

    [Authorize(Policy = "AdministratorOnly")]
    [HttpGet("/admin/settings")]
    public async Task<IActionResult> Settings() =>
        View(await db.SystemSettings.AsNoTracking().SingleOrDefaultAsync(HttpContext.RequestAborted)
             ?? new SystemSettings());

    private async Task<(List<SupportTicketRow> Rows, int Total)> LoadTicketsAsync(
        int? readerId,
        string? queryText,
        SupportTicketStatus? status,
        int page,
        int pageSize)
    {
        var query = from t in db.SupportTickets.AsNoTracking()
                    join r in db.Readers.AsNoTracking() on t.ReaderId equals r.Id
                    select new { Ticket = t, Reader = r };
        if (readerId.HasValue) query = query.Where(x => x.Ticket.ReaderId == readerId.Value);
        if (status.HasValue) query = query.Where(x => x.Ticket.Status == status.Value);
        if (!string.IsNullOrWhiteSpace(queryText))
        {
            var term = queryText.Trim();
            query = query.Where(x => x.Ticket.Code.Contains(term)
                || x.Ticket.Subject.Contains(term)
                || x.Reader.FullName.Contains(term));
        }

        var total = await query.CountAsync(HttpContext.RequestAborted);
        var items = await query
            .OrderByDescending(x => x.Ticket.LastMessageAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(HttpContext.RequestAborted);

        var rows = items.Select(x => new SupportTicketRow(x.Ticket, x.Reader)).ToList();
        return (rows, total);
    }

    private async Task<SupportTicketRow?> LoadSelectedTicketMessagesAsync(SupportTicketRow? selected)
    {
        if (selected is null) return null;
        selected.Ticket.Messages = await db.SupportMessages.AsNoTracking()
            .Where(x => x.SupportTicketId == selected.Ticket.Id)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(HttpContext.RequestAborted);
        return selected;
    }

    private async Task MarkTicketReadAsync(SupportTicket ticket, bool staff)
    {
        if (staff && ticket.UnreadByStaff)
            await db.SupportTickets.Where(x => x.Id == ticket.Id)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.UnreadByStaff, false));
        if (!staff && ticket.UnreadByReader)
            await db.SupportTickets.Where(x => x.Id == ticket.Id)
                .ExecuteUpdateAsync(x => x.SetProperty(y => y.UnreadByReader, false));
    }

    private Guid AccountId() =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id : throw new UnauthorizedAccessException();

    private async Task<int?> ReaderIdAsync() =>
        await db.Readers.AsNoTracking()
            .Where(x => x.UserAccountId == AccountId() && !x.IsDeleted)
            .Select(x => (int?)x.Id).SingleOrDefaultAsync(HttpContext.RequestAborted);

    private async Task<bool> HasAny(params string[] codes)
    {
        if (User.IsInRole("Administrator")) return true;
        var accountId = AccountId();
        foreach (var code in codes)
            if (await permissions.HasAsync(accountId, code, HttpContext.RequestAborted)) return true;
        return false;
    }
}
