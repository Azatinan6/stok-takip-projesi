using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.Business.Abstract; // ICargoDefinitionService için gerekli
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
        private readonly ICargoDefinitionService _cargoDefinitionService; // 1. EKLENDİ: Dinamik durumları çekmek için

        public InventoryController(AppDbContext context, IWebHostEnvironment env, ICargoDefinitionService cargoDefinitionService)
        {
            _context = context;
            _env = env;
            _cargoDefinitionService = cargoDefinitionService; // EKLENDİ
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Warehouses = new SelectList(_context.MainRepoLocations.Where(x => !x.IsDeleted), "Id", "Name");
            ViewBag.Categories = new SelectList(_context.Categories.Where(x => !x.IsDeleted), "Id", "Name");

            var inboundStatuses = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 2);
            ViewBag.ProductStatuses = new SelectList(inboundStatuses, "Id", "Name");

            return View(new InventoryAddDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(InventoryAddDto dto)
        {
            // 1. Model Geçerlilik Kontrolü
            if (!ModelState.IsValid)
            {
                ViewBag.Warehouses = new SelectList(_context.MainRepoLocations.Where(x => !x.IsDeleted), "Id", "Name");
                ViewBag.Categories = new SelectList(_context.Categories.Where(x => !x.IsDeleted), "Id", "Name");

                // Giriş Durumlarını (Tip 2) tekrar doldur
                var inboundStatuses = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 2);
                ViewBag.ProductStatuses = new SelectList(inboundStatuses, "Id", "Name");

                return View(dto);
            }

            // 2. Görsel Yükleme İşlemi
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

            // 3. Yeni Ürün Kaydı (Product)
            var newProduct = new Product
            {
                Name = dto.ProductName,
                Brand = dto.Brand,
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
            await _context.SaveChangesAsync(); // ID'nin oluşması için burada bir kez kaydediyoruz.

            // 4. Stok Köprü Tablosu Kaydı (ProductMainRepoLocation)
            var newStock = new ProductMainRepoLocation
            {
                ProductId = newProduct.Id,
                MainRepoLocationId = dto.WarehouseId,
                Quantity = dto.StockQuantity,
                IsActive = true,
                IsDeleted = false
            };
            _context.ProductMainRepoLocations.Add(newStock);

            // İçindeki boşlukları siler ve büyük/küçük harfe bakmaksızın "arcbox" kelimesini arar!
            bool isArcBox = newProduct.Name != null &&
                            newProduct.Name.Replace(" ", "").Contains("arcbox", StringComparison.OrdinalIgnoreCase);

            if (isArcBox && dto.StockQuantity > 0)
            {
                for (int i = 0; i < dto.StockQuantity; i++)
                {
                    _context.ProductSerialNumbers.Add(new ProductSerialNumber
                    {
                        ProductId = newProduct.Id,
                        MainRepoLocationId = dto.WarehouseId,
                        ProductStatusId = dto.ProductStatusId, // Formdan gelen fiziksel durum (Yeni/Arızalı vb.)
                        SerialNumber = "-",
                        EthMac = "-",
                        WlanMac = "-",
                        Description = "İlk Stok Girişi (Toplu)",
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    });
                }
            }

            // Tüm değişiklikleri (Stok ve Seri Noları) tek seferde kaydet
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ürün ve stok kayıtları başarıyla oluşturuldu!";
            return RedirectToAction("Stock");
        }
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

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            // 1. Ürünü bul
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return NotFound();

            // 2. Ana stok bilgisini al (Null kontrolü ile güvenli hale getirildi)
            var stockInfo = await _context.ProductMainRepoLocations
                .Include(x => x.MainRepoLocation)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            int currentStock = stockInfo?.Quantity ?? 0;
            string mainWarehouseName = stockInfo?.MainRepoLocation?.Name ?? "-";

            // 3. Stok hareketlerini çek (Log tablosu)
            var movements = await _context.StockMovements
                .Include(x => x.MainRepoLocation)
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            // 4. Arc Box kontrolü ve Tanımları çekme
            bool isArcBox = product.Name != null &&
                product.Name.Replace(" ", "").Contains("arcbox", StringComparison.OrdinalIgnoreCase);
            var definitions = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted);

            // 5. Eğer Arc Box ise seri numaralarını (cihaz detaylarını) çek
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
                    // ArcBoxSerialDto içindeki ImageUrl için Detail metodunda:
                    ImageUrl = string.IsNullOrEmpty(product.PhotoUrl) ? "/images/no-image.png" : product.PhotoUrl
                }).ToList();
            }

            // 6. DTO Oluşturma ve Hareketlerin Seçilmesi
            var dto = new InventoryDetailDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Brand = product.Brand,
                Model = product.Model,
                IsArcBox = isArcBox,
                CurrentStock = currentStock,

                // GİRİŞ HAREKETLERİ
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

                // ÇIKIŞ HAREKETLERİ
                OutboundMovements = movements.Where(m => m.MovementType == "OUT").Select(m => new MovementDto
                {
                    Date = m.CreatedDate,
                    Quantity = m.MovementQuantity,
                    OldStock = m.OldStockQuantity,
                    NewStock = m.NewStockQuantity,
                    LocationName = m.MainRepoLocation?.Name ?? "-",
                    // ÖNEMLİ: Miktar 1 ise seri noyu göster
                    SerialNumber = m.MovementQuantity == 1 ? (m.SerialNumber ?? "-") : "Çoklu Kayıt",
                    EthMac = m.MovementQuantity == 1 ? (m.EthMac ?? "-") : "Detayda Mevcut",
                    WlanMac = m.MovementQuantity == 1 ? (m.WlanMac ?? "-") : "Detayda Mevcut",
                    ProductStatusName = definitions.FirstOrDefault(d => d.Id == m.ProductStatusId)?.Name ?? "-",
                    MovementStatusName = definitions.FirstOrDefault(d => d.Id == m.MovementStatusId)?.Name ?? "-",
                    Description = m.Description
                }).ToList(),

                SerialNumbers = serials
            };
            // 7. View için Ek Bilgiler 
            ViewBag.MainWarehouseName = mainWarehouseName;

            // Ürün Durumları (Örn: Sıfır, İkinci El, Arızalı) -> Veritabanındaki Type ID'si 2 ise:
            ViewBag.ProductStatuses = new SelectList(definitions.Where(x => x.DefinitionType == 2), "Id", "Name");

            // Giriş Nedenleri (Örn: Merkezden Kargo, Stok Arttır) -> Veritabanındaki Type ID'si 3 ise:
            ViewBag.InboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 3), "Id", "Name");

            // Çıkış Nedenleri (Örn: Hastaneye Kurulum, Stok Azalt) -> Veritabanındaki Type ID'si 4 ise:
            ViewBag.OutboundReasons = new SelectList(definitions.Where(x => x.DefinitionType == 4), "Id", "Name");

            // 8. Stok durum özeti (Sadece standart adetli ürünler için)
            if (!dto.IsArcBox)
            {
                List<string> statusTexts = new List<string>();
                // Sadece miktar olan durumları (Yeni, Kullanılmış vb.) hesapla
                var relevantStatuses = definitions.Where(x => x.DefinitionType == 2 || x.DefinitionType == 3).ToList();

                foreach (var tanim in definitions.Where(x => x.DefinitionType == 2))
                {
                    int girenAdet = movements.Where(m => m.MovementType == "IN" && m.ProductStatusId == tanim.Id).Sum(m => m.MovementQuantity);
                    int cikanAdet = movements.Where(m => m.MovementType == "OUT" && m.ProductStatusId == tanim.Id).Sum(m => m.MovementQuantity);
                    int kalan = girenAdet - cikanAdet;

                    if (kalan > 0)
                    {
                        statusTexts.Add($"{kalan} {tanim.Name}");
                    }
                }
                dto.StatusDetailsText = statusTexts.Any() ? string.Join(", ", statusTexts) : "Stok Yok";
            }

            return View(dto);
        }

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

        [HttpPost]
        public async Task<IActionResult> RemoveStock(StockMovementDto dto)
        {
            var existingStock = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

            if (existingStock == null)
            {
                return RedirectToAction("Detail", new { id = dto.ProductId });
            }

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
                ProductStatusId = dto.ProductStatusId, // Çıkış durumu Dropdown'dan gelecek
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

        [HttpGet]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var stockItem = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == id);

            if (stockItem != null)
            {
                stockItem.IsActive = !stockItem.IsActive;

                _context.ProductMainRepoLocations.Update(stockItem);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Kayıt durumu başarıyla güncellendi!";
            }

            return RedirectToAction("Stock");
        }

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
            var stockItem = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == id);

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
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            // Hem ürün silinmiş olabilir hem de stok kaydı silinmiş olabilir. 
            // Ürün bazlı listelemek en mantıklısıdır.
            var deletedProducts = await _context.ProductMainRepoLocations
                .Include(pm => pm.Product)
                    .ThenInclude(p => p.Category)
                .Include(pm => pm.MainRepoLocation)
                .Where(x => x.IsDeleted || x.Product.IsDeleted) // Herhangi biri silindiyse göster
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

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var stockItem = await _context.ProductMainRepoLocations.FirstOrDefaultAsync(x => x.ProductId == id);
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);

            if (stockItem != null && product != null)
            {
                // Silinme durumlarını geri alıyoruz
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
