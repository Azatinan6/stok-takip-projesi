using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.CargoDetail;
using StockTrack.Dto.RequestForm;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Enums;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class CargoDetailController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;

        public CargoDetailController(AppDbContext appDbContext, UserManager<AppUser> userManager)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
        }

        //Onay Bekleyen kargolar
        public async Task<IActionResult> AwaitingApproval()
        {
            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");

            var resultAwaitingApprovals = (from rfd in _appDbContext.RequestFormDetails
                                           join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                           join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id
                                           join rl in _appDbContext.LocationLists on rf.LocationListId equals rl.Id
                                           join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id
                                           where rfd.StatusId == (int)EnumStatusType.OnayBekliyor && !rfd.IsDeleted
                                           select new ResultAwitingApprovalDto
                                           {
                                               Id = rfd.Id,
                                               StatusId = rfd.StatusId,
                                               StatusName = st.Name,
                                               ReceiverFullName = rfd.ToPerson,
                                               Phone = rfd.Phone,
                                               LocationName = rl.Name,
                                               LocationAdress = rl.Address,
                                               RequestFormRequestedBy = rfd.RequestBy,
                                               RequestFormRequestedDate = rfd.RequestDate,
                                               MainRepoName = mrl.Name,
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
            return View(resultAwaitingApprovals);

        }

        //Paketlenmiş kargoya hazır ürünler
        public async Task<IActionResult> ReadyForCargo()
        {
            ViewBag.CargoNames = new SelectList(_appDbContext.CargoNames.AsNoTracking().OrderBy(x => x.Name).ToList(), "Id", "Name");
            // Paketlenmiş kargo taleplerini konum, ürün ve diğer detaylarla birlikte listeliyor
            var resultCargoForReadyDtos = (from rfd in _appDbContext.RequestFormDetails
                                           join rf in _appDbContext.RequestForms on rfd.RequestFormId equals rf.Id
                                           join mrl in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals mrl.Id
                                           join rl in _appDbContext.LocationLists on rf.LocationListId equals rl.Id
                                           join st in _appDbContext.StatusTypes on rfd.StatusId equals st.Id
                                           where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                           where rfd.StatusId == (int)EnumStatusType.Paketlendi && !rfd.IsDeleted
                                           select new ResultCargoForReadyDto
                                           {
                                               Id = rfd.Id,
                                               StatusId = rfd.StatusId,
                                               ReceiverFullName = rfd.ToPerson,
                                               Phone = rfd.Phone,
                                               LocationName = rl.Name,
                                               LocationAdress = rl.Address,
                                               RequestFormRequestedBy = rfd.CreatedBy, //talebi onaylayan kişi 
                                               RequestFormRequestedDate = rfd.PackingDate, //tarihi
                                               MainRepoName = mrl.Name,
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

            return View(resultCargoForReadyDtos);
        }

        //Kargo teslimattında
        public async Task<IActionResult> CargoInDelivery()
        {
            // Kargoya verilmiş yolda olan talepleri kargo firması, takip numarası, konum ve ürün detaylarıyla birlikte listeliyor
            var resultCargoInDeliveries = (from rf in _appDbContext.RequestForms
                                           join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                           join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                           join cn in _appDbContext.CargoNames on rfd.CargoNameId equals cn.Id
                                           join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                           where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                           where rfd.StatusId == (int)EnumStatusType.Kargoda && !rfd.IsDeleted
                                           select new ResultCargoInDeliveryDto
                                           {
                                               Id = rfd.Id,
                                               StatusId = rfd.StatusId,
                                               ReceiverFullName = rfd.ToPerson,
                                               Phone = rfd.Phone,
                                               LocationName = l.Name,
                                               LocationAdress = l.Address,
                                               RequestFormBy = rfd.CreatedBy,
                                               RequestFormDate = rfd.CreatedDate,
                                               MainRepoName = m.Name,
                                               TrakingNumber = rfd.TrackingNumber,
                                               CargoCompany = cn.Name,

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


            return View(resultCargoInDeliveries);

        }

        //Kargo teslim edilmiş
        public async Task<IActionResult> Delivered()
        {
            // Teslim edilmiş kargo taleplerini ürün, konum, kargo firması, takip numarası ve işlem tarihleriyle birlikte listeliyor          
            var resultCargoDelivereds = (from rf in _appDbContext.RequestForms
                                         join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                         join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                         join cn in _appDbContext.CargoNames on rfd.CargoNameId equals cn.Id
                                         join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                         join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id
                                         where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                         where rfd.StatusId == (int)EnumStatusType.Tamamlandı && !rfd.IsDeleted
                                         select new ResultCargoDeliveredDto
                                         {
                                             Id = rfd.Id,
                                             StatusId = rfd.StatusId,
                                             ReceiverFullName = rfd.ToPerson,
                                             Phone = rfd.Phone,
                                             LocationName = l.Name,
                                             LocationAdress = l.Address,
                                             RequestFormBy = rfd.CreatedBy,
                                             RequestFormDate = rfd.CreatedDate,
                                             MainRepoName = m.Name,
                                             TrakingNumber = rfd.TrackingNumber,
                                             CargoCompany = cn.Name,
                                             CargoGivenDate = rfd.CargoGivenDate,
                                             CargoProccessBy = rfd.CreatedBy,
                                             PackingDate = rfd.PackingDate,
                                             CompletedCargoDate = rfd.CompletedDate,
                                             StatusName = s.Name,
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



            return View(resultCargoDelivereds);
        }

        //iptal  edilmiş kargolar
        public async Task<IActionResult> Cancelled()
        {
            var resultCargoCanceleds = (from rf in _appDbContext.RequestForms
                                        join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                        join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                        join cn in _appDbContext.CargoNames on rfd.CargoNameId equals cn.Id
                                        join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                        join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id
                                        where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                        where rfd.StatusId == (int)EnumStatusType.İptal && !rfd.IsDeleted
                                        select new ResultCargoCanceledDto
                                        {
                                            Id = rfd.Id,
                                            StatusId = rfd.StatusId,
                                            ReceiverFullName = rfd.ToPerson,
                                            Phone = rfd.Phone,
                                            LocationName = l.Name,
                                            LocationAdress = l.Address,
                                            CancaledBy = rfd.CanceledBy,
                                            CanceledDesc = rfd.CanceledDesc,
                                            RequestFormBy = rfd.CreatedBy,
                                            RequestFormDate = rfd.CreatedDate,
                                            MainRepoName = m.Name,
                                            StatusName = s.Name,
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


            return View(resultCargoCanceleds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        // Kargo işlemlerini (paketleme, kargoya verme, teslim etme, iptal etme) durumuna göre kaydedip ilgili liste sayfasına yönlendiriyor
        public async Task<IActionResult> SaveCargoInfo(SaveCargoInfoDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Lütfen zorunlu alanları doldurun.";
                return RedirectToAction("AwaitingApproval");
            }

            var findCargoDetail = _appDbContext.RequestFormDetails.FirstOrDefault(x => x.Id == dto.Id);

            if (findCargoDetail == null)
            {
                TempData["ErrorMessage"] = "Kayıt bulunamadı.";
                return RedirectToAction("AwaitingApproval");
            }
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();


            var now = DateTime.Now;
            if (dto.StatusId == (int)EnumStatusType.Paketlendi)
            {
                findCargoDetail.CreatedBy = currentUser.NameSurname;
                findCargoDetail.IsActive = true;
                findCargoDetail.IsDeleted = false;

                findCargoDetail.ApprovalDate = now;//Onaylanma tarihi
                findCargoDetail.ApprovalBy = currentUser.NameSurname;//Onaylayan kişi
                findCargoDetail.PackingDate = now;//Onaylayan kişi
                findCargoDetail.StatusId = (int)EnumStatusType.Paketlendi;

                _appDbContext.RequestFormDetails.Update(findCargoDetail);
                _appDbContext.SaveChanges();

                TempData["SuccessMessage"] = "Kargo paketleme işlemi kaydedildi.";
                return RedirectToAction("AwaitingApproval");

            }
            else if (dto.StatusId == (int)EnumStatusType.Kargoda)
            {
                findCargoDetail.TrackingNumber = dto.TrackingNumber;
                findCargoDetail.StatusId = dto.StatusId;
                findCargoDetail.CargoGivenDate = now;//Kargoya verilme  tarihi
                findCargoDetail.CargoNameId = dto.CargoNameId;

                _appDbContext.RequestFormDetails.Update(findCargoDetail);
                _appDbContext.SaveChanges();

                TempData["SuccessMessage"] = "Kargo teslimatta işlemi kaydedildi.";
                return RedirectToAction("ReadyForCargo");
            }
            else if (dto.StatusId == (int)EnumStatusType.Tamamlandı)
            {
                findCargoDetail.CompletedDate = dto.CargoDeliveredDate;//Teslim edilmiş kargo tarihi
                findCargoDetail.StatusId = (int)EnumStatusType.Tamamlandı;

                _appDbContext.RequestFormDetails.Update(findCargoDetail);
                _appDbContext.SaveChanges();

                TempData["SuccessMessage"] = "Kargo teslim edildi işlemi kaydedildi.";
                return RedirectToAction("CargoInDelivery");
            }
            else if (dto.StatusId == (int)EnumStatusType.İptal)
            {
                findCargoDetail.CanceledDate = now;
                findCargoDetail.CanceledBy = currentUser.NameSurname;
                findCargoDetail.CanceledDesc = dto.CancelDescription;
                findCargoDetail.StatusId = (int)EnumStatusType.İptal;

                _appDbContext.RequestFormDetails.Update(findCargoDetail);
                _appDbContext.SaveChanges();

                TempData["SuccessMessage"] = "Kargo iptal edildi.";
                return RedirectToAction("Cancelled");
            }

            return RedirectToAction("AwaitingApproval");

        }


        [HttpPost]
        //Kargo IsDelete işlemi
        public async Task<IActionResult> DeleteCargo(int id)
        {
            try
            {
                var cargo = await _appDbContext.RequestFormDetails.FindAsync(id);
                if (cargo == null)
                {
                    return Json(new { success = false, message = "Kargo bulunamadı." });
                }

                var requestForm = await _appDbContext.RequestForms.FindAsync(cargo.RequestFormId);
                if (requestForm == null)
                {
                    return Json(new { success = false, message = "Talep Formu bulunamadı." });
                }

                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null)
                    return Challenge();

                var now = DateTime.Now;

                // Cargo alanlarını güncelle
                cargo.IsDeleted = true;
                cargo.DeletedDate = now;
                cargo.DeletedBy = currentUser.NameSurname;

                // RequestForm alanlarını güncelle
                requestForm.IsDeleted = true;
                requestForm.DeletedDate = now;
                requestForm.DeletedBy = currentUser.NameSurname;

                // Transaction ile güncellemeleri kaydet
                using var transaction = await _appDbContext.Database.BeginTransactionAsync();
                try
                {
                    await _appDbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Json(new { success = true, receiver = cargo.ToPerson });
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false, message = ex.Message });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }


        [HttpGet]
        //Silinen kargolar
        public async Task<IActionResult> Deleted()
        {
            var resultCargoDeleteds = (from rf in _appDbContext.RequestForms
                                       join rfd in _appDbContext.RequestFormDetails on rf.Id equals rfd.RequestFormId
                                       join m in _appDbContext.MainRepoLocations on rf.MainRepoLocationId equals m.Id
                                       join l in _appDbContext.LocationLists on rf.LocationListId equals l.Id
                                       join s in _appDbContext.StatusTypes on rfd.StatusId equals s.Id
                                       where rf.RequestFormTypeId == (int)EnumRequestType.Kargo
                                       where rfd.IsDeleted == true
                                       select new ResultCargoDeletedDto
                                       {
                                           Id = rfd.Id,
                                           ReceiverFullName = rfd.ToPerson,
                                           Phone = rfd.Phone,
                                           LocationName = l.Name,
                                           LocationAdress = l.Address,
                                           DeletedBy = rfd.DeletedBy,
                                           DeletedDate = rfd.DeletedDate,
                                           RequestFormBy = rfd.CreatedBy,
                                           RequestFormDate = rfd.CreatedDate,
                                           MainRepoName = m.Name,
                                           StatusName = s.Name,
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
            return View(resultCargoDeleteds);
        }



    }
}
