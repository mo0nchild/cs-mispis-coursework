using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace WebApplication.Pages.ManagerPages
{
    [AuthorizeAttribute(Policy = "Manager")]
    public partial class ManagerWorkersModel : PageModel
    {
        protected IDbContextFactory<MispisCourseworkContext> DatabaseFactory { get; set; } = default!;

        protected readonly ILogger<ManagerWorkersModel> Logger = default!;
        public ManagerWorkersModel(ILogger<ManagerWorkersModel> logger, IDbContextFactory<MispisCourseworkContext> factory)
            : base() { this.Logger = logger; this.DatabaseFactory = factory; }

        [BindPropertyAttribute(SupportsGet = true)]
        public string? ErrorMessage { get; set; } = default;
        public virtual async Task<IActionResult> OnGetAsync()
        {
            using (var dbcontext = await this.DatabaseFactory.CreateDbContextAsync())
            {

            }
            return this.Page();
        }
    }
}
