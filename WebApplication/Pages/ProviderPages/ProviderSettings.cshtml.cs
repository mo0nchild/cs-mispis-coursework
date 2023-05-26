using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Pages.ProviderPages
{
    [AuthorizeAttribute(Policy = "Provider"), ProfileFilterAttribute("ProviderSettings")]
    public partial class ProviderSettingsModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<ProviderSettingsModel> Logger = default!;
        public ProviderSettingsModel(ILogger<ProviderSettingsModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute]
        public Resourceprovider ProviderModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Account Account { get => this.ProviderModel.Account; set => this.ProviderModel.Account = value; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.ProviderModel = (await dbcontext.Resourceproviders.Where(item => item.Accountid == profileId)
                    .Include(item => item.Account).FirstOrDefaultAsync())!;

                if (this.ProviderModel == null) return base.BadRequest("Данные невозможно загрузить");
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnPostAsync([FromServices] IProfileService profileService)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var errorMessage = await profileService.UpdateAccount(this.ProviderModel.Account, profileId);

            if (errorMessage != null) return base.RedirectToPage("ProviderSettings", new { ErrorMessage = errorMessage.ErrorMessage });
            var errorModel = new Dictionary<string, string?>() 
            { 
                { "Неверная компания", this.ProviderModel.Companyname }, { "Неверный адрес компании", this.ProviderModel.Address },
            };
            foreach (KeyValuePair<string, string?> error in errorModel) 
            {
                if(error.Value == null || error.Value.Length < 5) return base.RedirectToPage("ProviderSettings", new { ErrorMessage = error.Key });
            }
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Resourceproviders.Where(item => item.Accountid == profileId)
                    .ExecuteUpdateAsync(item => item.SetProperty(p => p.Companyname, p => this.ProviderModel.Companyname)
                    .SetProperty(p => p.Address, p => this.ProviderModel.Address));

                await dbcontext.SaveChangesAsync();
            }
            return await this.OnGetAsync();
        }
    }
}
