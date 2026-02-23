using Microsoft.AspNetCore.Mvc;
using StockTrack.Business.Abstract;
using StockTrack.Dto.Hospital;
using StockTrack.Entity.Enitities;

namespace StockTrack.WebUI.Controllers
{
    public class HospitalController : Controller
    {
        private readonly IHospitalService _hospitalService;

        public HospitalController(IHospitalService hospitalService)
        {
            _hospitalService = hospitalService;
        }
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            //var values = await _hospitalService.TGetListAsync();

            var values = await _hospitalService.TGetFilteredListAsync(h => !h.IsDeleted); // Silinmemiş hastaneleri getir

            return View(values);
        }

        [HttpGet]
        public IActionResult Add()
        { 
            return View(); 
        }

        [HttpPost]
        public async Task<IActionResult> Add(HospitalAddDto model)
        {
            if (ModelState.IsValid)
            {
                Hospital hospital = new Hospital()
                {
                    Name = model.Name,
                    Branch = model.Branch,
                    City = model.City,
                    Address = model.Address,
                    Phone = model.Phone,
                    Email = model.Email,
                    WebSite = model.WebSite,

                    // --- PDF'ten Gelen Yeni Alanlar ---
                    HbysName = model.HbysName,
                    HbysVersion = model.HbysVersion,
                    InstallationDate = model.InstallationDate,
                    IntegrationUrl = model.IntegrationUrl,
                    IntegrationUsername = model.IntegrationUsername,
                    IntegrationPassword = model.IntegrationPassword,

                    IsActive = true, // Varsayılan aktif
                    CreatedDate = DateTime.Now
                };

                await _hospitalService.TAddAsync(hospital);

                return RedirectToAction("Index");

            }
            return View(model);
        }
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var hospital = await _hospitalService.TGetByIdAsync(id);

            hospital.IsDeleted = true;
            hospital.DeletedDate = DateTime.Now;
            hospital.IsActive = false;

            await _hospitalService.TUpdateAsync(hospital);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var deletedHospitals = await _hospitalService.TGetFilteredListAsync(h => h.IsDeleted);

            var result = deletedHospitals.Select(h => new HospitalDeletedDto
            {
                Id = h.Id,
                Name = h.Name,
                Branch = h.Branch,
                HbysName = h.HbysName,
                HbysVersion = h.HbysVersion,
                DeletedDate = h.DeletedDate,
                IsActive = h.IsActive
            }).ToList();

            return View("Deleted",result);
        }
    }
}
