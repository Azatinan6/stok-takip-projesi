namespace StockTrack.Entity.Enitities
{
    public class RequestFormDetail:EntityBase
    {
        public int RequestFormId { get; set; }

        //ortak
        public string? RequestBy { get; set; } //Talep onaylayan
        public DateTime? RequestDate { get; set; } // Talep onaylanma tarihi
        public string? ApprovalBy { get; set; } //Onaylayan Kişi
        public DateTime? ApprovalDate { get; set; } // Onaylayan kişi
        public DateTime? CanceledDate { get; set; }// iptal edilen tarih
        public string? CanceledBy { get; set; }// iptal eden
        public DateTime? CompletedDate { get; set; }//teslimati tamamlanan tarih
        public string? CanceledDesc { get; set; }//Açıklama

        //Kargo işlemleri
        public string? TrackingNumber { get; set; }//Kargo numarası
        public string? Adress { get; set; }//Adres
        public string? ToPerson { get; set; }//Alıcı bilgisi
        public string? Phone { get; set; }//Alıcı Numarası
        public DateTime? PackingDate { get; set; }//Hazırlanma tarihi
        public DateTime? CargoGivenDate { get; set; }//Kargoya verildi  Tarihi

        public int? CargoNameId { get; set; }

        //Kurulum & Servis 
        public DateTime? InstallationDate { get; set; }
        public string? Description { get; set; }
        public int StatusId { get; set; }
    }
}
