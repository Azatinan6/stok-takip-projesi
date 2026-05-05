using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.Login
{
    public class LoginDto
    {
        [Required(ErrorMessage = "E-posta boş olamaz")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Şifre boş olamaz")]
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string? ReturnUrl { get; set; }
    }
}
