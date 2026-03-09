using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Inventory;
using StockTrack.Entity.Enitities;
using Microsoft.EntityFrameworkCore;
using StockTrack.Business.Abstract; // ICargoDefinitionService için gerekli
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public async Task<IActionResult> AddProduct() // 2. EKLENDİ: async Task yaptık
        {
            ViewBag.Warehouses = new SelectList(_context.MainRepoLocations.Where(x => !x.IsDeleted), "Id", "Name");
            ViewBag.Categories = new SelectList(_context.Categories.Where(x => !x.IsDeleted), "Id", "Name");

            // 3. EKLENDİ: Sadece GİRİŞ (Tip 2) durumlarını veritabanından çekiyoruz
            var inboundStatuses = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 2);
            ViewBag.ProductStatuses = new SelectList(inboundStatuses, "Id", "Name");

            return View(new InventoryAddDto());
        }

        [HttpPost]
        public async Task<IActionResult> AddProduct(InventoryAddDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Warehouses = new SelectList(_context.MainRepoLocations.Where(x => !x.IsDeleted), "Id", "Name");
                ViewBag.Categories = new SelectList(_context.Categories.Where(x => !x.IsDeleted), "Id", "Name");

                // Hata durumunda formu geri döndürürken listeyi tekrar dolduruyoruz
                var inboundStatuses = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 2);
                ViewBag.ProductStatuses = new SelectList(inboundStatuses, "Id", "Name");

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
            await _context.SaveChangesAsync();

            // 3. Stok ve Depo Bilgisini Köprü Tabloya Ekleme
            var newStock = new ProductMainRepoLocation
            {
                ProductId = newProduct.Id,
                MainRepoLocationId = dto.WarehouseId,
                Quantity = dto.StockQuantity,
                // ProductStatusId = dto.ProductStatusId  (Entity'de varsa açabilirsin)
            };

            _context.ProductMainRepoLocations.Add(newStock);
            await _context.SaveChangesAsync();

            // 4. ARC BOX İÇİN BOŞ SATIRLARI ÜRET!
            // İsterler Belgesi Kuralı: "Arc Box adeti girildikten sonra eklenen sayı kadar satır oluşacak."
            bool isArcBox = newProduct.Name != null && newProduct.Name.Contains("Arc Box");

            if (isArcBox)
            {
                // Kaç adet girildiyse o kadar dönecek bir döngü kuruyoruz
                for (int i = 0; i < dto.StockQuantity; i++)
                {
                    var emptySerialRecord = new ProductSerialNumber
                    {
                        ProductId = newProduct.Id,

                        // YENİ EKLENEN KISIM: Depo ve Durum bilgileri Entity'de var olduğu için dolduruyoruz
                        MainRepoLocationId = dto.WarehouseId,
                        ProductStatusId = dto.ProductStatusId,

                        SerialNumber = "-", // Başlangıçta boş
                        EthMac = "-",
                        WlanMac = "-",
                        Description = dto.Description, // Formda girilen açıklama varsa onu da atalım
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.ProductSerialNumbers.Add(emptySerialRecord);
                }
                await _context.SaveChangesAsync();
            }

            TempData["SuccessMessage"] = "Ürün depoya başarıyla eklendi!";
            return RedirectToAction("Stock");
        }

        [HttpGet]
        public async Task<IActionResult> Stock()
        {
            var stockList = await _context.ProductMainRepoLocations
                .Include(pm => pm.Product)
                    .ThenInclude(p => p.Category)
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
                    IsArcBox = x.Product.Name.Contains("Arc Box")
                }).ToListAsync();

            return View(stockList);
        }

        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return NotFound();

            var stockInfo = await _context.ProductMainRepoLocations
                .Include(x => x.MainRepoLocation)
                .FirstOrDefaultAsync(x => x.ProductId == id);

            int currentStock = stockInfo != null ? stockInfo.Quantity : 0;
            string mainWarehouseName = stockInfo != null && stockInfo.MainRepoLocation != null ? stockInfo.MainRepoLocation.Name : "-";

            var movements = await _context.StockMovements
                .Include(x => x.MainRepoLocation)
                .Where(x => x.ProductId == id && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            var isArcBox = product.Name != null && product.Name.Contains("Arc Box");

            // Tüm kargo tanımlarını (isimlerini bulmak için) hafızaya alalım
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
                    SerialNumber = s.SerialNumber,
                    EthMac = s.EthMac,
                    WlanMac = s.WlanMac,
                    WarehouseName = s.MainRepoLocation != null ? s.MainRepoLocation.Name : "-",
                    StatusName = definitions.FirstOrDefault(d => d.Id == s.ProductStatusId)?.Name ?? "Yeni",
                    Description = s.Description ?? "-"
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
                // DİKKAT: Standart ürünlerin depo tablosu için ana depo adını taşıyacak bir alana ihtiyacımız olabilir
                // Şimdilik ViewBag ile taşıyalım

                InboundMovements = movements.Where(m => m.MovementType == "IN").Select(m => new MovementDto
                {
                    Date = m.CreatedDate,
                    Quantity = m.MovementQuantity,
                    OldStock = m.OldStockQuantity,
                    NewStock = m.NewStockQuantity,
                    LocationName = m.MainRepoLocation != null ? m.MainRepoLocation.Name : "-",
                    MovementStatusName = definitions.FirstOrDefault(d => d.Id == m.MovementStatusId)?.Name ?? "-",
                    Description = m.Description
                }).ToList(),

                OutboundMovements = movements.Where(m => m.MovementType == "OUT").Select(m => new MovementDto
                {
                    Date = m.CreatedDate,
                    Quantity = m.MovementQuantity,
                    OldStock = m.OldStockQuantity,
                    NewStock = m.NewStockQuantity,
                    LocationName = m.MainRepoLocation != null ? m.MainRepoLocation.Name : "-",
                    MovementStatusName = definitions.FirstOrDefault(d => d.Id == m.MovementStatusId)?.Name ?? "-",
                    Description = m.Description
                }).ToList(),

                SerialNumbers = serials
            };

            ViewBag.MainWarehouseName = mainWarehouseName;

            var inStatuses = definitions.Where(x => x.DefinitionType == 2).ToList();
            var outStatuses = definitions.Where(x => x.DefinitionType == 3).ToList();
            ViewBag.InboundStatuses = new SelectList(inStatuses, "Id", "Name");
            ViewBag.OutboundStatuses = new SelectList(outStatuses, "Id", "Name");

            return View(dto);
        }

        [HttpPost]
        public async Task<IActionResult> AddStock(StockMovementDto dto)
        {
            var existingStock = await _context.ProductMainRepoLocations
                .FirstOrDefaultAsync(x => x.ProductId == dto.ProductId);

            if (existingStock == null)
            {
                TempData["ErrorMessage"] = "Bu ürün depoda bulunamadı!";
                return RedirectToAction("Detail", new { id = dto.ProductId });
            }

            int oldStock = existingStock.Quantity;
            int newStock = oldStock + dto.Quantity;

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
                IsActive = true
            };
            _context.StockMovements.Add(movement);

            existingStock.Quantity = newStock;
            _context.ProductMainRepoLocations.Update(existingStock);

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = $"{dto.Quantity} adet ürün başarıyla stoğa eklendi.";

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
