using StockTrack.Dto.Dashboard;

namespace StockTrack.Business.Extension.Dashboard
{
    public interface IDashboardService
    {
        Task<DashboardStatsDto> GetDashboardStatsAsync();
    }
}
