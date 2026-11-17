using System.Security.Claims;
using System.Text.Json;
using LibraryHub.Domain;
using LibraryHub.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace LibraryHub.Web;

public sealed class BorrowCartFlowService(
    LibraryDbContext db,
    UserManager<LibraryUser> users,
    SignInManager<LibraryUser> signIn)
{
    private const string PendingActionKey = "PendingBorrowCartAction";
    private static readonly TimeSpan PendingLifetime = TimeSpan.FromHours(2);

    public void SavePendingAction(
        ITempDataDictionary tempData,
        int bookId,
        string returnUrl)
    {
        var pending = new PendingBorrowCartAction(
            bookId,
            returnUrl,
            DateTimeOffset.UtcNow.Add(PendingLifetime));
        tempData[PendingActionKey] = JsonSerializer.Serialize(pending);
    }

    public bool HasPendingAction(ITempDataDictionary tempData)
    {
        var pending = ReadPendingAction(tempData.Peek(PendingActionKey) as string);
        if (pending is not null) return true;
        tempData.Remove(PendingActionKey);
        return false;
    }

    public PendingBorrowCartAction? TakePendingAction(ITempDataDictionary tempData) =>
        ReadPendingAction(tempData[PendingActionKey] as string);

    public async Task<BorrowCartItem> ValidatePublicBookAsync(
        int bookId,
        CancellationToken cancellationToken)
    {
        var book = await db.Books.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == bookId && !x.IsDeleted, cancellationToken)
            ?? throw new DomainRuleException("book.not_found", "Không tìm thấy sách.");
        if (book.Status is not BookStatus.Available || book.AvailableQuantity <= 0)
            throw new DomainRuleException(
                "book.stock.insufficient",
                $"Sách “{book.Title}” hiện không còn bản khả dụng.");
        return new BorrowCartItem(book.Id, book.Title, book.AvailableQuantity);
    }

    public async Task<BorrowCartItem> ValidateForReaderAsync(
        ClaimsPrincipal principal,
        int bookId,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId))
            throw new DomainRuleException("account.invalid", "Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.");

        var account = await users.FindByIdAsync(accountId.ToString());
        if (account is null || !account.IsActive)
            throw new DomainRuleException("account.inactive", "Tài khoản đã ngừng hoạt động.");
        if (account.AccountKind is not AccountKind.Reader
            || principal.IsInRole("Staff")
            || principal.IsInRole("Administrator"))
            throw new DomainRuleException(
                "reader.required",
                "Chỉ tài khoản độc giả mới có thể thêm sách vào giỏ mượn.");
        if (await users.IsLockedOutAsync(account))
            throw new DomainRuleException("account.locked", "Tài khoản đang tạm khóa. Vui lòng thử lại sau.");
        if (!await signIn.CanSignInAsync(account))
            throw new DomainRuleException(
                "account.not_allowed",
                "Tài khoản hiện chưa đủ điều kiện đăng nhập và mượn sách.");

        var hasReaderProfile = await db.Readers.AsNoTracking()
            .AnyAsync(x => x.UserAccountId == accountId && !x.IsDeleted, cancellationToken);
        if (!hasReaderProfile)
            throw new DomainRuleException(
                "reader.not_found",
                "Tài khoản chưa được liên kết với hồ sơ độc giả.");

        return await ValidatePublicBookAsync(bookId, cancellationToken);
    }

    private static PendingBorrowCartAction? ReadPendingAction(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        try
        {
            var pending = JsonSerializer.Deserialize<PendingBorrowCartAction>(value);
            return pending is { BookId: > 0 }
                && pending.ExpiresAt > DateTimeOffset.UtcNow
                && !string.IsNullOrWhiteSpace(pending.ReturnUrl)
                    ? pending
                    : null;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}

public sealed record PendingBorrowCartAction(
    int BookId,
    string ReturnUrl,
    DateTimeOffset ExpiresAt);

public sealed record BorrowCartItem(
    int BookId,
    string Title,
    int AvailableQuantity);
