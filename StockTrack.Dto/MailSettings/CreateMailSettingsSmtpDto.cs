using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.MailSettings
{
    public class CreateMailSettingsSmtpDto
    {
        [Required(ErrorMessage = "SmtpPort alanı zorunludur.")]
        [Range(1, 65535, ErrorMessage = "SmtpPort 1 ile 65535 arasında olmalıdır.")]
        public int SmtpPort { get; set; }

        [Required(ErrorMessage = "SmtpHost alanı zorunludur.")]
        public string SmtpHost { get; set; }

        [Required(ErrorMessage = "Password alanı zorunludur.")]
        public string Password { get; set; }

        [Required(ErrorMessage = "FromMail alanı zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        public string FromMail { get; set; }

    }
}
