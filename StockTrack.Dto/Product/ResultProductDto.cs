namespace StockTrack.Dto.Product
{
    public class ResultProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Model { get; set; }       
        public int? WarningThreshold { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }
        public int  CategoryId { get; set; }
        public string CategoryName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
  
}
