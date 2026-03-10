using Fido2NetLib;
using Fido2NetLib.Objects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PatientCrm.Core.Entities;
using PatientCrm.Infrastructure.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PatientCrm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly Fido2 _fido2;
    private readonly ApplicationDbContext _context;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        Fido2 fido2,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _fido2 = fido2;
        _context = context;
    }

    // Standard Login

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return StatusCode(423, new { message = "Account locked out" });
            return Unauthorized(new { message = "Invalid credentials" });
        }

        if (await _userManager.GetTwoFactorEnabledAsync(user))
        {
            var partialToken = GeneratePartialToken(user.Id, user.Email!);
            return Ok(new
            {
                requiresTwoFactor = true,
                partialToken,
                message = "Two-factor authentication required"
            });
        }

        var token = await GenerateJwtToken(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Ok(new
        {
            token,
            requiresTwoFactor = false,
            user = new { user.Id, user.Email, user.FullName, user.TenantId, user.Theme, user.TwoFactorEnabled }
        });
    }

    // Two-Factor Authentication

    [HttpPost("2fa/verify")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorRequest request)
    {
        var userId = ValidatePartialToken(request.PartialToken);
        if (userId == null)
            return Unauthorized(new { message = "Invalid or expired session" });

        var user = await _userManager.FindByIdAsync(userId.ToString()!);
        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid session" });

        var code = request.Code.Replace(" ", "").Replace("-", "");
        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Authenticator", code);
        if (!isValid)
        {
            var recoveryResult = await _userManager.RedeemTwoFactorRecoveryCodeAsync(user, code);
            if (!recoveryResult.Succeeded)
                return Unauthorized(new { message = "Invalid authenticator code" });
        }

        var token = await GenerateJwtToken(user);
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        return Ok(new { token, user = new { user.Id, user.Email, user.FullName, user.TenantId, user.Theme } });
    }

    [HttpGet("2fa/setup")]
    [Authorize]
    public async Task<IActionResult> SetupTwoFactor()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        await _userManager.ResetAuthenticatorKeyAsync(user);
        var key = await _userManager.GetAuthenticatorKeyAsync(user);
        var email = user.Email ?? user.UserName ?? "user";
        var qrCodeUri = $"otpauth://totp/PatientCRM:{Uri.EscapeDataString(email)}?secret={key}&issuer=PatientCRM&digits=6";

        return Ok(new { sharedKey = key, qrCodeUri });
    }

    [HttpPost("2fa/enable")]
    [Authorize]
    public async Task<IActionResult> EnableTwoFactor([FromBody] EnableTwoFactorRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isValid = await _userManager.VerifyTwoFactorTokenAsync(user, "Authenticator", request.Code.Replace(" ", "").Replace("-", ""));
        if (!isValid)
            return BadRequest(new { message = "Invalid authenticator code. Please try again." });

        await _userManager.SetTwoFactorEnabledAsync(user, true);
        var recoveryCodes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        return Ok(new { message = "Two-factor authentication enabled successfully.", recoveryCodes });
    }

    [HttpPost("2fa/disable")]
    [Authorize]
    public async Task<IActionResult> DisableTwoFactor([FromBody] DisableTwoFactorRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
        if (!isPasswordValid)
            return BadRequest(new { message = "Invalid password" });

        await _userManager.SetTwoFactorEnabledAsync(user, false);
        await _userManager.ResetAuthenticatorKeyAsync(user);

        return Ok(new { message = "Two-factor authentication disabled." });
    }

    [HttpGet("2fa/recovery-codes")]
    [Authorize]
    public async Task<IActionResult> RegenerateRecoveryCodes()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var codes = await _userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);
        return Ok(new { recoveryCodes = codes });
    }

    // Passkeys / FIDO2

    [HttpPost("passkey/register-options")]
    [Authorize]
    public async Task<IActionResult> PasskeyRegisterOptions()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var existingKeys = await _context.UserCredentials
            .Where(c => c.UserId == user.Id)
            .ToListAsync();

        var excludeCredentials = existingKeys
            .Select(c => new PublicKeyCredentialDescriptor(WebEncoders.Base64UrlDecode(c.CredentialId)))
            .ToList();

        var fidoUser = new Fido2User
        {
            Id = Encoding.UTF8.GetBytes(user.Id.ToString()),
            Name = user.Email!,
            DisplayName = user.FullName
        };

        var options = _fido2.RequestNewCredential(new RequestNewCredentialParams
        {
            User = fidoUser,
            ExcludeCredentials = excludeCredentials,
            AuthenticatorSelection = new AuthenticatorSelection
            {
                ResidentKey = ResidentKeyRequirement.Required,
                UserVerification = UserVerificationRequirement.Required
            },
            AttestationPreference = AttestationConveyancePreference.None
        });

        HttpContext.Session.SetString("fido2.attestation.options", options.ToJson());
        return Ok(options);
    }

    [HttpPost("passkey/register")]
    [Authorize]
    public async Task<IActionResult> PasskeyRegister([FromBody] AuthenticatorAttestationRawResponse attestationResponse, [FromQuery] string? displayName)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var jsonOptions = HttpContext.Session.GetString("fido2.attestation.options");
        if (string.IsNullOrEmpty(jsonOptions))
            return BadRequest(new { message = "Registration session expired. Please try again." });

        var options = CredentialCreateOptions.FromJson(jsonOptions);

        var result = await _fido2.MakeNewCredentialAsync(new MakeNewCredentialParams
        {
            AttestationResponse = attestationResponse,
            OriginalOptions = options,
            IsCredentialIdUniqueToUserCallback = async (args, _) =>
            {
                var credId = WebEncoders.Base64UrlEncode(args.CredentialId);
                return !await _context.UserCredentials.AnyAsync(c => c.CredentialId == credId);
            }
        });

        var credential = new UserCredential
        {
            UserId = user.Id,
            CredentialId = WebEncoders.Base64UrlEncode(result.Id),
            PublicKey = result.PublicKey,
            SignatureCounter = result.SignCount,
            AaGuid = result.AaGuid.ToString(),
            DisplayName = displayName ?? "Passkey"
        };

        _context.UserCredentials.Add(credential);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Passkey registered successfully.", credentialId = credential.CredentialId });
    }

    [HttpPost("passkey/assertion-options")]
    [AllowAnonymous]
    public async Task<IActionResult> PasskeyAssertionOptions([FromBody] PasskeyLoginOptionsRequest request)
    {
        var allowedCredentials = new List<PublicKeyCredentialDescriptor>();

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user != null)
            {
                var keys = await _context.UserCredentials.Where(c => c.UserId == user.Id).ToListAsync();
                allowedCredentials = keys
                    .Select(c => new PublicKeyCredentialDescriptor(WebEncoders.Base64UrlDecode(c.CredentialId)))
                    .ToList();
            }
        }

        var options = _fido2.GetAssertionOptions(new GetAssertionOptionsParams
        {
            AllowedCredentials = allowedCredentials,
            UserVerification = UserVerificationRequirement.Required
        });

        HttpContext.Session.SetString("fido2.assertion.options", options.ToJson());
        return Ok(options);
    }

    [HttpPost("passkey/login")]
    [AllowAnonymous]
    public async Task<IActionResult> PasskeyLogin([FromBody] AuthenticatorAssertionRawResponse assertionResponse)
    {
        var jsonOptions = HttpContext.Session.GetString("fido2.assertion.options");
        if (string.IsNullOrEmpty(jsonOptions))
            return BadRequest(new { message = "Login session expired. Please try again." });

        var options = AssertionOptions.FromJson(jsonOptions);
        var credentialId = assertionResponse.Id;   // Id is already base64url string
        var stored = await _context.UserCredentials.Include(c => c.User)
            .FirstOrDefaultAsync(c => c.CredentialId == credentialId);

        if (stored?.User == null || !stored.User.IsActive)
            return Unauthorized(new { message = "Passkey not found" });

        var result = await _fido2.MakeAssertionAsync(new MakeAssertionParams
        {
            AssertionResponse = assertionResponse,
            OriginalOptions = options,
            StoredPublicKey = stored.PublicKey,
            StoredSignatureCounter = stored.SignatureCounter,
            IsUserHandleOwnerOfCredentialIdCallback = (args, _) =>
                Task.FromResult(Encoding.UTF8.GetString(args.UserHandle) == stored.UserId.ToString())
        });

        stored.SignatureCounter = result.SignCount;
        stored.LastUsedAt = DateTime.UtcNow;
        stored.User.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = await GenerateJwtToken(stored.User);
        return Ok(new
        {
            token,
            user = new { stored.User.Id, stored.User.Email, stored.User.FullName, stored.User.TenantId, stored.User.Theme }
        });
    }

    [HttpGet("passkeys")]
    [Authorize]
    public async Task<IActionResult> GetPasskeys()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var credentials = await _context.UserCredentials
            .Where(c => c.UserId == user.Id)
            .Select(c => new { c.Id, c.DisplayName, c.CreatedAt, c.LastUsedAt })
            .ToListAsync();

        return Ok(credentials);
    }

    [HttpDelete("passkeys/{id:guid}")]
    [Authorize]
    public async Task<IActionResult> DeletePasskey(Guid id)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var credential = await _context.UserCredentials.FirstOrDefaultAsync(c => c.Id == id && c.UserId == user.Id);
        if (credential == null) return NotFound();

        _context.UserCredentials.Remove(credential);
        await _context.SaveChangesAsync();
        return Ok(new { message = "Passkey removed." });
    }

    // Profile and Settings

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok(new { message = "Password changed successfully" });
    }

    [HttpGet("profile")]
    [Authorize]
    public async Task<IActionResult> GetProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var passkeyCount = await _context.UserCredentials.CountAsync(c => c.UserId == user.Id);

        return Ok(new
        {
            user.Id,
            user.Email,
            user.FullName,
            user.FirstName,
            user.LastName,
            user.Title,
            user.GmcNumber,
            user.GdcNumber,
            user.TenantId,
            user.Theme,
            user.TwoFactorEnabled,
            PasskeyCount = passkeyCount,
            Roles = roles
        });
    }

    [HttpPut("theme")]
    [Authorize]
    public async Task<IActionResult> UpdateTheme([FromBody] ThemeRequest request)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        user.Theme = request.Theme == "dark" ? "dark" : "light";
        await _userManager.UpdateAsync(user);
        return Ok(new { theme = user.Theme });
    }

    // Helpers

    private async Task<string> GenerateJwtToken(ApplicationUser user)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key configuration is required.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var roles = await _userManager.GetRolesAsync(user);
        var userClaims = await _userManager.GetClaimsAsync(user);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new(ClaimTypes.Name, user.FullName),
            new("TenantId", user.TenantId.ToString()),
        };

        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));
        claims.AddRange(userClaims);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "PatientCrm",
            audience: _configuration["Jwt:Audience"] ?? "PatientCrm",
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GeneratePartialToken(Guid userId, string email)
    {
        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key configuration is required.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new("partial", "true"),
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email)
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "PatientCrm",
            audience: _configuration["Jwt:Audience"] ?? "PatientCrm",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private Guid? ValidatePartialToken(string token)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey)) return null;

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var principal = handler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"] ?? "PatientCrm",
                ValidAudience = _configuration["Jwt:Audience"] ?? "PatientCrm",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
            }, out _);

            if (principal.FindFirst("partial")?.Value != "true") return null;
            var idStr = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idStr, out var id) ? id : null;
        }
        catch
        {
            return null;
        }
    }
}

public record LoginRequest(string Email, string Password);
public record TwoFactorRequest(string PartialToken, string Code);
public record EnableTwoFactorRequest(string Code);
public record DisableTwoFactorRequest(string Password);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ThemeRequest(string Theme);
public record PasskeyLoginOptionsRequest(string? Email = null);
