using ClosedXML.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Excel;
using StockTrack.WebUI.Enums;
using System.Data;

namespace StockTrack.WebUI.Controllers
{
    public class ProductExcelController : Controller
    {
        private readonly AppDbContext _appDbContext;

        public ProductExcelController(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ExportProductInExcel(DateTime startDate, DateTime endDate)
        {

            var data = (from rf in _appDbContext.RequestForms
                        join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                        join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                        join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                        join rp in _appDbContext.RequestProducts on rf.Id equals rp.RequestFormId
                        join p in _appDbContext.Products on rp.ProductId equals p.Id
                        join c in _appDbContext.Categories on p.CategoryId equals c.Id                       
                        where rfd.CompletedDate >= startDate && rfd.CompletedDate <= endDate && !rfd.IsDeleted && rfd.IsActive && rfd.StatusId == (int)EnumStatusType.Tamamlandı

                        select new ProductExcelDto
                        {
                            RequestedBy = rfd.RequestBy,
                            RequestDate = rfd.RequestDate.ToString(),
                            MainDepo = m.Name,
                            Location = l.Name,
                            Address = l.Address,
                            ReciverName = rfd.ToPerson,
                            TrackingNumber = rfd.TrackingNumber,
                            CargoCompany = _appDbContext.CargoNames.Where(x => x.Id == rfd.CargoNameId).Select(x => x.Name).FirstOrDefault(),
                            PackedDate = rfd.PackingDate.ToString(),
                            CargoGivenDate = rfd.CargoGivenDate.ToString(),
                            CompletedCargoDate = rfd.CompletedDate.ToString(),
                            InstallationDate = rfd.InstallationDate.ToString(),
                            InstallationNote = rfd.Description,
                            Status = rfd.StatusId.ToString(),
                            RequestTypeId = rf.RequestFormTypeId,
                            // İlgili kişi verisini ayrı bir sorgu ile çekiyoruz
                            RelevantPerson = _appDbContext.PersonDetails.Where(x => x.RequestFormDetailId == rfd.Id).Select(x => x.AppUser.NameSurname).FirstOrDefault(),
                            CategoryName = c.Name,
                            ProductName = p.Name,
                            Quantitiy = rp.Quantity,

                        }).OrderByDescending(x => x.RequestTypeId == (int)EnumRequestType.Kargo).ToList();
            if (data == null || !data.Any())
            {
                TempData["NoDataMessage"] = $"{startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy} tarihleri arasında kayıt bulunamadı.";
                return RedirectToAction("Index", "ProductExcel"); // formun olduğu sayfaya yönlendir
            }

            var fileName = $"{startDate:dd.MM.yyyy}-{endDate:dd.MM.yyyy}.xlsx";
            return GenerateExcel(fileName, data);
        }
        
        private FileResult GenerateExcel(string fileName, IEnumerable<ProductExcelDto> products)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Ürünler");

            // Başlık satırı
            worksheet.Cell(1, 1).Value = "Id"; // Yeni eklenen sütun
            worksheet.Cell(1, 2).Value = "Talep Eden";
            worksheet.Cell(1, 3).Value = "Talep Tarihi";
            worksheet.Cell(1, 4).Value = "Depo";
            worksheet.Cell(1, 5).Value = "Kategori Adları";
            worksheet.Cell(1, 6).Value = "Ürün Adları";
            worksheet.Cell(1, 7).Value = "Adetler";
            worksheet.Cell(1, 8).Value = "Lokasyon";
            worksheet.Cell(1, 9).Value = "Adres";
            worksheet.Cell(1, 10).Value = "Alıcı Ad Soyad";
            worksheet.Cell(1, 11).Value = "İlgili Personel";
            worksheet.Cell(1, 12).Value = "Takip Numarası";
            worksheet.Cell(1, 13).Value = "Kargo Şirketi";
            worksheet.Cell(1, 14).Value = "Paketlenme Tarihi";
            worksheet.Cell(1, 15).Value = "Kargoya Verilme";
            worksheet.Cell(1, 16).Value = "Teslim Tarihi";
            worksheet.Cell(1, 17).Value = "Kurulum Tarihi";
            worksheet.Cell(1, 18).Value = "Kurulum Notu";
            worksheet.Cell(1, 19).Value = "Durum";

            // Veri satırları
            int row = 2;
            foreach (var p in products)
            {
                worksheet.Cell(row, 1).Value = p.RequestTypeId;
                worksheet.Cell(row, 2).Value = p.RequestedBy;
                worksheet.Cell(row, 3).Value = p.RequestDate;
                worksheet.Cell(row, 4).Value = p.MainDepo;
                worksheet.Cell(row, 5).Value = p.CategoryName;
                worksheet.Cell(row, 6).Value = p.ProductName;
                worksheet.Cell(row, 7).Value = p.Quantitiy;
                worksheet.Cell(row, 8).Value = p.Location;
                worksheet.Cell(row, 9).Value = p.Address;
                worksheet.Cell(row, 10).Value = p.ReciverName;
                worksheet.Cell(row, 11).Value = p.RelevantPerson;
                worksheet.Cell(row, 12).Value = p.TrackingNumber;
                worksheet.Cell(row, 13).Value = p.CargoCompany;
                worksheet.Cell(row, 14).Value = p.PackedDate;
                worksheet.Cell(row, 15).Value = p.CargoGivenDate;
                worksheet.Cell(row, 16).Value = p.CompletedCargoDate;
                worksheet.Cell(row, 17).Value = p.InstallationDate;
                worksheet.Cell(row, 18).Value = p.InstallationNote;
                worksheet.Cell(row, 19).Value = "Tamamlandı";
                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }


    }

}