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

        [BindPropertyAttribute(SupportsGet = true)]
        public int? SelectedOrderId { get; set; } = default!;
        public Order? OrderModel { get; set; } = default!;

        public virtual async Task<IActionResult> OnGetAsync() 
        {
            var profileId = this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid);
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.ClientModel = (await dbcontext.Clients.Where(item => item.Accountid == int.Parse(profileId))
                    .Include(item => item.Orders).Include(item => item.Payments).FirstOrDefaultAsync())!;

                this.OrderModel = (this.SelectedOrderId == null) ? null : await dbcontext.Orders.Where(
                    item => item.Orderid == this.SelectedOrderId).FirstOrDefaultAsync();

                if(this.OrderModel != null && this.OrderModel.Managerid != null)
                {
                    this.OrderModel.Manager = await dbcontext.Managers.Where(item => item.Managerid == this.OrderModel.Managerid)
                        .Include(item => item.Employee).ThenInclude(item => item.Account).FirstOrDefaultAsync();
                }
                if (this.ClientModel == null || (this.OrderModel == null && this.SelectedOrderId != null)) 
                { return base.BadRequest("Данные невозможно загрузить"); }
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnPostAsync([FromForm] Order orderModel)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var profileRecord = await dbcontext.Clients.Where(item => item.Accountid == profileId)
                    .Include(item => item.Payments).FirstAsync();

                await Console.Out.WriteLineAsync($"\nprofileRecord: {profileRecord.Payments.Count()}\n");

                if (profileRecord.Payments.Count() <= 0)
                    return this.RedirectToPage("ClientPage", new { ErrorMessage = "Платежные данные не установлены" });

                orderModel.State = "Ожидание"; orderModel.Clientid = profileRecord.Clientid;
                orderModel.Ordertime = DateTime.Now;

                await dbcontext.Orders.AddRangeAsync(orderModel); await dbcontext.SaveChangesAsync();
            }
            return await this.OnGetAsync();
        }

        public virtual async Task<IActionResult> OnPostDeleteAsync(int orderId)
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Orders.Where(item => item.Orderid == orderId).ExecuteDeleteAsync();
                await dbcontext.SaveChangesAsync();
            }
            return base.RedirectToPage("ClientPage");
        }

        public virtual async Task<IActionResult> OnPostPayAsync([FromForm]int selectedId, [FromForm] int paymentType)
        {
            Payment myPayment = default!;
            Order myOrder = default!;
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                myOrder = await dbcontext.Orders.FirstAsync(item => item.Orderid == selectedId);
                myPayment = await dbcontext.Payments.FirstAsync(item => item.Paymentid == paymentType);

                await dbcontext.Orders.Where(item => item.Orderid == selectedId).ExecuteDeleteAsync();
                await dbcontext.SaveChangesAsync();
            }
            using (var fileWriter = new StreamWriter("./payment.txt"))
            {
                await fileWriter.WriteLineAsync("hello");
            }
            return base.File(System.IO.File.ReadAllBytes("./payment.txt"), "application/octet-stream", "payment.txt");
        }
    }
}
