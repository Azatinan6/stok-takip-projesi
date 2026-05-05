using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.UserManagement
{
    public class PasswordResetDto
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Şifre alanı boş olamaz.")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalı.")]

        public string Password { get; set; }

        [Required(ErrorMessage = "Şifre tekrar alanı boş olamaz.")]
        [Compare("Password", ErrorMessage = "Şifreler uyuşmuyor.")]
        public string ConfirmPassword { get; set; }
    }
}
