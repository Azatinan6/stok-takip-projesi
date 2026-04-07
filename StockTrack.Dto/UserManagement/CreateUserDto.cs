using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.UserManagement
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "Ad Soyad alanı boş olamaz.")]
        [MaxLength(100, ErrorMessage = "Ad Soyad en fazla 100 karakter olabilir.")]
        public string NameSurname { get; set; }


        public IFormFile? ImageUrl { get; set; }

        [Required(ErrorMessage = "Email alanı boş olamaz.")]
        [EmailAddress(ErrorMessage = "Geçerli bir email adresi giriniz.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Rol seçimi zorunludur.")]
        public string RoleName { get; set; }
      
        public string? Username { get; set; }


        public string? Password { get; set; }

        //[Required(ErrorMessage = "Şifre tekrarı boş olamaz.")]
        //[Compare("Password", ErrorMessage = "Şifre ve şifre tekrarı uyuşmuyor.")]
        //public string ConfirmPassword { get; set; }
    }
}
