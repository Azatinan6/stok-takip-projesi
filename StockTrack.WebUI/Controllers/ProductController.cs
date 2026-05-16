using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.Business.Extension.ImageManagement;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Product;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;
using StockTrack.WebUI.Enums;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IImageService _imageService;
        private readonly IToastNotification _toastNotification;
        private readonly AppDbContext _appDbContext;
        private readonly IMainRepoLocationService _mainRepoLocationService;
        public ProductController(IProductService productService, UserManager<AppUser> userManager, ICategoryService categoryService, IImageService imageService, IToastNotification toastNotification, AppDbContext appDbContext, IMainRepoLocationService mainRepoLocationService)
        {
            _productService = productService;
            _userManager = userManager;
            _categoryService = categoryService;
            _imageService = imageService;
            _toastNotification = toastNotification;
            _appDbContext = appDbContext;
            _mainRepoLocationService = mainRepoLocationService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.TGetFilteredListAsync(c => !c.IsDeleted && c.IsActive);
            ViewBag.Categories = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name }).ToList();

            var resultProductList = await _appDbContext.Products
                .Where(p => !p.IsDeleted && p.Category != null && !p.Category.IsDeleted)
                .Select(p => new ResultProductDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Model = p.Model,
                    WarningThreshold = p.WarningThreshold,
                    CreatedDate = p.CreatedDate,
                    IsActive = p.IsActive,
                    PhotoUrl = p.PhotoUrl,
                    Description = p.Description,
                    CategoryId = p.Category.Id,
                    CategoryName = p.Category.Name,
                }).OrderByDescending(x => x.Id).ToListAsync();

            return View(resultProductList);
        }


        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Create(CreateProductDto createProductDto)
        {
            // Model doğrulamasını kontrol et
            if (!ModelState.IsValid)
            {
                var allErrors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                TempData["ErrorMessage"] = string.Join("<br>", allErrors);
                return RedirectToAction("Index");
            }

            // Mevcut kullanıcıyı al
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            // Ürün adını normalize et (trim + küçük harf)
            var textInfo = new System.Globalization.CultureInfo("tr-TR").TextInfo;
            var nameNorm = textInfo.ToTitleCase((createProductDto.Name ?? string.Empty).Trim().ToLower());
            createProductDto.Model = textInfo.ToTitleCase((createProductDto.Model ?? string.Empty).Trim().ToLower());

            // Aynı kategori ve aynı isimdeki ürünü al
            var existingProduct = await _productService.TGetExistingCategoryProductAsync(createProductDto.CategoryId, nameNorm);

            // Ürün bulundu mu?
            if (existingProduct != null)
            {
                if (!existingProduct.IsDeleted)
                {
                    // Aktif ürün zaten var
                    TempData["ErrorMessage"] = $"Aynı kategoride '{createProductDto.Name}' adıyla zaten bir ürün mevcut, ekleme yapılmadı.";
                }
                else
                {
                    // Silinmiş ürün var, geri yükleme uyarısı ver
                    TempData["HasDeletedProducts"] = true;
                    TempData["DeletedProductName"] = existingProduct.Name;
                    TempData["DeletedProductId"] = existingProduct.Id;
                    TempData["WarningMessage"] = $"Bu ürün daha önce silinmiş: {existingProduct.Name}. Geri yüklemek ister misiniz?";
                }

                return RedirectToAction("Index");
            }


            // Yeni ürün nesnesini hazırla
            var now = DateTime.Now;
            var newProduct = new Product
            {
                Name = nameNorm,
                CategoryId = createProductDto.CategoryId,
                Model = createProductDto.Model,
                WarningThreshold = createProductDto.WarningThreshold,
                CreatedBy = currentUser.NameSurname,
                CreatedDate = now,
                ModifiedDate = now,
                Description = createProductDto.Description,
                IsActive = true,
                PhotoUrl = null
            };

            // Fotoğraf yüklenmişse kaydet
            if (createProductDto.PhotoUrl != null)
            {
                newProduct.PhotoUrl = await _imageService.SaveImageAsync(createProductDto.PhotoUrl, "productimages");
            }

            // Veritabanına kaydet
            await _productService.TCreateAsync(newProduct);

            // Başarı bildirimi göster
            _toastNotification.AddSuccessToastMessage("Ürün kaydı başarıyla gerçekleştirildi", new ToastrOptions { Title = "Başarılı" }
            );

            return RedirectToAction("Index");
        }

        // [HttpPost]
        // [Authorize(Roles = RoleConsts.Admin)]
        // public async Task<IActionResult> RestoreDeletedProduct([FromBody] int productId)
        // {
        //     if (productId <= 0)
        //         return BadRequest("Geçersiz ürün kimliği.");

        //     var product = await _productService.TGetByIdAsync(productId);
        //     if (product == null)
        //         return NotFound("Ürün bulunamadı.");

        //     if (!product.IsDeleted)
        //         return BadRequest("Bu ürün silinmiş değil.");

        //     var currentUser = await _userManager.GetUserAsync(User);
        //     var now = DateTime.Now;

        //     product.IsDeleted = false;
        //     product.IsActive = true;
        //     product.ModifiedDate = now;
        //     product.ModifiedBy = currentUser?.NameSurname;

        //     await _productService.TUpdateAsync(product);

        //     return Ok(new { status = "ok", message = $"'{product.Name}' ürünü başarıyla geri yüklendi." });
        // }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var product = await _productService.TGetByIdAsync(id);
            if (product == null)
                return NotFound(new { success = false, message = "Ürün bulunamadı." });


            product.IsActive = false;
            await _productService.TUpdateAsync(product);
            return Ok(new { success = true, message = $"{product.Name} pasif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Activate(int id)
        {
            var product = await _productService.TGetByIdAsync(id);
            if (product == null)
                return NotFound(new { success = false, message = "Ürün bulunamadı." });


            product.IsActive = true;
            await _productService.TUpdateAsync(product);
            return Ok(new { success = true, message = $"{product.Name} aktif hale getirildi." });
        }

        // [HttpPost]
        // [Authorize(Roles = RoleConsts.Admin)]
        // public async Task<IActionResult> DeleteProduct(int id)
        // {
        //     var product = await _productService.TGetByIdAsync(id);
        //     if (product == null)
        //         return NotFound(new { success = false, message = "Ürün bulunamadı." });

        //     var currentUser = await _userManager.GetUserAsync(User);
        //     if (currentUser == null)
        //         return Challenge();

        //     product.IsDeleted = true;
        //     product.DeletedDate = DateTime.Now;
        //     product.DeletedBy = currentUser.NameSurname;
        //     await _productService.TUpdateAsync(product);
        //     return Ok(new
        //     {
        //         success = true,
        //         message = $"{product.Name} başarıyla silindi."
        //     });
        // }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Edit(UpdateProductDto updateProductDto, IFormFile? PhotoFile, bool IsPhotoRemoved)
        {
            var product = await _productService.TGetByIdAsync(updateProductDto.Id);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Ürün güncellenemedi. Lütfen alanları kontrol edin.";
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                // Oturum açmamışsa giriş sayfasına yönlendir
                return Challenge();

            var textInfo = new System.Globalization.CultureInfo("tr-TR").TextInfo;
            updateProductDto.Name = textInfo.ToTitleCase((updateProductDto.Name ?? string.Empty).Trim().ToLower());
            updateProductDto.Model = textInfo.ToTitleCase((updateProductDto.Model ?? string.Empty).Trim().ToLower());
            // Veritabanında bu isim varmı kontrolu
            if (product.Name.ToLower() != updateProductDto.Name.ToLower())
            {
                var existsProduct = await _productService.TExistsProductAsync(updateProductDto.Name);
                if (existsProduct)
                {
                    TempData["ErrorMessage"] = $"{updateProductDto.Name}  zaten mevcut kayıt güncellenmedi.";
                    return RedirectToAction("Index");
                }
            }

            product.Name = updateProductDto.Name;
            product.CategoryId = updateProductDto.CategoryId;
            product.Description = updateProductDto.Description;
            product.ModifiedBy = currentUser.NameSurname;
            product.ModifiedDate = DateTime.Now;
            product.Model = updateProductDto.Model;
            product.WarningThreshold = updateProductDto.WarningThreshold;
            // Resim kaldırılmış ise
            if (IsPhotoRemoved && (PhotoFile == null || PhotoFile.Length == 0))
            {
                if (!string.IsNullOrEmpty(product.PhotoUrl))
                {
                    var oldPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/productimages", product.PhotoUrl);
                    if (System.IO.File.Exists(oldPath))
                        System.IO.File.Delete(oldPath);

                    product.PhotoUrl = null;
                }
            }

            //  Yeni resim yüklendiyse
            if (PhotoFile != null && PhotoFile.Length > 0)
            {
                // Önce eski fotoğraf varsa sil 
                if (!string.IsNullOrEmpty(product.PhotoUrl))
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/productimages", product.PhotoUrl);
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }

                // Yeni fotoğraf      
                product.PhotoUrl = await _imageService.SaveImageAsync(PhotoFile, "productimages");
            }

            await _productService.TUpdateAsync(product);
            _toastNotification.AddSuccessToastMessage("Ürün başarıyla güncellendi", new ToastrOptions { Title = "Başarılı" });

            return RedirectToAction("Index");
        }

        // [HttpGet]        
        // public async Task<IActionResult> Deleted()
        // {
        //     var result = await _appDbContext.Products.Where(p => p.IsDeleted).Select(p => new DeletedProductDto
        //     {
        //         Id = p.Id,
        //         CategoryName = p.Category != null ? p.Category.Name : null,
        //         Name = p.Name,
        //         Model = p.Model,
        //         DeletedBy = p.DeletedBy,
        //         DeletedDate = p.DeletedDate,
        //         Description = p.Description,
        //         IsActive = p.IsActive,
        //         PhotoUrl = p.PhotoUrl,
        //         CreatedDate = p.CreatedDate,
        //     }).OrderByDescending(x => x.DeletedDate).ToListAsync();
        //     return View(result);
        // }

        //ürünün hangi depoda kaç adet oldugunu gösterme

        [HttpGet]
        public async Task<IActionResult> GetProductStockDetails(int productId)
        {
            // Depodaki ürünleri alıyoruz (Hiç dağıtılmamış olsa bile listeye gelir!)
            var productStock = await _appDbContext.ProductMainRepoLocations
                .Include(pml => pml.MainRepoLocation)
                .Where(pml => pml.ProductId == productId)
                .Select(pml => new ProductStockDto
                {
                    MainRepoName = pml.MainRepoLocation.Name,
                    TotalQuantity = pml.Quantity,

                    // Sadece bu depodan ve bu üründen "Kargoda" veya "Tamamlandı" olanları topla
                    DistributedQuantity = _appDbContext.RequestProducts
                        .Where(rp => rp.ProductId == pml.ProductId
                                && rp.RequestForm.MainRepoLocationId == pml.MainRepoLocationId
                                && !rp.RequestForm.IsDeleted
                                && rp.RequestForm.RequestFormDetails.Any(rfd =>
                                        rfd.StatusId == (int)EnumStatusType.Kargoda ||
                                        rfd.StatusId == (int)EnumStatusType.Tamamlandı))
                        .Sum(rp => (int?)rp.Quantity) ?? 0
                }).ToListAsync();

            // Kalan miktarı C# tarafında hesaplıyoruz (SQL'i yormamak için)
            foreach (var item in productStock)
            {
                item.RemainingQuantity = item.TotalQuantity - item.DistributedQuantity;
            }

            if (!productStock.Any())
            {
                return Json(new { message = "Bu ürün için stok kaydı bulunamadı." });
            }

            return Json(productStock);
        }

        // [HttpPost]
        // public async Task<IActionResult> Restore(int id)
        // {
        //     var product = await _productService.TGetByIdAsync(id);

        //     if(product != null)
        //     {
        //         product.IsDeleted = false;
        //         product.IsActive = true;
        //         product.DeletedDate = null;

        //         await _productService.TUpdateAsync(product);
        //         TempData["SuccessMessage"] = "Ürün başarıyla geri yüklendi.";
        //     }
        //     else
        //     {
        //         TempData["ErrorMessage"] = "Ürün bulunamadı.";

        //     }
        //     return RedirectToAction("Deleted");
        // }
    }
}
