using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PatientCrm.Web.Services;
using System.Security.Claims;

namespace PatientCrm.Web.Controllers;

public class AccountController : Controller
{
    private readonly PatientApiClient _api;

    public AccountController(PatientApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewBag.ReturnUrl = returnUrl;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(string email, string password, bool rememberMe = false, string? returnUrl = null)
    {
        var result = await _api.LoginAsync(email, password);
        if (result == null)
        {
            ModelState.AddModelError(string.Empty, "Invalid credentials. Please try again.");
            return View();
        }

        if (result.RequiresTwoFactor)
        {
            TempData["2FA_PartialToken"] = result.PartialToken;
            TempData["ReturnUrl"] = returnUrl;
            TempData["RememberMe"] = rememberMe;
            return RedirectToAction(nameof(TwoFactor));
        }

        if (!string.IsNullOrEmpty(result.Token))
        {
            await SignInWithToken(result.Token, result.User?.FullName ?? email, rememberMe);
            return LocalRedirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
        }

        ModelState.AddModelError(string.Empty, "Login failed. Please try again.");
        return View();
    }

    [HttpGet]
    public IActionResult TwoFactor()
    {
        if (TempData["2FA_PartialToken"] == null)
            return RedirectToAction(nameof(Login));

        ViewBag.PartialToken = TempData["2FA_PartialToken"];
        ViewBag.ReturnUrl = TempData["ReturnUrl"];
        ViewBag.RememberMe = TempData["RememberMe"];
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TwoFactor(string partialToken, string code, bool rememberMe = false, string? returnUrl = null)
    {
        var result = await _api.VerifyTwoFactorAsync(partialToken, code);
        if (result == null || string.IsNullOrEmpty(result.Token))
        {
            ModelState.AddModelError(string.Empty, "Invalid authenticator code. Please try again.");
            ViewBag.PartialToken = partialToken;
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        await SignInWithToken(result.Token, result.User?.FullName ?? "User", rememberMe);
        return LocalRedirect(!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? returnUrl : "/");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult AccessDenied() => View();

    // Called after successful passkey authentication via JavaScript
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> PasskeyCallback(string token)
    {
        if (string.IsNullOrEmpty(token))
            return RedirectToAction(nameof(Login));

        try
        {
            var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);
            var name = jwt.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Name)?.Value ?? "User";
            await SignInWithToken(token, name, false);
            return RedirectToAction("Index", "Home");
        }
        catch
        {
            return RedirectToAction(nameof(Login));
        }
    }

    [HttpPost]
    [Authorize]
    [Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryToken] // AJAX-only endpoint called with JSON body
    public IActionResult UpdateTheme([FromBody] ThemeUpdateModel model)
    {
        // Theme is stored client-side; the API will be called via JS
        return Ok(new { success = true });
    }

    // Signs in user using claims from the JWT token
    private async Task SignInWithToken(string token, string displayName, bool rememberMe)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        var claims = new List<Claim>(jwt.Claims)
        {
            new("api_token", token)
        };

        // Ensure Name claim is set
        if (!claims.Any(c => c.Type == ClaimTypes.Name))
            claims.Add(new Claim(ClaimTypes.Name, displayName));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var authProps = new AuthenticationProperties
        {
            IsPersistent = rememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
        };

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, authProps);
    }

    public record ThemeUpdateModel(string Theme);
}
