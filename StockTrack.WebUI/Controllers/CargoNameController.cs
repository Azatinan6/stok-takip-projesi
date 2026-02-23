using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTrack.Business.Abstract;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;
using System;
using System.Threading.Tasks;

namespace StockTrack.WebUI.Controllers
{
    [Authorize(Roles = RoleConsts.Admin)]
    public class CargoNameController : Controller
    {
        private readonly ICargoNameService _cargoNameService;

        public CargoNameController(ICargoNameService cargoNameService)
        {
            _cargoNameService = cargoNameService;
        }

        public async Task<IActionResult> Index()
        {
            // Silinmemiş olanları listele
            var values = await _cargoNameService.TGetFilteredListAsync(x => !x.IsDeleted);
            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CargoName model)
        {
            model.CreatedDate = DateTime.Now;
            model.IsActive = true;
            model.IsDeleted = false;
            await _cargoNameService.TCreateAsync(model);
            return RedirectToAction("Index");
        }

        // --- YENİ EKLENEN METODLAR ---

        // 1. DÜZENLEME (Edit) İŞLEMİ
        [HttpPost]
        public async Task<IActionResult> Edit(CargoName model)
        {
            // Önce güncellenecek kaydı veritabanından bul
            var existingCargo = await _cargoNameService.TGetByIdAsync(model.Id);

            if (existingCargo != null)
            {
                // Sadece ismini güncelle (ID ve diğer tarihleri bozmamak için)
                existingCargo.Name = model.Name;

                await _cargoNameService.TUpdateAsync(existingCargo);
            }

            return RedirectToAction("Index");
        }

        // 2. SİLME (Soft Delete) İŞLEMİ
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var existingCargo = await _cargoNameService.TGetByIdAsync(id);

            if (existingCargo != null)
            {
                existingCargo.IsDeleted = true;
                existingCargo.IsActive = false;
                existingCargo.DeletedDate = DateTime.Now;

                await _cargoNameService.TUpdateAsync(existingCargo);
            }

            return RedirectToAction("Index");
        }
    }
}