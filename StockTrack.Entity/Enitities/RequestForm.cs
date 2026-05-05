namespace StockTrack.Entity.Enitities
{
    public class RequestForm : EntityBase
    {
        public int MainRepoLocationId { get; set; }

        //public int LocationListId { get; set; }

        public int HospitalId { get; set; }

        public int RequestFormTypeId { get; set; }

        // --- YENİ EKLENEN EK SEÇENEKLER ---
        public bool IsShipAfterReturn { get; set; }
        public bool IsOfficeDelivery { get; set; }

        // --- RequestForm'un içindeki detayların listesini tutacak olan bağlantı (Navigation Property) ---
        public virtual ICollection<RequestFormDetail> RequestFormDetails { get; set; }
    }
}
