using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Text.RegularExpressions;
using WebApplication.Filters;
using WebApplication.Models;
using WebApplication.Services;

namespace WebApplication.Pages.ClientPages
{
    [AuthorizeAttribute(Policy = "Client"), ProfileFilterAttribute("ClientSettings")]
    public class ClientSettingsModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<ClientPage> Logger = default!;
        public ClientSettingsModel(ILogger<ClientPage> logger, IDbContextFactory<MispisCourseworkContext> factory) 
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute]
        public Client ClientModel { get; set; } = default!;

        [BindPropertyAttribute]
        public Account Account { get => this.ClientModel.Account; set => this.ClientModel.Account = value; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;

        public async virtual Task<IActionResult> OnGetAsync()
        {
            var profileId = this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid);
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                this.ClientModel = (await dbcontext.Clients.Where(item => item.Accountid == int.Parse(profileId))
                    .Include(item => item.Payments).Include(item => item.Account).FirstOrDefaultAsync())!;

                if (this.ClientModel == null) return base.BadRequest("Данные невозможно загрузить");
            }
            await Console.Out.WriteLineAsync($"\nemial: {this.ClientModel.Emailaddress}");

            return this.Page();
        }
        public async virtual Task<IActionResult> OnPostAsync([FromServices] IProfileService profileService)
        {
            var validateEmail = this.ClientModel.Emailaddress != null && Regex.IsMatch(this.ClientModel.Emailaddress, 
                @"^\w{6,}@(mail|gmail|yandex){1}.(ru|com){1}$");
            if (!validateEmail) { this.ErrorMessage = "Неверный формат email"; return await this.OnGetAsync(); }

            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            var errorMessage = await profileService.UpdateAccount(this.ClientModel.Account, profileId);

            if(errorMessage != null) return this.BadRequest(errorMessage.ErrorMessage);
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Clients.Where(item => item.Accountid == profileId).ExecuteUpdateAsync(
                    item => item.SetProperty(p => p.Emailaddress, p => this.ClientModel.Emailaddress));
            }
            return await this.OnGetAsync();
        }
        public async virtual Task<IActionResult> OnPostPaymentAsync(Payment paymentModel)
        {
            var cardNumberValidation = paymentModel.Bankaccount != null 
                && Regex.IsMatch(paymentModel.Bankaccount, "^[0-9]{4}-[0-9]{4}-[0-9]{4}-[0-9]{4}$");

            if (!cardNumberValidation || (paymentModel.Cvv != null && paymentModel.Cvv.Length != 3))
            {
                this.ErrorMessage = "Неверный формат данных банковкой карты";
                return await this.OnGetAsync();
            }
            var profileId = int.Parse(this.HttpContext.User.FindFirstValue(ClaimTypes.PrimarySid));
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Payments.AddRangeAsync(paymentModel);

                paymentModel.Client = await dbcontext.Clients.FirstAsync(item => item.Accountid == profileId);
                await dbcontext.SaveChangesAsync();
            }
            return await this.OnGetAsync();
        }
        public async virtual Task<IActionResult> OnGetPaymentRemoveAsync([FromQuery] int paymentId)
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {
                await dbcontext.Payments.Where(item => item.Paymentid == paymentId).ExecuteDeleteAsync();
            }
            return await this.OnGetAsync();
        }
    }
}
