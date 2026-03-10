namespace PatientCrm.Web.Services;

/// <summary>
/// Scoped service that captures the JWT from the auth cookie claim during the
/// initial SSR phase and exposes it to PatientApiClient throughout the Blazor circuit.
/// </summary>
public class TokenProvider
{
    public string? AccessToken { get; set; }
}
