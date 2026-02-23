namespace StockTrack.Entity.Enitities
{
    public class MailSetting: EntityBase
    {
        public int SmtpPort { get; set; }
        public string SmtpHost { get; set; }
        public string Password { get; set; }
        public string FromMail { get; set; }
      
    }
}
