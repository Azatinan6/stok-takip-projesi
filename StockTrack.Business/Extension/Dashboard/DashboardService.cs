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

            // ESKİ: LocationLists üzerinden sayıyordu
            // YENİ: Artık hastaneleri (Hospitals) sayıyoruz
            var hospitalCount = await _appDbContext.Hospitals.CountAsync(h => !h.IsDeleted);

            var activeUserCount = await _appDbContext.Users.CountAsync(u => !u.IsDeleted);
            var totalMainRepoCount = await _appDbContext.MainRepoLocations.CountAsync(m => m.IsActive && !m.IsDeleted);

            // DTO ile istatistikleri döndür
            return new DashboardStatsDto
            {
                TotalProducts = totalProducts,
                ActiveProducts = activeProducts,
                CategoryCount = categoryCount,

                // Dashboard ekranında 'Lokasyon Sayısı' yazan yere 'Hastane Sayısı' gelecek
                LocationCount = hospitalCount,
                TotalLocationCount = hospitalCount,

                ActiveUserCount = activeUserCount,
                TotalCategories = categoryCount,
                TotalMainRepoCount = totalMainRepoCount
            };
        }
    }
}