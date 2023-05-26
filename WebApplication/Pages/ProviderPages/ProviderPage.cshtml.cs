using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Models;

namespace WebApplication.Pages
{
    [AuthorizeAttribute(Policy = "Provider")]
    public class ProviderPageModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<ProviderPageModel> Logger = default!;
        public ProviderPageModel(ILogger<ProviderPageModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        [BindPropertyAttribute(SupportsGet = true)]
        public int? SelectedRequiredId { get; set; } = default!;

        [BindPropertyAttribute(SupportsGet = true)]
        public int? SelectedResourceId { get; set; } = default!;
        public Resource? SelectedResourceModel { get; private set; } = default!;
        public bool ResourceOwner { get; private set; } = default!;

        public List<Resource> ResourcesList { get; private set; } = new();
        public List<Resourceorder> ResourceOrderList { get; private set; } = new();

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.ResourcesList = await dbcontext.Resources.Include(item => item.Provider)
                    .ThenInclude(item => item.Account).ToListAsync();

                if (this.SelectedResourceId != null)
                {
                    this.SelectedResourceModel = await dbcontext.Resources.Where(
                        item => item.Resourceid == this.SelectedResourceId)
                        .Include(item => item.Provider).ThenInclude(item => item.Account).FirstOrDefaultAsync();

                    this.ResourceOwner = this.SelectedResourceModel?.Provider.Accountid == profileId;
                }
                this.ResourceOrderList = await dbcontext.Resourceorders.ToListAsync();
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnGetDeleteAsync([FromQuery] int selectedId)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var checkCollision = await dbcontext.Resources.Include(item => item.Provider)
                    .Where(item => item.Provider.Accountid == profileId && item.Resourceid == selectedId).CountAsync();

                if(checkCollision <= 0) return this.RedirectToPage("ProviderPage");
                await dbcontext.Resources.Where(item => item.Resourceid == selectedId).ExecuteDeleteAsync();
            }
            return this.RedirectToPage("ProviderPage");
        }
        public virtual async Task<IActionResult> OnPostAsync([FromForm] int requirePrice, [FromForm] int requireId)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var resourceRecord = await dbcontext.Resourceorders.FirstAsync(item => item.Resourceorderid == requireId);

                var record = await dbcontext.Resourceproviders.FirstOrDefaultAsync(item => item.Accountid == profileId);
                if (record == null) return base.BadRequest("Запись о работнике не найдена");

                var checkCollision = await dbcontext.Resources.FirstOrDefaultAsync(item => item.Providerid == record.Providerid
                    && item.Resourcename == resourceRecord.Resourcename && item.Priceforone == requirePrice);
                if (checkCollision == null)
                {
                    await dbcontext.Resources.AddRangeAsync(new Resource()
                    {
                        Count = resourceRecord.Count, Resourcename = resourceRecord.Resourcename,
                        Priceforone = requirePrice, Providerid = record.Providerid
                    });
                }
                else checkCollision.Count += requirePrice;
                await dbcontext.SaveChangesAsync();

                await dbcontext.Resourceorders
                    .Where(item => item.Resourceorderid == resourceRecord.Resourceorderid).ExecuteDeleteAsync();
            }
            return this.RedirectToPage("ProviderPage");
        }

        public virtual async Task<IActionResult> OnPostRequestAsync([FromForm] string requireResource,
            [FromForm] int requireCount, [FromForm] int requirePrice)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var record = await dbcontext.Resourceproviders.FirstOrDefaultAsync(item => item.Accountid == profileId);
                if (record == null) return base.BadRequest("Запись о работнике не найдена");

                var checkCollision = await dbcontext.Resources.FirstOrDefaultAsync(item => item.Providerid == record.Providerid 
                    && item.Resourcename == requireResource && item.Priceforone == requirePrice);
                if (checkCollision == null)
                {
                    await dbcontext.Resources.AddRangeAsync(new Resource()
                    { 
                        Count = requireCount, Resourcename = requireResource, 
                        Priceforone = requirePrice, Providerid = record.Providerid 
                    });
                }
                else checkCollision.Count += requireCount;
                await dbcontext.SaveChangesAsync();
            }
            return this.RedirectToPage("ProviderPage");
        }
    }
}
