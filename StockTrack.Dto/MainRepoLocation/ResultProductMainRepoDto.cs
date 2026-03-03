namespace StockTrack.Dto.MainRepoLocation
{
    public class ResultProductMainRepoDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int CategoryId { get; set; }
        public int MainRepoLocationId { get; set; }
        public string MainRepoLocationName { get; set; }
        public string ProductName { get; set; }
        public string ProductModel { get; set; }
        public string ProductDescription { get; set; }
        public string PhotoUrl { get; set; }
        public int TotalQuantity { get; set; }
        public int DistributedQuantity { get; set; } //Dağıtılmış miktar
        public int RemainingQuantity { get; set; } /*=> TotalQuantity - DistributedQuantity;*/  //Kalan Miktar
        public string CategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
