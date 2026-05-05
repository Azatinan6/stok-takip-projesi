using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockTrack.Dto.Account;
using StockTrack.Entity.Enitities;

namespace StockTrack.WebUI.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }
        //Şifre değiştirme işlemleri
        public async Task<IActionResult> ChangePassword()
        {
            return View(new ChangePasswordDto());
        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
                return View();

            var currentUser = await _userManager.GetUserAsync(User);
            var isCurrentOk = await _userManager.CheckPasswordAsync(currentUser, changePasswordDto.PasswordUsed);
            if (!isCurrentOk)
            {
                ModelState.AddModelError(nameof(changePasswordDto.PasswordUsed), "Mevcut şifre hatalı lütfen tekrar deneyiniz.");
                return View(changePasswordDto);
            }

            if (changePasswordDto.PasswordUsed == changePasswordDto.NewPassword)
            {
                ModelState.AddModelError(nameof(changePasswordDto.NewPassword), "Yeni şifre mevcut şifreyle aynı olamaz.");
                return View(changePasswordDto);
            }

            
            var result = await _userManager.ChangePasswordAsync(currentUser, changePasswordDto.PasswordUsed, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var e in result.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                return View(changePasswordDto);
            }

            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Şifreniz başarıyla güncellendi. Lütfen yeniden giriş yapın.";
            return RedirectToAction("Login", "Auth");
        }
    }
}
