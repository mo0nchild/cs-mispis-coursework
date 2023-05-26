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

        [BindPropertyAttribute(SupportsGet = true)]
        public int? SelectedOrderId { get; set; } = default;
        public Order? SelectedOrderModel { get; private set; } = default!;

        public List<Order> AcceptedOrdersList { get; private set; } = new();
        public List<Order> OrdersList { get; private set; } = new();

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.OrdersList = await dbcontext.Orders.Where(item => item.Managerid == null && item.State == "Ожидание")
                    .Include(item => item.Client).ThenInclude(item => item.Account).ToListAsync();

                var managerRecord = await dbcontext.Managers
                    .Include(item => item.Employee).FirstAsync(item => item.Employee.Accountid == profileId);
                this.AcceptedOrdersList = dbcontext.Orders.Where(item => item.State != "Ожидание" && item.Managerid != null).ToList();
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnGetDeleteAsync([FromQuery] int selectedId)
        {
            this.Logger.LogInformation($"Order record cleared: #{selectedId}\n");
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Orders.Where(item => item.Orderid == selectedId).ExecuteUpdateAsync(item =>
                    item.SetProperty(p => p.Managerid, p => null).SetProperty(p => p.State, p => "Отказано"));

                await dbcontext.Buildingorders.Where(item => item.Orderid == selectedId).ExecuteDeleteAsync();
            }
            return base.RedirectToPage("ManagerPage");
        }
    }
}
