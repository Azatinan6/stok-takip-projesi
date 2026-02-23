using DocumentFormat.OpenXml.Bibliography;
using Humanizer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.InstallationDetail;
using StockTrack.Dto.RequestForm;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Enums;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class InstallationController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;

        public InstallationController(AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        //Onay bekleyen kurulumlar
        public async Task<IActionResult> PendingInstallations()
        {
            var requestFroms = (from rf in _appDbContext.RequestForms
                                join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                where rf.RequestFormTypeId == (int)EnumRequestType.Kurulum && !rfd.IsDeleted && rfd.StatusId == (int)EnumStatusType.OnayBekliyor
                                select new ApprovalPendingRequestFormDto
                                {
                                    RequestFormDetailId = rfd.Id,
                                    MainRepoName = m.Name,
                                    LocationName = l.Name,
                                    LocationAdress = l.Address,
                                    CreatedBy = rfd.CreatedBy,
                                    CreatedDate = rfd.CreatedDate,
                                    InstallationDate = rfd.InstallationDate,
                                    Description = rfd.Description,
                                    RequestFormBy = rfd.RequestBy,
                                    RequestFormDate = rfd.RequestDate,
                                    Persons = (from p in _appDbContext.PersonDetails
                                               where p.RequestFormDetailId == rfd.Id
                                               select p.AppUser.NameSurname).ToList(),

                                    Products = (from rp in _appDbContext.RequestProducts
                                     join p in _appDbContext.Products on rp.ProductId equals p.Id
                                     join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                     where rp.RequestFormId == rf.Id
                                     select new ProductDetailDto
                                     {
                                         CategoryName = c.Name,
                                         ProductName = p.Name,
                                         ImageUrl = p.PhotoUrl,
                                         Quantity = rp.Quantity
                                     }).ToList()
                                    
                                }).ToList();

            return View(requestFroms);

        }
        //Kurulum Bekleyen ürünler
        public async Task<IActionResult> ReadyToInstall()
        {
            var requestForms = (from rf in _appDbContext.RequestForms
                                join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                where rf.RequestFormTypeId == (int)EnumRequestType.Kurulum && !rfd.IsDeleted && rfd.StatusId == (int)EnumStatusType.KurulumBekliyor
                                select new ReadyToInstallDto
                                {
                                    RequestFormDetailId = rfd.Id,
                                    RequestFormTypeId = rf.RequestFormTypeId,
                                    MainRepoName = m.Name,
                                    LocationName = l.Name,
                                    LocationAdress = l.Address,
                                    CreatedBy = rfd.CreatedBy,
                                    CreatedDate = rfd.CreatedDate,
                                    InstallationDate = rfd.InstallationDate,
                                    Description = rfd.Description,
                                    RequestFormBy = rfd.RequestBy,
                                    RequestFormDate = rfd.RequestDate,
                                    ApprovalBy = rfd.ApprovalBy,
                                    ApprovalDate = rfd.ApprovalDate,
                                    Persons = (from p in _appDbContext.PersonDetails
                                               where p.RequestFormDetailId == rfd.Id
                                               select p.AppUser.NameSurname).ToList(),

                                    Products = (from rp in _appDbContext.RequestProducts
                                                join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                where rp.RequestFormId == rf.Id
                                                select new ProductDetailDto
                                                {
                                                    CategoryName = c.Name,
                                                    ProductName = p.Name,
                                                    ImageUrl = p.PhotoUrl,
                                                    Quantity = rp.Quantity
                                                }).ToList()
                                }).ToList();
           
            return View(requestForms);
        }
        //Kurulum tamamlananlar 
        public async Task<IActionResult> CompletedInstallations()
        {
            var requestForms = (from rf in _appDbContext.RequestForms
                                join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                where rf.RequestFormTypeId == (int)EnumRequestType.Kurulum && !rfd.IsDeleted && rfd.StatusId == (int)EnumStatusType.Tamamlandı
                                select new CompletedInstallDto
                                {
                                    RequestFormDetailId = rfd.Id,
                                    RequestFormTypeId = rf.RequestFormTypeId,
                                    MainRepoName = m.Name,
                                    LocationName = l.Name,
                                    LocationAdress = l.Address,
                                    CreatedBy = rfd.CreatedBy,
                                    CreatedDate = rfd.CreatedDate,
                                    InstallationDate = rfd.InstallationDate,
                                    Description = rfd.Description,
                                    RequestFormBy = rfd.RequestBy,
                                    RequestFormDate = rfd.RequestDate,
                                    ApprovalBy = rfd.ApprovalBy,
                                    ApprovalDate = rfd.ApprovalDate,
                                    CompletedDate = rfd.CompletedDate,
                                    Persons = (from p in _appDbContext.PersonDetails
                                               where p.RequestFormDetailId == rfd.Id
                                               select p.AppUser.NameSurname).ToList(),

                                    Products = (from rp in _appDbContext.RequestProducts
                                                join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                where rp.RequestFormId == rf.Id
                                                select new ProductDetailDto
                                                {
                                                    CategoryName = c.Name,
                                                    ProductName = p.Name,
                                                    ImageUrl = p.PhotoUrl,
                                                    Quantity = rp.Quantity
                                                }).ToList()
                                }).ToList();
            return View(requestForms);
        }

        //Kurulum iptal edilenler 
        public async Task<IActionResult> CancelledInstallations()
        {
            var requestForms = (from rf in _appDbContext.RequestForms
                                join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                where rf.RequestFormTypeId == (int)EnumRequestType.Kurulum && !rfd.IsDeleted && rfd.StatusId == (int)EnumStatusType.İptal
                                select new CancelledInstallDto
                                {
                                    RequestFormDetailId = rfd.Id,
                                    RequestFormTypeId = rf.RequestFormTypeId,
                                    MainRepoName = m.Name,
                                    LocationName = l.Name,
                                    LocationAdress = l.Address,
                                    CreatedBy = rfd.CreatedBy,
                                    CreatedDate = rfd.CreatedDate,
                                    InstallationDate = rfd.InstallationDate,
                                    Description = rfd.Description,
                                    CancelledDesc = rfd.CanceledDesc,
                                    RequestFormBy = rfd.RequestBy,
                                    RequestFormDate = rfd.RequestDate,
                                    ApprovalBy = rfd.ApprovalBy,
                                    ApprovalDate = rfd.ApprovalDate,
                                    CancelledDate = rfd.CanceledDate,
                                    CancelledBy = rfd.CanceledBy,
                                    Persons = (from p in _appDbContext.PersonDetails
                                               where p.RequestFormDetailId == rfd.Id
                                               select p.AppUser.NameSurname).ToList(),

                                    Products = (from rp in _appDbContext.RequestProducts
                                                join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                where rp.RequestFormId == rf.Id
                                                select new ProductDetailDto
                                                {
                                                    CategoryName = c.Name,
                                                    ProductName = p.Name,
                                                    ImageUrl = p.PhotoUrl,
                                                    Quantity = rp.Quantity
                                                }).ToList()

                                }).ToList();
            return View(requestForms);
        }
        public async Task<IActionResult> InstallationCalendar()
        {
            var today = DateTime.Today;

            var data = await (from rf in _appDbContext.RequestForms
                              join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                              join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                              join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                              where rf.RequestFormTypeId == (int)EnumRequestType.Kurulum && !rfd.IsDeleted
                              orderby rfd.CanceledDate descending
                              select new
                              {
                                  rf,
                                  rfd,
                                  l,
                                  m,
                                  Products = (from rp in _appDbContext.RequestProducts
                                              join p in _appDbContext.Products on rp.ProductId equals p.Id
                                              join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                              where rp.RequestFormId == rf.Id
                                              select new ProductDetailDto
                                              {
                                                  CategoryName = c.Name,
                                                  ProductName = p.Name,
                                                  ImageUrl = p.PhotoUrl,
                                                  Quantity = rp.Quantity
                                              }).ToList(),

                                  Persons = (from pd in _appDbContext.PersonDetails
                                             where pd.RequestFormDetailId == rfd.Id
                                             select pd.AppUser.NameSurname).ToList()
                              }).ToListAsync();

            // hesaplamalar
            var result = data.Select(x =>
            {
                // --- Status & renk ---
                var status = EnumStatusType.Talep;
                string color = "#3788d8"; // default mavi
                string humanizedDays = "";

                if (x.rfd.CanceledDate != null)
                {
                    status = EnumStatusType.İptal;
                    color = "#dc3545";
                }
                else if (x.rfd.CompletedDate != null)
                {
                    status = EnumStatusType.Tamamlandı;
                    color = "#198754";
                }
                else if (x.rfd.InstallationDate != null)
                {
                    status = EnumStatusType.KurulumBekliyor;
                    color = "#ffc107";
                }

                if (x.rfd.InstallationDate.HasValue)
                {
                    int diff = (x.rfd.InstallationDate.Value.Date - today).Days;
                    if (diff > 0) humanizedDays = $"{diff} gün kaldı";
                    else if (diff == 0) humanizedDays = "Bugün";
                    else humanizedDays = $"{Math.Abs(diff)} gün önce";
                }

                return new InstallationCalenderDto
                {
                    RequestFormDetailId = x.rfd.Id,
                    RequestFormTypeId = x.rf.RequestFormTypeId,
                    MainRepoName = x.m.Name,
                    LocationName = x.l.Name,
                    LocationAdress = x.l.Address,
                    CreatedBy = x.rfd.CreatedBy,
                    CreatedDate = x.rfd.CreatedDate,
                    InstallationDate = x.rfd.InstallationDate,
                    Description = x.rfd.Description,
                    CancelledDesc = x.rfd.CanceledDesc,
                    RequestFormBy = x.rfd.RequestBy,
                    RequestFormDate = x.rfd.RequestDate,
                    ApprovalBy = x.rfd.ApprovalBy,
                    ApprovalDate = x.rfd.ApprovalDate,
                    CancelledDate = x.rfd.CanceledDate,
                    CancelledBy = x.rfd.CanceledBy,
                    CompletedDate = x.rfd.CompletedDate,

                    Products = x.Products,
                    Persons = x.Persons,

                    Status = status.ToString(),
                    Color = color,
                    HumanizedDays = humanizedDays
                };
            }).ToList();

            return View(result);
        }


        public async Task<IActionResult> InstallationCalender()
        {

            var today = DateTime.Today;

            var data = await (from rf in _appDbContext.RequestForms
                              join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                              join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                              join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                              where rf.RequestFormTypeId == (int)EnumRequestType.Kurulum && !rfd.IsDeleted
                              orderby rfd.CanceledDate descending
                              select new
                              {
                                  rf,
                                  rfd,
                                  l,
                                  m,
                                  Products = (from rp in _appDbContext.RequestProducts
                                              join p in _appDbContext.Products on rp.ProductId equals p.Id
                                              join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                              where rp.RequestFormId == rf.Id
                                              select new ProductDetailDto
                                              {
                                                  CategoryName = c.Name,
                                                  ProductName = p.Name,
                                                  ImageUrl = p.PhotoUrl,
                                                  Quantity = rp.Quantity
                                              }).ToList(),

                                  Persons = (from pd in _appDbContext.PersonDetails
                                             where pd.RequestFormDetailId == rfd.Id
                                             select pd.AppUser.NameSurname).ToList()
                              }).ToListAsync();

            // C# tarafında DTO mapleme + hesaplamalar
            var result = data.Select(x =>
            {
                // --- Status & renk ---
                var status = EnumStatusType.Talep;
                string color = "#3788d8"; // default mavi
                string humanizedDays = "";

                if (x.rfd.CanceledDate != null)
                {
                    status = EnumStatusType.İptal;
                    color = "#dc3545";
                }
                else if (x.rfd.CompletedDate != null)
                {
                    status = EnumStatusType.Tamamlandı;
                    color = "#198754";
                }
                else if (x.rfd.InstallationDate != null)
                {
                    status = EnumStatusType.KurulumBekliyor;
                    color = "#ffc107";
                }

                if (x.rfd.InstallationDate.HasValue)
                {
                    int diff = (x.rfd.InstallationDate.Value.Date - today).Days;
                    if (diff > 0) humanizedDays = $"{diff} gün kaldı";
                    else if (diff == 0) humanizedDays = "Bugün";
                    else humanizedDays = $"{Math.Abs(diff)} gün önce";
                }

                return new InstallationCalenderDto
                {
                    RequestFormDetailId = x.rfd.Id,
                    RequestFormTypeId = x.rf.RequestFormTypeId,
                    MainRepoName = x.m.Name,
                    LocationName = x.l.Name,
                    LocationAdress = x.l.Address,
                    CreatedBy = x.rfd.CreatedBy,
                    CreatedDate = x.rfd.CreatedDate,
                    InstallationDate = x.rfd.InstallationDate,
                    Description = x.rfd.Description,
                    CancelledDesc = x.rfd.CanceledDesc,
                    RequestFormBy = x.rfd.RequestBy,
                    RequestFormDate = x.rfd.RequestDate,
                    ApprovalBy = x.rfd.ApprovalBy,
                    ApprovalDate = x.rfd.ApprovalDate,
                    CancelledDate = x.rfd.CanceledDate,
                    CancelledBy = x.rfd.CanceledBy,
                    CompletedDate = x.rfd.CompletedDate,

                    Products = x.Products,
                    Persons = x.Persons,

                    Status = status.ToString(),
                    Color = color,
                    HumanizedDays = humanizedDays
                };
            }).ToList();

            return View(result);
        }

        public async Task<IActionResult> ServicesPage()
        {
            var requestForms = (from rf in _appDbContext.RequestForms
                                      join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                      join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                      join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                      where rf.RequestFormTypeId == (int)EnumRequestType.Servis && !rfd.IsDeleted && rfd.StatusId == (int)EnumStatusType.Tamamlandı
                                      select new CompletedServicesDto
                                      {
                                          RequestFormDetailId = rfd.Id,
                                          RequestFormTypeId = rf.RequestFormTypeId,
                                          MainRepoName = m.Name,
                                          LocationName = l.Name,
                                          LocationAdress = l.Address,
                                          CreatedBy = rfd.CreatedBy,
                                          CreatedDate = rfd.CreatedDate,
                                          InstallationDate = rfd.InstallationDate,
                                          Description = rfd.Description,
                                          Products = (from rp in _appDbContext.RequestProducts
                                                      join p in _appDbContext.Products on rp.ProductId equals p.Id
                                                      join c in _appDbContext.Categories on p.CategoryId equals c.Id
                                                      where rp.RequestFormId == rf.Id
                                                      select new ProductDetailDto
                                                      {
                                                          CategoryName = c.Name,
                                                          ProductName = p.Name,
                                                          ImageUrl = p.PhotoUrl,
                                                          Quantity = rp.Quantity
                                                      }).ToList(),

                                          Persons = (from pd in _appDbContext.PersonDetails
                                                     where pd.RequestFormDetailId == rfd.Id
                                                     select pd.AppUser.NameSurname).ToList()
                                      }).OrderBy(x => x.CreatedDate).ToList();

            return View(requestForms);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveRequestFormStatus(SaveRequestFormStatusDto dto)
        {
            if (!ModelState.IsValid)
            {
                // ModelState'deki tüm hata mesajlarını tek bir string'de birleştiriyoruz.
                var errorMessages = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                TempData["ErrorMessage"] = string.Join("<br>", errorMessages);


                return RedirectToAction("PendingInstallations");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            var now = DateTime.Now;
            var requestFormDetail = _appDbContext.RequestFormDetails.FirstOrDefault(x => x.Id == dto.RequestFormDetailId);
            if (requestFormDetail == null)
            {
                TempData["ErrorMessage"] = "Talep bulunamadı .";
                return RedirectToAction("RequestFormList");
            }

            requestFormDetail.StatusId = dto.StatusId;

            if (dto.StatusId == (int)EnumStatusType.İptal)
            {
                requestFormDetail.CanceledBy = currentUser.NameSurname;
                requestFormDetail.CanceledDate = now;
                requestFormDetail.CanceledDesc = dto.CancelledDesc;
            }
            else if (dto.StatusId == (int)EnumStatusType.OnayBekliyor)
            {
                requestFormDetail.ApprovalBy = currentUser.NameSurname;
                requestFormDetail.ApprovalDate = now;
            }
            else if (dto.StatusId == (int)EnumStatusType.KurulumBekliyor)
            {
                requestFormDetail.ApprovalBy = currentUser.NameSurname;
                requestFormDetail.ApprovalDate = now;
            }
            else if (dto.StatusId == (int)EnumStatusType.Tamamlandı)
            {
                requestFormDetail.CompletedDate = now;
            }

            _appDbContext.RequestFormDetails.Update(requestFormDetail);
            _appDbContext.SaveChanges();

            TempData["SuccessMessage"] = "Talep durumu değiştirildi.";
            return RedirectToAction("PendingInstallations");
        }
    }
}
