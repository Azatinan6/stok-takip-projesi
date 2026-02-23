using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTrack.Business.Extension.Dashboard;
using StockTrack.DataAccess.Context;
using StockTrack.WebUI.Enums;
using System.Threading.Tasks;

namespace StockTrack.WebUI.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;
        private readonly AppDbContext _appDbContext;
        public DashboardController(IDashboardService dashboardService, AppDbContext appDbContext)
        {
            _dashboardService = dashboardService;
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
        
            var model = await _dashboardService.GetDashboardStatsAsync();

            //kargo işlemleri(Talep edildi,onay bekliyor,paketlendi,kargoda,iptal edildi)

            var allCounts = await (from rf in _appDbContext.RequestForms
                                   where !rf.IsDeleted && rf.IsActive
                                   join rfd in _appDbContext.RequestFormDetails.Where(x => !x.IsDeleted && x.IsActive) on rf.Id equals rfd.RequestFormId
                                   group new { rf, rfd } by new { rf.RequestFormTypeId, rfd.StatusId } into g
                                   select new
                                   {
                                       RequestType = g.Key.RequestFormTypeId,
                                       Status = g.Key.StatusId,
                                       Count = g.Count()
                                   }).ToListAsync();

            // Bu veriden kargo statü sayılarını al
            model.TotalAwaitingApprovalCargo = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kargo && x.Status == (int)EnumStatusType.OnayBekliyor)?.Count ?? 0;
            model.TotalPackingCargo = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kargo && x.Status == (int)EnumStatusType.Paketlendi)?.Count ?? 0;
            model.TotalTransportationCargo = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kargo && x.Status == (int)EnumStatusType.Kargoda)?.Count ?? 0;
            model.TotalCompletedCargo = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kargo && x.Status == (int)EnumStatusType.Tamamlandı)?.Count ?? 0;
            model.TotalCanceledCargo = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kargo && x.Status == (int)EnumStatusType.İptal)?.Count ?? 0;
            model.TotalCargo = allCounts.Where(x => x.RequestType == (int)EnumRequestType.Kargo).Sum(x => x.Count);


            // Kurulum statü sayılarını al
            model.TotalInstillation = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kurulum)?.Count ?? 0;
            model.InstallationPending = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kurulum && x.Status == (int)EnumStatusType.OnayBekliyor)?.Count ?? 0;
            model.InstallationReady = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kurulum && x.Status == (int)EnumStatusType.KurulumBekliyor)?.Count ?? 0;
            model.InstallationCompleted = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kurulum && x.Status == (int)EnumStatusType.Tamamlandı)?.Count ?? 0;
            model.InstallationCancelled = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Kurulum && x.Status == (int)EnumStatusType.İptal)?.Count ?? 0;

            //Servis toplamı
            model.ServiceCompleted = allCounts.FirstOrDefault(x => x.RequestType == (int)EnumRequestType.Servis && x.Status == (int)EnumStatusType.Tamamlandı)?.Count ?? 0;


            //Talepler toplamı
            model.TotalRequested = allCounts.FirstOrDefault(x=>x.Status == (int)EnumStatusType.Talep)?.Count ?? 0;
            return View(model);
        }



        // 2) Ürün Bazlı Stok Durumu
        [HttpGet]
        public IActionResult GetProductStockData()
        {
            var data = _appDbContext.Products
                .Where(p => !p.IsDeleted)
                .Select(p => new
                {
                    label = p.Name,
                    totalStock = _appDbContext.ProductMainRepoLocations.Where(pml => pml.ProductId == p.Id).Sum(pml => pml.Quantity),
                    distributedStock = ( from rp in _appDbContext.RequestProducts
                                         join rf in _appDbContext.RequestFormDetails on rp.RequestFormId equals rf.RequestFormId
                                         where rp.ProductId == p.Id && !rf.IsDeleted && rf.StatusId == (int)EnumStatusType.Tamamlandı select (int?)rp.Quantity).Sum() ?? 0 })
                .Select(x => new
                {
                    label = x.label,
                    totalStock = x.totalStock,
                    distributedStock = x.distributedStock,
                    remainingStock = x.totalStock - x.distributedStock
                }).ToList();

            return Json(data);
        }

        // 3) Kritik Stok Uyarıları
        public async Task<IActionResult> GetCriticalStockData()
        {
            var criticalStockItems = await (from p in _appDbContext.Products
                                            where !p.IsDeleted && p.IsActive && p.WarningThreshold.HasValue
                                            join pml in _appDbContext.ProductMainRepoLocations
                                                .Where(x => x.MainRepoLocation.IsActive && !x.MainRepoLocation.IsDeleted)
                                                on p.Id equals pml.ProductId
                                            join rp in _appDbContext.RequestProducts
                                                .Where(x => x.RequestForm.IsActive && !x.RequestForm.IsDeleted)
                                                on p.Id equals rp.ProductId
                                            join rpd in _appDbContext.RequestFormDetails.Where(x => x.StatusId == (int)EnumStatusType.Tamamlandı)
                                                on rp.RequestFormId equals rpd.RequestFormId
                                            join m in _appDbContext.MainRepoLocations on pml.MainRepoLocationId equals m.Id
                                            group new { p, pml, rp, m } by new
                                            {
                                                p.Id,
                                                p.Name,
                                                p.Model,
                                                p.WarningThreshold,
                                                CategoryName = p.Category.Name,
                                                MainRepoName = m.Name   
                                            } into g

                                            select new
                                            {
                                                MainRepoName = g.Key.MainRepoName,
                                                ProductId = g.Key.Id,
                                                ProductName = g.Key.Model + " - " + g.Key.Name,
                                                CategoryName = g.Key.CategoryName,
                                                WarningThreshold = g.Key.WarningThreshold,
                                                GroupData = g.ToList()
                                            })
                                            .ToListAsync();

            // Bellekte hesaplamalar
            var result = criticalStockItems
                .Select(x => {
                    var totalStock = x.GroupData.Sum(item => item.pml.Quantity);
                    var distributedStock = x.GroupData.Sum(item => item.rp.Quantity);
                    var currentStock = totalStock - distributedStock;

                    return new
                    {
                        x.MainRepoName,   // ✅ Sonuçlara depo adını da ekledik
                        x.ProductName,
                        x.CategoryName,
                        CurrentStock = currentStock,
                        Threshold = x.WarningThreshold,
                        StockStatus = currentStock == 0 ? "Stok Tükendi" :
                                    (currentStock <= x.WarningThreshold ? "Düşük Stok" : "Stok Yeterli"),
                        Color = currentStock == 0 ? "#dc3545" :
                                (currentStock <= x.WarningThreshold ? "#fd7e14" : "#28a745")
                    };
                })
                .Where(x => x.CurrentStock <= x.Threshold) // Sadece kritik stokları filtrele
                .Distinct()
                .ToList();

            return Json(result);
        }



    }
}
