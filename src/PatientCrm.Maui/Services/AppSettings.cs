namespace PatientCrm.Maui.Services;

/// <summary>
/// Application settings — the API base URL is stored here and can be configured
/// by the user via app settings or through MDM/configuration profiles.
/// </summary>
public static class AppSettings
{
    private const string ApiUrlKey = "api_base_url";
    private const string DefaultApiUrl = "https://localhost:7001/";

    public static string ApiBaseUrl
    {
        get => Preferences.Get(ApiUrlKey, DefaultApiUrl);
        set => Preferences.Set(ApiUrlKey, value);
    }
}
