namespace StockTrack.Entity.Enitities
{
    public class Product : EntityBase
    {
        public string Name { get; set; }
        public string? Model { get; set; }
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? PhotoUrl { get; set; }
        public int? WarningThreshold { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }
}
