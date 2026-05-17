using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using System.Threading.Tasks;
using System.Linq;

namespace StockTrack.WebUI.ViewComponents
{
    public class GlobalFilterBarViewComponent : ViewComponent
    {
        private readonly AppDbContext _appDbContext;

        public GlobalFilterBarViewComponent(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Veritabanından silinmemiş hastane ve lokasyonları çeker
            ViewBag.FilterHospitals = new SelectList(await _appDbContext.Hospitals
                .Where(h => !h.IsDeleted)
                .OrderBy(h => h.Name)
                .ToListAsync(), "Id", "Name");

            // Veritabanından silinmemiş kategorileri çeker
            ViewBag.FilterCategories = new SelectList(await _appDbContext.Categories
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.Name)
                .ToListAsync(), "Id", "Name");

            return View();
        }
    }
}