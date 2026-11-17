using LibraryHub.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LibraryHub.Web.Controllers;

[AutoValidateAntiforgeryToken]
public sealed class BorrowCartController(BorrowCartFlowService flow) : Controller
{
    [AllowAnonymous]
    [HttpPost("/borrow-cart/add")]
    public async Task<IActionResult> Add(
        BorrowCartAddInput input,
        CancellationToken cancellationToken)
    {
        var returnUrl = SafeReturnUrl(input.ReturnUrl) ?? $"/books/{input.BookId}";
        await flow.ValidatePublicBookAsync(input.BookId, cancellationToken);

        if (User.Identity?.IsAuthenticated != true)
        {
            flow.SavePendingAction(TempData, input.BookId, returnUrl);
            var continuationUrl = Url.Action(nameof(Continue), "BorrowCart")
                ?? "/borrow-cart/continue";
            var loginUrl = Url.Action("Login", "Auth", new { returnUrl = continuationUrl })
                ?? $"/login?returnUrl={Uri.EscapeDataString(continuationUrl)}";
            return StatusCode(StatusCodes.Status401Unauthorized, new
            {
                requiresAuthentication = true,
                loginUrl,
                message = "Bạn cần đăng nhập hoặc đăng ký trước khi thêm sách vào giỏ mượn."
            });
        }

        return Ok(await flow.ValidateForReaderAsync(User, input.BookId, cancellationToken));
    }

    [Authorize]
    [HttpGet("/borrow-cart/continue")]
    [ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
    public async Task<IActionResult> Continue(CancellationToken cancellationToken)
    {
        var pending = flow.TakePendingAction(TempData);
        if (pending is null)
        {
            TempData["Notice"] = "Hành động thêm vào giỏ mượn không còn hiệu lực.";
            return Redirect("/");
        }

        var returnUrl = SafeReturnUrl(pending.ReturnUrl) ?? $"/books/{pending.BookId}";
        try
        {
            var item = await flow.ValidateForReaderAsync(User, pending.BookId, cancellationToken);
            return View(new BorrowCartContinuationViewModel(
                true,
                item.BookId,
                item.Title,
                item.AvailableQuantity,
                returnUrl,
                $"Đã thêm “{item.Title}” vào giỏ mượn."));
        }
        catch (DomainRuleException exception)
        {
            return View(new BorrowCartContinuationViewModel(
                false,
                pending.BookId,
                null,
                0,
                returnUrl,
                exception.Message));
        }
    }

    private string? SafeReturnUrl(string? returnUrl) =>
        !string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl)
            ? returnUrl
            : null;
}

public sealed class BorrowCartAddInput
{
    public int BookId { get; set; }
    public string? ReturnUrl { get; set; }
}

public sealed record BorrowCartContinuationViewModel(
    bool CanAdd,
    int BookId,
    string? Title,
    int AvailableQuantity,
    string ReturnUrl,
    string Message);
