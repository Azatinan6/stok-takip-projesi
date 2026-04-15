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
            var resultAllCargo = (from rfd in _appDbContext.RequestFormDetails
                                join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                
                                // --- DEPO ZIRHI (LEFT JOIN) ---
                                join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id into repoGroup
                                from mrl in repoGroup.DefaultIfEmpty()
                                
                                // --- HASTANE ZIRHI (LEFT JOIN) ---
                                join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id into hospitalGroup
                                from h in hospitalGroup.DefaultIfEmpty()
                                
                                join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id
                                where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                where !rfd.IsDeleted // SADECE SİLİNMEMİŞLER (Statü filtresi yok)
                                select new ResultAwitingApprovalDto // Mevcut Dto'nu kullanabiliriz, alanlar aynı
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
                                    SendReason = rfd.SendReason,
                                    ProductCondition = rfd.ProductCondition,
                                    Note = rfd.Note, 
                                    SerialNumber = rfd.SerialNumber,
                                    EthMac = rfd.EthMac,
                                    WlanMac = rfd.WlanMac,
                                    ConnectionType = rfd.ConnectionType,
                                    ConfigUrl = rfd.ConfigUrl,
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
                                
            return View(resultAllCargo);
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
                                               Label = rfd.Label,
                                               SendReason = rfd.SendReason,
                                               ProductCondition = rfd.ProductCondition,
                                               Note = rfd.Note, 
                                               SerialNumber = rfd.SerialNumber,
                                               EthMac = rfd.EthMac,
                                               WlanMac = rfd.WlanMac,
                                               ConnectionType = rfd.ConnectionType,
                                               ConfigUrl = rfd.ConfigUrl,
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
                                               SendReason = rfd.SendReason,
                                               ProductCondition = rfd.ProductCondition,
                                               Note = rfd.Note, 
                                               SerialNumber = rfd.SerialNumber,
                                               EthMac = rfd.EthMac,
                                               WlanMac = rfd.WlanMac,
                                               ConnectionType = rfd.ConnectionType,
                                               ConfigUrl = rfd.ConfigUrl,
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
                                               SendReason = rfd.SendReason,
                                               ProductCondition = rfd.ProductCondition,
                                               Note = rfd.Note, 
                                               SerialNumber = rfd.SerialNumber,
                                               EthMac = rfd.EthMac,
                                               WlanMac = rfd.WlanMac,
                                               ConnectionType = rfd.ConnectionType,
                                               ConfigUrl = rfd.ConfigUrl,
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
                                             SendReason = rfd.SendReason,
                                             ProductCondition = rfd.ProductCondition,
                                             Note = rfd.Note, 
                                             SerialNumber = rfd.SerialNumber,
                                             EthMac = rfd.EthMac,
                                             WlanMac = rfd.WlanMac,
                                             ConnectionType = rfd.ConnectionType,
                                             ConfigUrl = rfd.ConfigUrl,                                             
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
                                            SendReason = rfd.SendReason,
                                            ProductCondition = rfd.ProductCondition,
                                            Note = rfd.Note, 
                                            SerialNumber = rfd.SerialNumber,
                                            EthMac = rfd.EthMac,
                                            WlanMac = rfd.WlanMac,
                                            ConnectionType = rfd.ConnectionType,
                                            ConfigUrl = rfd.ConfigUrl,
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Kargo işlemlerini (paketleme, kargoya verme, teslim etme, iptal etme) durumuna göre kaydedip ilgili liste sayfasına yönlendiriyor
        
        public async Task<IActionResult> SaveCargoInfo(SaveCargoInfoDto dto)
        {
            var findCargoDetail = _appDbContext.RequestFormDetails.FirstOrDefault(x => x.Id == dto.Id);

            if (findCargoDetail == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return Redirect(Request.Headers["Referer"].ToString() ?? "/CargoDetail/Index"); // Geldiği sayfaya geri döner
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var now = DateTime.Now;

            if (dto.StatusId == (int)EnumStatusType.Paketlendi)
            {
                findCargoDetail.CreatedBy = currentUser.NameSurname;
                findCargoDetail.IsActive = true;
                findCargoDetail.IsDeleted = false;
                findCargoDetail.CargoPreparerUserId = dto.CargoPreparerUserId;
                findCargoDetail.ApprovalDate = now; // Onaylanma tarihi
                findCargoDetail.ApprovalBy = currentUser.NameSurname; // Onaylayan kişi
                findCargoDetail.PackingDate = now; // Paketlenme tarihi
                findCargoDetail.StatusId = (int)EnumStatusType.Paketlendi;

                TempData["SuccessMessage"] = "Kargo paketleme işlemi kaydedildi.";
            }
            else if (dto.StatusId == (int)EnumStatusType.Kargoda)
            {
                findCargoDetail.TrackingNumber = dto.TrackingNumber;
                findCargoDetail.StatusId = dto.StatusId;
                findCargoDetail.CargoGivenDate = now; // Kargoya verilme tarihi
                findCargoDetail.CargoNameId = dto.CargoNameId;

                TempData["SuccessMessage"] = "Kargo teslimatta işlemi kaydedildi.";
            }
            else if (dto.StatusId == (int)EnumStatusType.Tamamlandı)
            {
                // Not: Eğer DTO'da tarih gelmiyorsa direkt now kullanabiliriz.
                findCargoDetail.CompletedDate = now; // Teslim edilmiş kargo tarihi
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

            // Tek bir yerden güncelle ve kaydet (Kod tekrarını önledik)
            _appDbContext.RequestFormDetails.Update(findCargoDetail);
            _appDbContext.SaveChanges();

            // PRO İPUCU: Kullanıcı Tümü sayfasındaysa Tümü'ne, Onay Bekliyor'daysa Onay Bekliyor'a geri döner.
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
                                    
                                    // İŞTE EKSİK OLAN VE EKLENEN VERİLER:
                                    Label = rfd.Label,
                                    SendReason = rfd.SendReason,
                                    ProductCondition = rfd.ProductCondition,
                                    SerialNumber = rfd.SerialNumber,
                                    EthMac = rfd.EthMac,
                                    WlanMac = rfd.WlanMac,
                                    ConnectionType = rfd.ConnectionType,
                                    ConfigUrl = rfd.ConfigUrl,
                                    Note = rfd.Note,

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
            var findDetail = await _appDbContext.RequestFormDetails.FindAsync(dto.Id);
            if (findDetail == null)
            {
                TempData["ErrorMessage"] = "İade kaydı bulunamadı.";
                return RedirectToAction("ReturnsIndex");
            }

            findDetail.StatusId = dto.StatusId;
            findDetail.ReceivedQuantity = dto.ReceivedQuantity;
            findDetail.ZayiatQuantity = dto.ZayiatQuantity;
            findDetail.ControlResult = dto.ControlResult;
            findDetail.CargoGivenDate = DateTime.Now; 

            if (!string.IsNullOrEmpty(dto.NewSerialNumber))
            {
                findDetail.Note = $"[ESKİ SERİ NO: {findDetail.SerialNumber} - YENİSİ İLE DEĞİŞTİ] " + findDetail.Note;
                findDetail.SerialNumber = dto.NewSerialNumber;
            }

            if (!string.IsNullOrEmpty(dto.ExtraProductName) && dto.ExtraProductQty > 0)
            {
                findDetail.Note += $" | [SÜRPRİZ ÜRÜN: {dto.ExtraProductName} - {dto.ExtraProductQty} Adet]";
            }

            // Stok güncelleme kısmı
            if (dto.StatusId != 22 && dto.ReceivedQuantity > 0)
            {
                int saglamGelenAdet = dto.ReceivedQuantity - dto.ZayiatQuantity;

                if (saglamGelenAdet > 0)
                {
                    var requestForm = await _appDbContext.RequestForms.FindAsync(findDetail.RequestFormId);
                    var requestedProducts = _appDbContext.RequestProducts.Where(x => x.RequestFormId == findDetail.RequestFormId).ToList();

                    foreach (var rp in requestedProducts)
                    {
                        // Inventories yerine ProductMainRepoLocations kullanıyoruz
                        var stockItem = _appDbContext.ProductMainRepoLocations.FirstOrDefault(i => 
                            i.ProductId == rp.ProductId && 
                            i.MainRepoLocationId == requestForm.MainRepoLocationId);

                        if (stockItem != null)
                        {
                            // Stok adetini güncelliyoruz (Eğer modelindeki miktar alanının adı Quantity değilse burayı ona göre düzeltmelisin)
                             stockItem.Quantity += saglamGelenAdet; 
                            _appDbContext.ProductMainRepoLocations.Update(stockItem);
                        }
                    }
                }
            }

            _appDbContext.RequestFormDetails.Update(findDetail);
            await _appDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "İade başarıyla tamamlandı ve sağlam ürünler stoklara geri eklendi!";
            return RedirectToAction("ReturnsIndex");
        }

    }
}
