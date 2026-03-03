using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Dashboard;

namespace StockTrack.Business.Extension.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly AppDbContext _appDbContext;

        public DashboardService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            // Temel istatistikler
            var totalProducts = await _appDbContext.Products.CountAsync(p => !p.IsDeleted);
            var activeProducts = await _appDbContext.Products.CountAsync(p => !p.IsDeleted && p.IsActive);
            var categoryCount = await _appDbContext.Categories.CountAsync(c => !c.IsDeleted);
            var locationCount = await _appDbContext.LocationLists.CountAsync(l => !l.IsDeleted);
            var activeUserCount = await _appDbContext.Users.CountAsync(u => !u.IsDeleted);
            var totalMainRepoCount = await _appDbContext.MainRepoLocations.CountAsync(m => m.IsActive && !m.IsDeleted);


            // DTO ile istatistikleri döndür
            return new DashboardStatsDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                CategoryCount = categoryCount,
                LocationCount = locationCount,
                ActiveUserCount = activeUserCount,
                TotalLocationCount = locationCount, 
                TotalCategories = categoryCount,    
                TotalMainRepoCount = totalMainRepoCount
            };
        }



    }
}

