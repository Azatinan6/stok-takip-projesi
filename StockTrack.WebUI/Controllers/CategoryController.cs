using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.Dto.Category;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class CategoryController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IToastNotification _toastNotification;

        public CategoryController(UserManager<AppUser> userManager, IToastNotification toastNotification, ICategoryService categoryService)
        {
            _userManager = userManager;
            _toastNotification = toastNotification;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _categoryService.TGetFilteredListAsync(x => !x.IsDeleted);

            var resultCategoryList = categories.OrderByDescending(x => x.CreatedDate).ThenByDescending(x => x.IsActive).Select(x => new ResultCategoryDto
            {
                Id = x.Id,
                Name = x.Name,                
                CreatedDate = x.CreatedDate,
                IsActive = x.IsActive
            }).ToList();

            return View(resultCategoryList);
        }
        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Create(IEnumerable<CreateCategoryDto> categoryDataList)
        {
            // 1) Boş/null kontrolü
            if (categoryDataList.Count() == 0)
            {
                TempData["ErrorMessage"] = "En az bir kategori eklemelisiniz.";
                return RedirectToAction("Index");
            }

            // 2) ModelState kontrolü
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Bazı kategori verileri geçersiz. Lütfen tekrar kontrol edin.";
                return RedirectToAction("Index");
            }

            // 3) Mevcut kullanıcı
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            // 4) DTO isimlerini normalize et
            var dtoNames = categoryDataList.Select(d => d.Name?.Trim()).Where(n => !string.IsNullOrWhiteSpace(n)).Select(n => n!.ToLower()).Distinct().ToList();

            if (!dtoNames.Any())
            {
                TempData["ErrorMessage"] = "Kategori adları boş olamaz.";
                return RedirectToAction("Index");
            }

            // 5) İsimleri veritabanında ara (silinmiş dahil)
            //var existingCatsAll = await _categoryService.TGetFilteredListAsync(c => dtoNames.Contains(c.Name.ToLower()));
            var existingCatsAll = (await _categoryService.TGetListAsync())
                .Where(c => dtoNames.Any(d => d.Equals(c.Name, StringComparison.OrdinalIgnoreCase)))
                .ToList();


            // Aktif olanlar (IsDeleted = false)  doğrudan eklenmesin
            var existingActive = existingCatsAll.Where(c => !c.IsDeleted).ToList();
            var existingActiveNames = existingActive.Select(c => c.Name).ToList();

            // Silinmiş olanlar (IsDeleted = true) → kullanıcı onayı ile geri yüklenecek
            var existingDeleted = existingCatsAll.Where(c => c.IsDeleted).ToList();
            var deletedNames = existingDeleted.Select(c => c.Name).ToList();
            var deletedIds = existingDeleted.Select(c => c.Id).ToList();

            // 6) Tamamen yeni olanlar (ne aktif var ne de silinmiş var)
            var newDtos = categoryDataList
                .Where(d =>
                    !existingActiveNames.Any(en => string.Equals(en, d.Name, StringComparison.OrdinalIgnoreCase)) &&
                    !deletedNames.Any(dn => string.Equals(dn, d.Name, StringComparison.OrdinalIgnoreCase))
                )
                .ToList();

            // 7) Hiç yeni yok ve silinmiş de yok → hepsi zaten aktif mevcut
            if (!newDtos.Any() && !existingDeleted.Any())
            {
                TempData["ErrorMessage"] = $"Aşağıdaki kategoriler zaten mevcut ve eklenmedi: {string.Join(", ", existingActiveNames)}";
                return RedirectToAction("Index");
            }

            // 8) Silinmiş kategoriler varsa, Index'te confirm sorulması için bilgileri TempData ile taşı
            if (existingDeleted.Any())
            {
                TempData["HasDeletedCategories"] = true;
                TempData["DeletedCategoryNames"] = string.Join(", ", deletedNames);
                TempData["DeletedCategoryIds"] = string.Join(",", deletedIds); // JS tarafında AJAX ile Restore'a gönderilecek
            }

            // 9) Tamamen yeni olanları ekle
            if (newDtos.Any())
            {
                var now = DateTime.Now;
                var toInsert = newDtos.Select(dto => new Category
                {
                    Name = dto.Name.Trim(),                   
                    CreatedBy = currentUser.NameSurname,
                    CreatedDate = now,
                    ModifiedDate = now,
                    IsActive = true
                }).ToList();

                await _categoryService.TCreateRangeAsync(toInsert);
                TempData["SuccessMessage"] = $"{toInsert.Count} adet kategori başarıyla oluşturuldu.";
            }

            // 10) Zaten aktif olanlar varsa uyarı ver
            if (existingActiveNames.Any())
            {
                TempData["ErrorMessage"] = $"Ancak zaten mevcut kategoriler de vardı: {string.Join(", ", existingActiveNames)}";
            }

            // 11) Index'e dön (silinmişler için confirm burada çalışacak)
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        //silinmiş kategorileri geri yükleme
        public async Task<IActionResult> RestoreDeletedCategories([FromBody]List<int> categoryIds)
        {
            if (categoryIds == null || !categoryIds.Any())
                return BadRequest("Geri yüklenecek kategori bulunamadı.");

            //var deletedCats = await _categoryService.TGetFilteredListAsync(c => categoryIds.Contains(c.Id) && c.IsDeleted);
            var deletedCats = (await _categoryService.TGetListAsync()).Where(c => categoryIds.Any(d => d.Equals(c.Id) && c.IsDeleted)).ToList();

            if (!deletedCats.Any())
                return BadRequest("Silinmiş kategori bulunamadı.");

            var now = DateTime.Now;
            foreach (var cat in deletedCats)
            {
                cat.IsDeleted = false;
                cat.ModifiedDate = now;
            }
            await _categoryService.TUpdateRangeAsync(deletedCats);

            var restoredNames = string.Join(", ", deletedCats.Select(c => c.Name));

            TempData["SuccessMessage"] = $"{deletedCats.Count} adet kategori tekrar aktif hale getirildi: {restoredNames}";
            return Ok(new { status = "ok", message = TempData["SuccessMessage"] });
        }


        //Pasif hale getirme
        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var category = await _categoryService.TGetByIdAsync(id);
            if (category == null)
                return NotFound(new { success = false, message = "Kategori bulunamadı." });

            category.IsActive = false;
            await _categoryService.TUpdateAsync(category);
            return Ok(new { success = true, message = $"{category.Name} pasif hale getirildi." });
        }
        //Aktif hale getirme
        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Activate(int id)
        {
            var category = await _categoryService.TGetByIdAsync(id);
            if (category == null)
                return NotFound(new { success = false, message = "Kategori bulunamadı." });

            category.IsActive = true;
            await _categoryService.TUpdateAsync(category);
            return Ok(new { success = true, message = $"{category.Name} aktif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Edit(UpdateCategoryDto updateCategoryDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Kategori güncellenemedi. Kategori adı boş geçilemez tekrar kontrol edin.";
                return RedirectToAction("Index");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)                
                return Challenge();// Oturum açmamışsa giriş sayfasına yönlendir

            //  boş olma durumunu kontrolü
            var newName = updateCategoryDto.Name?.Trim() ?? "";
            //  Veritabanında bu isimde var olan, fakat güncellenecek kayıt olmayan ilk kaydı al
            var duplicates = await _categoryService.TGetFilteredListAsync(c => c.Name.ToLower() == newName.ToLower());
            if (duplicates.Any())
            {
                TempData["ErrorMessage"] = $"{newName} isimli kategori zaten mevcut. Güncelleme yapılmadı daha farklı bir kategori adı deneyiniz.";
                return RedirectToAction("Index");
            }

            //  Güncellenecek kaydı çek
            var category = await _categoryService.TGetByIdAsync(updateCategoryDto.Id);
            if (category == null)
            {
                TempData["ErrorMessage"] = "Kategori bulunamadı";
                return RedirectToAction("Index");
            }
           
            category.Name = updateCategoryDto.Name;
            category.ModifiedBy = currentUser.NameSurname;
            category.ModifiedDate = DateTime.Now;
            await _categoryService.TUpdateAsync(category);
            _toastNotification.AddSuccessToastMessage("Kategori başarıyla güncellendi.", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _categoryService.TGetByIdAsync(id);
            if (category == null)
                return NotFound(new { success = false, message = "Kategori bulunamadı." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            category.IsDeleted = true;
            category.DeletedDate = DateTime.Now;
            category.DeletedBy = currentUser.NameSurname;
            await _categoryService.TUpdateAsync(category);
            return Ok(new
            {
                success = true,
                message = $"{category.Name} başarıyla silindi."
            });
        }


        [HttpGet]
        //Silinen Kategoriler
        public async Task<IActionResult> Deleted()
        {
            var getCategories = await _categoryService.TGetFilteredListAsync(x => x.IsDeleted == true);
            var result = getCategories.Select(x => new DeletedCategoryDto()
            {
                DeletedBy = x.DeletedBy,
                DeletedDate = x.DeletedDate,
                Id = x.Id,
                Name = x.Name,
                IsActive = x.IsActive
            });
            return View(result);
        }

    }

}

