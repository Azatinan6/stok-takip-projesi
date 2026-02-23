namespace StockTrack.Dto.MailSettings
{
    public class ResultMailSetting
    {
        public int Id { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpHost { get; set; }
        public string Password { get; set; }
        public string FromMail { get; set; }
        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
