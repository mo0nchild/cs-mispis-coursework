using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using System.Security.Claims;
using WebApplication.Filters;
using WebApplication.Models;

namespace WebApplication.Pages
{
    [FlagsAttribute]
    public enum IndexMode : sbyte
    {
        Authorization = 32, Client = 1, Manager = 2, Builder = 4, Delivery = 8, Provider = 16,
    }
    [AuthorizationFilterAttribute]
    public partial class IndexModel : PageModel
    {
        protected readonly ILogger<IndexModel> _logger = default!;
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        [BindPropertyAttribute(SupportsGet = true)]
        public IndexMode IndexMode { get; set; } = IndexMode.Authorization;

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;
        public bool HasError { get => this.ErrorMessage != null; }

        [BindPropertyAttribute]
        public Authorization AuthorizationModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Account AccountModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Employee EmployeeModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Client ClientModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Deliveryman DeliverymanModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Resourceprovider ProviderModel { get; set; } = default!;

        public IndexModel(ILogger<IndexModel> logger, IDbContextFactory<MispisCourseworkContext> factory) : base() 
        { this._logger = logger; this.DatabaseFactory = factory; }

        public virtual Task<IActionResult> OnGetAsync()
        {
            this._logger.LogInformation("Authorization GET Method");
            if (this.HttpContext.User.Identity != null && this.HttpContext.User.Identity.IsAuthenticated)
            {
                return Task.FromResult<IActionResult>(base.LocalRedirect("/profile/login"));
            }
            return Task.FromResult<IActionResult>(this.Page());
        }
        public async Task<IActionResult> OnPostRegistrationAsync()
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.AuthorizationModel.Accesscode = Guid.NewGuid().ToString();
                if(dbcontext.Authorizations.Where(item => item.Login == this.AuthorizationModel.Login).Count() > 0)
                {
                    return this.RedirectToPage("Index", new { ErrorMessage = "Логин уже используется", IndexMode = this.IndexMode });
                }
                await dbcontext.Authorizations.AddAsync(this.AuthorizationModel);
                this.AccountModel.Authorization = this.AuthorizationModel;

                this.AccountModel.Profiletype = this.IndexMode.ToString();
                await dbcontext.Accounts.AddAsync(this.AccountModel);

                await dbcontext.SaveChangesAsync();
                if((IndexMode.Manager | IndexMode.Builder | IndexMode.Delivery).HasFlag(this.IndexMode))
                {
                    this.EmployeeModel.Accountid = this.AccountModel.Accountid;
                    this.EmployeeModel.Activated = default(bool);
                    await dbcontext.Employees.AddAsync(this.EmployeeModel);
                }
                switch (this.IndexMode)
                {
                    case IndexMode.Builder:
                        await dbcontext.Builders.AddAsync(new Builder() { Employeeid = this.EmployeeModel.Employeeid }); break;
                    case IndexMode.Client:
                        this.ClientModel.Accountid = this.AccountModel.Accountid;
                        await dbcontext.Clients.AddAsync(this.ClientModel); 
                        break;
                    case IndexMode.Provider:
                        this.ProviderModel.Accountid = this.AccountModel.Accountid;
                        await dbcontext.Resourceproviders.AddAsync(this.ProviderModel); break;
                    case IndexMode.Manager:
                        await dbcontext.Managers.AddAsync(new Manager() { Employee = this.EmployeeModel, Isadmin = default });
                        break;
                    case IndexMode.Delivery:
                        this.DeliverymanModel.Employee = this.EmployeeModel;
                        await dbcontext.Deliverymen.AddAsync(this.DeliverymanModel);
                        break;
                }
                await dbcontext.SaveChangesAsync();
            }
            return await this.OnPostLoginAsync();
        }
        public async Task<IActionResult> OnPostLoginAsync()
        {
            await Console.Out.WriteLineAsync("POST LOGIN");
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                var profileRecord = await dbcontext.Authorizations.Include(item => item.Account).FirstOrDefaultAsync(item =>
                    item.Login == this.AuthorizationModel.Login && item.Password == this.AuthorizationModel.Password);

                if(profileRecord == null || profileRecord.Account == null) 
                { return this.RedirectToPage("Index", new { ErrorMessage = "Профиль не найден", IndexMode = this.IndexMode }); }

                this.AccountModel = profileRecord.Account;
            }
            var profilePrinciple = new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.PrimarySid, this.AccountModel.Accountid.ToString()),
                new Claim(ClaimTypes.Role, this.AccountModel.Profiletype),
            },
            CookieAuthenticationDefaults.AuthenticationScheme);
            await this.HttpContext.SignInAsync(new ClaimsPrincipal(profilePrinciple));

            return this.RedirectToPage($"/{this.AccountModel.Profiletype}Pages/{this.AccountModel.Profiletype}Page");
        }
    }
}