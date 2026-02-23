using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StockTrack.Business.Abstract;
using StockTrack.Entity.Enitities; // Enitities klasör yazımına dikkat
using StockTrack.WebUI.Consts;
using System;
using System.Threading.Tasks;

namespace StockTrack.WebUI.Controllers
{
    [Authorize(Roles = RoleConsts.Admin)]
    public class CargoDefinitionController : Controller // Controller kelimesini ekledik
    {
        private readonly ICargoDefinitionService _cargoDefinitionService; // Doğru servisi çağırdık

        public CargoDefinitionController(ICargoDefinitionService cargoDefinitionService)
        {
            _cargoDefinitionService = cargoDefinitionService;
        }

        public async Task<IActionResult> Index()
        {
            // Sadece silinmemiş ve Tipi 1 (Kontrol Sonucu) olanları getir
            var values = await _cargoDefinitionService.TGetFilteredListAsync(x => !x.IsDeleted && x.DefinitionType == 1);
            return View(values);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CargoDefinition model) // Doğru modeli aldık
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
    }
}