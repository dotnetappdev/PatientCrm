using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PatientCrm.Maui.Services;

namespace PatientCrm.Maui.ViewModels;

public partial class LoginViewModel : BaseViewModel
{
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _email = string.Empty;

    [ObservableProperty]
    private string _password = string.Empty;

    [ObservableProperty]
    private string _twoFactorCode = string.Empty;

    [ObservableProperty]
    private bool _showTwoFactor;

    [ObservableProperty]
    private string? _partialToken;

    public LoginViewModel(IAuthService authService)
    {
        _authService = authService;
        Title = "Sign In";
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            SetError("Please enter your email and password.");
            return;
        }

        IsBusy = true;
        ClearMessages();

        try
        {
            var result = await _authService.LoginAsync(Email, Password);
            if (result == null)
            {
                SetError("Invalid email or password.");
                return;
            }

            if (result.RequiresTwoFactor)
            {
                PartialToken = result.PartialToken;
                ShowTwoFactor = true;
                return;
            }

            await NavigateAfterLoginAsync();
        }
        catch (Exception ex)
        {
            SetError($"Login failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task VerifyTwoFactorAsync()
    {
        if (string.IsNullOrWhiteSpace(TwoFactorCode) || PartialToken == null)
        {
            SetError("Please enter your verification code.");
            return;
        }

        IsBusy = true;
        ClearMessages();

        try
        {
            var result = await _authService.VerifyTwoFactorAsync(PartialToken, TwoFactorCode);
            if (result == null)
            {
                SetError("Invalid verification code.");
                return;
            }
            await NavigateAfterLoginAsync();
        }
        catch (Exception ex)
        {
            SetError($"Verification failed: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task NavigateAfterLoginAsync()
    {
        if (_authService.IsPatient)
            await Shell.Current.GoToAsync("//portal/dashboard");
        else
            await Shell.Current.GoToAsync("//dashboard");
    }
}
