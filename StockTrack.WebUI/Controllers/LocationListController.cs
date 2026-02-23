using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.LocationList;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class LocationListController : Controller
    {
        private readonly ILocationListService _locationListService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IToastNotification _toastNotification;
        private readonly AppDbContext _appDbContext;

        public LocationListController(ILocationListService locationListService, UserManager<AppUser> userManager, IToastNotification toastNotification, AppDbContext appDbContext)
        {
            _locationListService = locationListService;
            _userManager = userManager;
            _toastNotification = toastNotification;
            _appDbContext = appDbContext;
        }

        public async Task<IActionResult> Index()
        {
            var locationList = _locationListService.TGetFilteredListAsync(x => x.IsDeleted != true).Result.Select(x => new ResultLocationListDto
            {
                Id = x.Id,
                Adress = x.Address,
                Name = x.Name,
                Phone = x.Phone,
                ContactPerson = x.ContactPerson,
                CreatedDate = x.CreatedDate,
                IsActive = x.IsActive
            }).ToList();
            return View(locationList);
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Create(CreateLocationListDto createLocationList)
        {

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Alanları doğru giriniz.";
                return RedirectToAction("Index");
            }
            //Mevcut kullanıcıyı al
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            var now = DateTime.Now;
            var toInsert = new LocationList()
            {
                Name = createLocationList.Name,
                Address = createLocationList.Address,
                ContactPerson = createLocationList.ContactPerson,
                Phone = createLocationList.Phone,
                CreatedBy = currentUser.NameSurname,
                IsActive = true,
                CreatedDate = now,
                ModifiedDate = now
            };
            await _locationListService.TCreateAsync(toInsert);
            _toastNotification.AddSuccessToastMessage("Lokasyon kaydı başarıyla eklendi", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Deactivate(int id)
        {
            var location = await _locationListService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });


            location.IsActive = false;
            await _locationListService.TUpdateAsync(location);
            return Ok(new { success = true, message = $"{location.Name} pasif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Activate(int id)
        {
            var location = await _locationListService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });


            location.IsActive = true;
            await _locationListService.TUpdateAsync(location);
            return Ok(new { success = true, message = $"{location.Name} aktif hale getirildi." });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> DeleteLocation(int id)
        {
            var location = await _locationListService.TGetByIdAsync(id);
            if (location == null)
                return NotFound(new { success = false, message = "Lokasyon bulunamadı." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            location.IsDeleted = true;
            location.DeletedDate = DateTime.Now;
            location.DeletedBy = currentUser.NameSurname;
            await _locationListService.TUpdateAsync(location);
            return Ok(new
            {
                success = true,
                message = $"{location.Name} başarıyla silindi."
            });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> Edit(UpdateLocationListDto updateLocationListDto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lokasyon güncellenemedi. Lütfen alanları kontrol edin.";
                return View("Index");
            }
            var location = await _locationListService.TGetByIdAsync(updateLocationListDto.Id);
            if (location == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return NotFound();
            }

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            location.Name = updateLocationListDto.Name;
            location.Address = updateLocationListDto.Address;
            location.ModifiedDate = DateTime.Now;
            location.ContactPerson = updateLocationListDto.ContactPerson;
            location.Phone = updateLocationListDto.Phone;
            location.ModifiedBy = currentUser.ModifiedBy;

            await _locationListService.TUpdateAsync(location);
            _toastNotification.AddSuccessToastMessage("Lokasyon başarıyla güncellendi", new ToastrOptions { Title = "Başarılı" });

            return RedirectToAction("Index");
        }
        [HttpGet]
        //Silinen lokasyonlar
        public async Task<IActionResult> Deleted()
        {
            var getLocationDeleted = (await _locationListService.TGetFilteredListAsync(x => x.IsDeleted == true)).Select(x => new DeletedLocationListDto
            {
                Address = x.Address,
                ContactPerson = x.ContactPerson,
                CreatedDate = x.CreatedDate,
                DeletedBy = x.DeletedBy,
                DeletedDate = x.DeletedDate,
                Id = x.Id,
                IsActive = x.IsActive,
                Name = x.Name,
                Phone = x.Phone
            }).ToList();

            return View(getLocationDeleted);
        }

        [HttpGet]
        public IActionResult ProductDetailsForLocation(int id)
        {
            if (id <= 0)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı";
                return RedirectToAction("Index");
            }
            var locationInfo = (from rf in _appDbContext.RequestForms
                                join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id
                                join rftype in _appDbContext.RequestFormTypes on rf.RequestFormTypeId equals rftype.Id
                                join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id
                                from cn in _appDbContext.CargoNames.Where(x => x.Id == rfd.CargoNameId).DefaultIfEmpty()
                                where !rf.IsDeleted && l.Id == id
                                select new ProductDetailsForLocationDto
                                {
                                    MainRepoName = mrl.Name,
                                    LocationName = l.Name,
                                    RequestBy = rfd.RequestBy,
                                    RequestDate = rfd.RequestDate.Value.ToString("dd MMMM yyyy"),
                                    StatusName = s.Name,
                                    RequestFormType = rftype.Name,
                                    Adress = l.Address,
                                    ToPerson = rfd.ToPerson,
                                    Phone = rfd.Phone,
                                    PackingDate = rfd.PackingDate.Value.ToString("dd MMMM yyyy"),
                                    //ApprovalDate = rfd.ApprovalDate.Value.ToString("dd MMMM yyyy"),
                                    TrackingNumber = rfd.TrackingNumber,
                                    CargoGivenDate = rfd.CargoGivenDate.Value.ToString("dd MMMM yyyy"),
                                    CompletedDate = rfd.CompletedDate.Value.ToString("dd MMMM yyyy"),
                                    Description = rfd.Description,
                                    InstallationDate = rfd.InstallationDate.Value.ToString("dd MMMM yyyy"),
                                    CargoName = cn != null ? cn.Name : null,
                                    Persons = (from p in _appDbContext.PersonDetails
                                               where p.RequestFormDetailId == rfd.Id
                                               select p.AppUser.NameSurname).ToList(),

                                    Products = (from rp in _appDbContext.RequestProducts
                                                join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                where rp.RequestFormId == rf.Id
                                                select new ProductDetailsLocationVM
                                                {
                                                    CategoryName = c.Name,
                                                    ProductName = p.Name,
                                                    UsedQuantity = rp.Quantity
                                                }).ToList()
                                }).AsNoTracking().ToList();

            return Ok(locationInfo);


        }
    }
}
