using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockTrack.DataAccess.Context; // Senin Context yolun
using StockTrack.Dto.Inventory;
using StockTrack.Entity.Enitities;
using Microsoft.EntityFrameworkCore;

namespace StockTrack.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public InventoryController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public IActionResult AddProduct()
        {
            // Senin DbSet'lerine tam uygun şekilde çekiyoruz
            ViewBag.Warehouses = new SelectList(_context.MainRepoLocations.Where(x => !x.IsDeleted), "Id", "Name");
            ViewBag.Categories = new SelectList(_context.Categories.Where(x => !x.IsDeleted), "Id", "Name");

            // "Yeni, Kullanılmış, Arızalı" tanımlarını StatusTypes tablosundan çekiyoruz
            ViewBag.ProductStatuses = new SelectList(_context.StatusTypes, "Id", "Name");

            return View(new InventoryAddDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(InventoryAddDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Warehouses = new SelectList(_context.MainRepoLocations.Where(x => !x.IsDeleted), "Id", "Name");
                ViewBag.Categories = new SelectList(_context.Categories.Where(x => !x.IsDeleted), "Id", "Name");
                ViewBag.ProductStatuses = new SelectList(_context.StatusTypes, "Id", "Name");
                return View(dto);
            }

            // 1. Görseli Sunucuya Yükleme
            string photoUrl = null;
            if (dto.ProductImage != null && dto.ProductImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ProductImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProductImage.CopyToAsync(fileStream);
                }
                photoUrl = "/images/products/" + uniqueFileName;
            }

            // 2. Ürünü Products Tablosuna Ekleme
            var newProduct = new Product
            {
                Name = dto.ProductName,
                Brand = dto.Brand, // Eğer Product.cs içine public string Brand { get; set; } eklediysen
                Model = dto.Model,
                Description = dto.Description,
                WarningThreshold = dto.StockWarningLevel,
                CategoryId = dto.CategoryId,
                PhotoUrl = photoUrl,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync(); // Burada SQL, ürüne yeni bir ID atıyor (newProduct.Id)

            // 3. Stok ve Depo Bilgisini Köprü Tabloya Ekleme (Senin Composite Key Mimarine Göre)
            var newStock = new ProductMainRepoLocation
            {
                ProductId = newProduct.Id,
                MainRepoLocationId = dto.WarehouseId,
                Quantity = dto.StockQuantity,

                // Eğer ProductMainRepoLocation sınıfına ProductStatusId (int) eklediysen burayı da açabilirsin:
                // ProductStatusId = dto.ProductStatusId 
            };

            // Not: Composite key olduğu için Add() yaparken Id vermiyoruz, DbContext o ikiliyi birleştirip kaydedecek.
            _context.ProductMainRepoLocations.Add(newStock);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ürün depoya başarıyla eklendi!";

            // Stok listesi sayfasına gönder (O sayfayı Stock adıyla tasarlamıştık)
            return RedirectToAction("Stock");
        }
        [HttpGet]
        public async Task<IActionResult> Stock()
        {
            // Veritabanındaki stokları; Ürün, Kategori ve Depo bilgileriyle birleştirerek çekiyoruz
            var stockList = await _context.ProductMainRepoLocations
                .Include(pm => pm.Product)
                    .ThenInclude(p => p.Category)
                .Include(pm => pm.MainRepoLocation)
                .Where(x => !x.IsDeleted && !x.Product.IsDeleted)
                .Select(x => new InventoryListDto
                {
                    Id = x.ProductId, // Düzenle/Sil butonları için Ürün ID'sini alıyoruz
                    WarehouseName = x.MainRepoLocation.Name,
                    CategoryName = x.Product.Category != null ? x.Product.Category.Name : "Kategori Yok",
                    Brand = x.Product.Brand,
                    Model = x.Product.Model,
                    ProductName = x.Product.Name,
                    ImageUrl = x.Product.PhotoUrl,
                    StockQuantity = x.Quantity, // Sen entity'de Quantity olarak değiştirdiğin için burayı öyle ayarladım
                    StockWarningLevel = x.Product.WarningThreshold ?? 5,
                    IsActive = x.IsActive,
                    IsArcBox = x.Product.Name.Contains("Arc Box") // PDF'teki özel Arc Box mantığı için
                }).ToListAsync();

            return View(stockList);
        }
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return NotFound();

            // Ana stok sayısını alıyoruz
            var stockInfo = await _context.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.ProductId == id);
            int currentStock = stockInfo != null ? stockInfo.Quantity : 0;

            // Ürünün tüm hareket geçmişini tarihe göre yeniden eskiye doğru çekiyoruz
            var movements = await _context.StockMovements
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var isArcBox = product.Name != null && product.Name.Contains("Arc Box");

            // Eğer Arc Box ise seri numaralarını da çekiyoruz
            var serials = new List<ProductSerialNumber>();
            if (isArcBox)
            {
                serials = await _context.ProductSerialNumbers
                    .Where(x => x.ProductId == id && !x.IsDeleted)
                    .ToListAsync();
            }

            // DTO'yu doldurup View'a gönderiyoruz
            var dto = new InventoryDetailDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Brand = product.Brand,
                Model = product.Model,
                IsArcBox = isArcBox,
                CurrentStock = currentStock,

                // Gelen listeyi IN ve OUT olarak ikiye ayırıyoruz
                InboundMovements = movements.Where(m => m.MovementType == "IN")
                    .Select(m => new MovementDto { Date = m.CreatedDate, Quantity = m.MovementQuantity, MovementStatusId = m.MovementStatusId, Description = m.Description }).ToList(),

                OutboundMovements = movements.Where(m => m.MovementType == "OUT")
                    .Select(m => new MovementDto { Date = m.CreatedDate, Quantity = m.MovementQuantity, MovementStatusId = m.MovementStatusId, Description = m.Description }).ToList(),

                SerialNumbers = serials.Select(s => new ArcBoxSerialDto { SerialNumber = s.SerialNumber, EthMac = s.EthMac, WlanMac = s.WlanMac }).ToList()
            };

            return View(dto);
        }
        [HttpPost]
        public async Task<IActionResult> AddStock(StockMovementDto dto)
        {
            // 1. Ürünün depodaki ana stok kaydını buluyoruz
            var existingStock = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

            if (existingStock == null)
            {
                TempData["ErrorMessage"] = "Bu ürün depoda bulunamadı!";
                return RedirectToAction("Detail", new { id = dto.ProductId });
            }

            // 2. Eski ve Yeni stok sayılarını hesaplıyoruz
            int oldStock = existingStock.Quantity;
            int newStock = oldStock + dto.Quantity;

            // 3. Hareket defterine (StockMovement) bu işlemi 'GİRİŞ (IN)' olarak yazıyoruz
            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                MainRepoLocationId = existingStock.MainRepoLocationId,
                MovementType = "IN", // Bu bir stok girişidir
                OldStockQuantity = oldStock,
                NewStockQuantity = newStock,
                MovementQuantity = dto.Quantity,
                MovementStatusId = dto.MovementStatusId,
                ProductStatusId = dto.ProductStatusId, // Yeni, Kullanılmış vb.
                Description = dto.Description,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _context.StockMovements.Add(movement);

            // 4. Ana stok defterindeki sayıyı güncelliyoruz
            existingStock.Quantity = newStock;
            _context.ProductMainRepoLocations.Update(existingStock);

            // 5. Her şeyi SQL'e kaydedip aynı sayfaya geri dönüyoruz
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{dto.Quantity} adet ürün başarıyla stoğa eklendi.";

            return RedirectToAction("Detail", new { id = dto.ProductId });
        }


        [HttpPost]
        public async Task<IActionResult> RemoveStock(StockMovementDto dto)
        {
            // 1. Ürünün depodaki ana stok kaydını buluyoruz
            var existingStock = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

            if (existingStock == null)
            {
                return RedirectToAction("Detail", new { id = dto.ProductId });
            }

            // ÇOK ÖNEMLİ KONTROL: Depoda yeterli ürün var mı?
            if (existingStock.Quantity < dto.Quantity)
            {
                TempData["ErrorMessage"] = $"Hata! Depoda sadece {existingStock.Quantity} adet ürün var. {dto.Quantity} adet çıkış yapamazsınız.";
                return RedirectToAction("Detail", new { id = dto.ProductId });
            }

            // 2. Eski ve Yeni stok sayılarını hesaplıyoruz
            int oldStock = existingStock.Quantity;
            int newStock = oldStock - dto.Quantity; // Bu sefer eksi (-) yapıyoruz

            // 3. Hareket defterine bu işlemi 'ÇIKIŞ (OUT)' olarak yazıyoruz
            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                MainRepoLocationId = existingStock.MainRepoLocationId,
                MovementType = "OUT", // Bu bir stok çıkışıdır
                OldStockQuantity = oldStock,
                NewStockQuantity = newStock,
                MovementQuantity = dto.Quantity,
                MovementStatusId = dto.MovementStatusId,
                ProductStatusId = existingStock.ProductStatusId, // Çıkışta ürünün mevcut durumunu baz alıyoruz
                Description = dto.Description,
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _context.StockMovements.Add(movement);

            // 4. Ana stok defterindeki sayıyı güncelliyoruz
            existingStock.Quantity = newStock;
            _context.ProductMainRepoLocations.Update(existingStock);

            // 5. SQL'e kaydet ve geri dön
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{dto.Quantity} adet ürün stoktan düşüldü.";

            return RedirectToAction("Detail", new { id = dto.ProductId });
        }
        // 1. AKTİF / PASİF YAPMA METODU
        [HttpGet]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            // Listede Id olarak ProductId göndermiştik, o ürünü ve deposunu buluyoruz
            var stockItem = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (stockItem != null)
            {
                // Durumu tam tersine çevir (Aktifse Pasif, Pasifse Aktif yap)
                stockItem.IsActive = !stockItem.IsActive;

                _context.ProductMainRepoLocations.Update(stockItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kayıt durumu başarıyla güncellendi!";
            }

            // İşlem bitince tekrar Stok listesine dön
            return RedirectToAction("Stock");
        }

        // 2. DÜZENLE SAYFASINI AÇMA METODU (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.Where(x => !x.IsDeleted), "Id", "Name", product.CategoryId);

            var dto = new InventoryEditDto
            {
                Id = product.Id,
                CategoryId = product.CategoryId,
                Brand = product.Brand,
                Model = product.Model,
                ProductName = product.Name,
                StockWarningLevel = product.WarningThreshold ?? 5,
                Description = product.Description,
                ExistingPhotoUrl = product.PhotoUrl
            };

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(InventoryEditDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (product == null) return NotFound();

            // Yeni görsel yüklendiyse onu kaydet, yüklenmediyse eskisini koru
            if (dto.ProductImage != null && dto.ProductImage.Length > 0)
            {
                string uploadsFolder = Path.Combine(_env.WebRootPath, "images/products");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                string uniqueFileName = Guid.NewGuid().ToString() + "_" + dto.ProductImage.FileName;
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProductImage.CopyToAsync(fileStream);
                }
                product.PhotoUrl = "/images/products/" + uniqueFileName;
            }

            product.Name = dto.ProductName;
            product.Brand = dto.Brand;
            product.Model = dto.Model;
            product.Description = dto.Description;
            product.WarningThreshold = dto.StockWarningLevel;
            product.CategoryId = dto.CategoryId;
            product.ModifiedDate = DateTime.Now;

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ürün bilgileri başarıyla güncellendi.";
            return RedirectToAction("Stock");
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            // 1. Önce stoğu buluyoruz
            var stockItem = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == id);

            // 2. Ana ürünü buluyoruz
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (stockItem != null && product != null)
            {
                // 3. İkisini de "Silindi" (IsDeleted = true) olarak işaretliyoruz
                stockItem.IsDeleted = true;
                stockItem.DeletedDate = DateTime.Now;

                product.IsDeleted = true;
                product.DeletedDate = DateTime.Now;

                _context.ProductMainRepoLocations.Update(stockItem);
                _context.Products.Update(product);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ürün başarıyla silindi (çöp kutusuna taşındı).";
            }

            // İşlem bitince Stok listesine geri dön
            return RedirectToAction("Stock");
        }
        [HttpGet]
        public async Task<IActionResult> Inbound()
        {
            // Sadece "IN" (Giriş) hareketlerini çekiyoruz
            var inboundList = await _context.StockMovements
                .Include(x => x.Product)
                .Include(x => x.MainRepoLocation)
                .Where(x => x.MovementType == "IN" && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(inboundList);
        }

        [HttpGet]
        public async Task<IActionResult> Outbound()
        {
            // Sadece "OUT" (Çıkış) hareketlerini çekiyoruz
            var outboundList = await _context.StockMovements
                .Include(x => x.Product)
                .Include(x => x.MainRepoLocation)
                .Where(x => x.MovementType == "OUT" && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(outboundList);
        }
    }
}