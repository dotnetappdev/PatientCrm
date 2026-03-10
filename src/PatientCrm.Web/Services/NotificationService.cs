namespace PatientCrm.Web.Services;

/// <summary>
/// Scoped service used in place of TempData for Blazor components.
/// Components set a message here then navigate; the layout reads and clears it.
/// </summary>
public class NotificationService
{
    public string? SuccessMessage { get; private set; }
    public string? ErrorMessage { get; private set; }

    public void SetSuccess(string message) { SuccessMessage = message; ErrorMessage = null; }
    public void SetError(string message) { ErrorMessage = message; SuccessMessage = null; }
    public void Clear() { SuccessMessage = null; ErrorMessage = null; }
}
