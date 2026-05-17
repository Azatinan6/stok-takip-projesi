using Azure.Core;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.RequestForm;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Enums;
using System.Text.Json;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class RequestFormController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly ICategoryService _categoryService;
        private readonly ICargoDefinitionService _cargoDefinitionService;
        private readonly IMainRepoLocationService _mainRepoLocationService;
        private readonly IProductService _productService;
        private readonly IHospitalService _hospitalService;
        private readonly UserManager<AppUser> _userManager;

        public RequestFormController(AppDbContext appDbContext, ICategoryService categoryService, IMainRepoLocationService mainRepoLocationService, IProductService productService, IHospitalService hospitalService, ICargoDefinitionService cargoDefinitionService, UserManager<AppUser> userManager)
        {
            _appDbContext = appDbContext;
            _categoryService = categoryService;
            _mainRepoLocationService = mainRepoLocationService;
            _productService = productService;
            _hospitalService = hospitalService;
            _cargoDefinitionService = cargoDefinitionService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var hospitals = await _appDbContext.Hospitals.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                .ToListAsync();

            var repos = await _appDbContext.MainRepoLocations.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                .ToListAsync();

            var persons = await _userManager.Users
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.NameSurname })
                .ToListAsync();

            var definitions = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.IsActive);

            // Giriş (İade) Nedenleri -> DefinitionType = 3 (Veritabanındaki ID'sine göre kontrol et)
            ViewBag.InboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 5), "Id", "Name");

            // Çıkış (Gönderim) Nedenleri -> DefinitionType = 4 (Veritabanındaki ID'sine göre kontrol et)
            ViewBag.OutboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 4), "Id", "Name");

            ViewBag.Locations = hospitals;
            ViewBag.MainRepos = repos;
            ViewBag.Persons = persons;

            return View(new CreateRequestFormDto());
        }

        // FORM GÖNDERİLDİĞİNDE ÇALIŞAN KAYIT METODU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestFormDto dto)
        {
            // ZIRH 1: ModelState'i manuel olarak temizle! 
            // Çünkü dinamik formlarda (Kargo mu Servis mi değiştiği için) DTO'daki her alanı zorunlu sayıp patlayabilir.
            // Biz doğrulamayı (Validation) kendi if-else Türkçe kurallarımızla yapacağız!
            ModelState.Clear();

            // ADIM 1) ItemsJson Çözümle          
            var items = new List<ItemDto>();

            if (string.IsNullOrWhiteSpace(dto.ItemsJson))
                ModelState.AddModelError("", "Lütfen en az bir ürün ekleyiniz.");
            else
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    items = JsonSerializer.Deserialize<List<ItemDto>>(dto.ItemsJson, options) ?? new();
                    if (items.Count == 0)
                        ModelState.AddModelError("", "Seçilen ürün listesi boş olamaz.");
                }
                catch
                {
                    ModelState.AddModelError("", "Ürün listesi sunucu tarafından işlenemedi.");
                }
            }

            // ADIM 2) Sunucu Tarafı Mantıksal Türkçe Doğrulamaları
            if (dto.MainRepoId <= 0) ModelState.AddModelError("", "Lütfen ürünlerin çıkacağı / gireceği depoyu seçiniz.");
            if (dto.LocationId <= 0) ModelState.AddModelError("", "Lütfen işlem yapılacak hastaneyi seçiniz.");
            if (dto.TypeId <= 0) ModelState.AddModelError("", "Lütfen talep türünü (Örn: Kargo) seçiniz.");

            // GÖREV LİSTESİ MADDE 2 (Gizlenen seçenekler): Sadece Kargo kuralları çalışmalı
            if (dto.TypeId == (int)EnumRequestType.Kargo)
            {
                // Kargo Modu Kontrolleri
                if (string.IsNullOrWhiteSpace(dto.ReceiverFullName))
                    ModelState.AddModelError("", "Kargo gönderimi için Alıcı Adı Soyadı zorunludur.");
                if (string.IsNullOrWhiteSpace(dto.Phone))
                    ModelState.AddModelError("", "Kargo gönderimi için Alıcı Telefon Numarası zorunludur.");
            }
            else if (dto.TypeId == (int)EnumRequestType.Kurulum || dto.TypeId == (int)EnumRequestType.Servis)
            {
                // Kurulum Modu Kontrolleri (İleride açılırsa diye koruma olarak kalabilir)
                if (dto.InstallationDate == null) ModelState.AddModelError("", "Planlanan kurulum / servis tarihi zorunludur.");
                if (dto.Persons == null || dto.Persons.Count == 0) ModelState.AddModelError("", "Saha operasyonu için en az bir personel seçilmelidir.");
            }

            // Ürün satır kontrolleri ve AKILLI STOK
            var duplicate = new HashSet<string>();
            foreach (var it in items)
            {
                if (it.ProductId <= 0 || it.Quantity <= 0)
                {
                    ModelState.AddModelError("", "Ürün seçimi veya miktar girişi eksik olan bir satır tespit edildi.");
                    continue;
                }

                string uniqueKey = $"{it.ProductId}_{it.OperationType}";
                if (!duplicate.Add(uniqueKey))
                    ModelState.AddModelError("", $"Aynı ürünü (ID: {it.ProductId}) aynı işlem türüyle (Gönderim/İade) birden fazla kez ekleyemezsiniz.");

                // STOK KONTROLÜ: Gönderim (OperationType == 1)
                if (it.OperationType == 1)
                {
                    var inRepoQty = await _appDbContext.ProductMainRepoLocations.AsNoTracking()
                        .Where(x => x.MainRepoLocationId == dto.MainRepoId && x.ProductId == it.ProductId).Select(x => (int?)x.Quantity).FirstOrDefaultAsync() ?? 0;

                    var distributed = await (from rp in _appDbContext.RequestProducts.AsNoTracking()
                                             join rf in _appDbContext.RequestForms.AsNoTracking() on rp.RequestFormId equals rf.Id
                                             where rp.ProductId == it.ProductId && rf.MainRepoLocationId == dto.MainRepoId && !rf.IsDeleted && rp.OperationType == 1
                                             select (int?)rp.Quantity).SumAsync() ?? 0;

                    var remaining = Math.Max(0, inRepoQty - distributed);
                    if (it.Quantity > remaining)
                        ModelState.AddModelError("", $"Stok yetersiz! İstenen miktar stoktan fazla. Ürün için stokta kalan rezerve edilebilir miktar: {remaining} adet.");
                }
            }

            // ZIRH 2: Eğer bizim eklediğimiz TÜRKÇE hatalardan biri varsa, View'a temizce dön.
            if (!ModelState.IsValid)
            {
                // Görünüm (ViewBag) verilerini geri doldur (Sayfa patlamasın diye)
                ViewBag.Locations = await _appDbContext.Hospitals.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToListAsync();
                ViewBag.MainRepos = await _appDbContext.MainRepoLocations.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToListAsync();
                ViewBag.Persons = await _userManager.Users.Where(x => x.IsActive && !x.IsDeleted).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.NameSurname }).ToListAsync();

                var definitions = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.IsActive);
                ViewBag.InboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 5), "Id", "Name");
                ViewBag.OutboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 4), "Id", "Name");

                // Hataları ekrana basmak için TempData'ya al
                TempData["ErrorMessage"] = string.Join("<br/>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(dto);
            }

            // ADIM 3) Kayıt + Transaction
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                var now = DateTime.Now;

                // ANA FORM KAYDI
                var requestForm = new RequestForm
                {
                    MainRepoLocationId = dto.MainRepoId,
                    HospitalId = dto.LocationId,
                    RequestFormTypeId = dto.TypeId,
                    IsShipAfterReturn = dto.IsShipAfterReturn,
                    IsOfficeDelivery = dto.IsOfficeDelivery,
                    CreatedBy = currentUser.NameSurname,
                    CreatedDate = now,
                    ModifiedDate = now,
                    IsDeleted = false,
                    IsActive = true,
                };

                await _appDbContext.RequestForms.AddAsync(requestForm);
                await _appDbContext.SaveChangesAsync();

                // ÜRÜNLERİ VE ARC BOX CONFIGLERİNİ KAYDET
                var rpList = items.Where(x => x.ProductId > 0 && x.Quantity > 0)
                    .Select(x => new RequestProduct
                    {
                        RequestFormId = requestForm.Id,
                        ProductId = x.ProductId,
                        Quantity = x.Quantity,
                        OperationType = x.OperationType,       // 1 Gönderim, 2 İade
                        Label = x.Label,                       // Cihaz etiketi
                        ReasonId = x.ReasonId,                 // İade / Gönderim Nedeni

                        // ZIRH 3: ArcBox JSON Değerlerini Güvenli Alma (Null Kontrollü)
                        ConnectionType = x.ArcBoxConfig != null ? x.ArcBoxConfig.ConnectionType : null,
                        ConfigUrl = x.ArcBoxConfig != null ? x.ArcBoxConfig.ConfigUrl : null,
                        DhcpdConf = x.ArcBoxConfig != null ? x.ArcBoxConfig.DhcpdConf : null,
                        WpaSupplicantConf = x.ArcBoxConfig != null ? x.ArcBoxConfig.WpaSupplicantConf : null,
                        IpAddress = x.ArcBoxConfig != null ? x.ArcBoxConfig.IpAddress : null,
                        EthMacAddress = x.ArcBoxConfig != null ? x.ArcBoxConfig.EthMacAddress : null,
                        WlanMacAddress = x.ArcBoxConfig != null ? x.ArcBoxConfig.WlanMacAddress : null
                    }).ToList();

                if (rpList.Count > 0)
                {
                    await _appDbContext.RequestProducts.AddRangeAsync(rpList);
                    await _appDbContext.SaveChangesAsync();
                }

                // DETAY KAYDI (KARGO)
                var requestFormDetail = new RequestFormDetail
                {
                    RequestFormId = requestForm.Id,
                    StatusId = (int)EnumStatusType.Talep,
                    CreatedBy = currentUser.NameSurname,
                    CreatedDate = now,
                    ModifiedDate = now,
                    IsDeleted = false,
                    IsActive = true,
                    RequestBy = currentUser.NameSurname,
                    RequestDate = now,

                    // ZIRH: Veritabanı NULL kabul etmediği için boş resim linki yerine geçici bir tire atıyoruz!
                    ImageUrl = "-"
                };

                // Görev listesine göre "Kurulum" iptal edildiği için Kargo olarak devam ediyor
                if (dto.TypeId == (int)EnumRequestType.Kargo)
                {
                    requestFormDetail.ToPerson = dto.ReceiverFullName ?? "Belirtilmedi";
                    requestFormDetail.Phone = dto.Phone ?? "Belirtilmedi";
                    requestFormDetail.ReceiverDepartment = dto.ReceiverDepartment ?? "-";
                    // UI'daki 'Açık Adres' kutusu için Zırhlı kontrol (Zorunlu değilse null patlatmasın)
                    requestFormDetail.Address = string.IsNullOrEmpty(dto.CargoAddress) ? "-" : dto.CargoAddress;
                    requestFormDetail.Description = string.IsNullOrEmpty(dto.Note) ? "-" : dto.Note;
                }

                await _appDbContext.RequestFormDetails.AddAsync(requestFormDetail);
                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                TempData["SuccessMessage"] = "Kargo talebiniz başarıyla oluşturuldu ve onay aşamasına iletildi.";

                return RedirectToAction("RequestFormList");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // ZIRH 4: Sinsi SQL Hatalarını (InnerException) Yakalayıp Ekrana Basan Ajan
                string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                Console.WriteLine(realError);

                TempData["ErrorMessage"] = "Veritabanı kayıt işlemi sırasında bir hata oluştu. Girilen teknik parametreleri veya veritabanı bağlantısını kontrol ediniz.";
                // Sayfa boş gelmesin diye ViewBag'leri tekrar doldur
                ViewBag.Locations = await _appDbContext.Hospitals.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToListAsync();
                ViewBag.MainRepos = await _appDbContext.MainRepoLocations.AsNoTracking().Where(x => !x.IsDeleted && x.IsActive).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToListAsync();
                ViewBag.Persons = await _userManager.Users.Where(x => x.IsActive && !x.IsDeleted).Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.NameSurname }).ToListAsync();

                var definitions = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.IsActive);
                ViewBag.InboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 5), "Id", "Name");
                ViewBag.OutboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 4), "Id", "Name");

                return View(dto);
            }
        }

        // Depoya göre kategoriler 
        [HttpGet]
        public async Task<IActionResult> GetCategoriesByRepo(int mainRepoId)
        {
            var cats = await (from pml in _appDbContext.ProductMainRepoLocations.AsNoTracking()
                              join p in _appDbContext.Products.AsNoTracking() on pml.ProductId equals p.Id
                              join c in _appDbContext.Categories.AsNoTracking() on p.CategoryId equals c.Id
                              where pml.MainRepoLocationId == mainRepoId && !p.IsDeleted && p.IsActive && !c.IsDeleted && c.IsActive
                              select new { id = c.Id, text = c.Name })
                     .GroupBy(x => x.id)
                     .Select(g => g.First())
                     .ToListAsync();

            return Json(cats);
        }

        // Depo + kategoriye göre ürünler
        [HttpGet]
        public async Task<IActionResult> GetProductsByRepoAndCategory(int mainRepoId, int categoryId)
        {
            var prods = await (from pml in _appDbContext.ProductMainRepoLocations.AsNoTracking()
                               join p in _appDbContext.Products.AsNoTracking() on pml.ProductId equals p.Id
                               where pml.MainRepoLocationId == mainRepoId && pml.Quantity > 0 && !p.IsDeleted && p.IsActive && p.CategoryId == categoryId
                               select new
                               {
                                   id = p.Id,
                                   text = (p.Model ?? "").Trim() == "" ? p.Name : (p.Model + " - " + p.Name)
                               }).ToListAsync();

            return Json(prods);
        }

        // Ürünün kalan stok miktarı
        [HttpGet]
        public async Task<IActionResult> GetProductRemainingStock(int productId, int mainRepoId)
        {
            var inRepoQty = await _appDbContext.ProductMainRepoLocations.AsNoTracking().Where(x => x.MainRepoLocationId == mainRepoId && x.ProductId == productId).Select(x => (int?)x.Quantity).FirstOrDefaultAsync() ?? 0;

            var distributed = await (from rp in _appDbContext.RequestProducts.AsNoTracking()
                                     join rf in _appDbContext.RequestForms.AsNoTracking() on rp.RequestFormId equals rf.Id
                                     join rfd in _appDbContext.RequestFormDetails.AsNoTracking() on rf.Id equals rfd.RequestFormId
                                     where rp.ProductId == productId && rf.MainRepoLocationId == mainRepoId && !rf.IsDeleted && !rfd.IsDeleted && rfd.StatusId != (int)EnumStatusType.İptal && rp.OperationType == 1
                                     select (int?)rp.Quantity).SumAsync() ?? 0;

            var remaining = inRepoQty - distributed;
            if (remaining < 0) remaining = 0;

            return Json(new { remaining, inRepoQty, distributed });
        }

        //oluşturulan taleplerin listesi
        public async Task<IActionResult> RequestFormList()
        {
            var requestList = (from rf in _appDbContext.RequestForms
                               join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id
                               join h in _appDbContext.Hospitals on rf.HospitalId equals h.Id
                               join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                               join rft in _appDbContext.RequestFormTypes on rf.RequestFormTypeId equals rft.Id
                               where rf.IsActive && !rf.IsDeleted && rfd.StatusId == (int)EnumStatusType.Talep && rfd.StatusId != (int)EnumStatusType.İptal
                               select new RequestedForRequestFormDto
                               {
                                   RequestFormDetailId = rfd.Id,
                                   MainRepoLocationName = mrl.Name,
                                   HospitalName = h.Name,
                                   RequestTypeName = rft.Name,
                                   RequestFormTypeId = rf.RequestFormTypeId,

                                   // YENİ EKLENEN BİLGİLER BURADA ÇEKİLİYOR
                                   Products = (from rp in _appDbContext.RequestProducts
                                               join p in _appDbContext.Products on rp.ProductId equals p.Id
                                               join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                               where rp.RequestFormId == rf.Id
                                               select new ProductDetailDto
                                               {
                                                   ProductName = p.Name,
                                                   ImageUrl = p.PhotoUrl,
                                                   Quantity = rp.Quantity,
                                                   CategoryName = c.Name,
                                                   OperationType = rp.OperationType, // Gönderim mi İade mi?
                                                   Label = rp.Label,                 // Cihaz etiketi
                                                   HasConfig = rp.ConnectionType != null // Config var mı?
                                               }).ToList(),

                                   RequestBy = rfd.RequestBy,
                                   RequestDate = rfd.RequestDate,
                                   RequestStatusId = rfd.StatusId,
                                   InstallationDate = rfd.InstallationDate,
                                   Description = rfd.Description,
                                   ReceiverName = rfd.ToPerson,
                                   Phone = rfd.Phone,
                                   HospitalAddress = h.Address,
                                   Persons = (from pd in _appDbContext.PersonDetails
                                              join u in _appDbContext.Users on pd.AppUserId equals u.Id
                                              where rfd.Id == pd.RequestFormDetailId
                                              select u.NameSurname).ToList(),
                               }).ToList();
            return View(requestList);
        }

        //Talep durumunu değiştirme
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRequestFormStatus(SaveRequestFormStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = "Lütfen formda kırmızı ile belirtilen zorunlu alanları doldurunuz.";                return RedirectToAction("RequestFormList");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            var now = DateTime.Now;
            var requestFormDetail = _appDbContext.RequestFormDetails.FirstOrDefault(x => x.Id == dto.RequestFormDetailId);
            if (requestFormDetail == null)
            {
                TempData["ErrorMessage"] = "Talep bulunamadı .";
                return RedirectToAction("RequestFormList");
            }

            requestFormDetail.StatusId = dto.StatusId;

            if (dto.StatusId == (int)EnumStatusType.İptal)
            {
                requestFormDetail.CanceledBy = currentUser.NameSurname;
                requestFormDetail.CanceledDate = now;
                requestFormDetail.CanceledDesc = dto.CancelledDesc;
            }
            else if (dto.StatusId == (int)EnumStatusType.OnayBekliyor)
            {
                requestFormDetail.RequestBy = currentUser.NameSurname;
                requestFormDetail.RequestDate = now;
            }

            _appDbContext.RequestFormDetails.Update(requestFormDetail);
            _appDbContext.SaveChanges();

            TempData["SuccessMessage"] = "Talep durumu değiştirildi.";
            return RedirectToAction("RequestFormList");
        }
    }
}