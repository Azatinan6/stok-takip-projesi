using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.Account
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Mevcut şifre zorunludur.")]
        public string PasswordUsed { get; set; }

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]      
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrar zorunludur.")]
        [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor.")]
        public string ConfirmPassword { get; set; }
    }
}
