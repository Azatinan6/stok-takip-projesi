using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.Set;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;

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

        // 1. SETLERİ LİSTELEME EKRANI
        public async Task<IActionResult> Index()
        {
            var sets = await _context.ProductSets
                .Include(x => x.ProductSetItems)
                    .ThenInclude(psi => psi.Product) // Ürünlerin isimlerini çekmek için Product'ı Include ediyoruz
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.Id)
                .ToListAsync();

            return View(sets);
        }

        // 2. YENİ SET EKLEME SAYFASI (GET)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // Ürünleri isme göre sıralı getiriyoruz ki Select2 içinde aramak kolay olsun
            var products = await _context.Products
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.Products = products;

            return View(new SetCreateDto());
        }

        // 3. YENİ SETİ KAYDETME (POST)
        [HttpPost]
        public async Task<IActionResult> Create(SetCreateDto dto)
        {
            if (string.IsNullOrEmpty(dto.Name) || dto.ProductIds == null || !dto.ProductIds.Any())
            {
                TempData["ErrorMessage"] = "Set adı ve en az bir ürün zorunludur!";
                return RedirectToAction("Create");
            }

            // --- YENİ EKLENEN İMLA KURALI (TitleCase) ---
            var textInfo = new System.Globalization.CultureInfo("tr-TR").TextInfo;
            var normalizedSetName = textInfo.ToTitleCase((dto.Name ?? string.Empty).Trim().ToLower());

            var newSet = new ProductSet
            {
                Name = normalizedSetName, // İmlası düzeltilmiş ismi kaydediyoruz
                IsActive = dto.IsActive,
                CreatedDate = DateTime.Now,
                IsDeleted = false
            };

            _context.ProductSets.Add(newSet);
            await _context.SaveChangesAsync();

            foreach (var productId in dto.ProductIds)
            {
                var setItem = new ProductSetItem
                {
                    ProductSetId = newSet.Id,
                    ProductId = productId,
                    Quantity = 1,
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

        // 4. AKTİF/PASİF YAPMA İŞLEMİ (AJAX/SweetAlert Uyumlu)
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var set = await _context.ProductSets.FirstOrDefaultAsync(x => x.Id == id);
            if (set != null)
            {
                set.IsActive = !set.IsActive;
                set.ModifiedDate = DateTime.Now;
                _context.ProductSets.Update(set);
                await _context.SaveChangesAsync();
                return Ok(new { success = true, message = $"Set durumu güncellendi." });
            }
            return NotFound(new { success = false, message = "Set bulunamadı." });
        }

        // 5. DÜZENLEME SAYFASINI AÇ (GET)
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var set = await _context.ProductSets
                .Include(x => x.ProductSetItems)
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);

            if (set == null) return NotFound();

            var products = await _context.Products
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Name)
                .ToListAsync();

            ViewBag.Products = products;

            var dto = new SetEditDto
            {
                Id = set.Id,
                Name = set.Name,
                IsActive = set.IsActive,
                ProductIds = set.ProductSetItems.Where(x => !x.IsDeleted).Select(x => x.ProductId).ToList()
            };

            return View(dto);
        }

        // 6. DÜZENLEMEYİ KAYDET (POST)
        [HttpPost]
        public async Task<IActionResult> Edit(SetEditDto dto)
        {
            var set = await _context.ProductSets
                .Include(x => x.ProductSetItems.Where(psi => !psi.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (set == null) return NotFound();

            // --- YENİ EKLENEN İMLA KURALI (TitleCase) ---
            var textInfo = new System.Globalization.CultureInfo("tr-TR").TextInfo;
            set.Name = textInfo.ToTitleCase((dto.Name ?? string.Empty).Trim().ToLower());
            set.IsActive = dto.IsActive;
            set.ModifiedDate = DateTime.Now;

            // ZEKİ GÜNCELLEME (Sadece farkları bul)
            var currentProductIds = set.ProductSetItems.Select(x => x.ProductId).ToList();
            var incomingProductIds = dto.ProductIds ?? new List<int>();

            // Çıkarılan Ürünleri Bul ve Pasife Çek (Soft Delete)
            var itemsToRemove = set.ProductSetItems.Where(x => !incomingProductIds.Contains(x.ProductId)).ToList();
            foreach (var item in itemsToRemove)
            {
                item.IsDeleted = true;
                item.DeletedDate = DateTime.Now;
                _context.ProductSetItems.Update(item);
            }

            // Yeni Eklenen Ürünleri Bul ve Ekle
            var idsToAdd = incomingProductIds.Except(currentProductIds).ToList();
            foreach (var prodId in idsToAdd)
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

            _context.ProductSets.Update(set);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Set başarıyla güncellendi.";
            return RedirectToAction("Index");
        }
    }
}