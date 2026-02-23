using Microsoft.AspNetCore.Identity;
using StockTrack.Dto.Login;
using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Extension.Login
{
    public class LoginService : ILoginService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public LoginService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<(bool Success, string ErrorMessage, AppUser appUser)> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return (false, "E-posta veya şifre yanlış", null);
            }
            if (!user.IsActive || user.IsDeleted)
            {
                return (false, "Sistemi kullanmaya yetkiniz yoktur. Lütfen yöneticinizle iletişime geçin.", null);
            }


            var result = await _signInManager.PasswordSignInAsync(user, loginDto.Password, loginDto.RememberMe, lockoutOnFailure: true);
            if (result.IsLockedOut)
            {
                var lockoutEnd = await _userManager.GetLockoutEndDateAsync(user);
                var remaining = lockoutEnd?.UtcDateTime.Subtract(DateTime.UtcNow);
                var minutesLeft = remaining?.Minutes > 0 ? remaining?.Minutes : 0;

                return (false, $"Hesabınız kilitlendi. Lütfen {minutesLeft} dakika sonra tekrar deneyin.", null);
            }
            if (!result.Succeeded)
            {
                int failedCount = await _userManager.GetAccessFailedCountAsync(user);
                int maxAttempts = _userManager.Options.Lockout.MaxFailedAccessAttempts;
                int remainingAttempts = maxAttempts - failedCount;

                if (remainingAttempts <= 0)
                {
                    // Kilitlenme zaten gerçekleşmiş olabilir ama kontrolü tekrar ekleyelim
                    return (false, "Hesabınız çok sayıda başarısız giriş nedeniyle kilitlendi.", null);
                }

                return (false, $"Email veya parola yanlış. Kalan deneme hakkınız: {remainingAttempts}", null);
            }
            
            // Başarılı girişte başarısız giriş sayısını sıfırlaması
            await _userManager.ResetAccessFailedCountAsync(user);
            return (true, "işlem başarılı", user);
        }

    }
}
