using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApplication.Pages;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore;
using WebApplication.Models;

namespace WebApplication.Controller
{
    [ControllerAttribute, RouteAttribute("profile")]
    public partial class ProfileController : Microsoft.AspNetCore.Mvc.Controller
    {
        protected ILogger<ProfileController> Logger { get; set; } = default!;
        public ProfileController(ILogger<ProfileController> logger) : base() { this.Logger = logger; }

        [RouteAttribute("login", Name = "login"), HttpGetAttribute, AuthorizeAttribute]
        public async Task<IActionResult> Login([FromServices] IDbContextFactory<MispisCourseworkContext> factory)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirst(ClaimTypes.PrimarySid)!.Value);
            using (var dbcontext = await factory.CreateDbContextAsync())
            {
                var record = await dbcontext.Accounts.FirstOrDefaultAsync(item => item.Accountid == profileId);
                if (record != null) return this.RedirectToPage($"/{record.Profiletype}Pages/{record.Profiletype}Page");
            }
            return await this.Logout();
        }
        [RouteAttribute("logout", Name = "logout"), HttpGetAttribute]
        public async Task<IActionResult> Logout()
        {
            await this.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return base.LocalRedirect("/");
        }
    }
}
