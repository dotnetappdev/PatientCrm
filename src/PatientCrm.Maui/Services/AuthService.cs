using System.Net.Http.Json;
using System.Text.Json;
using PatientCrm.Maui.Models;

namespace PatientCrm.Maui.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private const string TokenKey = "jwt_token";
    private const string UserKey = "current_user";
    private UserInfo? _currentUser;

    public AuthService(IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient("PatientCrmApi");
    }

    public bool IsPatient => _currentUser?.Roles.Contains("Patient") == true;
    public bool IsAdmin => _currentUser?.Roles.Any(r => r is "SuperAdmin" or "TenantAdmin") == true;
    public bool IsClinician => _currentUser?.Roles.Any(r => r is "GP" or "Dentist" or "Consultant" or "Nurse" or "Receptionist" or "Specialist") == true;

    public async Task<LoginResult?> LoginAsync(string email, string password)
    {
        var response = await _http.PostAsJsonAsync("api/auth/login", new { email, password });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        if (result?.Token != null)
        {
            await SecureStorage.SetAsync(TokenKey, result.Token);
            if (result.User != null)
            {
                _currentUser = result.User;
                await SecureStorage.SetAsync(UserKey, JsonSerializer.Serialize(result.User));
            }
        }
        return result;
    }

    public async Task<TwoFactorResult?> VerifyTwoFactorAsync(string partialToken, string code)
    {
        var response = await _http.PostAsJsonAsync("api/auth/verify-two-factor", new { partialToken, code });
        if (!response.IsSuccessStatusCode) return null;
        var result = await response.Content.ReadFromJsonAsync<TwoFactorResult>();
        if (result?.Token != null)
        {
            await SecureStorage.SetAsync(TokenKey, result.Token);
            if (result.User != null)
            {
                _currentUser = result.User;
                await SecureStorage.SetAsync(UserKey, JsonSerializer.Serialize(result.User));
            }
        }
        return result;
    }

    public Task LogoutAsync()
    {
        SecureStorage.Remove(TokenKey);
        SecureStorage.Remove(UserKey);
        _currentUser = null;
        return Task.CompletedTask;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await SecureStorage.GetAsync(TokenKey);
    }

    public async Task<UserInfo?> GetCurrentUserAsync()
    {
        if (_currentUser != null) return _currentUser;
        var json = await SecureStorage.GetAsync(UserKey);
        if (json != null)
            _currentUser = JsonSerializer.Deserialize<UserInfo>(json);
        return _currentUser;
    }
}
