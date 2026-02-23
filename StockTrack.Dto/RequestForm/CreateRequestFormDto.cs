namespace StockTrack.Dto.RequestForm
{
    public class CreateRequestFormDto
    {
        public int LocationId { get; set; }
        public int MainRepoId { get; set; }
        public int TypeId { get; set; } // 1=Kurulum, 2=Kargo, 3 servis
        public string ItemsJson { get; set; } // Hidden’dan gelecek
        public List<ItemDto>? Items { get; set; } 
        public DateTime? InstallationDate { get; set; }
        //kurulum Servis
        public List<int>? Persons { get; set; }
        public string? Note { get; set; }
        public string? ReceiverFullName { get; set; }

        //Kargo
        public string? Phone { get; set; }

    }
    public class ItemDto
    {
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int Quantity { get; set; }
    }
}
