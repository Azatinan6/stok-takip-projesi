using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using StockTrack.Business.Abstract;
using StockTrack.Business.Extension.Login;
using StockTrack.Dto.Login;
using StockTrack.Dto.Password;
using StockTrack.Dto.PasswordDto;
using StockTrack.Entity.Enitities;

namespace StockTrack.WebUI.Controllers
{
    public class AuthController : Controller
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ILoginService _loginService;
        private readonly IToastNotification _toastNotification;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMailSettingService _mailSettingService;
        public AuthController(SignInManager<AppUser> signInManager, ILoginService loginService, IToastNotification toastNotification, UserManager<AppUser> userManager, IMailSettingService mailSettingService)
        {
            _signInManager = signInManager;
            _loginService = loginService;
            _toastNotification = toastNotification;
            _userManager = userManager;
            _mailSettingService = mailSettingService;
        }

        [HttpGet]
        public async Task<IActionResult> Login(string? returnUrl)
        {
            return View(new LoginDto() { ReturnUrl = returnUrl });
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                {
                    ModelState.AddModelError("", error.ErrorMessage);
                }
                return View(loginDto);
            }
            var result = await _loginService.LoginAsync(loginDto);

           
            if (!result.Success && result.appUser == null)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(loginDto);
            }
            _toastNotification.AddSuccessToastMessage("Giriş işleminiz başarılı", new ToastrOptions { Title = "Başarılı" });

            if (!string.IsNullOrEmpty(loginDto.ReturnUrl))
            {
                return Redirect(loginDto.ReturnUrl);
            }

            return RedirectToAction("Index", "Dashboard");
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

            return RedirectToAction("Login", "Auth");
        }

        [Route("Error/StatusCode")]
        public IActionResult StatusCodeHandler(int code)
        {
            if (code == 404)
            {
                return View("NotFound404");
            }
            else if (code == 403)
            {
                return RedirectToAction("AccessDenied");
            }

            return View("NotFound404");
        }

        //şifremi unuttum
        public async Task<IActionResult> ForgetPassword()
        {
            return View(new ForgetPasswordDto());
        }
        //şifremi unuttum
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordDto forgetPasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return View(forgetPasswordDto);
            }

            var user = await _userManager.FindByEmailAsync(forgetPasswordDto.Email);

            if (user == null)
            {
                ViewBag.ErrorMessage = "Kullanıcı bulunamadı.";
                return View();
            }


            // Token oluştur
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Linki oluştur (token'ı URL için güvenli hale getir)
            var callbackUrl = Url.Action(action: "ResetPassword", controller: "Auth", new { email = user.Email, code = token }, protocol: Request.Scheme);


            //E - postayı gönder
            var result = await _mailSettingService.SendEmailAsync(user.Email, "Şifre Sıfırlama Talebi", $"Şifrenizi sıfırlamak için lütfen <a href='{callbackUrl}'>buraya tıklayın</a>.");
            if (!result)
            {
                ViewBag.ErrorMessage = "Mail gönderme işlemi başarısız oldu yöneticinizle iletişime geçiniz.";
                return RedirectToAction("ForgetPassword");
            }


            return View("PasswordResetLinkNotification");
        }

        //  Şifre sıfırlama
        [HttpGet]
        public IActionResult ResetPassword(string code = null, string email = null)
        {
            if (code == null || email == null)
            {
                ModelState.AddModelError("", "Geçersiz şifre sıfırlama linki.");
                return View("Error");
            }

            var model = new ResetPasswordDto { Token = code, Email = email };
            return View(model);
        }

      
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {

                TempData["ErrorMessage"] = "Kullanıcı bulunamadı";
                return RedirectToAction("Login");
            }

            // Şifreyi sıfırla (token kontrolü ve süre limiti burada otomatik yapılır)
            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirilmiştir. Artık yeni şifrenizle giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }

            ModelState.AddModelError(string.Empty, "Şifre sıfırlama süresi geçmiş veya kullanılmış tekrar şifre sıfırlama talebi gönderiniz.");
            return View(model);
        }

        public IActionResult PasswordResetLinkNotification()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
        [HttpGet]
        public IActionResult NotFound404()
        {
            return View();
        }
    }
}
