using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTrack.Business.Abstract;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace StockTrack.WebUI.Controllers
{
    [Authorize(Roles = RoleConsts.Admin)]
    public class CargoNameController : Controller
    {
        private readonly ICargoNameService _cargoNameService;

        // Sadece Servis yeterli, DbContext'e burada ihtiyacımız yok (Clean Code)
        public CargoNameController(ICargoNameService cargoNameService)
        {
            _cargoNameService = cargoNameService;
        }

        // 1. LİSTELEME EKRANI (Az önce eksikti)
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            // Sadece silinmemiş kargo firmalarını getir
            var values = await _cargoNameService.TGetFilteredListAsync(x => !x.IsDeleted);
            return View(values.OrderByDescending(x => x.Id).ToList());
        }

        // 2. YENİ KARGO FİRMASI EKLE
        [HttpPost]
        public async Task<IActionResult> Create(CargoName model)
        {
            // İmla zırhı (TitleCase ve Trim)
            var textInfo = new System.Globalization.CultureInfo("tr-TR").TextInfo;
            model.Name = textInfo.ToTitleCase((model.Name ?? string.Empty).Trim().ToLower());

            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.IsDeleted = false; // Silme işlemi kalktığı için default false

            await _cargoNameService.TCreateAsync(model);
            TempData["SuccessMessage"] = "Kargo firması başarıyla eklendi.";

            return RedirectToAction("Index");
        }

        // 3. KARGO FİRMASI DÜZENLE
        [HttpPost]
        public async Task<IActionResult> Edit(CargoName model)
        {
            var existingCargo = await _cargoNameService.TGetByIdAsync(model.Id);

            if (existingCargo != null)
            {
                // İmla zırhı (TitleCase ve Trim)
                var textInfo = new System.Globalization.CultureInfo("tr-TR").TextInfo;
                existingCargo.Name = textInfo.ToTitleCase((model.Name ?? string.Empty).Trim().ToLower());
                existingCargo.ModifiedDate = DateTime.Now;

                await _cargoNameService.TUpdateAsync(existingCargo);
                TempData["SuccessMessage"] = "Kargo firması başarıyla güncellendi.";
            }

            return RedirectToAction("Index");
        }

        // 4. AKTİF/PASİF YAPMA (Ajax ve SweetAlert ile uyumlu hale getirildi)
        [HttpPost]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var existingCargo = await _cargoNameService.TGetByIdAsync(id);

            if (existingCargo != null)
            {
                existingCargo.IsActive = !existingCargo.IsActive;
                existingCargo.ModifiedDate = DateTime.Now;

                await _cargoNameService.TUpdateAsync(existingCargo);

                return Ok(new { success = true, message = "Firma durumu başarıyla güncellendi." });
            }

            return NotFound(new { success = false, message = "Kargo firması bulunamadı." });
        }
    }
}