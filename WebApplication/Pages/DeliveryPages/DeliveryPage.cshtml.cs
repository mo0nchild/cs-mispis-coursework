using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebApplication.Pages.DeliveryPages
{
    [Authorize(Policy = "Delivery")]
    public class DeliveryPageModel : PageModel
    {
        public void OnGet()
        {
        }
    }
}
