using Azure.Core;
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
        private readonly IMainRepoLocationService _mainRepoLocationService;
        private readonly IProductService _productService;
        private readonly ILocationListService _locationListService;
        private readonly UserManager<AppUser> _userManager;

        public RequestFormController(AppDbContext appDbContext, ICategoryService categoryService, IMainRepoLocationService mainRepoLocationService, IProductService productService, ILocationListService locationListService, UserManager<AppUser> userManager)
        {
            _appDbContext = appDbContext;
            _categoryService = categoryService;
            _mainRepoLocationService = mainRepoLocationService;
            _productService = productService;
            _locationListService = locationListService;
            _userManager = userManager;
        }

        // 🚀 İŞTE EKSİK OLAN VE 404 HATASINI ÇÖZEN METOT (Sayfayı ilk açan kısım)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var locations = await _appDbContext.LocationLists.AsNoTracking()
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

            ViewBag.Locations = locations;
            ViewBag.MainRepos = repos;
            ViewBag.Persons = persons;

            return View(new CreateRequestFormDto());
        }

        // FORM GÖNDERİLDİĞİNDE ÇALIŞAN KAYIT METODU
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRequestFormDto dto)
        {
            // ADIM 1) ItemsJson Çözümle          
            var items = new List<ItemDto>();

            if (string.IsNullOrWhiteSpace(dto.ItemsJson))
                ModelState.AddModelError("", "En az bir ürün eklemelisiniz.");
            else
            {
                try
                {
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    items = JsonSerializer.Deserialize<List<ItemDto>>(dto.ItemsJson ?? "[]", options) ?? new();
                    if (items.Count == 0)
                        ModelState.AddModelError("", "Ürün listesi boş olamaz.");
                }
                catch
                {
                    ModelState.AddModelError("", "Ürün listesi işlenemedi.");
                }
            }

            // ADIM 2) Görünüm verilerini her durumda doldur (Hata olursa sayfa dolu gelsin)
            ViewBag.Locations = await _appDbContext.LocationLists.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                .ToListAsync();

            ViewBag.MainRepos = await _appDbContext.MainRepoLocations.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name })
                .ToListAsync();

            ViewBag.Persons = await _userManager.Users
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.NameSurname })
                .ToListAsync();


            // ADIM 3) Sunucu Tarafı Doğrulamaları
            if (dto.MainRepoId <= 0) ModelState.AddModelError("", "Depo seçimi zorunludur.");
            if (dto.LocationId <= 0) ModelState.AddModelError("", "Lokasyon seçimi zorunludur.");
            if (dto.TypeId <= 0) ModelState.AddModelError("", "Talep türü zorunludur.");

            if (dto.TypeId == (int)EnumRequestType.Kargo)
            {
                if (string.IsNullOrWhiteSpace(dto.ReceiverFullName)) ModelState.AddModelError("", "Kargo için alıcı adı zorunludur.");
            }
            else if (dto.TypeId == (int)EnumRequestType.Kurulum || dto.TypeId == (int)EnumRequestType.Servis)
            {
                if (dto.InstallationDate == null) ModelState.AddModelError("", "Kurulum/Servis için tarih zorunludur.");
                if (dto.Persons == null || dto.Persons.Count == 0) ModelState.AddModelError("", "Kurulum/Servis için en az bir personel eklenmelidir.");
            }

            // Ürün satır kontrolleri ve AKILLI STOK
            var duplicate = new HashSet<string>();
            foreach (var it in items)
            {
                if (it.ProductId <= 0 || it.Quantity <= 0)
                {
                    ModelState.AddModelError("", "Ürün ve miktar bilgisi hatalı.");
                    continue;
                }

                // Aynı ürünü aynı işlem türüyle (Örn: 2 tane Gönderilecek aynı ArcBox) engelle
                string uniqueKey = $"{it.ProductId}_{it.OperationType}";
                if (!duplicate.Add(uniqueKey))
                    ModelState.AddModelError("", "Aynı ürünü aynı işlem türüyle birden fazla kez ekleyemezsiniz.");

                // STOK KONTROLÜ: Sadece Gönderim (OperationType == 1) ise stok kontrolü yap!
                if (it.OperationType == 1)
                {
                    var inRepoQty = await _appDbContext.ProductMainRepoLocations.AsNoTracking()
                        .Where(x => x.MainRepoLocationId == dto.MainRepoId && x.ProductId == it.ProductId).Select(x => (int?)x.Quantity).FirstOrDefaultAsync() ?? 0;

                    // Sadece daha önce gönderilenleri (OperationType == 1) dağıtılmış say
                    var distributed = await (from rp in _appDbContext.RequestProducts.AsNoTracking()
                                             join rf in _appDbContext.RequestForms.AsNoTracking() on rp.RequestFormId equals rf.Id
                                             where rp.ProductId == it.ProductId && rf.MainRepoLocationId == dto.MainRepoId && !rf.IsDeleted && rp.OperationType == 1
                                             select (int?)rp.Quantity).SumAsync() ?? 0;

                    var remaining = Math.Max(0, inRepoQty - distributed);
                    if (it.Quantity > remaining)
                        ModelState.AddModelError("", $"İstenen miktar stoktan fazla: {it.ProductName} için kalan {remaining}.");
                }
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = string.Join("<br>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                return View(dto);
            }

            // ADIM 4) Kayıt + Transaction
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
                    LocationListId = dto.LocationId,
                    RequestFormTypeId = dto.TypeId,
                    IsShipAfterReturn = dto.IsShipAfterReturn, // Yeni
                    IsOfficeDelivery = dto.IsOfficeDelivery,   // Yeni
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
                        // UI'dan text (örn: "Arıza / Bozuk") geliyor
                        ReasonId = int.TryParse(x.Reason, out int rId) ? rId : null,
                        ConnectionType = x.ArcBoxConfig?.ConnectionType,
                        ConfigUrl = x.ArcBoxConfig?.ConfigUrl,
                        DhcpdConf = x.ArcBoxConfig?.DhcpdConf,
                        WpaSupplicantConf = x.ArcBoxConfig?.WpaSupplicantConf
                    }).ToList();

                if (rpList.Count > 0)
                {
                    await _appDbContext.RequestProducts.AddRangeAsync(rpList);
                    await _appDbContext.SaveChangesAsync();
                }

                // DETAY KAYDI (KARGO / KURULUM VB)
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
                    RequestDate = now
                };

                if (dto.TypeId == (int)EnumRequestType.Kurulum || dto.TypeId == (int)EnumRequestType.Servis)
                {
                    if (dto.TypeId == (int)EnumRequestType.Servis)
                    {
                        requestFormDetail.StatusId = (int)EnumStatusType.Tamamlandı;
                        requestFormDetail.CompletedDate = now;
                    }
                    requestFormDetail.InstallationDate = dto.InstallationDate;
                    requestFormDetail.Description = dto.Note;

                    await _appDbContext.RequestFormDetails.AddAsync(requestFormDetail);
                    await _appDbContext.SaveChangesAsync();

                    if (dto.Persons != null && dto.Persons.Count > 0)
                    {
                        var personDetailList = dto.Persons.Select(uid => new PersonDetail
                        {
                            RequestFormDetailId = requestFormDetail.Id,
                            AppUserId = uid
                        }).ToList();

                        await _appDbContext.PersonDetails.AddRangeAsync(personDetailList);
                        await _appDbContext.SaveChangesAsync();
                    }
                    TempData["SuccessMessage"] = "Kurulum/Servis talebiniz başarıyla oluşturuldu.";
                }
                else if (dto.TypeId == (int)EnumRequestType.Kargo)
                {
                    requestFormDetail.ToPerson = dto.ReceiverFullName;
                    requestFormDetail.Phone = dto.Phone;

                    await _appDbContext.RequestFormDetails.AddAsync(requestFormDetail);
                    await _appDbContext.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Kargo talebiniz başarıyla oluşturuldu.";
                }

                await transaction.CommitAsync();
                return RedirectToAction("RequestFormList"); // Başarılıysa Listeye Yönlendir

            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Talep oluşturulurken bir hata oluştu: " + ex.Message;
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
                               join rl in _appDbContext.LocationLists on rf.LocationListId equals rl.Id
                               join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                               join rft in _appDbContext.RequestFormTypes on rf.RequestFormTypeId equals rft.Id
                               where rf.IsActive && !rf.IsDeleted && rfd.StatusId == (int)EnumStatusType.Talep && rfd.StatusId != (int)EnumStatusType.İptal
                               select new RequestedForRequestFormDto
                               {
                                   RequestFormDetailId = rfd.Id,
                                   MainRepoLocationName = mrl.Name,
                                   Location = rl.Name,
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
                                   Address = rl.Address,
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
                TempData["ErrorMessage"] = string.Join("<br>", errorMessages);
                return RedirectToAction("RequestFormList");
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