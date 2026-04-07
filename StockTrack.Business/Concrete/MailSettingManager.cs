using StockTrack.Business.Abstract;
using StockTrack.DataAccess.Abstract;
using StockTrack.DataAccess.Context;
using StockTrack.Entity.Enitities;
using System.Net.Mail;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace StockTrack.Business.Concrete
{
    public class MailSettingManager : GenericManager<MailSetting>, IMailSettingService
    {
        private readonly string _key = "M3rVx8q2YtGpL9zKwX4Ns7Da6eJuBh0f";
        private readonly string _iv = "R8tKm1XpVz4Lq7Cf";
        private readonly AppDbContext _appDbContext;

        public MailSettingManager(IGenericDal<MailSetting> genericDal, AppDbContext appDbContext) : base(genericDal)
        {
            _appDbContext = appDbContext;
        }


        public async Task<MailSetting> GetLastMailSettingAsync()
        {
            var mailSetting = await _appDbContext.MailSettings.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
            return mailSetting;
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
            var lastMail = await GetLastMailSettingAsync();
            if (lastMail == null)
            {
                return false;
            }
            var solvePassword = Decrypt(lastMail.Password);

            try
            {
                // E-posta mesajını oluştur
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(lastMail.FromMail, "Stok Takip"),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true,
                };
                mailMessage.To.Add(new MailAddress(toEmail));

                // SMTP istemcisini yapılandır
                using var smtpClient = new SmtpClient
                {
                    Host = lastMail.SmtpHost,
                    Port = lastMail.SmtpPort,
                    EnableSsl = true,
                    Credentials = new NetworkCredential(lastMail.FromMail, solvePassword),
                    DeliveryMethod = SmtpDeliveryMethod.Network
                };

                await smtpClient.SendMailAsync(mailMessage);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public string Encrypt(string password)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream();
            using (var cryptoStream = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            using (var writer = new StreamWriter(cryptoStream))
            {
                writer.Write(password);
            }

            return Convert.ToBase64String(ms.ToArray());
        }

        public string Decrypt(string encryptedPassword)
        {
            using Aes aes = Aes.Create();
            aes.Key = Encoding.UTF8.GetBytes(_key);
            aes.IV = Encoding.UTF8.GetBytes(_iv);

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var ms = new MemoryStream(Convert.FromBase64String(encryptedPassword));
            using var cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cryptoStream);

            return reader.ReadToEnd();
        }       

    }
}
