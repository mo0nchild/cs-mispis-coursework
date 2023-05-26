using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Models;

namespace WebApplication.Pages.ManagerPages
{
    [AuthorizeAttribute(Policy = "Manager")]
    public partial class ManagerOrderDetailsModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;
        public ManagerOrderDetailsModel(IDbContextFactory<MispisCourseworkContext> factory) : base() 
        { this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public int SelectedId { get; set; } = default!;

        public List<Builder> BuildersList { get; private set; } = default!;
        public int CurrentBuilder { get; set; } = default;
        public Order SelectedOrder { get; private set; } = default!;

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.SelectedOrder = (await dbcontext.Orders.Where(item => item.Orderid == this.SelectedId)
                    .Include(item => item.Client).ThenInclude(item => item.Account).FirstOrDefaultAsync())!;

                var managerRecord = await dbcontext.Managers.Include(item => item.Employee)
                    .Where(item => item.Employee.Accountid == profileId).FirstOrDefaultAsync();
                if (managerRecord == null) return base.BadRequest("Невозможно загрузить запись о менеджере");

                await Console.Out.WriteLineAsync($"\nmanagerId = {managerRecord.Managerid}\n");

                this.BuildersList = await dbcontext.Builders.Where(item => item.Managerid == managerRecord.Managerid)
                    .Include(item => item.Employee).ThenInclude(item => item.Account).ToListAsync(); 

                if (this.SelectedOrder == null) return base.BadRequest("Заказ невозможно найти");
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnPostAsync([FromForm] int selectedBuilder)
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var managerRecord = await dbcontext.Managers.Include(item => item.Employee)
                    .FirstOrDefaultAsync(item => item.Employee.Accountid == profileId);
                if (managerRecord == null) return base.BadRequest("Запись о менеджере не найдена");

                if (dbcontext.Buildingorders.Where(item => item.Orderid == this.SelectedId).Count() <= 0)
                {
                    await dbcontext.Buildingorders.AddRangeAsync(new Buildingorder()
                    { Builderid = selectedBuilder, Orderid = this.SelectedId });
                }
                else await dbcontext.Buildingorders.Where(item => item.Orderid == this.SelectedId).ExecuteUpdateAsync(item =>
                    item.SetProperty(p => p.Builderid, p => selectedBuilder));

                await dbcontext.Orders.Where(item => item.Orderid == this.SelectedId).ExecuteUpdateAsync( item => 
                    item.SetProperty(p => p.State, p => "Обрабатывается").SetProperty(p => p.Managerid, p => managerRecord.Managerid));

                await dbcontext.SaveChangesAsync();
            }
            return base.RedirectToPage("ManagerPage");
        }
        public virtual async Task<IActionResult> OnGetRefuseAsync()
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Orders.Where(item => item.Orderid == this.SelectedId)
                    .ExecuteUpdateAsync(item => item.SetProperty(p => p.State, p => "Отказано"));

                await dbcontext.Buildingorders.Where(item => item.Orderid == this.SelectedId).ExecuteDeleteAsync();
                await dbcontext.SaveChangesAsync();
            }
            return base.RedirectToPage("ManagerPage");
        }
    }
}
