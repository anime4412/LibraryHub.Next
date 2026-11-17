using System.ComponentModel.DataAnnotations;
using LibraryHub.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace LibraryHub.Web;

public static class VietnameseUi
{
    public static string IdentityError(IdentityError error) => error.Code switch
    {
        "DuplicateEmail" or "DuplicateUserName" => "Email này đã được sử dụng.",
        "InvalidEmail" or "InvalidUserName" => "Địa chỉ email không hợp lệ.",
        "PasswordTooShort" => "Mật khẩu phải có ít nhất 8 ký tự.",
        "PasswordRequiresDigit" => "Mật khẩu phải có ít nhất một chữ số.",
        "PasswordRequiresLetter" => "Mật khẩu phải có ít nhất một chữ cái.",
        "PasswordMismatch" => "Mật khẩu hiện tại không đúng.",
        "InvalidToken" => "Liên kết không hợp lệ hoặc đã hết hạn.",
        "LoginAlreadyAssociated" => "Tài khoản đăng nhập này đã được liên kết.",
        "UserAlreadyHasPassword" => "Tài khoản đã có mật khẩu.",
        "UserLockoutNotEnabled" => "Tài khoản không hỗ trợ khóa đăng nhập.",
        "RecoveryCodeRedemptionFailed" => "Mã khôi phục không hợp lệ.",
        _ => "Không thể hoàn tất yêu cầu tài khoản. Vui lòng kiểm tra thông tin và thử lại."
    };
}

public sealed class VietnamesePasswordValidator : IPasswordValidator<LibraryUser>
{
    public Task<IdentityResult> ValidateAsync(
        UserManager<LibraryUser> manager,
        LibraryUser user,
        string? password)
    {
        if (!string.IsNullOrEmpty(password) && password.Any(char.IsLetter))
            return Task.FromResult(IdentityResult.Success);

        return Task.FromResult(IdentityResult.Failed(new IdentityError
        {
            Code = "PasswordRequiresLetter",
            Description = "Mật khẩu phải có ít nhất một chữ cái."
        }));
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
public sealed class LibraryPasswordAttribute : ValidationAttribute
{
    public LibraryPasswordAttribute() =>
        ErrorMessage = "Mật khẩu phải có ít nhất 8 ký tự, gồm tối thiểu một chữ cái và một chữ số.";

    public override bool IsValid(object? value)
    {
        if (value is not string password) return true;
        return password.Length >= 8 && password.Any(char.IsLetter) && password.Any(char.IsDigit);
    }
}
