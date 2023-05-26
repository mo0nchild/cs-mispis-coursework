using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;
using WebApplication.Models;

namespace WebApplication.Pages.DeliveryPages
{
    [AuthorizeAttribute(Policy = "Delivery")]
    public partial class DeliveryPageModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<DeliveryPageModel> Logger = default!;
        public DeliveryPageModel(ILogger<DeliveryPageModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        public List<Order> OrdersList { get; set; } = new();

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.OrdersList = await dbcontext.Orders.Where(item => item.State == "Готов к отправке").ToListAsync();
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnPostAsync([FromForm] int selectedId)
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Orders.Where(item => item.Orderid == selectedId).ExecuteUpdateAsync(
                    item => item.SetProperty(p => p.State, p => "Доставлен"));
            }
            return this.RedirectToPage("DeliveryPage");
        }
    }
}
