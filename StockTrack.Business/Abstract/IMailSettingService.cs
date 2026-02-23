using StockTrack.Entity.Enitities;

namespace StockTrack.Business.Abstract
{
    public interface IMailSettingService : IGenericService<MailSetting>
    {
        //Son mail getirme
        Task<MailSetting> GetLastMailSettingAsync();
        //mail gönderme
        Task<bool> SendEmailAsync(string toEmail, string subject, string message);
        //şifreleme
        string Encrypt(string password);
        //şifre çözme
        string Decrypt(string encryptedPassword);
    }
}
