using LibraryHub.Domain;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web;

public sealed class DomainRuleExceptionHandler : IExceptionHandler
{
    private static readonly HashSet<string> ConflictCodes = new(StringComparer.OrdinalIgnoreCase)
    {
        "book.unavailable",
        "book.stock.insufficient",
        "book.lifecycle.status",
        "book.suspension.active_borrow",
        "book.maintenance.active_borrow",
        "book.permanent_delete.history",
        "borrow.limit",
        "borrow.status",
        "borrow.pickup.expired",
        "extension.count",
        "extension.overdue",
        "extension.invalid",
        "reservation.active",
        "reservation.status",
        "reservation.duplicate",
        "catalog.in_use",
        "fine.status",
        "fine.payment.exceeds_balance"
    };

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DbUpdateConcurrencyException)
        {
            var concurrencyProblem = new ProblemDetails
            {
                Type = "https://libraryhub.local/problems/concurrency",
                Title = "Dữ liệu đã thay đổi",
                Status = StatusCodes.Status409Conflict,
                Detail = "Dữ liệu đã được cập nhật bởi một thao tác khác. Vui lòng tải lại và thử lại.",
                Instance = httpContext.Request.Path
            };
            concurrencyProblem.Extensions["code"] = "concurrency.conflict";
            concurrencyProblem.Extensions["traceId"] = httpContext.TraceIdentifier;
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            await httpContext.Response.WriteAsJsonAsync(
                concurrencyProblem,
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
            return true;
        }

        if (exception is DbUpdateException dbUpdateEx && IsSqlConflict(dbUpdateEx, out var conflictCode, out var conflictDetail))
        {
            var conflictProblem = new ProblemDetails
            {
                Type = "https://libraryhub.local/problems/db-conflict",
                Title = "Xung đột dữ liệu",
                Status = StatusCodes.Status409Conflict,
                Detail = conflictDetail,
                Instance = httpContext.Request.Path
            };
            conflictProblem.Extensions["code"] = conflictCode;
            conflictProblem.Extensions["traceId"] = httpContext.TraceIdentifier;
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            await httpContext.Response.WriteAsJsonAsync(
                conflictProblem,
                options: null,
                contentType: "application/problem+json",
                cancellationToken);
            return true;
        }

        if (exception is not DomainRuleException domainException) return false;

        var status = ConflictCodes.Contains(domainException.Code)
            ? StatusCodes.Status409Conflict
            : StatusCodes.Status400BadRequest;
        var problem = new ProblemDetails
        {
            Type = "https://libraryhub.local/problems/domain-rule",
            Title = status == StatusCodes.Status409Conflict
                ? "Xung đột quy tắc nghiệp vụ"
                : "Yêu cầu vi phạm quy tắc nghiệp vụ",
            Status = status,
            Detail = domainException.Message,
            Instance = httpContext.Request.Path
        };
        problem.Extensions["code"] = domainException.Code;
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;

        httpContext.Response.StatusCode = status;
        await httpContext.Response.WriteAsJsonAsync(
            problem,
            options: null,
            contentType: "application/problem+json",
            cancellationToken);
        return true;
    }

    private static bool IsSqlConflict(DbUpdateException exception, out string code, out string detail)
    {
        for (Exception? current = exception.InnerException; current is not null; current = current.InnerException)
        {
            if (current is SqlException sqlEx)
            {
                if (sqlEx.Number is 2601 or 2627)
                {
                    code = "db.duplicate_key";
                    detail = "Dữ liệu bị trùng lặp với một bản ghi đã tồn tại.";
                    return true;
                }
                if (sqlEx.Number == 547)
                {
                    code = "db.reference_conflict";
                    detail = "Thao tác vi phạm ràng buộc tham chiếu dữ liệu.";
                    return true;
                }
            }
        }
        code = string.Empty;
        detail = string.Empty;
        return false;
    }
}
