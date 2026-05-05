using DocumentFormat.OpenXml.Math;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.CargoDetail;
using StockTrack.Dto.RequestForm;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Enums;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class CargoDetailController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;

        public CargoDetailController(AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        private async Task SetCargoCountsAsync()
        {
            //Sadece "Kargo" tipinde olan ve "Silinmemiş" kayıtları ana sorgu olarak alıyoruz.
            var baseQuery = _appDbContext.RequestFormDetails
                .Include(x => x.RequestForm)
                .Where(x => x.RequestForm.RequestFormTypeId == (int)EnumRequestType.Kargo && !x.IsDeleted);

            // 2. ADIM: Sayıları hesaplayıp çantaya (ViewBag) atıyoruz.
            ViewBag.AllCount = await baseQuery.CountAsync();

            ViewBag.PendingCount = await baseQuery.CountAsync(x => x.StatusId == (int)EnumStatusType.OnayBekliyor);
            ViewBag.PackagedCount = await baseQuery.CountAsync(x => x.StatusId == (int)EnumStatusType.Paketlendi);
            ViewBag.InCargoCount = await baseQuery.CountAsync(x => x.StatusId == (int)EnumStatusType.Kargoda);
            ViewBag.DeliveredCount = await baseQuery.CountAsync(x => x.StatusId == (int)EnumStatusType.Tamamlandı);
            ViewBag.CancelledCount = await baseQuery.CountAsync(x => x.StatusId == (int)EnumStatusType.İptal);
            
            ViewBag.ReturnWaitingCount = await baseQuery.CountAsync(x => x.StatusId == (int)EnumStatusType.IadeGeldigindeKargolanacak);
            ViewBag.OfficePickupCount = await baseQuery.CountAsync(x => x.StatusId == (int)EnumStatusType.OfistenTeslimAlinacak);
            // 3. ADIM: Silinenler sekmesi için (IsDeleted == true) ayrı bir sayım yapıyoruz.
            ViewBag.DeletedCount = await _appDbContext.RequestFormDetails
                .Include(x => x.RequestForm)
                .Where(x => x.RequestForm.RequestFormTypeId == (int)EnumRequestType.Kargo && x.IsDeleted)
                .CountAsync();
        }

        // Tüm Kargolar (Ana Sayfa)
        public async Task<IActionResult> Index()
        {
            await SetCargoCountsAsync();

            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");

            // URL'DEN PARAMETREYİ ZORLA (GARANTİLİ) OKUMA YÖNTEMİ
            string status = Request.Query["status"].ToString();

            // 1. AŞAMA: TEMEL SORGUMUZU OLUŞTURUYORUZ
            var baseQuery = _appDbContext.RequestFormDetails
                .Include(x => x.RequestForm)
                .Where(x => x.RequestForm.RequestFormTypeId == (int)EnumRequestType.Kargo && !x.IsDeleted)
                .AsQueryable();

            // 2. AŞAMA: TÜRKÇE KARAKTER TUZAĞI ÇÖZÜLMÜŞ FİLTRE!
            if (!string.IsNullOrEmpty(status))
            {
                // ToLower() sildik, uluslararası harf duyarsızlaştırma ekledik!
                if (status.Equals("IadeBekleyen", StringComparison.OrdinalIgnoreCase))
                {
                    baseQuery = baseQuery.Where(x => x.StatusId == (int)EnumStatusType.IadeGeldigindeKargolanacak);
                }
                else if (status.Equals("OfistenTeslim", StringComparison.OrdinalIgnoreCase))
                {
                    baseQuery = baseQuery.Where(x => x.StatusId == (int)EnumStatusType.OfistenTeslimAlinacak);
                }
            }

            // 3. AŞAMA: VERİYİ BİRLEŞTİR (Sadece filtrelenmiş veriler SQL'e gidecek)
            var resultAllCargo = await (from rfd in baseQuery
                                        join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                        join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id into repoGroup
                                        from mrl in repoGroup.DefaultIfEmpty()
                                        join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                        from h in hospitalGroup.DefaultIfEmpty()
                                        join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id
                                        
                                        select new ResultAwitingApprovalDto 
                                        {
                                            Id = rfd.Id,
                                            StatusId = rfd.StatusId,
                                            StatusName = st.Name,
                                            ReceiverFullName = rfd.ToPerson,
                                            Phone = rfd.Phone,
                                            HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                            HospitalAddress = h != null ? h.Address : "-",
                                            RequestFormRequestedBy = rfd.RequestBy,
                                            RequestFormRequestedDate = rfd.RequestDate,
                                            CargoGivenDate = rfd.CargoGivenDate,
                                            IsOfficeDelivery = rf.IsOfficeDelivery,
                                            TrackingNumber = rfd.TrackingNumber,
                                            MainRepoName = mrl != null ? mrl.Name : "Bilinmiyor",
                                            Label = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                            SendReason = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ReasonId != null).Select(x => x.ReasonId.ToString()).FirstOrDefault(),
                                            ProductCondition = rfd.ProductCondition,
                                            Note = rfd.Description,
                                            SerialNumber = rfd.SerialNumber,
                                            EthMac = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null).Select(x => x.EthMacAddress).FirstOrDefault(),
                                            WlanMac = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null).Select(x => x.WlanMacAddress).FirstOrDefault(),
                                            ConnectionType = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null).Select(x => x.ConnectionType).FirstOrDefault(),
                                            ConfigUrl = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null).Select(x => x.ConfigUrl).FirstOrDefault(),
                                            Products = (from rp in _appDbContext.RequestProducts
                                                        join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                        join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                        where rp.RequestFormId == rf.Id
                                                        select new ProductDetailDto
                                                        {
                                                            CategoryName = c.Name,
                                                            ProductName = p.Name,
                                                            ImageUrl = p.PhotoUrl,
                                                            Quantity = rp.Quantity
                                                        }).ToList()
                                        }).ToListAsync();

            return View(resultAllCargo);
        }

        // Iade geldiğinde Kargoya Verilecek 
        [HttpGet]
        public async Task<IActionResult> ReturnWaiting()
        {
            await SetCargoCountsAsync();

            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");

            var resultReturnWaiting = (from rfd in _appDbContext.RequestFormDetails
                                       join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                       join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id into repoGroup
                                       from mrl in repoGroup.DefaultIfEmpty()
                                       join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                       from h in hospitalGroup.DefaultIfEmpty()
                                       
                                       // ---> LEFT JOIN ZIRHI <---
                                       join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id into statusGroup
                                       from st in statusGroup.DefaultIfEmpty()
                                       
                                       where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                       where rfd.StatusId == (int)EnumStatusType.IadeGeldigindeKargolanacak && !rfd.IsDeleted
                                       select new ResultAwitingApprovalDto
                                       {
                                           Id = rfd.Id,
                                           StatusId = rfd.StatusId,
                                           // Veritabanında yoksa Enum'dan bas
                                           StatusName = st != null ? st.Name : "İade Geldiği Zaman Kargolanacak",
                                           ReceiverFullName = rfd.ToPerson,
                                           Phone = rfd.Phone,
                                           HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                           HospitalAddress = h != null ? h.Address : "-",
                                           RequestFormRequestedBy = rfd.RequestBy,
                                           RequestFormRequestedDate = rfd.RequestDate,
                                           CargoGivenDate = rfd.CargoGivenDate,
                                           IsOfficeDelivery = rf.IsOfficeDelivery,
                                           TrackingNumber = rfd.TrackingNumber,
                                           MainRepoName = mrl != null ? mrl.Name : "Bilinmiyor",
                                           Label = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                           SendReason = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ReasonId != null).Select(x => x.ReasonId.ToString()).FirstOrDefault(),
                                           ProductCondition = rfd.ProductCondition,
                                           Note = rfd.Description,
                                           SerialNumber = rfd.SerialNumber,
                                           EthMac = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null).Select(x => x.EthMacAddress).FirstOrDefault(),
                                           WlanMac = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null).Select(x => x.WlanMacAddress).FirstOrDefault(),
                                           ConnectionType = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null).Select(x => x.ConnectionType).FirstOrDefault(),
                                           ConfigUrl = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null).Select(x => x.ConfigUrl).FirstOrDefault(),
                                           Products = (from rp in _appDbContext.RequestProducts
                                                       join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                       join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                       where rp.RequestFormId == rf.Id
                                                       select new ProductDetailDto
                                                       {
                                                           CategoryName = c.Name,
                                                           ProductName = p.Name,
                                                           ImageUrl = p.PhotoUrl,
                                                           Quantity = rp.Quantity
                                                       }).ToList()
                                       }).ToList();

            return View(resultReturnWaiting);
        }

        // Ofisten Teslim Alınacak Kargolar
        [HttpGet]
        public async Task<IActionResult> OfficePickup()
        {
            await SetCargoCountsAsync();

            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");

            var resultOfficePickup = (from rfd in _appDbContext.RequestFormDetails
                                      join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                      join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id into repoGroup
                                      from mrl in repoGroup.DefaultIfEmpty()
                                      join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                      from h in hospitalGroup.DefaultIfEmpty()
                                      
                                      // ---> İŞTE HAYAT KURTARAN LEFT JOIN ZIRHI <---
                                      join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id into statusGroup
                                      from st in statusGroup.DefaultIfEmpty()
                                      
                                      where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                      where rfd.StatusId == (int)EnumStatusType.OfistenTeslimAlinacak && !rfd.IsDeleted
                                      select new ResultAwitingApprovalDto
                                      {
                                          Id = rfd.Id,
                                          StatusId = rfd.StatusId,
                                          // Veritabanında yoksa bile Enum'dan ismini basıyoruz:
                                          StatusName = st != null ? st.Name : "Ofisten Teslim Alınacak",
                                          ReceiverFullName = rfd.ToPerson,
                                          Phone = rfd.Phone,
                                          HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                          HospitalAddress = h != null ? h.Address : "-",
                                          RequestFormRequestedBy = rfd.RequestBy,
                                          RequestFormRequestedDate = rfd.RequestDate,
                                          CargoGivenDate = rfd.CargoGivenDate,
                                          IsOfficeDelivery = rf.IsOfficeDelivery,
                                          TrackingNumber = rfd.TrackingNumber,
                                          MainRepoName = mrl != null ? mrl.Name : "Bilinmiyor",
                                          Label = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                          SendReason = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ReasonId != null).Select(x => x.ReasonId.ToString()).FirstOrDefault(),
                                          ProductCondition = rfd.ProductCondition,
                                          Note = rfd.Description,
                                          SerialNumber = rfd.SerialNumber,
                                          EthMac = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null).Select(x => x.EthMacAddress).FirstOrDefault(),
                                          WlanMac = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null).Select(x => x.WlanMacAddress).FirstOrDefault(),
                                          ConnectionType = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null).Select(x => x.ConnectionType).FirstOrDefault(),
                                          ConfigUrl = _appDbContext.RequestProducts.Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null).Select(x => x.ConfigUrl).FirstOrDefault(),
                                          Products = (from rp in _appDbContext.RequestProducts
                                                      join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                      join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                      where rp.RequestFormId == rf.Id
                                                      select new ProductDetailDto
                                                      {
                                                          CategoryName = c.Name,
                                                          ProductName = p.Name,
                                                          ImageUrl = p.PhotoUrl,
                                                          Quantity = rp.Quantity
                                                      }).ToList()
                                      }).ToList();

            return View(resultOfficePickup);
        }

        //Onay Bekleyen kargolar
        public async Task<IActionResult> AwaitingApproval()
        {
            await SetCargoCountsAsync();

            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");

            var resultAwaitingApprovals = (from rfd in _appDbContext.RequestFormDetails
                                           join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                           join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id into repoGroup
                                           from mrl in repoGroup.DefaultIfEmpty()
                                           join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                           from h in hospitalGroup.DefaultIfEmpty()
                                           join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id
                                           where rfd.StatusId == (int)EnumStatusType.OnayBekliyor && !rfd.IsDeleted
                                           where rf.RequestFormTypeId == (int)EnumRequestType.Kargo

                                           select new ResultAwitingApprovalDto
                                           {
                                               Id = rfd.Id,
                                               StatusId = rfd.StatusId,
                                               StatusName = st.Name,
                                               ReceiverFullName = rfd.ToPerson,
                                               Phone = rfd.Phone,
                                               HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                               HospitalAddress = h != null ? h.Address : "-",
                                               RequestFormRequestedBy = rfd.RequestBy,
                                               RequestFormRequestedDate = rfd.RequestDate,
                                               CargoGivenDate = rfd.CargoGivenDate,
                                               IsOfficeDelivery = rf.IsOfficeDelivery,
                                               TrackingNumber = rfd.TrackingNumber,
                                               MainRepoName = mrl != null ? mrl.Name : "Bilinmiyor",
                                               Label = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                               SendReason = (from rp in _appDbContext.RequestProducts
                                                             join cd in _appDbContext.CargoDefinitions on rp.ReasonId equals cd.Id
                                                             where rp.RequestFormId == rf.Id
                                                             select cd.Name).FirstOrDefault(),
                                               ProductCondition = (from rp in _appDbContext.RequestProducts
                                                                   join cd in _appDbContext.CargoDefinitions on rp.ProductStatusId equals cd.Id
                                                                   where rp.RequestFormId == rf.Id
                                                                   select cd.Name).FirstOrDefault(),
                                               Note = rfd.Description,
                                               SerialNumber = rfd.SerialNumber,
                                               EthMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null)
                                                        .Select(x => x.EthMacAddress)
                                                        .FirstOrDefault(),
                                               WlanMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null)
                                                        .Select(x => x.WlanMacAddress)
                                                        .FirstOrDefault(),
                                               ConnectionType = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null)
                                                        .Select(x => x.ConnectionType)
                                                        .FirstOrDefault(),
                                               ConfigUrl = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null)
                                                        .Select(x => x.ConfigUrl)
                                                        .FirstOrDefault(),
                                               Products = (from rp in _appDbContext.RequestProducts
                                                           join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                           join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                           where rp.RequestFormId == rf.Id
                                                           select new ProductDetailDto
                                                           {
                                                               CategoryName = c.Name,
                                                               ProductName = p.Name,
                                                               ImageUrl = p.PhotoUrl, // Fotoğraf yoksa placeholder
                                                               Quantity = rp.Quantity
                                                           }).ToList()
                                           }).ToList();
            return View(resultAwaitingApprovals);

        }

        //Paketlenmiş kargoya hazır ürünler
        public async Task<IActionResult> ReadyForCargo()
        {
            await SetCargoCountsAsync();

            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            // Paketlenmiş kargo taleplerini konum, ürün ve diğer detaylarla birlikte listeliyor
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");
            var resultCargoForReadyDtos = (from rfd in _appDbContext.RequestFormDetails
                                           join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                           join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id into repoGroup
                                           from mrl in repoGroup.DefaultIfEmpty()
                                           join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                           from h in hospitalGroup.DefaultIfEmpty()
                                           join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id
                                           where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                           where rfd.StatusId == (int)EnumStatusType.Paketlendi && !rfd.IsDeleted
                                           select new ResultCargoForReadyDto
                                           {
                                               Id = rfd.Id,
                                               StatusId = rfd.StatusId,
                                               StatusName = st.Name,
                                               ReceiverFullName = rfd.ToPerson,
                                               Phone = rfd.Phone,
                                               HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                               HospitalAddress = h != null ? h.Address : "-",
                                               RequestFormRequestedBy = rfd.CreatedBy, //talebi onaylayan kişi 
                                               RequestFormRequestedDate = rfd.PackingDate, //tarihi
                                               MainRepoName = mrl != null ? mrl.Name : "Bilinmiyor",
                                               CargoGivenDate = rfd.CargoGivenDate,
                                               IsOfficeDelivery = rf.IsOfficeDelivery,
                                               TrackingNumber = rfd.TrackingNumber,
                                               Label = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                               SendReason = (from rp in _appDbContext.RequestProducts
                                                             join cd in _appDbContext.CargoDefinitions on rp.ReasonId equals cd.Id
                                                             where rp.RequestFormId == rf.Id
                                                             select cd.Name).FirstOrDefault(),
                                               ProductCondition = rfd.ProductCondition,
                                               Note = rfd.Description,
                                               SerialNumber = rfd.SerialNumber,
                                               EthMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null)
                                                        .Select(x => x.EthMacAddress)
                                                        .FirstOrDefault(),
                                               WlanMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null)
                                                        .Select(x => x.WlanMacAddress)
                                                        .FirstOrDefault(),
                                               ConnectionType = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null)
                                                        .Select(x => x.ConnectionType)
                                                        .FirstOrDefault(),
                                               ConfigUrl = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null)
                                                        .Select(x => x.ConfigUrl)
                                                        .FirstOrDefault(),
                                               Products = (from rp in _appDbContext.RequestProducts
                                                           join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                           join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                           where rp.RequestFormId == rf.Id
                                                           select new ProductDetailDto
                                                           {
                                                               CategoryName = c.Name,
                                                               ProductName = p.Name,
                                                               ImageUrl = p.PhotoUrl,
                                                               Quantity = rp.Quantity
                                                           }).ToList()
                                           }).ToList();

            return View(resultCargoForReadyDtos);
        }

        //Kargo teslimattında
        public async Task<IActionResult> CargoInDelivery()
        {
            await SetCargoCountsAsync();
            // Kargoya verilmiş yolda olan talepleri kargo firması, takip numarası, konum ve ürün detaylarıyla birlikte listeliyor
            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");
            var resultCargoInDeliveries = (from rf in _appDbContext.RequestForms
                                           join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                           join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id into repoGroup
                                           from m in repoGroup.DefaultIfEmpty()
                                           join cn in _appDbContext.CargoNames on rfd.CargoNameId equals cn.Id into cargoGroup
                                           from cn in cargoGroup.DefaultIfEmpty()
                                           join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                           from h in hospitalGroup.DefaultIfEmpty()
                                           where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                           where rfd.StatusId == (int)EnumStatusType.Kargoda && !rfd.IsDeleted
                                           select new ResultCargoInDeliveryDto
                                           {
                                               Id = rfd.Id,
                                               StatusId = rfd.StatusId,
                                               StatusName = "Kargoda",
                                               ReceiverFullName = rfd.ToPerson,
                                               Phone = rfd.Phone,
                                               HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                               HospitalAddress = h != null ? h.Address : "-",
                                               RequestFormBy = rfd.CreatedBy,
                                               RequestFormDate = rfd.CreatedDate,
                                               MainRepoName = m != null ? m.Name : "Bilinmiyor",
                                               CargoGivenDate = rfd.CargoGivenDate,
                                               IsOfficeDelivery = rf.IsOfficeDelivery,
                                               CargoNameId = rfd.CargoNameId,
                                               TrackingNumber = rfd.TrackingNumber,
                                               CargoCompany = cn != null ? cn.Name : "Atanmadı",
                                               Label = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                               SendReason = (from rp in _appDbContext.RequestProducts
                                                             join cd in _appDbContext.CargoDefinitions on rp.ReasonId equals cd.Id
                                                             where rp.RequestFormId == rf.Id
                                                             select cd.Name).FirstOrDefault(),
                                               ProductCondition = rfd.ProductCondition,
                                               Note = rfd.Description,
                                               SerialNumber = rfd.SerialNumber,
                                               EthMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null)
                                                        .Select(x => x.EthMacAddress)
                                                        .FirstOrDefault(),
                                               WlanMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null)
                                                        .Select(x => x.WlanMacAddress)
                                                        .FirstOrDefault(),
                                               ConnectionType = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null)
                                                        .Select(x => x.ConnectionType)
                                                        .FirstOrDefault(),
                                               ConfigUrl = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null)
                                                        .Select(x => x.ConfigUrl)
                                                        .FirstOrDefault(),
                                               Products = (from rp in _appDbContext.RequestProducts
                                                           join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                           join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                           where rp.RequestFormId == rf.Id
                                                           select new ProductDetailDto
                                                           {
                                                               CategoryName = c.Name,
                                                               ProductName = p.Name,
                                                               ImageUrl = p.PhotoUrl,
                                                               Quantity = rp.Quantity
                                                           }).ToList()
                                           }).ToList();


            return View(resultCargoInDeliveries);

        }

        //Kargo teslim edilmiş
        public async Task<IActionResult> Delivered()
        {
            await SetCargoCountsAsync();
            // Teslim edilmiş kargo taleplerini ürün, konum, kargo firması, takip numarası ve işlem tarihleriyle birlikte listeliyor      
            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");
            var resultCargoDelivereds = (from rf in _appDbContext.RequestForms
                                         join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                         join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id into repoGroup
                                         from m in repoGroup.DefaultIfEmpty()
                                         join cn in _appDbContext.CargoNames on rfd.CargoNameId equals cn.Id into cargoGroup
                                         from cn in cargoGroup.DefaultIfEmpty()
                                         join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                         from h in hospitalGroup.DefaultIfEmpty()
                                         join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id
                                         where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                         where rfd.StatusId == (int)EnumStatusType.Tamamlandı && !rfd.IsDeleted
                                         select new ResultCargoDeliveredDto
                                         {
                                             Id = rfd.Id,
                                             StatusId = rfd.StatusId,
                                             StatusName = "Teslim Edildi",
                                             ReceiverFullName = rfd.ToPerson,
                                             Phone = rfd.Phone,
                                             HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                             HospitalAddress = h != null ? h.Address : "-",
                                             RequestFormBy = rfd.CreatedBy,
                                             RequestFormDate = rfd.CreatedDate,
                                             MainRepoName = m != null ? m.Name : "Bilinmiyor",
                                             TrackingNumber = rfd.TrackingNumber,
                                             CargoGivenDate = rfd.CargoGivenDate,
                                             CargoProccessBy = rfd.CreatedBy,
                                             PackingDate = rfd.PackingDate,
                                             CargoCompany = cn != null ? cn.Name : "Atanmadı",
                                             CompletedCargoDate = rfd.CompletedDate,
                                             IsOfficeDelivery = rf.IsOfficeDelivery,
                                             Label = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                             SendReason = (from rp in _appDbContext.RequestProducts
                                                           join cd in _appDbContext.CargoDefinitions on rp.ReasonId equals cd.Id
                                                           where rp.RequestFormId == rf.Id
                                                           select cd.Name).FirstOrDefault(),
                                             ProductCondition = rfd.ProductCondition,
                                             Note = rfd.Description,
                                             SerialNumber = rfd.SerialNumber,
                                             EthMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null)
                                                        .Select(x => x.EthMacAddress)
                                                        .FirstOrDefault(),
                                             WlanMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null)
                                                        .Select(x => x.WlanMacAddress)
                                                        .FirstOrDefault(),
                                             ConnectionType = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null)
                                                        .Select(x => x.ConnectionType)
                                                        .FirstOrDefault(),
                                             ConfigUrl = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null)
                                                        .Select(x => x.ConfigUrl)
                                                        .FirstOrDefault(),
                                             Products = (from rp in _appDbContext.RequestProducts
                                                         join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                         join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                         where rp.RequestFormId == rf.Id
                                                         select new ProductDetailDto
                                                         {
                                                             CategoryName = c.Name,
                                                             ProductName = p.Name,
                                                             ImageUrl = p.PhotoUrl,
                                                             Quantity = rp.Quantity
                                                         }).ToList()
                                         }).ToList();



            return View(resultCargoDelivereds);
        }

        //iptal  edilmiş kargolar
        public async Task<IActionResult> Cancelled()
        {
            await SetCargoCountsAsync();
            ViewBag.UserNames = new SelectList(_appDbContext.Users.AsNoTracking().OrderBy(x => x.NameSurname).ToList(), "NameSurname", "NameSurname");

            var resultCargoCanceleds = (from rf in _appDbContext.RequestForms
                                        join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                        join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id into repoGroup
                                        from m in repoGroup.DefaultIfEmpty()
                                        join cn in _appDbContext.CargoNames on rfd.CargoNameId equals cn.Id into cargoGroup
                                        from cn in cargoGroup.DefaultIfEmpty()
                                        join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                        from h in hospitalGroup.DefaultIfEmpty()
                                        join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id
                                        where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                        where rfd.StatusId == (int)EnumStatusType.İptal && !rfd.IsDeleted
                                        select new ResultCargoCanceledDto
                                        {
                                            Id = rfd.Id,
                                            StatusId = rfd.StatusId,
                                            StatusName = "İptal Edildi", // Sabit isim
                                            CancaledBy = rfd.CanceledBy,
                                            CanceledDesc = rfd.CanceledDesc,
                                            ReceiverFullName = rfd.ToPerson,
                                            Phone = rfd.Phone,
                                            HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                            HospitalAddress = h != null ? h.Address : "-",
                                            MainRepoName = m != null ? m.Name : "Bilinmiyor",
                                            RequestFormBy = rfd.CreatedBy,
                                            RequestFormDate = rfd.CreatedDate,
                                            CargoCompany = cn != null ? cn.Name : "Atanmadı",
                                            CargoGivenDate = rfd.CargoGivenDate,
                                            IsOfficeDelivery = rf.IsOfficeDelivery,
                                            TrackingNumber = rfd.TrackingNumber,
                                            Label = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                            SendReason = (from rp in _appDbContext.RequestProducts
                                                          join cd in _appDbContext.CargoDefinitions on rp.ReasonId equals cd.Id
                                                          where rp.RequestFormId == rf.Id
                                                          select cd.Name).FirstOrDefault(),
                                            ProductCondition = rfd.ProductCondition,
                                            Note = rfd.Description,
                                            SerialNumber = rfd.SerialNumber,
                                            EthMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null)
                                                        .Select(x => x.EthMacAddress)
                                                        .FirstOrDefault(),
                                            WlanMac = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null)
                                                        .Select(x => x.WlanMacAddress)
                                                        .FirstOrDefault(),
                                            ConnectionType = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null)
                                                        .Select(x => x.ConnectionType)
                                                        .FirstOrDefault(),
                                            ConfigUrl = _appDbContext.RequestProducts
                                                        .Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null)
                                                        .Select(x => x.ConfigUrl)
                                                        .FirstOrDefault(),
                                            Products = (from rp in _appDbContext.RequestProducts
                                                        join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                        join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                        where rp.RequestFormId == rf.Id
                                                        select new ProductDetailDto
                                                        {
                                                            CategoryName = c.Name,
                                                            ProductName = p.Name,
                                                            ImageUrl = p.PhotoUrl,
                                                            Quantity = rp.Quantity
                                                        }).ToList()
                                        }).ToList();

            return View(resultCargoCanceleds);
        }


        // Kargo işlemlerini (paketleme, kargoya verme, teslim etme, iptal etme) durumuna göre kaydedip ilgili liste sayfasına yönlendiriyor
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveCargoInfo(SaveCargoInfoDto dto)
        {
            var findCargoDetail = _appDbContext.RequestFormDetails.FirstOrDefault(x => x.Id == dto.Id);

            if (findCargoDetail == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/CargoDetail/Index");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var now = DateTime.Now;

            // ZIRH 1: Kullanıcı bir personel seçtiyse, statü ne olursa olsun (İptal hariç) bunu kaydet.
            if (!string.IsNullOrEmpty(dto.CargoPreparerUserId))
            {
                findCargoDetail.CargoPreparerUserId = dto.CargoPreparerUserId;
            }

            if (dto.StatusId == (int)EnumStatusType.Paketlendi)
            {
                findCargoDetail.CreatedBy = currentUser.NameSurname;
                findCargoDetail.IsActive = true;
                findCargoDetail.IsDeleted = false;
                findCargoDetail.ApprovalDate = now;
                findCargoDetail.ApprovalBy = currentUser.NameSurname;
                findCargoDetail.PackingDate = now;
                findCargoDetail.StatusId = (int)EnumStatusType.Paketlendi;

                TempData["SuccessMessage"] = "Kargo paketleme işlemi kaydedildi.";
            }
            else if (dto.StatusId == (int)EnumStatusType.Kargoda)
            {
                findCargoDetail.TrackingNumber = dto.TrackingNumber;
                findCargoDetail.StatusId = dto.StatusId;
                findCargoDetail.CargoGivenDate = now;
                findCargoDetail.CargoNameId = dto.CargoNameId;

                TempData["SuccessMessage"] = "Kargo teslimatta işlemi kaydedildi.";
            }
            else if (dto.StatusId == (int)EnumStatusType.Tamamlandı)
            {
                findCargoDetail.CompletedDate = now;
                findCargoDetail.StatusId = (int)EnumStatusType.Tamamlandı;

                TempData["SuccessMessage"] = "Kargo teslim edildi işlemi kaydedildi.";
            }
            else if (dto.StatusId == (int)EnumStatusType.İptal)
            {
                findCargoDetail.CanceledDate = now;
                findCargoDetail.CanceledBy = currentUser.NameSurname;
                findCargoDetail.CanceledDesc = dto.CancelDescription;
                findCargoDetail.StatusId = (int)EnumStatusType.İptal;

                TempData["SuccessMessage"] = "Kargo iptal edildi.";
            }
            // ZIRH 2: PDF'teki "Eksik" Statülerin Sisteme Tanıtılması
            else if (dto.StatusId == (int)EnumStatusType.OfistenTeslimAlinacak) // 11
            {
                findCargoDetail.StatusId = (int)EnumStatusType.OfistenTeslimAlinacak;
                TempData["SuccessMessage"] = "Kargo durumu 'Ofisten Teslim Alınacak' olarak güncellendi.";
            }
            else if (dto.StatusId == (int)EnumStatusType.IadeGeldigindeKargolanacak) // 13
            {
                findCargoDetail.StatusId = (int)EnumStatusType.IadeGeldigindeKargolanacak;
                TempData["SuccessMessage"] = "Kargo durumu 'İade Geldiği Zaman Kargolanacak' olarak beklemeye alındı.";
            }

            _appDbContext.RequestFormDetails.Update(findCargoDetail);
            _appDbContext.SaveChanges();

            return Redirect(Request.Headers["Referer"].ToString() ?? "/CargoDetail/Index");
        }


        [HttpPost]
        //Kargo IsDelete işlemi
        public async Task<IActionResult> DeleteCargo(int id)
        {
            try
            {
                var cargo = await _appDbContext.RequestFormDetails.FindAsync(id);
                if (cargo == null)
                {
                    return Json(new { success = false, message = "Kargo bulunamadı." });
                }

                var requestForm = await _appDbContext.RequestForms.FindAsync(cargo.RequestFormId);
                if (requestForm == null)
                {
                    return Json(new { success = false, message = "Talep Formu bulunamadı." });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Challenge();

                var now = DateTime.Now;

                // Cargo alanlarını güncelle
                cargo.IsDeleted = true;
                cargo.DeletedDate = now;
                cargo.DeletedBy = currentUser.NameSurname;

                // RequestForm alanlarını güncelle
                requestForm.IsDeleted = true;
                requestForm.DeletedDate = now;
                requestForm.DeletedBy = currentUser.NameSurname;

                // Transaction ile güncellemeleri kaydet
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _appDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, receiver = cargo.ToPerson });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        //Silinen kargolar
        public async Task<IActionResult> Deleted()
        {
            await SetCargoCountsAsync();
            var resultCargoDeleteds = (from rf in _appDbContext.RequestForms
                                       join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                       join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id into repoGroup
                                       from m in repoGroup.DefaultIfEmpty()
                                       join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                       from h in hospitalGroup.DefaultIfEmpty()
                                       join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id
                                       where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                       where rfd.IsDeleted == true
                                       select new ResultCargoDeletedDto
                                       {
                                           Id = rfd.Id,
                                           StatusName = "Silindi",
                                           ReceiverFullName = rfd.ToPerson,
                                           Phone = rfd.Phone,
                                           HospitalName = h != null ? h.Name : "Ofisten Teslim / Belirtilmemiş",
                                           HospitalAddress = h != null ? h.Address : "-",
                                           DeletedBy = rfd.DeletedBy,
                                           DeletedDate = rfd.DeletedDate,
                                           RequestFormBy = rfd.CreatedBy,
                                           RequestFormDate = rfd.CreatedDate,
                                           MainRepoName = m != null ? m.Name : "Bilinmiyor",
                                           CargoGivenDate = rfd.CargoGivenDate,
                                           IsOfficeDelivery = rf.IsOfficeDelivery,
                                           TrackingNumber = rfd.TrackingNumber,
                                           Products = (from rp in _appDbContext.RequestProducts
                                                       join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                       join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                       where rp.RequestFormId == rf.Id
                                                       select new ProductDetailDto
                                                       {
                                                           CategoryName = c.Name,
                                                           ProductName = p.Name,
                                                           ImageUrl = p.PhotoUrl,
                                                           Quantity = rp.Quantity
                                                       }).ToList()
                                       }).ToList();
            return View(resultCargoDeleteds);
        }

        [HttpGet]
        public async Task<IActionResult> ReturnsIndex()
        {
            // await SetCargoCountsAsync();
            var resultReturns = (from rfd in _appDbContext.RequestFormDetails
                                 join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                 join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id into repoGroup
                                 from m in repoGroup.DefaultIfEmpty()
                                 join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                 from h in hospitalGroup.DefaultIfEmpty()
                                 join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id into statusGroup
                                 from s in statusGroup.DefaultIfEmpty()
                                 where rfd.StatusId == 20 ||
                                     rfd.StatusId == 21 ||
                                     rfd.StatusId == 22 ||
                                     rfd.StatusId == 23 ||
                                     rfd.StatusId == 24
                                 select new ResultAwitingApprovalDto
                                 {
                                     Id = rfd.Id,
                                     StatusId = rfd.StatusId,
                                     StatusName = s != null ? s.Name : "Tanımsız Statü",
                                     ReceiverFullName = rfd.ToPerson,
                                     HospitalName = h != null ? h.Name : "Belirtilmemiş",
                                     RequestFormRequestedDate = rfd.RequestDate,
                                     RequestFormRequestedBy = rfd.RequestBy,
                                     MainRepoName = m != null ? m.Name : "Bilinmiyor",
                                     CargoGivenDate = rfd.CargoGivenDate,

                                     //EKSİK OLAN VE EKLENEN VERİLER:
                                     Label = _appDbContext.RequestProducts
                                                         .Where(x => x.RequestFormId == rf.Id && x.Label != null).Select(x => x.Label).FirstOrDefault(),
                                     SendReason = (from rp in _appDbContext.RequestProducts
                                                   join cd in _appDbContext.CargoDefinitions on rp.ReasonId equals cd.Id
                                                   where rp.RequestFormId == rf.Id
                                                   select cd.Name).FirstOrDefault(),
                                     ProductCondition = rfd.ProductCondition,
                                     Note = rfd.Description,
                                     SerialNumber = rfd.SerialNumber,
                                     EthMac = _appDbContext.RequestProducts
                                                         .Where(x => x.RequestFormId == rf.Id && x.EthMacAddress != null)
                                                         .Select(x => x.EthMacAddress)
                                                         .FirstOrDefault(),
                                     WlanMac = _appDbContext.RequestProducts
                                                         .Where(x => x.RequestFormId == rf.Id && x.WlanMacAddress != null)
                                                         .Select(x => x.WlanMacAddress)
                                                         .FirstOrDefault(),
                                     ConnectionType = _appDbContext.RequestProducts
                                                         .Where(x => x.RequestFormId == rf.Id && x.ConnectionType != null)
                                                         .Select(x => x.ConnectionType)
                                                         .FirstOrDefault(),
                                     ConfigUrl = _appDbContext.RequestProducts
                                                         .Where(x => x.RequestFormId == rf.Id && x.ConfigUrl != null)
                                                         .Select(x => x.ConfigUrl)
                                                         .FirstOrDefault(),
                                     ReturnImageUrl = rfd.ImageUrl,

                                     Products = (from rp in _appDbContext.RequestProducts
                                                 join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                 join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                 where rp.RequestFormId == rf.Id
                                                 select new ProductDetailDto
                                                 {
                                                     CategoryName = c.Name,
                                                     ProductName = p.Name,
                                                     Quantity = rp.Quantity
                                                 }).ToList()
                                 }).ToList();

            return View(resultReturns);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveReturnInfo(SaveCargoReturnInfoDto dto)
        {
            // 1. Kaydı bul
            var findDetail = await _appDbContext.RequestFormDetails.FindAsync(dto.Id);
            if (findDetail == null)
            {
                TempData["ErrorMessage"] = "İade kaydı bulunamadı.";
                return RedirectToAction("ReturnsIndex");
            }

            // 2. GÖRSELİ SUNUCUYA KAYDETME (Adım 2'nin Devamı)
            if (dto.ReturnImage != null && dto.ReturnImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/returns");

                // Eğer klasör yoksa oluştur
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Fotoğrafa benzersiz (Guid) bir isim ver
                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ReturnImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayı fiziksel olarak sunucuya kopyala
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ReturnImage.CopyToAsync(fileStream);
                }

                // Veritabanındaki fotoğraf URL kolonunu güncelle (Eğer kolon adı ImageUrl ise)
                findDetail.ImageUrl = "/images/returns/" + uniqueFileName;
            }

            // 3. TEMEL BİLGİLERİ GÜNCELLE
            findDetail.StatusId = dto.StatusId;
            findDetail.ReceivedQuantity = dto.ReceivedQuantity;
            findDetail.ZayiatQuantity = dto.ZayiatQuantity;
            findDetail.ControlResult = dto.ControlResult;
            findDetail.CargoGivenDate = DateTime.Now; // İade işlem tarihi

            // 4. SERİ NO DEĞİŞİM ZEKASI
            if (!string.IsNullOrEmpty(dto.NewSerialNumber))
            {
                findDetail.Note = $"[ESKİ SERİ NO: {findDetail.SerialNumber} - YENİSİ İLE DEĞİŞTİ] " + findDetail.Note;
                findDetail.SerialNumber = dto.NewSerialNumber;

                var requestForm = await _appDbContext.RequestForms.FindAsync(findDetail.RequestFormId);
                var requestItem = await _appDbContext.RequestProducts.FirstOrDefaultAsync(x => x.RequestFormId == findDetail.RequestFormId);

                if (requestItem != null && requestForm != null)
                {
                    var serialRecord = await _appDbContext.ProductSerialNumbers
                        .FirstOrDefaultAsync(x => x.ProductId == requestItem.ProductId && x.MainRepoLocationId == requestForm.MainRepoLocationId);

                    if (serialRecord != null)
                    {
                        serialRecord.SerialNumber = dto.NewSerialNumber;
                        serialRecord.Description += " | İade kontrolünde seri no güncellendi.";
                        _appDbContext.ProductSerialNumbers.Update(serialRecord);
                    }
                }
            }

            // 5. SÜRPRİZ ÜRÜN ZEKASI
            if (!string.IsNullOrEmpty(dto.ExtraProductName) && dto.ExtraProductQty > 0)
            {
                findDetail.Note += $" | [SÜRPRİZ ÜRÜN: {dto.ExtraProductName} - {dto.ExtraProductQty} Adet]";
            }

            // 6. KUSURSUZ STOK GÜNCELLEME (Kritik Düzeltme)
            // Statü "Teslim Alınamadı" (22) değilse ve teslim alınan ürün varsa
            if (dto.StatusId != 22 && dto.ReceivedQuantity > 0)
            {
                // DOĞRUSU: Zayiatı (dto.ZayiatQuantity) düşmüyoruz! 
                // Çünkü depocu zaten ReceivedQuantity kutusuna "Sağlam" olan adeti giriyor.
                int eklenecekStok = dto.ReceivedQuantity;

                var requestForm = await _appDbContext.RequestForms.FindAsync(findDetail.RequestFormId);
                var requestedProducts = await _appDbContext.RequestProducts.Where(x => x.RequestFormId == findDetail.RequestFormId).ToListAsync();

                foreach (var rp in requestedProducts)
                {
                    var stockItem = await _appDbContext.ProductMainRepoLocations.FirstOrDefaultAsync(i =>
                        i.ProductId == rp.ProductId &&
                        i.MainRepoLocationId == requestForm.MainRepoLocationId);

                    if (stockItem != null)
                    {
                        // Sağlam ürünleri depoya ekle
                        stockItem.Quantity += eklenecekStok;
                        _appDbContext.ProductMainRepoLocations.Update(stockItem);

                        // Stok Hareket (Log) Tablosuna Zayiatı Not Düşerek Kaydet
                        _appDbContext.StockMovements.Add(new StockMovement
                        {
                            ProductId = rp.ProductId,
                            MainRepoLocationId = requestForm.MainRepoLocationId,
                            MovementType = "IN",
                            MovementQuantity = eklenecekStok,
                            Description = $"İade Teslimatı. Sağlam Giren: {dto.ReceivedQuantity}, Zayiat: {dto.ZayiatQuantity}.",
                            CreatedDate = DateTime.Now
                        });
                    }
                }
            }

            _appDbContext.RequestFormDetails.Update(findDetail);
            await _appDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "İade başarıyla tamamlandı, fotoğraf kaydedildi ve stoklar güncellendi!";
            return RedirectToAction("ReturnsIndex");
        }


    }
}
