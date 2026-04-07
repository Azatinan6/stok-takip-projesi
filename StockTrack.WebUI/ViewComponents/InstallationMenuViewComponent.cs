using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.WebUI.Enums;
using StockTrack.WebUI.Models;

namespace StockTrack.WebUI.ViewComponents
{

    public class InstallationMenuViewComponent : ViewComponent
    {
        private readonly AppDbContext _appDbContext;

        public InstallationMenuViewComponent(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {

            var statusCounts = await (from rfd in _appDbContext.RequestFormDetails.AsNoTracking()
                                      join rf in _appDbContext.RequestForms.AsNoTracking() on rfd.RequestFormId equals rf.Id
                                      where rfd.IsActive && !rfd.IsDeleted && rf.IsActive && !rf.IsDeleted && rf.RequestFormTypeId == (int)EnumRequestType.Kurulum
                                      group rfd by rfd.StatusId into g
                                      select new { StatusId = g.Key, Count = g.Count() }).ToListAsync();

            int GetCount(EnumStatusType s) => statusCounts.FirstOrDefault(x => x.StatusId == (int)s)?.Count ?? 0;

            var model = new InstallationMenuCountsVM
            {
                Pending = GetCount(EnumStatusType.OnayBekliyor),
                Ready = GetCount(EnumStatusType.KurulumBekliyor)
            };

            return View(model);
        }
    }
}
