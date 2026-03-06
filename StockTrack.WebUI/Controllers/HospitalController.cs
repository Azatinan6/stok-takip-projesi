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

                    // --- Sağlık.Net (SN) Alanları ---
                    SnUsername = model.SnUsername,
                    SnPassword = model.SnPassword,

                    IsActive = true, // Varsayılan aktif
                    CreatedDate = DateTime.Now
                };

                await _hospitalService.TAddAsync(hospital);

                return RedirectToAction("Index");
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Update(HospitalUpdateDto hospitalUpdateDto)
        {
            if (ModelState.IsValid)
            {
                var hospital = await _hospitalService.TGetByIdAsync(hospitalUpdateDto.Id);

                if (hospital == null)
                {
                    TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                    return NotFound();
                }

                hospital.Name = hospitalUpdateDto.Name;
                hospital.Branch = hospitalUpdateDto.Branch;
                hospital.Address = hospitalUpdateDto.Address;
                hospital.Phone = hospitalUpdateDto.Phone;

                hospital.HbysName = hospitalUpdateDto.HbysName;
                hospital.HbysVersion = hospitalUpdateDto.HbysVersion;

                // Sağlık.Net (SN) Alanları Eklendi
                hospital.SnUsername = hospitalUpdateDto.SnUsername;
                hospital.SnPassword = hospitalUpdateDto.SnPassword;

                hospital.IsActive = hospitalUpdateDto.IsActive;

                await _hospitalService.TUpdateAsync(hospital);

                return RedirectToAction("Index");
            }
            else
            {
                var errorMessages = string.Join(" | ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                TempData["ErrorMessage"] = "Güncelleme başarısız: " + errorMessages;

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
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

            return View("Deleted", result);
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var hospital = await _hospitalService.TGetByIdAsync(id);

            if (hospital != null)
            {
                hospital.IsDeleted = false; // Silinme durumunu kaldır
                hospital.IsActive = true;    // Geri yükleyince otomatik aktif yap
                hospital.DeletedDate = null; // Silinme tarihini temizle

                await _hospitalService.TUpdateAsync(hospital);
                TempData["SuccessMessage"] = "Hastane başarıyla geri yüklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Hastane bulunamadı.";
            }

            return RedirectToAction("Deleted"); // Silinenler listesine geri dön
        }

        [HttpGet]
        public async Task<IActionResult> GetHospitalDetail(int id)
        {
            var h = await _hospitalService.TGetByIdAsync(id);
            if (h == null) return NotFound();

            var dto = new HospitalDetailDto
            {
                Name = h.Name,
                Branch = h.Branch ?? "Merkez",
                City = h.City,
                Address = h.Address,
                WebSite = h.WebSite,
                Email = h.Email,
                Phone = h.Phone,
                HbysName = h.HbysName,
                HbysVersion = h.HbysVersion,
                InstallationDate = h.InstallationDate?.ToString("dd.MM.yyyy") ?? "-",

                IntegrationUrl = h.IntegrationUrl,
                IntegrationUsername = h.IntegrationUsername,
                IntegrationPassword = h.IntegrationPassword,

                // Sağlık.Net (SN) Alanları Eklendi
                SnUsername = h.SnUsername,
                SnPassword = h.SnPassword
            };

            return Ok(dto);
        }
    }
}