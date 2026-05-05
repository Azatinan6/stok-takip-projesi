namespace StockTrack.Dto.Category
{
    public class DeletedCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
