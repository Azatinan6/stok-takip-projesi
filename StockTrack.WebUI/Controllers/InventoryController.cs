using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Inventory;
using StockTrack.Entity.Enitities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StockTrack.Controllers
{
    public class InventoryController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly ICargoDefinitionService _cargoDefinitionService;

        public InventoryController(AppDbContext context, IWebHostEnvironment env, ICargoDefinitionService cargoDefinitionService)
        {
            _context = context;
            _env = env;
            _cargoDefinitionService = cargoDefinitionService;
        }

        // 1. STOK GİRİŞ SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Warehouses = new SelectList(await _context.MainRepoLocations.Where(x => !x.IsDeleted).ToListAsync(), "Id", "Name");
            ViewBag.Categories = new SelectList(await _context.Categories.Where(x => !x.IsDeleted && x.IsActive).ToListAsync(), "Id", "Name");

            // Ürün durumu (ProductStatuses) çantadan tamamen kaldırıldı.

            return View(new InventoryAddDto());
        }

        // 2. ZİNCİRLEME (CASCADING) AJAX METODU
        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _context.Products
                .Where(x => x.CategoryId == categoryId && x.IsActive && !x.IsDeleted)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    model = x.Model ?? "-",
                    brand = x.Brand ?? "-",
                    warningThreshold = x.WarningThreshold ?? 5,
                    photoUrl = x.PhotoUrl
                })
                .OrderBy(x => x.name)
                .ToListAsync();

            return Json(products);
        }

        // 3. STOK GİRİŞİNİ KAYDET 
        [HttpPost]
        public async Task<IActionResult> AddProduct(InventoryAddDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Warehouses = new SelectList(await _context.MainRepoLocations.Where(x => !x.IsDeleted).ToListAsync(), "Id", "Name");
                ViewBag.Categories = new SelectList(await _context.Categories.Where(x => !x.IsDeleted && x.IsActive).ToListAsync(), "Id", "Name");
                return View(dto);
            }

            var existingProduct = await _context.Products.FindAsync(dto.ProductId);
            if (existingProduct == null)
            {
                TempData["ErrorMessage"] = "Seçilen ürün sistemde bulunamadı!";
                return RedirectToAction("AddProduct");
            }

            // KRİTİK DÜZELTME: !x.IsDeleted şartını buradan kaldırdık! Silinmiş olsa bile o satırı yakalayacağız.
            var existingStock = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId && x.MainRepoLocationId == dto.WarehouseId);

            int oldStockQuantity = 0;

            if (existingStock != null)
            {
                // Eğer kayıt bulunduysa ama daha önceden SİLİNMİŞSE (Soft Delete durumundaysa):
                if (existingStock.IsDeleted)
                {
                    oldStockQuantity = 0; // Silindiği için fiili eski stoğu 0 kabul et
                    existingStock.Quantity = dto.StockQuantity; // Yeni adeti doğrudan set et
                    existingStock.IsDeleted = false; // Silinme bayrağını indir, hayata döndür!
                    existingStock.DeletedDate = null;
                }
                else
                {
                    // Kayıt zaten aktif olarak varsa, mevcut stoğun üzerine ekle
                    oldStockQuantity = existingStock.Quantity;
                    existingStock.Quantity += dto.StockQuantity;
                }

                existingStock.IsActive = true;
                existingStock.ModifiedDate = DateTime.Now;
                _context.ProductMainRepoLocations.Update(existingStock);
            }
            else
            {
                // Veritabanında bu (Ürün - Depo) eşleşmesine dair hiç satır yoksa sıfırdan oluştur
                var newStock = new ProductMainRepoLocation
                {
                    ProductId = dto.ProductId,
                    MainRepoLocationId = dto.WarehouseId,
                    Quantity = dto.StockQuantity,
                    IsActive = true,
                    IsDeleted = false
                };
                _context.ProductMainRepoLocations.Add(newStock);
            }

            // Satırı sağlama al ve SQL'e hemen işlet
            await _context.SaveChangesAsync();

            // STOK HAREKET (LOG) KAYDI
            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                MainRepoLocationId = dto.WarehouseId,
                MovementType = "IN",
                OldStockQuantity = oldStockQuantity,
                NewStockQuantity = oldStockQuantity + dto.StockQuantity,
                MovementQuantity = dto.StockQuantity,
                Description = string.IsNullOrEmpty(dto.Description) ? "Yeni Stok Girişi (Form)" : dto.Description,
                CreatedDate = DateTime.Now,
                IsActive = true,
                IsDeleted = false,
                SerialNumber = dto.StockQuantity == 1 ? "-" : "Çoklu İşlem",
                EthMac = dto.StockQuantity == 1 ? "-" : "Detayda Mevcut",
                WlanMac = dto.StockQuantity == 1 ? "-" : "Detayda Mevcut"
            };
            _context.StockMovements.Add(movement);

            // ArcBox otomatik seri numarası üretme alanı
            bool isArcBox = existingProduct.Name != null &&
                            existingProduct.Name.Replace(" ", "").Contains("arcbox", StringComparison.OrdinalIgnoreCase);

            if (isArcBox && dto.StockQuantity > 0)
            {
                // FK Hatasını Engelleme: Veritabanındaki aktif ve geçerli ilk ürün durumunu (Tip 2) dinamik buluyoruz
                var defaultStatus = await _context.CargoDefinitions
                    .FirstOrDefaultAsync(x => x.DefinitionType == 2 && !x.IsDeleted);
                
                int validStatusId = defaultStatus != null ? defaultStatus.Id : 1;

                for (int i = 0; i < dto.StockQuantity; i++)
                {
                    // UNIQUE İndeks Hatasını Engelleme: Her cihaz için benzersiz geçici bir seri no üretiyoruz (Örn: TEMP-A45F12-1)
                    string tempGuid = Guid.NewGuid().ToString().Substring(0, 6).ToUpper();
                    string generatedSerial = $"TEMP-{tempGuid}-{i + 1}";

                    _context.ProductSerialNumbers.Add(new ProductSerialNumber
                    {
                        ProductId = dto.ProductId,
                        MainRepoLocationId = dto.WarehouseId,
                        ProductStatusId = validStatusId, // Dinamik olarak bulduğumuz geçerli ID basılıyor
                        SerialNumber = generatedSerial,   // Artık mükerrer hatası vermesi imkansız!
                        EthMac = "-",
                        WlanMac = "-",
                        Description = "İlk Stok Girişi (Otomatik)",
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
            }

            // Tüm log ve alt süreçleri veritabanına nihai olarak kaydet
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Stok girişi başarıyla gerçekleştirildi!";
            return RedirectToAction("Stock");
        }

        // 4. STOK LİSTELEME EKRANI
        [HttpGet]
        public async Task<IActionResult> Stock()
        {
            var stockList = await _context.ProductMainRepoLocations
                .Include(pm => pm.Product).ThenInclude(p => p.Category)
                .Include(pm => pm.MainRepoLocation)
                .Where(x => !x.IsDeleted && !x.Product.IsDeleted)
                .Select(x => new InventoryListDto
                {
                    Id = x.ProductId,
                    WarehouseName = x.MainRepoLocation.Name,
                    CategoryName = x.Product.Category != null ? x.Product.Category.Name : "Kategori Yok",
                    Brand = x.Product.Brand,
                    Model = x.Product.Model,
                    ProductName = x.Product.Name,
                    ImageUrl = x.Product.PhotoUrl,
                    StockQuantity = x.Quantity,
                    StockWarningLevel = x.Product.WarningThreshold ?? 5,
                    IsActive = x.IsActive,
                    IsArcBox = x.Product.Name != null && x.Product.Name.Replace(" ", "").ToLower().Contains("arcbox")
                }).ToListAsync();

            return View(stockList);
        }

        // 5. STOK DETAY VE LOG HAREKETLERİ EKRANI
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return NotFound();

            var stockInfo = await _context.ProductMainRepoLocations
                .Include(x => x.MainRepoLocation)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            int currentStock = stockInfo?.Quantity ?? 0;
            string mainWarehouseName = stockInfo?.MainRepoLocation?.Name ?? "-";

            var movements = await _context.StockMovements
                .Include(x => x.MainRepoLocation)
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            bool isArcBox = product.Name != null &&
                product.Name.Replace(" ", "").Contains("arcbox", StringComparison.OrdinalIgnoreCase);
            var definitions = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted);

            var serials = new List<ArcBoxSerialDto>();
            if (isArcBox)
            {
                var rawSerials = await _context.ProductSerialNumbers
                    .Include(x => x.MainRepoLocation)
                    .Where(x => x.ProductId == id && !x.IsDeleted)
                    .ToListAsync();

                serials = rawSerials.Select(s => new ArcBoxSerialDto
                {
                    Id = s.Id,
                    SerialNumber = s.SerialNumber ?? "-",
                    EthMac = s.EthMac ?? "-",
                    WlanMac = s.WlanMac ?? "-",
                    WarehouseName = s.MainRepoLocation?.Name ?? "-",
                    StatusName = definitions.FirstOrDefault(d => d.Id == s.ProductStatusId)?.Name ?? "Yeni",
                    Description = s.Description ?? "-",
                    ImageUrl = string.IsNullOrEmpty(product.PhotoUrl) ? "/images/no-image.png" : product.PhotoUrl
                }).ToList();
            }

            var dto = new InventoryDetailDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Brand = product.Brand,
                Model = product.Model,
                IsArcBox = isArcBox,
                CurrentStock = currentStock,

                InboundMovements = movements.Where(m => m.MovementType == "IN").Select(m => new MovementDto
                {
                    Date = m.CreatedDate,
                    Quantity = m.MovementQuantity,
                    OldStock = m.OldStockQuantity,
                    NewStock = m.NewStockQuantity,
                    LocationName = m.MainRepoLocation?.Name ?? "-",
                    SerialNumber = m.MovementQuantity == 1 ? (m.SerialNumber ?? "-") : "Çoklu Kayıt",
                    EthMac = m.MovementQuantity == 1 ? (m.EthMac ?? "-") : "Detayda Mevcut",
                    WlanMac = m.MovementQuantity == 1 ? (m.WlanMac ?? "-") : "Detayda Mevcut",
                    ProductStatusName = definitions.FirstOrDefault(d => d.Id == m.ProductStatusId)?.Name ?? "-",
                    MovementStatusName = definitions.FirstOrDefault(d => d.Id == m.MovementStatusId)?.Name ?? "-",
                    Description = m.Description
                }).ToList(),

                OutboundMovements = movements.Where(m => m.MovementType == "OUT").Select(m => new MovementDto
                {
                    Date = m.CreatedDate,
                    Quantity = m.MovementQuantity,
                    OldStock = m.OldStockQuantity,
                    NewStock = m.NewStockQuantity,
                    LocationName = m.MainRepoLocation?.Name ?? "-",
                    SerialNumber = m.MovementQuantity == 1 ? (m.SerialNumber ?? "-") : "Çoklu Kayıt",
                    EthMac = m.MovementQuantity == 1 ? (m.EthMac ?? "-") : "Detayda Mevcut",
                    WlanMac = m.MovementQuantity == 1 ? (m.WlanMac ?? "-") : "Detayda Mevcut",
                    ProductStatusName = definitions.FirstOrDefault(d => d.Id == m.ProductStatusId)?.Name ?? "-",
                    MovementStatusName = definitions.FirstOrDefault(d => d.Id == m.MovementStatusId)?.Name ?? "-",
                    Description = m.Description
                }).ToList(),

                SerialNumbers = serials
            };

            ViewBag.MainWarehouseName = mainWarehouseName;
            ViewBag.ProductStatuses = new SelectList(definitions.Where(x => x.DefinitionType == 2), "Id", "Name");
            ViewBag.InboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 5), "Id", "Name");
            ViewBag.OutboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 4), "Id", "Name");

            if (!dto.IsArcBox)
            {
                List<string> statusTexts = new List<string>();
                foreach (var tanim in definitions.Where(x => x.DefinitionType == 2))
                {
                    int girenAdet = movements.Where(m => m.MovementType == "IN" && m.ProductStatusId == tanim.Id).Sum(m => m.MovementQuantity);
                    int cikanAdet = movements.Where(m => m.MovementType == "OUT" && m.ProductStatusId == tanim.Id).Sum(m => m.MovementQuantity);
                    int kalan = girenAdet - cikanAdet;

                    if (kalan > 0) statusTexts.Add($"{kalan} {tanim.Name}");
                }
                dto.StatusDetailsText = statusTexts.Any() ? string.Join(", ", statusTexts) : "Stok Yok";
            }

            return View(dto);
        }

        // 6. DETAY SAYFASINDAN MANUEL STOK EKLEME (IN)
        [HttpPost]
        public async Task<IActionResult> AddStock(StockMovementDto dto)
        {
            if (dto.Quantity <= 0)
            {
                TempData["ErrorMessage"] = "Adet 0'dan büyük olmalıdır!";
                return RedirectToAction("Detail", new { id = dto.ProductId });
            }

            var existingStock = await _context.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);
            if (existingStock == null) return RedirectToAction("Stock");

            int oldStock = existingStock.Quantity;
            int newStock = oldStock + dto.Quantity;

            var product = await _context.Products.FindAsync(dto.ProductId);
            bool isArcBox = product != null && product.Name != null && product.Name.Replace(" ", "").Contains("arcbox", StringComparison.OrdinalIgnoreCase);

            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                MainRepoLocationId = existingStock.MainRepoLocationId,
                MovementType = "IN",
                OldStockQuantity = oldStock,
                NewStockQuantity = newStock,
                MovementQuantity = dto.Quantity,
                MovementStatusId = dto.MovementStatusId,
                ProductStatusId = dto.ProductStatusId,
                Description = dto.Description,
                CreatedDate = DateTime.Now,
                IsActive = true,
                SerialNumber = dto.Quantity == 1 ? "-" : "Çoklu İşlem",
                EthMac = dto.Quantity == 1 ? "-" : "Detayda Mevcut",
                WlanMac = dto.Quantity == 1 ? "-" : "Detayda Mevcut"
            };
            _context.StockMovements.Add(movement);

            existingStock.Quantity = newStock;
            existingStock.ModifiedDate = DateTime.Now;
            _context.ProductMainRepoLocations.Update(existingStock);

            if (isArcBox)
            {
                for (int i = 0; i < dto.Quantity; i++)
                {
                    _context.ProductSerialNumbers.Add(new ProductSerialNumber
                    {
                        ProductId = dto.ProductId,
                        MainRepoLocationId = existingStock.MainRepoLocationId,
                        ProductStatusId = dto.ProductStatusId,
                        SerialNumber = "-",
                        EthMac = "-",
                        WlanMac = "-",
                        Description = dto.Description,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Stok eklendi.";
            return RedirectToAction("Detail", new { id = dto.ProductId });
        }

        // 7. DETAY SAYFASINDAN MANUEL STOK DÜŞME (OUT)
        [HttpPost]
        public async Task<IActionResult> RemoveStock(StockMovementDto dto)
        {
            var existingStock = await _context.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);
            if (existingStock == null) return RedirectToAction("Detail", new { id = dto.ProductId });

            if (existingStock.Quantity < dto.Quantity)
            {
                TempData["ErrorMessage"] = $"Hata! Depoda sadece {existingStock.Quantity} adet ürün var. {dto.Quantity} adet çıkış yapamazsınız.";
                return RedirectToAction("Detail", new { id = dto.ProductId });
            }

            int oldStock = existingStock.Quantity;
            int newStock = oldStock - dto.Quantity;

            var movement = new StockMovement
            {
                ProductId = dto.ProductId,
                MainRepoLocationId = existingStock.MainRepoLocationId,
                MovementType = "OUT",
                OldStockQuantity = oldStock,
                NewStockQuantity = newStock,
                MovementQuantity = dto.Quantity,
                MovementStatusId = dto.MovementStatusId,
                ProductStatusId = dto.ProductStatusId,
                Description = dto.Description,
                SerialNumber = dto.Quantity == 1 ? "-" : "Çoklu Kayıt",
                EthMac = dto.Quantity == 1 ? "-" : "Detayda Mevcut",
                WlanMac = dto.Quantity == 1 ? "-" : "Detayda Mevcut",
                CreatedDate = DateTime.Now,
                IsActive = true
            };
            _context.StockMovements.Add(movement);

            existingStock.Quantity = newStock;
            _context.ProductMainRepoLocations.Update(existingStock);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{dto.Quantity} adet ürün stoktan düşüldü.";

            return RedirectToAction("Detail", new { id = dto.ProductId });
        }

        // 8. AKTİF/PASİF ETME
        [HttpGet]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var stockItem = await _context.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.ProductId == id);
            if (stockItem != null)
            {
                stockItem.IsActive = !stockItem.IsActive;
                _context.ProductMainRepoLocations.Update(stockItem);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Kayıt durumu başarıyla güncellendi!";
            }
            return RedirectToAction("Stock");
        }

        // 9. TANIMLI ÜRÜN BİLGİSİNİ DÜZENLEME (GET)
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

        // 10. TANIMLI ÜRÜN BİLGİSİNİ DÜZENLEME (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(InventoryEditDto dto)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == dto.Id);
            if (product == null) return NotFound();

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

        // 11. SİLME (SOFT DELETE)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var stockItem = await _context.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.ProductId == id);
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (stockItem != null && product != null)
            {
                stockItem.IsDeleted = true;
                stockItem.DeletedDate = DateTime.Now;
                product.IsDeleted = true;
                product.DeletedDate = DateTime.Now;

                _context.ProductMainRepoLocations.Update(stockItem);
                _context.Products.Update(product);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Ürün başarıyla silindi (çöp kutusuna taşındı).";
            }
            return RedirectToAction("Stock");
        }

        // 12. SİLİNENLER LİSTESİ
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var deletedProducts = await _context.ProductMainRepoLocations
                .Include(pm => pm.Product).ThenInclude(p => p.Category)
                .Include(pm => pm.MainRepoLocation)
                .Where(x => x.IsDeleted || x.Product.IsDeleted)
                .Select(x => new InventoryListDto
                {
                    Id = x.ProductId,
                    WarehouseName = x.MainRepoLocation.Name,
                    CategoryName = x.Product.Category != null ? x.Product.Category.Name : "Kategori Yok",
                    Brand = x.Product.Brand,
                    Model = x.Product.Model,
                    ProductName = x.Product.Name,
                    ImageUrl = x.Product.PhotoUrl,
                    StockQuantity = x.Quantity,
                    IsActive = x.IsActive
                }).ToListAsync();

            return View(deletedProducts);
        }

        // 13. GERİ YÜKLEME (RESTORE)
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var stockItem = await _context.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.ProductId == id);
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (stockItem != null && product != null)
            {
                stockItem.IsDeleted = false;
                stockItem.DeletedDate = null;
                product.IsDeleted = false;
                product.DeletedDate = null;

                _context.ProductMainRepoLocations.Update(stockItem);
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ürün ve stok kayıtları başarıyla geri yüklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Geri yükleme sırasında bir hata oluştu.";
            }
            return RedirectToAction("Deleted");
        }

        // 14. GİRİŞ HAREKET RAPORU
        [HttpGet]
        public async Task<IActionResult> Inbound()
        {
            var inboundList = await _context.StockMovements
                .Include(x => x.Product)
                .Include(x => x.MainRepoLocation)
                .Where(x => x.MovementType == "IN" && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(inboundList);
        }

        // 15. ÇIKIŞ HAREKET RAPORU
        [HttpGet]
        public async Task<IActionResult> Outbound()
        {
            var outboundList = await _context.StockMovements
                .Include(x => x.Product)
                .Include(x => x.MainRepoLocation)
                .Where(x => x.MovementType == "OUT" && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return View(outboundList);
        }

        // 16. ARCBOX DETAY CİHAZ GÜNCELLEME
        [HttpPost]
        public async Task<IActionResult> UpdateArcBoxDetails(int id, string serialNumber, string ethMac, string wlanMac, string description)
        {
            var serialRecord = await _context.ProductSerialNumbers.FirstOrDefaultAsync(x => x.Id == id);
            if (serialRecord != null)
            {
                serialRecord.SerialNumber = serialNumber;
                serialRecord.EthMac = ethMac;
                serialRecord.WlanMac = wlanMac;
                serialRecord.Description = description;
                serialRecord.ModifiedDate = DateTime.Now;

                _context.ProductSerialNumbers.Update(serialRecord);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Cihaz bilgileri başarıyla güncellendi.";
                return RedirectToAction("Detail", new { id = serialRecord.ProductId });
            }
            TempData["ErrorMessage"] = "Kayıt bulunamadı!";
            return RedirectToAction("Stock");
        }
    }
}