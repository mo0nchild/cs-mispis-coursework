using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using WebApplication.Models;

namespace WebApplication.Pages.ManagerPages
{
    public enum ModalWindowMode : sbyte { NoneWindow, LookInfo, AddEmployee, DeleteEmployee }

    [AuthorizeAttribute(Policy = "Manager")]
    public partial class ManagerWorkersModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<ManagerWorkersModel> Logger = default!;
        public ManagerWorkersModel(ILogger<ManagerWorkersModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        [BindPropertyAttribute(SupportsGet = true)]
        public ModalWindowMode ModalWindowMode { get; set; } = ModalWindowMode.NoneWindow;
        public Employee? SelectedEmployee { get; set; } = default!;

        public List<Employee> RequestEmployeesList { get; set; } = new();
        public List<Employee> AllEmployeesList { get; set; } = new();

        public List<Builder> ActiveBuildersList { get; set; } = new();
        public List<Builder> NotActiveBuildersList { get; set; } = new();

        public virtual async Task<IActionResult> OnGetAsync()
        {
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var managerId = (await dbcontext.Managers.Include(item => item.Employee)
                    .FirstAsync(item => item.Employee.Accountid == profileId)).Managerid;

                IQueryable<Employee> employeesList = dbcontext.Employees.Include(item => item.Account)
                    .Where(item => item.Accountid != profileId);

                this.AllEmployeesList = await employeesList.Where(item => item.Activated == true).ToListAsync();
                this.RequestEmployeesList = await employeesList.Where(item => item.Activated == false).ToListAsync();

                IQueryable<Builder> buidlersList = dbcontext.Builders.Include(item => item.Employee)
                    .ThenInclude(item => item.Account).Where(item => item.Employee.Activated == true);

                this.ActiveBuildersList = await buidlersList.Where(item => item.Managerid == managerId).ToListAsync();
                this.NotActiveBuildersList = await buidlersList.Where(item => item.Managerid == null).ToListAsync();
            }
            return this.Page();
        }
        public virtual async Task<IActionResult> OnGetInfoAsync([FromQuery] int? selectedEmployeeId)
        {
            if (selectedEmployeeId == null) return await this.OnGetAsync();
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.SelectedEmployee = await dbcontext.Employees.Where(item => item.Employeeid == selectedEmployeeId)
                    .Include(item => item.Account).Include(item => item.Deliverymen).FirstOrDefaultAsync();

                if (this.SelectedEmployee == null) return base.BadRequest("Сотрудник не найден");
            }
            this.ModalWindowMode = ModalWindowMode.LookInfo;
            return await this.OnGetAsync();
        }
        public virtual async Task<IActionResult> OnGetAddBuilderAsync([FromQuery] int? builderId)
        {
            if (builderId == null) return base.BadRequest("Параметр builderId не установлен");
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));

            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var managerId = (await dbcontext.Managers.Include(item => item.Employee)
                    .Where(item => item.Employee.Accountid == profileId).FirstAsync()).Managerid;

                await dbcontext.Builders.Where(item => item.Builderid == builderId)
                    .ExecuteUpdateAsync(item => item.SetProperty(p => p.Managerid, p => managerId));
            }
            return base.RedirectToPage("ManagerWorkers");
        }
        public virtual async Task<IActionResult> OnGetRemoveBuilderAsync([FromQuery] int? builderId)
        {
            if (builderId == null) return base.BadRequest("Параметр builderId не установлен");
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Builders.Where(item => item.Builderid == builderId)
                    .ExecuteUpdateAsync(item => item.SetProperty(p => p.Managerid, p => null));
            }
            return base.RedirectToPage("ManagerWorkers");
        }

        public virtual async Task<IActionResult> OnGetEditAsync([FromQuery] int? employeeId)
        {
            if (employeeId == null) return base.BadRequest("Параметр employeeId не установлен");

            this.SelectedEmployee = new Employee() { Employeeid = employeeId.Value };
            return await this.OnGetAsync();
        }
        public virtual async Task<IActionResult> OnPostEditAsync([FromForm] int employeeId, [FromForm] int salary)
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Employees.Where(item => item.Employeeid == employeeId).ExecuteUpdateAsync(item =>
                    item.SetProperty(p => p.Activated, p => true).SetProperty(p => p.Salary, p => salary));
                await dbcontext.SaveChangesAsync();
            }
            return base.RedirectToPage("ManagerWorkers");
        }

        public virtual async Task<IActionResult> OnGetDeleteAsync([FromQuery] int? employeeId)
        {
            if (employeeId == null) return base.BadRequest("Параметр employeeId не установлен");

            this.SelectedEmployee = new Employee() { Employeeid = employeeId.Value };
            return await this.OnGetAsync();
        }
        public virtual async Task<IActionResult> OnPostDeleteAsync([FromForm] int employeeId)
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var employeeRecord = await dbcontext.Employees.Where(item => item.Employeeid == employeeId)
                    .Include(item => item.Account).FirstOrDefaultAsync();

                if (employeeRecord == null) return base.BadRequest("Сотрудник не найден");
                await dbcontext.Authorizations.Where(item => item.Authorizationid == employeeRecord.Account.Authorizationid)
                    .ExecuteDeleteAsync();
            }
            return base.RedirectToPage("ManagerWorkers");
        }
    }
}
