using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using StockTrack.Business.Abstract;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;
using System;
using System.Threading.Tasks;
using StockTrack.Dto.CargoDefinition;

namespace StockTrack.WebUI.Controllers
{
    [Authorize(Roles = RoleConsts.Admin)]
    public class CargoDefinitionController : Controller
    {
        private readonly ICargoDefinitionService _cargoDefinitionService;

        public CargoDefinitionController(ICargoDefinitionService cargoDefinitionService)
        {
            _cargoDefinitionService = cargoDefinitionService;
        }

        // ======================================================
        // 1. KONTROL SONUCU İŞLEMLERİ (DefinitionType = 1)
        // ======================================================

        public async Task<IActionResult> Index()
        {
            // Sadece silinmemiş ve Tipi 1 (Kontrol Sonucu) olanları getir
            var values = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 1);
            return View(values);
        }
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // ... Senin diğer ViewBag'lerin (Lokasyon vs.) ...

            // Gönderim Nedenlerini Gönder (Tip 4)
            ViewBag.DispatchReasons = new SelectList(await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 4), "Id", "Name");

            // İade Nedenlerini Gönder (Tip 5)
            ViewBag.ReturnReasons = new SelectList(await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 5), "Id", "Name");

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CargoDefinition model)
        {
            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.IsDeleted = false;
            model.DefinitionType = 1; // 1 = Kontrol Sonucu kategorisi

            await _cargoDefinitionService.TCreateAsync(model);
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CargoDefinition model)
        {
            var existingCargo = await _cargoDefinitionService.TGetByIdAsync(model.Id);

            if (existingCargo != null)
            {
                existingCargo.Name = model.Name;
                await _cargoDefinitionService.TUpdateAsync(existingCargo);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var existingCargo = await _cargoDefinitionService.TGetByIdAsync(id);

            if (existingCargo != null)
            {
                existingCargo.IsDeleted = true;
                existingCargo.IsActive = false;
                existingCargo.DeletedDate = DateTime.Now;

                await _cargoDefinitionService.TUpdateAsync(existingCargo);
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var deletedCargo = await _cargoDefinitionService.TGetFilteredListAsync(c => c.IsDeleted);

            var result = deletedCargo.Select(c => new CargoDefinitionDeletedDto
            {
                Id = c.Id,
                Name = c.Name,
                DeletedDate = c.DeletedDate
            }).ToList();

            return View(result);
        }
        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var cargoDef = await _cargoDefinitionService.TGetByIdAsync(id);

            if (cargoDef != null)
            {
                cargoDef.IsDeleted = false;
                cargoDef.IsActive = true;
                cargoDef.DeletedDate = null;

                await _cargoDefinitionService.TUpdateAsync(cargoDef);
                TempData["SuccessMessage"] = "Kayıt başarıyla geri yüklendi.";

                // --- SİHİRLİ YÖNLENDİRME KISMI ---
                // Hangi tipe aitse o çöp kutusuna geri dön!
                if (cargoDef.DefinitionType == 1)
                {
                    return RedirectToAction("Deleted"); // Kontrol Sonuçları
                }
                else if (cargoDef.DefinitionType == 2 || cargoDef.DefinitionType == 3)
                {
                    return RedirectToAction("DeletedProductStatus"); // Ürün Durumları
                }
                else if (cargoDef.DefinitionType == 4 || cargoDef.DefinitionType == 5)
                {
                    // YENİ EKLENEN KISIM: Tip 4 veya 5 ise, ID'siyle beraber kendi sayfasına yolla
                    return RedirectToAction("DeletedRequestReason", new { id = cargoDef.DefinitionType });
                }
            }

            TempData["ErrorMessage"] = "Kayıt bulunamadı.";
            return RedirectToAction("Index"); // Hata olursa veya tip bulunamazsa ana sayfaya at
        }


        // ======================================================
        // 2. ÜRÜN DURUMU İŞLEMLERİ (DefinitionType = 2 ve 3)
        // ======================================================

        public async Task<IActionResult> ProductStatusIndex()
        {
            // Sadece Giriş (2) ve Çıkış (3) durumlarını getir
            var values = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && (x.DefinitionType == 2 || x.DefinitionType == 3));
            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProductStatus(CargoDefinition model)
        {
            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.IsDeleted = false;
            // Not: DefinitionType formdan (Dropdown üzerinden 2 veya 3 olarak) geleceği için burada sabitlemiyoruz.

            await _cargoDefinitionService.TCreateAsync(model);
            return RedirectToAction("ProductStatusIndex");
        }

        [HttpPost]
        public async Task<IActionResult> EditProductStatus(CargoDefinition model)
        {
            var existingCargo = await _cargoDefinitionService.TGetByIdAsync(model.Id);

            if (existingCargo != null)
            {
                existingCargo.Name = model.Name;
                await _cargoDefinitionService.TUpdateAsync(existingCargo);
            }

            return RedirectToAction("ProductStatusIndex");
        }

        [HttpGet]
        public async Task<IActionResult> DeleteProductStatus(int id)
        {
            var existingCargo = await _cargoDefinitionService.TGetByIdAsync(id);

            if (existingCargo != null)
            {
                existingCargo.IsDeleted = true;
                existingCargo.IsActive = false;
                existingCargo.DeletedDate = DateTime.Now;

                await _cargoDefinitionService.TUpdateAsync(existingCargo);
            }

            return RedirectToAction("ProductStatusIndex");
        }
        [HttpGet]
        public async Task<IActionResult> DeletedProductStatus()
        {
            // Tip 2 (Giriş) veya 3 (Çıkış) olanları getir
            var deletedCargo = await _cargoDefinitionService.TGetFilteredListAsync(c => c.IsDeleted && (c.DefinitionType == 2 || c.DefinitionType == 3));

            var result = deletedCargo.Select(c => new CargoDefinitionDeletedDto
            {
                Id = c.Id,
                Name = c.Name,
                DeletedDate = c.DeletedDate,
                DefinitionType = c.DefinitionType // YENİ: Veritabanındaki tipi DTO'ya aktarıyoruz
            }).ToList();

            return View(result);
        }
        
        // ======================================================
        // 3. TALEP NEDENLERİ İŞLEMLERİ (DefinitionType = 4 ve 5)
        // ======================================================

        [HttpGet]
        public async Task<IActionResult> RequestReasonIndex(int id = 4) // Menüden gelen 4 veya 5
        {
            ViewBag.ReasonType = id; // Ekrana tipini gönderiyoruz ki başlıklar değişsin
            var values = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == id);
            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequestReason(CargoDefinition model)
        {
            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.IsDeleted = false;
            // DefinitionType artık View'daki gizli inputtan (4 veya 5) otomatik gelecek

            await _cargoDefinitionService.TCreateAsync(model);

            // Hangi sayfadaysa (Gönderim veya İade) oraya geri dönsün
            return RedirectToAction("RequestReasonIndex", new { id = model.DefinitionType });
        }

        [HttpPost]
        public async Task<IActionResult> EditRequestReason(CargoDefinition model)
        {
            var existingCargo = await _cargoDefinitionService.TGetByIdAsync(model.Id);
            int returnType = 4;

            if (existingCargo != null)
            {
                returnType = existingCargo.DefinitionType; // Geri dönülecek sayfayı bul
                existingCargo.Name = model.Name;
                await _cargoDefinitionService.TUpdateAsync(existingCargo);
            }
            return RedirectToAction("RequestReasonIndex", new { id = returnType });
        }

        [HttpGet]
        public async Task<IActionResult> DeleteRequestReason(int id)
        {
            var existingCargo = await _cargoDefinitionService.TGetByIdAsync(id);
            int returnType = 4;

            if (existingCargo != null)
            {
                returnType = existingCargo.DefinitionType; // Geri dönülecek sayfayı bul
                existingCargo.IsDeleted = true;
                existingCargo.IsActive = false;
                existingCargo.DeletedDate = DateTime.Now;
                await _cargoDefinitionService.TUpdateAsync(existingCargo);
            }
            return RedirectToAction("RequestReasonIndex", new { id = returnType });
        }
        [HttpGet]
        public async Task<IActionResult> DeletedRequestReason(int id = 4) // 4: Gönderim, 5: İade
        {
            ViewBag.ReasonType = id; // HTML tarafında başlığın "Gönderim" mi "İade" mi olacağını bilmek için

            // Sadece silinmiş ve ilgili tipe (4 veya 5) ait olanları getir
            var deletedCargo = await _cargoDefinitionService.TGetFilteredListAsync(c => c.IsDeleted && c.DefinitionType == id);

            var result = deletedCargo.Select(c => new CargoDefinitionDeletedDto
            {
                Id = c.Id,
                Name = c.Name,
                DeletedDate = c.DeletedDate,
                DefinitionType = c.DefinitionType
            }).ToList();

            return View(result);
        }
    }
}