using Microsoft.AspNetCore.Mvc;

namespace StockTrack.WebUI.ViewComponents
{
    public class HeadViewComponent: ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
