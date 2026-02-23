namespace StockTrack.Dto.Product
{
    public class DeletedProductDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Model { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }    
        public string CategoryName { get; set; }         
        public bool IsActive { get; set; }
        public string DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public DateTime CreatedDate { get; set; }
    }
}
