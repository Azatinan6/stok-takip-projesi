using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.Dto.MainRepoLocation;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class MainRepoLocationController : Controller
    {
        private readonly IMainRepoLocationService _mainRepoLocationService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IToastNotification _toastNotification;

        // GEREKSİZ SERVİSLER (Product, Category, DbContext) TEMİZLENDİ!
        public MainRepoLocationController(IMainRepoLocationService mainRepoLocationService, UserManager<AppUser> userManager, IToastNotification toastNotification)
        {
            _mainRepoLocationService = mainRepoLocationService;
            _userManager = userManager;
            _toastNotification = toastNotification;
        }

        public async Task<IActionResult> Index()
        {
            var getRepo = await _mainRepoLocationService.TGetFilteredListAsync(x => !x.IsDeleted);

            var dtoList = getRepo.Select(x => new ResultMainRepoLocationDto
            {
                Id = x.Id,
                Adress = x.Adress,
                Name = x.Name,
                CreatedDate = x.CreatedDate,
                IsActive = x.IsActive
            }).ToList();

            return View(dtoList);
        }

        public async Task<IActionResult> CreateMainRepoLocation(CreateMainRepoLocationDto createMainRepoLocationDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return RedirectToAction("Index");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var now = DateTime.Now;
            var toInsert = new MainRepoLocation()
            {
                Name = createMainRepoLocationDto.Name,
                Adress = createMainRepoLocationDto.Adress,
                IsActive = true,
                CreatedDate = now,
                ModifiedDate = now,
                CreatedBy = currentUser.NameSurname,
            };
            await _mainRepoLocationService.TCreateAsync(toInsert);
            _toastNotification.AddSuccessToastMessage("Lokasyon kaydı başarıyla eklendi", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var location = await _mainRepoLocationService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });

            location.IsActive = false;
            await _mainRepoLocationService.TUpdateAsync(location);
            return Ok(new { success = true, message = $"{location.Name} pasif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Activate(int id)
        {
            var location = await _mainRepoLocationService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });

            location.IsActive = true;
            await _mainRepoLocationService.TUpdateAsync(location);
            return Ok(new { success = true, message = $"{location.Name} aktif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Delete(int id)
        {
            var location = await _mainRepoLocationService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            location.IsDeleted = true;
            location.DeletedDate = DateTime.Now;
            location.DeletedBy = currentUser.NameSurname;
            await _mainRepoLocationService.TUpdateAsync(location);
            return Ok(new
            {
                success = true,
                message = $"{location.Name} başarıyla silindi."
            });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UpdateMainRepoLocationDto updateMainRepoLocationDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return RedirectToAction("Index");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var findRepo = await _mainRepoLocationService.TGetByIdAsync(updateMainRepoLocationDto.Id);

            findRepo.Adress = updateMainRepoLocationDto.Adress;
            findRepo.Name = updateMainRepoLocationDto.Name;
            findRepo.ModifiedBy = currentUser.NameSurname;
            findRepo.ModifiedDate = DateTime.Now;

            await _mainRepoLocationService.TUpdateAsync(findRepo);
            _toastNotification.AddSuccessToastMessage("Lokasyon kaydı başarıyla güncellendi", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Deleted()
        {
            var getLocationDeleted = (await _mainRepoLocationService.TGetFilteredListAsync(x => x.IsDeleted == true)).Select(x => new DeletedMainRepoLocationListDto
            {
                Adress = x.Adress,
                CreatedDate = x.CreatedDate,
                DeletedBy = x.DeletedBy,
                DeletedDate = x.DeletedDate,
                Id = x.Id,
                IsActive = x.IsActive,
                Name = x.Name
            }).ToList();

            return View(getLocationDeleted);
        }

        [HttpPost]
        public async Task<IActionResult> Restore(int id)
        {
            var depo = await _mainRepoLocationService.TGetByIdAsync(id);

            if (depo != null)
            {
                depo.IsDeleted = false;
                depo.IsActive = true;
                depo.DeletedDate = null;

                await _mainRepoLocationService.TUpdateAsync(depo);

                TempData["SuccessMessage"] = "Depo başarıyla geri yüklendi.";
            }
            else
            {
                TempData["ErrorMessage"] = "Depo bulunamadı.";
            }

            return RedirectToAction("Deleted");
        }
    }
}