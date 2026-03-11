using PatientCrm.Maui.Models;

namespace PatientCrm.Maui.Services;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(string email, string password);
    Task<TwoFactorResult?> VerifyTwoFactorAsync(string partialToken, string code);
    Task LogoutAsync();
    Task<string?> GetTokenAsync();
    Task<UserInfo?> GetCurrentUserAsync();
    bool IsPatient { get; }
    bool IsAdmin { get; }
    bool IsClinician { get; }
}
