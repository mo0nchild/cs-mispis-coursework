using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Models;
using WebApplication.Pages.ManagerPages;

namespace WebApplication.Pages
{
    [AuthorizeAttribute(Policy = "Manager")]
    public partial class ManagerPageModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<ManagerPageModel> Logger = default!;
        public ManagerPageModel(ILogger<ManagerPageModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {

            }
            return this.Page();
        }
    }
}
