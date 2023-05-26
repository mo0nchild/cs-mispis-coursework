using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Pages.ManagerPages;
using WebApplication.Services;

namespace WebApplication.Pages.BuilderPages
{
    [AuthorizeAttribute(Policy = "Builder"), ProfileFilter("BuilderSettings")]
    public partial class BuilderSettingsModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<BuilderSettingsModel> Logger = default!;
        public BuilderSettingsModel(ILogger<BuilderSettingsModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        [BindPropertyAttribute]
        public Employee EmployeeModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Account Account { get => this.EmployeeModel.Account; set => this.EmployeeModel.Account = value; }

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.EmployeeModel = (await dbcontext.Employees.Where(item => item.Accountid == profileId)
                    .Include(item => item.Account).FirstOrDefaultAsync())!;

                if (this.EmployeeModel == null) return base.BadRequest("Данные невозможно загрузить");
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnPostAsync([FromServices] IProfileService profileService)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            IProfileService.ProfileError? errorMessage = default!;

            if ((errorMessage = await profileService.UpdateAccount(this.EmployeeModel.Account, profileId)) != null
                || (errorMessage = await profileService.UpdateEmployee(this.EmployeeModel, profileId)) != null)
            {
                return base.RedirectToPage("BuilderSettings", new { ErrorMessage = errorMessage.ErrorMessage });
            }
            return await this.OnGetAsync();
        }
    }
}
