using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Set;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockTrack.WebUI.Controllers
{
    [Authorize(Roles = RoleConsts.Admin)]
    public class SetController : Controller
    {
        private readonly AppDbContext _context;

        public SetController(AppDbContext context)
        {
            _context = context;
        }

        // 1. SETLERİ LİSTELEME EKRANI (Ana Sayfa)
        public async Task<IActionResult> Index()
        {
            // Setleri ve içindeki ürün sayısını çekiyoruz
            var sets = await _context.ProductSets
                .Include(x => x.ProductSetItems) // Setin içindeki ürünleri de dahil et
                .Where(x => !x.IsDeleted)
                .ToListAsync();

            return View(sets);
        }

        // 2. YENİ SET EKLEME SAYFASI (GET - Formu Açar)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Ekranda seçilebilecek aktif ürünleri ViewBag ile gönderiyoruz
            var products = await _context.Products.Where(x => !x.IsDeleted && x.IsActive).ToListAsync();
            ViewBag.Products = new SelectList(products, "Id", "Name");

            return View(new SetCreateDto());
        }

        // 3. YENİ SETİ KAYDETME (POST - Formdan Gelen Veriyi Alır)
        [HttpPost]
        public async Task<IActionResult> Create(SetCreateDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || dto.ProductIds == null || !dto.ProductIds.Any())
            {
                TempData["ErrorMessage"] = "Set adı ve en az bir ürün zorunludur!";
                return RedirectToAction("Create");
            }

            // A) Önce Ana Seti Oluştur
            var newSet = new ProductSet
            {
                Name = dto.Name,
                IsActive = dto.IsActive,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };

            _context.ProductSets.Add(newSet);
            await _context.SaveChangesAsync(); // SQL tarafında ID oluştu (newSet.Id)

            // B) Sonra Dinamik Gelen Ürünleri "Köprü Tabloya" (ProductSetItem) Ekle
            foreach (var productId in dto.ProductIds)
            {
                var setItem = new ProductSetItem
                {
                    ProductSetId = newSet.Id,
                    ProductId = productId,
                    Quantity = 1, // Tasarımında adet yoktu, varsayılan 1 olarak ekliyoruz
                    CreatedDate = DateTime.Now,
                    IsActive = true,
                    IsDeleted = false
                };
                _context.ProductSetItems.Add(setItem);
            }

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Set başarıyla oluşturuldu!";

            return RedirectToAction("Index");
        }
        // 4. AKTİF/PASİF YAPMA İŞLEMİ
        [HttpGet]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var set = await _context.ProductSets.FirstOrDefaultAsync(x => x.Id == id);
            if (set != null)
            {
                set.IsActive = !set.IsActive; // Durumu tersine çevir
                set.ModifiedDate = DateTime.Now;
                _context.ProductSets.Update(set);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Set durumu başarıyla güncellendi.";
            }
            return RedirectToAction("Index");
        }

        // 5. SİLME İŞLEMİ (Çöp Kutusuna Gönder - Soft Delete)
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var set = await _context.ProductSets
                .Include(x => x.ProductSetItems) // İçindeki ürünleri de getir
                .FirstOrDefaultAsync(x => x.Id == id);

            if (set != null)
            {
                set.IsDeleted = true;
                set.DeletedDate = DateTime.Now;
                set.IsActive = false;

                // Set silinince, içindeki köprü kayıtlarını da silinmiş işaretliyoruz
                foreach (var item in set.ProductSetItems)
                {
                    item.IsDeleted = true;
                    item.DeletedDate = DateTime.Now;
                }

                _context.ProductSets.Update(set);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Set başarıyla silindi.";
            }
            return RedirectToAction("Index");
        }

        // 6. DÜZENLEME SAYFASINI AÇ (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var set = await _context.ProductSets
                .Include(x => x.ProductSetItems)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (set == null) return NotFound();

            var products = await _context.Products.Where(x => !x.IsDeleted && x.IsActive).ToListAsync();
            ViewBag.Products = products; // Dropdown'ları HTML tarafında manuel döneceğimiz için SelectList yerine ham listeyi atıyoruz

            var dto = new SetEditDto
            {
                Id = set.Id,
                Name = set.Name,
                IsActive = set.IsActive,
                // Setin içindeki silinmemiş ürünlerin ID'lerini çekiyoruz
                ProductIds = set.ProductSetItems.Where(x => !x.IsDeleted).Select(x => x.ProductId).ToList()
            };

            return View(dto);
        }

        // 7. DÜZENLEMEYİ KAYDET (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(SetEditDto dto)
        {
            var set = await _context.ProductSets
                .Include(x => x.ProductSetItems)
                .FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (set == null) return NotFound();

            set.Name = dto.Name;
            set.IsActive = dto.IsActive;
            set.ModifiedDate = DateTime.Now;

            // Çoka çok (Many-to-Many) güncellemenin en güvenli yolu: 
            // Önce eski köprü kayıtlarını pasife çek (Soft Delete), sonra formdan gelenleri yeni kayıt olarak ekle.
            foreach (var item in set.ProductSetItems.Where(x => !x.IsDeleted))
            {
                item.IsDeleted = true;
                item.DeletedDate = DateTime.Now;
            }

            if (dto.ProductIds != null && dto.ProductIds.Any())
            {
                foreach (var prodId in dto.ProductIds)
                {
                    var newItem = new ProductSetItem
                    {
                        ProductSetId = set.Id,
                        ProductId = prodId,
                        Quantity = 1,
                        CreatedDate = DateTime.Now,
                        IsActive = true,
                        IsDeleted = false
                    };
                    _context.ProductSetItems.Add(newItem);
                }
            }

            _context.ProductSets.Update(set);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Set başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var deletedSet = await _context.ProductSets
                .Include(s => s.ProductSetItems)
                    .ThenInclude(psi => psi.Product)
                .Where(s => s.IsDeleted)
                .ToListAsync();

            var result = deletedSet.Select(s => new SetDeletedDto
            {
                Id = s.Id,
                Name = s.Name,
                IsActive = s.IsActive,
                DeletedDate = s.DeletedDate,

                // DÜZELTİLEN KISIM BURASI: !x.IsDeleted şartını kaldırdık!
                ProductNames = s.ProductSetItems
                                .Where(x => x.Product != null)
                                .Select(x => x.Product.Name)
                                .ToList()
            }).ToList();

            return View(result);
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            // 1. DİKKAT: İçindeki ürün bağlantılarını (ProductSetItems) da dahil ediyoruz ki onları da kurtarabilelim!
            var set = await _context.ProductSets
                .Include(x => x.ProductSetItems)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (set != null)
            {
                // A) Ana Seti Kurtar
                set.IsDeleted = false;
                set.IsActive = true;
                set.DeletedDate = null;

                // B) Setin içindeki silinmiş ürün bağlantılarını da çöpten çıkar
                foreach (var item in set.ProductSetItems)
                {
                    item.IsDeleted = false;
                    item.DeletedDate = null;
                    item.IsActive = true;
                }

                // C) Eksik olan o sihirli satırlar: EF Core'a kaydetmesini söyle!
                _context.ProductSets.Update(set);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Set ve içindeki ürünler başarıyla geri yüklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Geri yüklenecek set bulunamadı.";
            }

            return RedirectToAction("Deleted");
        }
    }
}