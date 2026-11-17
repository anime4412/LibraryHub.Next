using System.Text;
using System.Text.RegularExpressions;
using LibraryHub.Domain;

namespace LibraryHub.Application;

public sealed class FineCalculator : IFineCalculator
{
    public FineQuote Calculate(FineInput input)
    {
        if (input.Quantity <= 0) throw new ArgumentOutOfRangeException(nameof(input.Quantity));
        if (input.BookPrice < 0 || input.LateFeePerDay < 0) throw new ArgumentOutOfRangeException(nameof(input));
        if (input.DamagePercent is < 0 or > 100) throw new ArgumentOutOfRangeException(nameof(input.DamagePercent));
        var lateDays = input.ReturnedAt > input.DueAt
            ? (int)Math.Ceiling((input.ReturnedAt - input.DueAt).TotalDays)
            : 0;
        var late = lateDays * input.LateFeePerDay * input.Quantity;
        var damage = input.Condition == ItemCondition.Damaged
            ? decimal.Round(input.BookPrice * input.DamagePercent / 100m * input.Quantity, 0, MidpointRounding.AwayFromZero)
            : 0m;
        var lost = input.Condition == ItemCondition.Lost ? input.BookPrice * input.Quantity : 0m;
        FineType? type = (lateDays > 0, input.Condition) switch
        {
            (true, ItemCondition.Damaged) => FineType.LateAndDamaged,
            (true, ItemCondition.Lost) => FineType.LateAndLost,
            (true, _) => FineType.Late,
            (false, ItemCondition.Damaged) => FineType.Damaged,
            (false, ItemCondition.Lost) => FineType.Lost,
            _ => null
        };
        return new FineQuote(type, lateDays, late, damage, lost);
    }
}

public static partial class VietnameseValidation
{
    [GeneratedRegex(@"^(?:\+84|0)(?:3|5|7|8|9)\d{8}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhonePattern();
    [GeneratedRegex(@"^\d{9}(?:\d{3})?$", RegexOptions.CultureInvariant)]
    private static partial Regex IdentityPattern();

    public static string Normalize(string value) => value.Trim().Normalize(NormalizationForm.FormC);
    public static bool IsPhone(string? value) =>
        !string.IsNullOrWhiteSpace(value) && PhonePattern().IsMatch(value.Replace(" ", "", StringComparison.Ordinal));
    public static bool IsIdentityNumber(string? value) =>
        !string.IsNullOrWhiteSpace(value) && IdentityPattern().IsMatch(value);
    public static bool IsFullName(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        var normalized = Normalize(value);
        return normalized.Length is >= 2 and <= 120
            && normalized.All(c => char.IsLetter(c) || char.IsWhiteSpace(c) || c is '-' or '\'')
            && normalized.Any(char.IsLetter);
    }
}

public static class EmployeePolicy
{
    public static void Validate(Employee employee, Position position, DateOnly today)
    {
        if (!VietnameseValidation.IsFullName(employee.FullName))
            throw new DomainRuleException("employee.name.invalid", "Họ tên không hợp lệ.");
        if (!VietnameseValidation.IsPhone(employee.Phone))
            throw new DomainRuleException("employee.phone.invalid", "Số điện thoại Việt Nam không hợp lệ.");
        if (!VietnameseValidation.IsIdentityNumber(employee.NationalId))
            throw new DomainRuleException("employee.identity.invalid", "CCCD/CMND phải có 9 hoặc 12 chữ số.");
        if (employee.HiredOn < employee.DateOfBirth)
            throw new DomainRuleException("employee.hire.before_birth", "Ngày vào làm không thể trước ngày sinh.");
        if (employee.HiredOn.Year < 2000 || employee.HiredOn > today)
            throw new DomainRuleException("employee.hire.invalid", "Ngày vào làm phải từ năm 2000 đến hiện tại.");
        var age = employee.HiredOn.Year - employee.DateOfBirth.Year;
        if (employee.HiredOn < employee.DateOfBirth.AddYears(age)) age--;
        if (age < 18) throw new DomainRuleException("employee.underage", "Nhân viên phải đủ 18 tuổi khi vào làm.");
        if (!position.AllowSalaryOverride && employee.Salary != position.SalaryAmount)
            throw new DomainRuleException("employee.salary.mismatch", "Lương phải khớp cấu hình của chức vụ.");
    }
}

public static class ReservationPolicy
{
    public static IReadOnlyList<ReservationStatus> AllowedTransitions(ReservationStatus current) =>
        current switch
        {
            ReservationStatus.Waiting =>
                [ReservationStatus.Approved, ReservationStatus.Rejected, ReservationStatus.Cancelled],
            ReservationStatus.Approved =>
                [ReservationStatus.ReadyToBorrow, ReservationStatus.Cancelled, ReservationStatus.Expired],
            ReservationStatus.ReadyToBorrow =>
                [ReservationStatus.Cancelled, ReservationStatus.Expired],
            _ => []
        };

    public static bool CanTransition(ReservationStatus current, ReservationStatus next) =>
        current != next && AllowedTransitions(current).Contains(next);
}

public sealed class PermissionService(ILibraryStore store) : IPermissionService
{
    public async Task<bool> HasAsync(Guid accountId, string permissionCode, CancellationToken cancellationToken)
    {
        var employeeId = await store.FindEmployeeIdByAccountAsync(accountId, cancellationToken);
        if (!employeeId.HasValue || string.IsNullOrWhiteSpace(permissionCode)) return false;
        var user = await store.FindUserPermissionAsync(employeeId.Value, permissionCode, cancellationToken);
        if (user.HasValue) return user == PermissionEffect.Allow;
        var role = await store.FindRolePermissionsAsync(employeeId.Value, permissionCode, cancellationToken);
        return !role.Contains(PermissionEffect.Deny) && role.Contains(PermissionEffect.Allow);
    }
}
