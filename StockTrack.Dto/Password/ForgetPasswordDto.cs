using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.Password
{
    public class ForgetPasswordDto
    {
        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Lütfen geçerli bir e-posta adresi girin.")]
        public string Email { get; set; }
    }
}
