// for Login and Logout with cookie auth + audit logging
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using UnifleqSolutions_IMS.Data;
using UnifleqSolutions_IMS.Services;
using UnifleqSolutions_IMS.ViewModels;

namespace UnifleqSolutions_IMS.Controllers
{
    public class AccountController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IAuditService _audit;
        private readonly CaptchaService _captcha;
        private readonly IConfiguration _config;
        private readonly GeoLocationService _geo;

        public AccountController(
            AppDbContext context,
            IAuditService audit,
            CaptchaService captcha,
            IConfiguration config,
            GeoLocationService geo)
        {
            _context = context;
            _audit = audit;
            _captcha = captcha;
            _config = config;
            _geo = geo;
        }

        // GET /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction("Index", "Dashboard");

            ViewData["ReCaptchaSiteKey"] = _config["ReCaptcha:SiteKey"];
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST /Account/Login
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Verify reCAPTCHA first UNIFLEQ
            var captchaToken = Request.Form["g-recaptcha-response"];
            var captchaValid = await _captcha.VerifyAsync(captchaToken);
            if (!captchaValid)
            {
                ViewData["ReCaptchaSiteKey"] = _config["ReCaptcha:SiteKey"];
                ModelState.AddModelError("", "Please complete the reCAPTCHA verification.");
                return View(model);
            }

            if (!ModelState.IsValid)
            {
                ViewData["ReCaptchaSiteKey"] = _config["ReCaptcha:SiteKey"];
                return View(model);
            }

            // Find user in DB UNIFLEQ
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == model.Username && u.Status == "Active");

            if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
            {
                ViewData["ReCaptchaSiteKey"] = _config["ReCaptcha:SiteKey"];
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Build claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name,           user.Username),
                new Claim("FullName",                user.FullName),
                new Claim(ClaimTypes.Role,           user.Role!.RoleName)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);
            var authProps = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(14)
                    : DateTimeOffset.UtcNow.AddHours(8)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,
                authProps);

            // Get IP and location UNIFLEQ
            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var location = await _geo.GetLocationAsync(ip ?? "");
                await _audit.LogAsync(user.UserID, "Login",
                    $"User '{user.Username}' logged in. Location: {location}");
            }
            catch
            {
                await _audit.LogAsync(user.UserID, "Login",
                    $"User '{user.Username}' logged in.");
            }

            if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                return Redirect(model.ReturnUrl);

            return RedirectToAction("Index", "Dashboard");
        }

        // POST /Account/Logout
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdStr, out int userId))
                await _audit.LogAsync(userId, "Logout",
                    $"User '{User.Identity?.Name}' logged out.");

            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        // GET /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied() => View();
    }
}