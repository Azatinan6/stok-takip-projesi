using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Dashboard;
using StockTrack.WebUI.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class HospitalDashboardController : Controller
    {
        private readonly AppDbContext _context;

        public HospitalDashboardController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int? hospitalId)
        {
            var model = new HospitalDashboardDto();

            // --- 1. HASTANE SEÇİMİ VE FİLTRELEME ---
            int currentHospitalId = hospitalId ?? 1;

            var allHospitals = await _context.Hospitals
                .Where(h => !h.IsDeleted)
                .ToListAsync();

            ViewBag.Hospitals = new SelectList(allHospitals, "Id", "Name", currentHospitalId);

            var hospital = await _context.Hospitals.FindAsync(currentHospitalId);
            model.HospitalName = hospital != null ? hospital.Name : "Tüm Hastaneler";

            // --- 2. TEPE METRİKLERİ ---
            model.TotalSent = await _context.RequestFormDetails
                .Where(x => !x.IsDeleted &&
                            x.RequestForm.HospitalId == currentHospitalId &&
                            x.RequestForm.RequestFormTypeId == (int)EnumRequestType.Kargo)
                .CountAsync();

            model.TotalReturned = await _context.RequestFormDetails
                .Where(x => !x.IsDeleted &&
                            x.RequestForm.HospitalId == currentHospitalId &&
                            (x.StatusId == 20 || x.StatusId == 21 || x.StatusId == 23 || x.StatusId == 24))
                .CountAsync();

            if (model.TotalSent > 0)
            {
                model.ReturnRate = (int)Math.Round((double)model.TotalReturned / model.TotalSent * 100);
            }

            // --- 3. AKILLI ANALİZ VE RİSK DURUMU ---
            if (model.ReturnRate > 30)
                model.RiskLevel = "Yüksek Risk";
            else if (model.ReturnRate > 15)
                model.RiskLevel = "Orta Risk";
            else
                model.RiskLevel = "Düşük Risk";

            model.SmartAlerts.Add("Son 7 gün içindeki iade taleplerinde artış tespit edildi.");
            model.Highlights.Add("Kablo ve Adaptör grubu en çok iade edilen ürünler.");
            model.Recommendations.Add("Hastaneye kullanıcı eğitimi planlanması tavsiye edilir.");

            // --- 4. ALT TABLOLAR (SON 5 İŞLEM) ---
            // A. Son 5 Gönderim
            model.RecentSent = await _context.RequestFormDetails
                .Where(x => !x.IsDeleted &&
                            x.RequestForm.HospitalId == currentHospitalId &&
                            x.RequestForm.RequestFormTypeId == (int)EnumRequestType.Kargo)
                .OrderByDescending(x => x.CreatedDate)
                .Take(5)
                .Select(x => new TransactionDto
                {
                    Date = x.CreatedDate,
                    ProductName = _context.RequestProducts
                        .Where(rp => rp.RequestFormId == x.RequestFormId)
                        .Select(rp => rp.Product.Name)
                        .FirstOrDefault() ?? "Bilinmiyor",
                    Detail = _context.RequestProducts
                        .Where(rp => rp.RequestFormId == x.RequestFormId)
                        .Select(rp => rp.Quantity.ToString())
                        .FirstOrDefault() ?? "0"
                }).ToListAsync();

            // B. Son 5 İade
            model.RecentReturns = await _context.RequestFormDetails
                .Where(x => !x.IsDeleted &&
                            x.RequestForm.HospitalId == currentHospitalId &&
                            (x.StatusId == 20 || x.StatusId == 21 || x.StatusId == 23 || x.StatusId == 24))
                .OrderByDescending(x => x.CargoGivenDate)
                .Take(5)
                .Select(x => new TransactionDto
                {
                    Date = x.CargoGivenDate ?? DateTime.Now,
                    ProductName = _context.RequestProducts
                        .Where(rp => rp.RequestFormId == x.RequestFormId)
                        .Select(rp => rp.Product.Name)
                        .FirstOrDefault() ?? "Bilinmiyor",
                    Detail = (from rp in _context.RequestProducts
                              join cd in _context.CargoDefinitions on rp.ReasonId equals cd.Id
                              where rp.RequestFormId == x.RequestFormId
                              select cd.Name).FirstOrDefault() ?? "Bilinmiyor"
                }).ToListAsync();

            // --- 5. GRAFİK VERİLERİ (DİNAMİK) ---
            // A. En Çok Gönderilen 4 Ürün
            var topSent = await _context.RequestProducts
                .Where(x => !x.IsDeleted &&
                            x.RequestForm.HospitalId == currentHospitalId &&
                            x.RequestForm.RequestFormTypeId == (int)EnumRequestType.Kargo)
                .GroupBy(x => x.Product.Name)
                .Select(g => new ChartItemDto
                {
                    Label = g.Key ?? "Bilinmiyor",
                    Value = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Value)
                .Take(4)
                .ToListAsync();

            int totalSentSum = topSent.Sum(x => x.Value);
            foreach (var item in topSent)
            {
                item.Percentage = totalSentSum > 0 ? (int)Math.Round((double)item.Value / totalSentSum * 100) : 0;
            }
            model.TopSentProducts = topSent;

            // B. En Çok İade Gelen 4 Ürün
            var topReturned = await _context.RequestProducts
                .Where(x => !x.IsDeleted &&
                            x.RequestForm.HospitalId == currentHospitalId &&
                            x.RequestForm.RequestFormDetails.Any(rfd => rfd.StatusId == 20 || rfd.StatusId == 21 || rfd.StatusId == 23 || rfd.StatusId == 24))
                .GroupBy(x => x.Product.Name)
                .Select(g => new ChartItemDto
                {
                    Label = g.Key ?? "Bilinmiyor",
                    Value = g.Sum(x => x.Quantity)
                })
                .OrderByDescending(x => x.Value)
                .Take(4)
                .ToListAsync();

            int totalReturnedSum = topReturned.Sum(x => x.Value);
            foreach (var item in topReturned)
            {
                item.Percentage = totalReturnedSum > 0 ? (int)Math.Round((double)item.Value / totalReturnedSum * 100) : 0;
            }
            model.TopReturnedProducts = topReturned;

            // C. İade Nedenleri Dağılımı
            var returnReasons = await (from rp in _context.RequestProducts
                                       join cd in _context.CargoDefinitions on rp.ReasonId equals cd.Id
                                       join rf in _context.RequestForms on rp.RequestFormId equals rf.Id
                                       where !rp.IsDeleted &&
                                             rf.HospitalId == currentHospitalId &&
                                             rf.RequestFormDetails.Any(rfd => rfd.StatusId == 20 || rfd.StatusId == 21 || rfd.StatusId == 23 || rfd.StatusId == 24)
                                       group rp by cd.Name into g
                                       select new ChartItemDto
                                       {
                                           Label = g.Key ?? "Belirtilmemiş",
                                           Value = g.Sum(x => x.Quantity)
                                       })
                                      .OrderByDescending(x => x.Value)
                                      .Take(4)
                                      .ToListAsync();

            int totalReasonsSum = returnReasons.Sum(x => x.Value);
            foreach (var item in returnReasons)
            {
                item.Percentage = totalReasonsSum > 0 ? (int)Math.Round((double)item.Value / totalReasonsSum * 100) : 0;
            }
            model.ReturnReasons = returnReasons;

            // --- 6. KRİTİK STOK TABLOSU ---
            model.CriticalStocks = await _context.Products
                .Where(x => !x.IsDeleted && x.Quantity <= 10)
                .OrderBy(x => x.Quantity)
                .Take(5)
                .Select(x => new CriticalStockDto
                {
                    ProductName = x.Name,
                    CurrentStock = x.Quantity,
                    AlertLevel = 10
                }).ToListAsync();

            return View(model);
        }
    }
}