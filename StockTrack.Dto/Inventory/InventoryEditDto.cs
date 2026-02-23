using Microsoft.AspNetCore.Http;

namespace StockTrack.Dto.Inventory
{
    public class InventoryEditDto
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string ProductName { get; set; }
        public int StockWarningLevel { get; set; }
        public string? Description { get; set; }
        public IFormFile? ProductImage { get; set; }
        public string? ExistingPhotoUrl { get; set; }
    }
}