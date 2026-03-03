using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.WebUI.Enums;
using StockTrack.WebUI.Models;

namespace StockTrack.WebUI.ViewComponents
{
    public class CargoMenuViewComponent : ViewComponent
    {
        private readonly AppDbContext _appDbContext;

        public CargoMenuViewComponent(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
           
            var statusCounts = await
                (from d in _appDbContext.RequestFormDetails.AsNoTracking()
                 join f in _appDbContext.RequestForms.AsNoTracking() on d.RequestFormId equals f.Id
                 where d.IsActive && !d.IsDeleted && f.IsActive && !f.IsDeleted && f.RequestFormTypeId == (int)EnumRequestType.Kargo
                 group d by d.StatusId into g
                 select new { StatusId = g.Key, Count = g.Count() }).ToListAsync();

            int GetCount(EnumStatusType s) => statusCounts.FirstOrDefault(x => x.StatusId == (int)s)?.Count ?? 0;

            var model = new CargoMenuCountsVM
            {
                Requested = GetCount(EnumStatusType.Talep),
                AwaitingApproval = GetCount(EnumStatusType.OnayBekliyor),
                ReadyForCargo = GetCount(EnumStatusType.Paketlendi),
                InTransit = GetCount(EnumStatusType.Kargoda),   
            };

            return View(model);
        }

    }
}
