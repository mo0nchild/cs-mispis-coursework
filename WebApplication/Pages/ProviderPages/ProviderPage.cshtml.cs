using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication.Pages
{
    [AuthorizeAttribute(Policy = "Provider")]
    public class ProviderPageModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
