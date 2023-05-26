using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Pages.ManagerPages;
using WebApplication.Services;

namespace WebApplication.Pages.DeliveryPages
{
    [AuthorizeAttribute(Policy = "Delivery"), ProfileFilter("DeliverySettings")]
    public partial class DeliverySettingsModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<DeliverySettingsModel> Logger = default!;
        public DeliverySettingsModel(ILogger<DeliverySettingsModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        [BindPropertyAttribute]
        public Deliveryman DeliveryModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Employee Employee { get => this.DeliveryModel.Employee; set => this.DeliveryModel.Employee = value; }

        [BindPropertyAttribute]
        public Account Account { get => this.DeliveryModel.Employee.Account; set => this.DeliveryModel.Employee.Account = value; }

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.DeliveryModel = (await dbcontext.Deliverymen.Include(item => item.Employee)
                    .Where(item => item.Employee.Accountid == profileId).Include(item => item.Employee.Account).FirstOrDefaultAsync())!;

                if (this.DeliveryModel == null) return base.BadRequest("Данные невозможно загрузить");
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnPostAsync([FromServices] IProfileService profileService)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            IProfileService.ProfileError? errorMessage = default!;

            if ((errorMessage = await profileService.UpdateAccount(this.Employee.Account, profileId)) != null
                || (errorMessage = await profileService.UpdateEmployee(this.Employee, profileId)) != null)
            {
                return base.RedirectToPage("DeliverySettings", new { ErrorMessage = errorMessage.ErrorMessage });
            }
            this.DeliveryModel.Driverlicence ??= string.Empty;
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Deliverymen.Include(item => item.Employee).Where(item => item.Employee.Accountid == profileId)
                    .ExecuteUpdateAsync(item => 
                    item.SetProperty(p => p.Driverlicence, p => this.DeliveryModel.Driverlicence)
                        .SetProperty(p => p.Transporttype, p => this.DeliveryModel.Transporttype));

                await dbcontext.SaveChangesAsync();
            }
            return await this.OnGetAsync();
        }
    }
}
