using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Models;

namespace WebApplication.Pages
{
    [AuthorizeAttribute(Policy = "Client")]
    public partial class ClientPage : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<ClientPage> Logger = default!;
        public ClientPage(ILogger<ClientPage> logger, IDbContextFactory<MispisCourseworkContext> factory) : base() 
        { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        [BindPropertyAttribute]
        public Client ClientModel { get; set; } = default!;

        public virtual async Task<IActionResult> OnGetAsync() 
        {
            var profileId = this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid);
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.ClientModel = (await dbcontext.Clients.Where(item => item.Accountid == int.Parse(profileId))
                    .Include(item => item.Orders).FirstOrDefaultAsync())!;

                if (this.ClientModel == null) return base.BadRequest("Данные невозможно загрузить");
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnPostAsync([FromForm] Order orderModel)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var profileRecord = await dbcontext.Clients.FirstAsync(item => item.Accountid == profileId);

                orderModel.State = "Ожидание"; orderModel.Clientid = profileRecord.Clientid;
                orderModel.Ordertime = DateTime.Now;

                await dbcontext.Orders.AddRangeAsync(orderModel); await dbcontext.SaveChangesAsync();
            }
            return await this.OnGetAsync();
        }
    }
}
