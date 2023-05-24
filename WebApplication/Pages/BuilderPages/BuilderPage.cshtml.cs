using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication.Pages
{
    [AuthorizeAttribute(Policy = "Builder")]
    public class BuilderPageModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
