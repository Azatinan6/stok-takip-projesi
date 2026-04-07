using System.ComponentModel.DataAnnotations.Schema;

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
        public string? Address { get; set; }//Adres
        public string? ToPerson { get; set; }//Alıcı bilgisi
        public string? Phone { get; set; }//Alıcı Numarası
        public DateTime? PackingDate { get; set; }//Hazırlanma tarihi
        public DateTime? CargoGivenDate { get; set; }//Kargoya verildi  Tarihi
        public string? ReceiverDepartment { get; set; }
        

        public int? CargoNameId { get; set; }

        //Kurulum & Servis 
        public DateTime? InstallationDate { get; set; }
        public string? Description { get; set; }
        public int StatusId { get; set; }

        // PDF: Kargo Hazırlayan (Kullanıcı Seçilecek) - Kullanıcının ID'si veya Adı Soyadı tutulur
        public string? CargoPreparerUserId { get; set; }

        //------------İade Seçeneği-----------------
        // 1. Ürün iade mi edildi? (Evet/Hayır)
        public bool IsReturned { get; set; } = false;

        // 2. Ürün bozuk/kırık mı geldi? (Zayiat durumu)
        public bool IsWastage { get; set; } = false;

        // 3. İade edildiyse sebebi ne?
        public string? ReturnReason { get; set; }

        // 4. Geri gelen ürünün üzerindeki Seri Numarası (Güvenlik kontrolü için)
        public string? ReturnedSerialNumber { get; set; }

        // 5. İade işlemini kim, ne zaman yaptı?
        public string? ReturnedBy { get; set; }
        public DateTime? ReturnedDate { get; set; }

        [ForeignKey("RequestFormId")]
        public virtual RequestForm? RequestForm { get; set; } // Navigation Property
    }

}
