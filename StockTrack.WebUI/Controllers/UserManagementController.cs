using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.Business.Extension.ImageManagement;
using StockTrack.Business.Extension.UserManagement;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.UserManagement;
using StockTrack.Dto.UserProfile;
using StockTrack.Entity.Enitities;
using StockTrack.WebUI.Consts;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class UserManagementController : Controller
    {
        private readonly AppDbContext _appDbContext;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IUserService _userService;
        private readonly IImageService _imageService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IToastNotification _toastNotification;
        private readonly IMailSettingService _mailSettingService;
        public UserManagementController(AppDbContext appDbContext, RoleManager<AppRole> roleManager, IUserService userService, IImageService imageService, UserManager<AppUser> userManager, IToastNotification toastNotification, IMailSettingService mailSettingService)
        {
            _appDbContext = appDbContext;
            _roleManager = roleManager;
            _userService = userService;
            _imageService = imageService;
            _userManager = userManager;
            _toastNotification = toastNotification;
            _mailSettingService = mailSettingService;
        }

        public async Task<IActionResult> Index()
        {
            var userWithRoleList = await _userService.UserWithRoleList();
            return View(userWithRoleList);
        }
        public async Task<IActionResult> UserProfil()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser != null)
            {
                var roleName = await _userManager.GetRolesAsync(currentUser);
                UserProfilDetailDto userProfilDetailDto = new()
                {
                    Email = currentUser.Email,
                    CreatedDate = currentUser.CreatedDate,
                    IsActive = currentUser.IsActive,
                    NameSurname = currentUser.NameSurname,
                    RoleName = roleName.FirstOrDefault(),
                    ImageUrl = currentUser.ImageUrl,
                    Username = currentUser.UserName
                };
                return View(userProfilDetailDto);
            }
            return BadRequest();
        }



        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> AssignRole(AssigningRoleToUser assigningRoleToUser)
        {
            if (!ModelState.IsValid)
            {
                // Hatalıysa form yeniden gösterilsin
                return View(assigningRoleToUser);
            }

            var user = await _userManager.FindByIdAsync(assigningRoleToUser.UserId.ToString());
            if (user == null)
                return NotFound();
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            var result = await _userManager.AddToRoleAsync(user, assigningRoleToUser.RoleName);
            _toastNotification.AddSuccessToastMessage("Role atama işleminiz başarılı", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }


        [HttpGet]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> CreateUser()
        {
            return View(new CreateUserDto());
        }
        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> CreateUser(CreateUserDto createUserDto)
        {
            if (!ModelState.IsValid)
                return View(createUserDto);

        
            var emailNorm = (createUserDto.Email ?? string.Empty).Trim();
           
            var existing = await _userManager.Users.Where(u => u.Email.ToLower() == emailNorm.ToLower()).FirstOrDefaultAsync();

            // 1) Aktif kullanıcı mevcutsa: hata
            if (existing != null && existing.IsDeleted == false)
            {
                ModelState.AddModelError(string.Empty, "Bu e-posta ile zaten aktif bir kullanıcı var.");
                return View(createUserDto);
            }

            // 2) Silinmiş kullanıcı mevcutsa: View’da SweetAlert ile onay iste
            if (existing != null && existing.IsDeleted)
            {
                TempData["HasDeletedUser"] = true;
                TempData["DeletedUserId"] = existing.Id.ToString();
                TempData["WarningMessage"] = $"Bu kullanıcı daha önce silinmiş. Geri yükleyip bilgileri  güncellemek ister misiniz?";
                return View(); 
            }

            // 3) Tamamen yeni kayıt akışı
            var passwordSecure = await _userService.GenerateSecurePassword();
            if (string.IsNullOrWhiteSpace(passwordSecure))
            {
                ModelState.AddModelError(string.Empty, "Şifre oluşturulamadı. Lütfen tekrar deneyiniz.");
                return View(createUserDto);
            }

            createUserDto.Password = passwordSecure;
            createUserDto.Username = createUserDto.Email;

            var createResult = await _userService.CreateUserAsync(createUserDto, User);
            if (!createResult.Success || createResult.Appuser == null)
            {
                ModelState.AddModelError(string.Empty, createResult.ErrorMessage ?? "Kullanıcı oluşturulamadı.");
                return View(createUserDto);
            }

            var mesaj = $@"
                            <p>Merhaba {createUserDto.NameSurname},</p>
                            <p>Sistemimize hoş geldiniz. Giriş bilgileriniz aşağıdadır:</p>
                            <ul>
                                <li><strong>Kullanıcı Adı:</strong> {createUserDto.Email}</li>
                                <li><strong>Şifre:</strong> {passwordSecure}</li>
                            </ul>
                            <p>Teşekkürler.</p>";

            var mailSend = await _mailSettingService.SendEmailAsync(createUserDto.Email, "Hesabınız Oluşturuldu", mesaj);
            if (!mailSend)
            {
                await _userManager.DeleteAsync(createResult.Appuser);
                ModelState.AddModelError(string.Empty, "E-posta gönderilemediği için kullanıcı kaydı geri alındı. Lütfen tekrar deneyiniz.");
                return View(createUserDto);
            }

            _toastNotification.AddSuccessToastMessage("Kayıt başarılı. Giriş bilgileri e-posta ile gönderildi.", new ToastrOptions { Title = "Başarılı" });
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        //Silinmiş kullanıcı kaydı geri getirme
        public async Task<IActionResult> RestoreDeletedUser([FromBody] string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return BadRequest(new { status = "error", message = "Geçersiz kullanıcı." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsDeleted)
                return NotFound(new { status = "error", message = "Silinmiş kullanıcı bulunamadı." });

            user.IsDeleted = false;
            user.ModifiedDate = DateTime.Now;
            user.ModifiedBy = User?.Identity?.Name;

            // Yeni şifre ata
            var newPassword = await _userService.GenerateSecurePassword();
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _userManager.ResetPasswordAsync(user, token, newPassword);

            await _userManager.UpdateAsync(user);

            // Mail gönder
            await _mailSettingService.SendEmailAsync(user.Email, "Hesabınız Aktifleştirildi",
                $"Hesabınız yeniden aktifleştirildi.<br>Yeni şifreniz: <b>{newPassword}</b>");

            return Ok(new { status = "ok", message = $"Kullanıcı geri yüklendi. Şifresi e-posta ile gönderildi: {user.Email}" });
        }



        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> ResetPassword(PasswordResetDto passwordResetDto)
        {
            if (!ModelState.IsValid)
                return View(passwordResetDto);

            // Kullanıcıyı bul
            var user = await _userManager.FindByIdAsync(passwordResetDto.UserId.ToString());
            if (user == null)
            {
                TempData["error"] = "Kullanıcı bulunamadı.";
                return RedirectToAction("Index");
            }

            // Şifre sıfırlama token oluştur
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);


            var result = await _userManager.ResetPasswordAsync(user, token, passwordResetDto.Password);

            if (result.Succeeded)
            {
                _toastNotification.AddSuccessToastMessage("Şifre değiştirme işleminiz başarılı", new ToastrOptions { Title = "Başarılı" });
                return RedirectToAction("Index");
            }

            // Hataları göster
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(passwordResetDto);
        }

        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> EditUser(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
            {
                //Kullanıcı bulunamadı
                return View();
            }
            EditUserDetailDto editUserDetailDto = new()
            {
                Id = Convert.ToInt32(user.Id),
                NameSurname = user.NameSurname,
                CreatedBy = user.CreatedBy,
                CreatedDate = user.CreatedDate,
                Email = user.Email,
                IsActive = user.IsActive,
                Username = user.UserName,
                ImageUrl = user.ImageUrl
            };
            return View(editUserDetailDto);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> EditUser(EditUserDetailDto model, IFormFile? ProfileImage)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id.ToString());
            if (user == null)
            {
                ModelState.AddModelError("", "Kullanıcı bulunamadı.");
                return View(model);
            }

            // Profil fotoğrafı yükleme işlemi (varsa)
            if (ProfileImage != null && ProfileImage.Length > 0 && user.ImageUrl != ProfileImage.Name)
            {
                var newImage = await _imageService.SaveImageAsync(ProfileImage, "uploaduserimages");
                // Eski profil fotoğrafını temizlemek 
                if (user.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine("uploaduserimages", user.ImageUrl);
                    if (System.IO.File.Exists(oldImagePath)) System.IO.File.Delete(oldImagePath);
                }
                user.ImageUrl = newImage;
            }

            // Diğer bilgileri güncelle
            user.NameSurname = model.NameSurname;
            user.Email = model.Email;
            user.UserName = model.Username;
            user.ModifiedBy = user.NameSurname;
            user.ModifiedDate = DateTime.Now;


            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded)
            {
                _toastNotification.AddSuccessToastMessage("Güncelleme işleminiz başarılı", new ToastrOptions { Title = "Başarılı" });
                return RedirectToAction("Index");
            }
            else
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return View(model);
            }
        }


        //Pasif duruma getirme
        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> DeactivateUser(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            user.IsActive = false;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { success = true, message = $"{user.NameSurname} başarıyla pasif hale getirildi." });

            // Hata varsa
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { success = false, message = $"{user.NameSurname} pasif hale getirilemedi.", errors });
        }

        //Aktif duruma getirme
        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> ActivateUser(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            user.IsActive = true;
            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
                return Ok(new { success = true, message = $"{user.NameSurname} başarıyla aktif hale getirildi." });
            //hata mesajı
            var errors = result.Errors.Select(e => e.Description);
            return BadRequest(new { success = false, message = $"{user.NameSurname} aktif hale getirilemedi.", errors });
        }

        [HttpPost]
        [Authorize(Roles = RoleConsts.Admin)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
                return NotFound(new { success = false, message = "Kullanıcı bulunamadı." });

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return Challenge();

            user.IsDeleted = true;
            user.DeletedDate = DateTime.Now;
            user.DeletedBy = currentUser.NameSurname;
            await _userManager.UpdateAsync(user);
            return Ok(new
            {
                success = true,
                message = $"{user.NameSurname} başarıyla silindi."
            });
        }

        [HttpGet]
        //silinen kullanıcıları listeleme
        public async Task<IActionResult> Deleted()
        {
            var users = _userManager.Users.Where(x => x.IsDeleted == true).ToList();
            var userRolesViewModel = new List<DeleteUserDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
               
                userRolesViewModel.Add(new DeleteUserDto
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    RoleName = roles.FirstOrDefault(),
                    CreatedDate = user.CreatedDate,
                    DeletedBy = user.CreatedBy,
                    DeletedDate = user.DeletedDate,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    NameSurname = user.NameSurname,
                    
                });
            }
            return View(userRolesViewModel);
        }


    }

}


