using DocumentFormat.OpenXml.Drawing;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.MainRepoLocation;
using StockTrack.Dto.Product;
using StockTrack.Dto.ProductInvoice;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;
using StockTrack.WebUI.Enums;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class MainRepoLocationController : Controller
    {
        private readonly IMainRepoLocationService _mainRepoLocationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IToastNotification _toastNotification;
        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly AppDbContext _appDbContext;

        public MainRepoLocationController(IMainRepoLocationService mainRepoLocationService, UserManager<AppUser> userManager, IToastNotification toastNotification, ICategoryService categoryService, IProductService productService, AppDbContext appDbContext)
        {
            _mainRepoLocationService = mainRepoLocationService;
            _userManager = userManager;
            _toastNotification = toastNotification;
            _categoryService = categoryService;
            _productService = productService;
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
            // Sadece silinmemiş kayıtları getir
            var getRepo = await _mainRepoLocationService.TGetFilteredListAsync(x => !x.IsDeleted);

            var dtoList = getRepo.Select(x => new ResultMainRepoLocationDto
            {
                Id = x.Id,
                Adress = x.Adress,
                Name = x.Name,
                CreatedDate = x.CreatedDate,
                IsActive = x.IsActive
            }).ToList();

            return View(dtoList);
        }

        public async Task<IActionResult> CreateMainRepoLocation(CreateMainRepoLocationDto createMainRepoLocationDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(); ;
                return RedirectToAction("Index");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var now = DateTime.Now;
            var toInsert = new MainRepoLocation()
            {
                Name = createMainRepoLocationDto.Name,
                Adress = createMainRepoLocationDto.Adress,
                IsActive = true,
                CreatedDate = now,
                ModifiedDate = now,
                CreatedBy = currentUser.NameSurname,

            };
            await _mainRepoLocationService.TCreateAsync(toInsert);
            _toastNotification.AddSuccessToastMessage("Lokasyon  kaydı başarıyla eklendi", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var location = await _mainRepoLocationService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });


            location.IsActive = false;
            await _mainRepoLocationService.TUpdateAsync(location);
            return Ok(new { success = true, message = $"{location.Name} pasif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Activate(int id)
        {
            var location = await _mainRepoLocationService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });


            location.IsActive = true;
            await _mainRepoLocationService.TUpdateAsync(location);
            return Ok(new { success = true, message = $"{location.Name} aktif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _mainRepoLocationService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            location.IsDeleted = true;
            location.DeletedDate = DateTime.Now;
            location.DeletedBy = currentUser.NameSurname;
            await _mainRepoLocationService.TUpdateAsync(location);
            return Ok(new
            {
                success = true,
                message = $"{location.Name} başarıyla silindi."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateMainRepoLocationDto updateMainRepoLocationDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(); ;
                return RedirectToAction("Index");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();


            var findRepo = await _mainRepoLocationService.TGetByIdAsync(updateMainRepoLocationDto.Id);
            //string pageUrl  = await _mainRepoLocationService.SetPageUrl(updateMainRepoLocationDto.Name);

            //if (findRepo.PageUrl != pageUrl)
            //{
            //    TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList(); ;
            //    return RedirectToAction("Index");
            //}

            findRepo.Adress = updateMainRepoLocationDto.Adress;
            findRepo.Name = updateMainRepoLocationDto.Name;
            findRepo.ModifiedBy = currentUser.NameSurname;
            findRepo.ModifiedDate = DateTime.Now;

            await _mainRepoLocationService.TUpdateAsync(findRepo);
            _toastNotification.AddSuccessToastMessage("Lokasyon  kaydı başarıyla güncellendi", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }

        [HttpGet]
        //Silinen lokasyonlar
        public async Task<IActionResult> Deleted()
        {
            var getLocationDeleted = (await _mainRepoLocationService.TGetFilteredListAsync(x => x.IsDeleted == true)).Select(x => new DeletedMainRepoLocationListDto
            {
                Adress = x.Adress,
                CreatedDate = x.CreatedDate,
                DeletedBy = x.DeletedBy,
                DeletedDate = x.DeletedDate,
                Id = x.Id,
                IsActive = x.IsActive,
                Name = x.Name
            }).ToList();

            return View(getLocationDeleted);
        }


        public async Task<IActionResult> ProductList()
        {
            var mainRepoLocations = await _mainRepoLocationService.TGetFilteredListAsync(x => !x.IsDeleted && x.IsActive);
            ViewBag.MainRepoLocations = mainRepoLocations.Select(x => new SelectListItem { Value = x.Id.ToString(), Text = x.Name }).ToList();

            var categories = await _categoryService.TGetFilteredListAsync(c => !c.IsDeleted && c.IsActive);
            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();


            var resultProductList = await (
           from pml in _appDbContext.ProductMainRepoLocations
           join p in _appDbContext.Products on pml.ProductId equals p.Id
           join c in _appDbContext.Categories on p.CategoryId equals c.Id
           join mrl in _appDbContext.MainRepoLocations on pml.MainRepoLocationId equals mrl.Id
           where !p.IsDeleted && p.IsActive && !c.IsDeleted && !mrl.IsDeleted
           let distributedQuantity = (
               from rp in _appDbContext.RequestProducts
               join rf in _appDbContext.RequestForms on rp.RequestFormId equals rf.Id
               join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
               where rp.ProductId == pml.ProductId && rf.MainRepoLocationId == pml.MainRepoLocationId && !rf.IsDeleted && (rfd.StatusId == (int)EnumStatusType.Tamamlandı || rfd.StatusId == (int)EnumStatusType.Kargoda)  // teslim edilmiş ürünler çıkarılır
               select (int?)rp.Quantity
           ).Sum() ?? 0
           select new ResultProductMainRepoDto
           {
               // Kimlikler
               Id = pml.MainRepoLocationId,
               ProductId = pml.ProductId,
               CategoryId = c.Id,

               // Depo ve kategori bilgileri                          
               MainRepoLocationName = mrl.Name,
               CategoryName = c.Name,

               // Ürün bilgileri
               ProductName = p.Name,
               ProductModel = p.Model,
               ProductDescription = p.Description,
               PhotoUrl = p.PhotoUrl,

               // Durum bilgileri
               CreatedDate = p.CreatedDate,
               IsActive = p.IsActive,

               // Miktarlar
               TotalQuantity = pml.Quantity,
               DistributedQuantity = distributedQuantity,
               RemainingQuantity = pml.Quantity - distributedQuantity
           }
       ).OrderByDescending(x => x.Id).ToListAsync();


            return View(resultProductList);
        }
        //Kategoriye göre ürünleri getirme
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            if (categoryId <= 0)
                return Json("Kayıt bulunamadı");

            var products = await _productService.TGetFilteredListAsync(x => !x.IsDeleted && x.IsActive && x.CategoryId == categoryId);
            var result = products.Select(x => new { id = x.Id, name = x.Name, model = x.Model });
            return Json(result);
        }

        //Depoya ürün atama 
        [HttpPost]
        public async Task<IActionResult> AssignProductToDepo(CreateProductToDepo createProductToDepo)
        {
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();

                TempData["ErrorMessage"] = string.Join("<br>", allErrors);
                return RedirectToAction("ProductList");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            //  Aynı depoda aynı ürün var mı kontrol et
            var existingEntry = await _appDbContext.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.MainRepoLocationId == createProductToDepo.MainRepoLocationId && x.ProductId == createProductToDepo.ProductId);

            if (existingEntry != null)
            {
                TempData["ErrorMessage"] = "Bu depo için seçilen ürün zaten mevcut!";
                return RedirectToAction("ProductList");
            }

            //  ProductMainRepoLocation kaydı oluştur
            var newRepo = new ProductMainRepoLocation
            {
                MainRepoLocationId = createProductToDepo.MainRepoLocationId,
                ProductId = createProductToDepo.ProductId,
                Quantity = createProductToDepo.Quantity
            };

            _appDbContext.ProductMainRepoLocations.Add(newRepo);

            // İrsaliye kaydı ekle

            var now = DateTime.Now;
            var invoice = new ProductInvoice
            {
                MainRepoId = createProductToDepo.MainRepoLocationId,
                ProductId = createProductToDepo.ProductId,
                Quantity = createProductToDepo.Quantity,
                InvoiceNumber = createProductToDepo.InvoiceNumber,
                InvoiceDate = createProductToDepo.InvoiceDate,
                Description = createProductToDepo.Description,
                CreatedBy = currentUser.NameSurname,
                CreatedDate = now,
                ModifiedDate = now,
                IsDeleted = false,
                IsActive = true,

            };
            _appDbContext.ProductInvoices.Add(invoice);


            await _appDbContext.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ürün başarıyla depoya eklendi!";
            return RedirectToAction("ProductList");
        }

        //Stok artırma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IncreaseStock(IncreaseStockDto dto)
        {
            if (dto.IncreaseBy <= 0)
            {
                TempData["ErrorMessage"] = "Eklenecek miktar 1 veya daha büyük olmalı.";
                return RedirectToAction("ProductList");
            }
            var product = await _productService.TGetByIdAsync(dto.ProductId);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün bulunamadı.";
                return RedirectToAction("ProductList");
            }
            var mainrepo = await _mainRepoLocationService.TGetByIdAsync(dto.MainRepoId);
            if (mainrepo == null)
            {
                TempData["ErrorMessage"] = "Depo bulunamadı.";
                return RedirectToAction("ProductList");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            try
            {
                // 1) ProductMainRepoLocation kaydını bul
                var productRepo = await _appDbContext.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.MainRepoLocationId == dto.MainRepoId && x.ProductId == dto.ProductId);

                // 2) Stok artır
                productRepo.Quantity += dto.IncreaseBy;

                DateTime now = DateTime.Now;

                // 3) İrsaliye kaydı oluştur
                var invoice = new ProductInvoice
                {
                    MainRepoId = mainrepo.Id,
                    ProductId = dto.ProductId,
                    Quantity = dto.IncreaseBy,
                    Description = dto.Description,
                    InvoiceNumber = dto.InvoiceNumber,
                    InvoiceDate = dto.InvoiceDate,
                    CreatedDate = now,
                    ModifiedDate = now,
                    CreatedBy = currentUser.NameSurname,
                    IsActive = true,
                    IsDeleted = false,
                };

                _appDbContext.ProductInvoices.Add(invoice);

                await _appDbContext.SaveChangesAsync();

                TempData["SuccessMessage"] = $"'{dto.ProductName}' için depo '{mainrepo.Name}' stok {dto.IncreaseBy} artırıldı ve irsaliye kaydı oluşturuldu.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"İşlem sırasında bir hata oluştu: {ex.Message}";
            }

            return RedirectToAction("ProductList");
        }

        //Stok azaltma
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DecreaseStock(int mainRepoId, int productId, int quantityToDecrease, string description)
        {
            if (mainRepoId <= 0 || productId <= 0 || quantityToDecrease <= 0)
            {
                TempData["ErrorMessage"] = "Geçersiz parametre";
                return RedirectToAction("ProductList");
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                var productMainRepoLocation = await _appDbContext.ProductMainRepoLocations.FirstOrDefaultAsync(m => m.MainRepoLocationId == mainRepoId && m.ProductId == productId);
                if (productMainRepoLocation == null)
                {
                    TempData["ErrorMessage"] = "Ürün bu depoda bulunamadı.";
                    return RedirectToAction("ProductList");
                }

                var requestedQuantity = await (from rf in _appDbContext.RequestForms
                                               join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                               join rp in _appDbContext.RequestProducts on rf.Id equals rp.RequestFormId
                                               where rf.MainRepoLocationId == mainRepoId && rp.ProductId == productId && rfd.StatusId == (int)EnumStatusType.Tamamlandı
                                               select rp.Quantity).SumAsync(); 

                var usableStock = productMainRepoLocation.Quantity - requestedQuantity;

              

                if (usableStock < quantityToDecrease)
                {
                    TempData["ErrorMessage"] = "Stok miktarından büyük sayı girmeyiniz.";
                    return RedirectToAction("ProductList");
                }

                productMainRepoLocation.Quantity -= quantityToDecrease;

                var productInvoice = new ProductInvoice
                {
                    MainRepoId = mainRepoId,
                    ProductId = productId,
                    Quantity = -quantityToDecrease,
                    Description = description,
                    CreatedBy = currentUser.NameSurname,
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };

                _appDbContext.ProductInvoices.Add(productInvoice);

                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // İşlem başarılı ise TempData'ya mesaj ekle
                TempData["SuccessMessage"] = "Stok başarıyla güncellendi.";
                return RedirectToAction("ProductList");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Stok güncellenirken bir hata oluştu işlemler geri alındı.";
                return RedirectToAction("ProductList");
            }
        }

        //irsaliye detayı
        [HttpGet]
        public async Task<IActionResult> GetProductInvoices(int productId, int mainRepoId)
        {
            var invoices = await _appDbContext.ProductInvoices
                .Where(pi => pi.ProductId == productId && pi.MainRepoId == mainRepoId)
                .Select(pi => new
                {
                    pi.InvoiceNumber,
                    InvoiceDate = pi.InvoiceDate.HasValue ? pi.InvoiceDate.Value.ToString("dd.MM.yyyy") : "",
                    pi.Quantity,
                    pi.Description
                })
                .ToListAsync();

            return Json(invoices);
        }

        //Depoda bulunan ürünün çıkış kayıtları
        [HttpGet]
        public async Task<IActionResult> GetStockDetails(int mainRepoId, int productId)
        {
            var requestDetails = await (from rf in _appDbContext.RequestForms
                                        join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                        join rp in _appDbContext.RequestProducts on rf.Id equals rp.RequestFormId
                                        join product in _appDbContext.Products on rp.ProductId equals product.Id
                                        join category in _appDbContext.Categories on product.CategoryId equals category.Id
                                        where rf.MainRepoLocationId == mainRepoId && rp.ProductId == productId && ( rfd.StatusId == (int)EnumStatusType.Tamamlandı || rfd.StatusId == (int)EnumStatusType.Kargoda)
                                        select new { rf, rfd, rp, product, category }).ToListAsync();

            var resultList = new List<StockDetailDto>();
            foreach (var g in requestDetails.GroupBy(x => new { x.rf.Id, RequestFormDetailId = x.rfd.Id }))
            {
                var firstItem = g.First();
                var persons = await _appDbContext.PersonDetails.Where(pd => pd.RequestFormDetailId == g.Key.RequestFormDetailId).Select(pd => pd.AppUser.NameSurname).ToListAsync();

                foreach (var item in g)
                {
                    var dto = new StockDetailDto
                    {
                        MainRepoName = _appDbContext.MainRepoLocations.Where(m => m.Id == mainRepoId).Select(m => m.Name).FirstOrDefault(),
                        LocationName = _appDbContext.LocationLists.Where(l => l.Id == firstItem.rf.LocationListId).Select(l => l.Name).FirstOrDefault(),
                        LocationAddress = _appDbContext.LocationLists.Where(l => l.Id == firstItem.rf.LocationListId).Select(l => l.Address).FirstOrDefault(),

                        CategoryName = item.category.Name,
                        ProductName = item.product.Name,
                        Quantity = item.rp.Quantity,

                        Persons = string.Join(", ", persons),

                        CreatedBy = firstItem.rf.CreatedBy,
                        CreatedDate = firstItem.rf.CreatedDate,
                        RequestBy = firstItem.rfd.RequestBy,
                        RequestDate = firstItem.rfd.RequestDate,
                        ApprovalDate = firstItem.rfd.ApprovalDate,
                        CompletedDate = firstItem.rfd.CompletedDate,
                        Description = firstItem.rfd.Description,
                        TrackingNumber = firstItem.rfd.TrackingNumber,
                        Adress = firstItem.rfd.Adress,
                        ToPerson = firstItem.rfd.ToPerson,
                        Phone = firstItem.rfd.Phone,
                        PackingDate = firstItem.rfd.PackingDate,
                        CargoGivenDate = firstItem.rfd.CargoGivenDate,
                        CargoName = _appDbContext.CargoNames.Where(c => c.Id == firstItem.rfd.CargoNameId).Select(c => c.Name).FirstOrDefault(),
                        InstallationDate = firstItem.rfd.InstallationDate,
                        StatusName = ((EnumStatusType)firstItem.rfd.StatusId).GetDisplayName(),
                        RequestFormType = ((EnumRequestType)firstItem.rf.RequestFormTypeId).GetDisplayName()
                    };
                    resultList.Add(dto);
                }
            }

            return Json(resultList);
        }

    }

}

