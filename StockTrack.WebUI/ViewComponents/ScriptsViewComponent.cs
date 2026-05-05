using Microsoft.AspNetCore.Mvc;

namespace StockTrack.WebUI.ViewComponents
{
    public class ScriptsViewComponent: ViewComponent
    {
        public async Task<IViewComponentResult> InvokeAsync()
        {
            return View();
        }
    }
}
