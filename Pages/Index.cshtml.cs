using Microsoft.AspNetCore.Mvc.RazorPages;

namespace GanjoorRandomPoemService.Pages
{
    public class IndexModel : PageModel
    {
        public void OnGet()
        {
            ViewData["Title"] = "خانه";
            ViewData["Url"] = "/";
        }
    }
}
