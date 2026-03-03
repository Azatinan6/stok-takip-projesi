using StockTrack.Dto.RequestForm;

namespace StockTrack.Dto.InstallationDetail
{
    public class CompletedInstallDto
    {
        public int RequestFormDetailId { get; set; }
        public string MainRepoName { get; set; }
        //public List<string> ImageUrl { get; set; }
        //public List<string> CategoriName { get; set; }
        //public List<string> ProductName { get; set; }
        //public List<int> Quantity { get; set; }
        public string LocationName { get; set; }
        public string LocationAdress { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int RequestFormTypeId { get; set; }
        public List<string> Persons { get; set; }
        public DateTime? InstallationDate { get; set; }
        public string Description { get; set; }

        public string RequestFormBy { get; set; }
        public DateTime? RequestFormDate { get; set; }
        public string ApprovalBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public DateTime? CompletedDate { get; set; }
        public List<ProductDetailDto> Products { get; set; }
    }
}
