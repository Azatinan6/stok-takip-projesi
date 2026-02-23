namespace StockTrack.Dto.RequestForm
{
    public class RequestedForRequestFormDto
    {
        public int RequestFormDetailId { get; set; }
        public int RequestFormTypeId { get; set; }
        public string RequestTypeName { get; set; }
        public int RequestStatusId { get; set; }
        public string MainRepoLocationName { get; set; }
        public string RequestBy { get; set; }
        public DateTime? RequestDate { get; set; }
       
     
        public string Location { get; set; }
        public string Address { get; set; }
        public string ReceiverName { get; set; }
        public string Phone { get; set; }

        //Kurulum & Servis
        public DateTime? InstallationDate { get; set; }
        public string? Description { get; set; }
        public List<string> Persons { get; set; }

        //ürünler
        public List<ProductDetailDto> Products { get; set; } 

    }
   
}
