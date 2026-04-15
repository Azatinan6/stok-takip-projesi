using Microsoft.AspNetCore.Http;

namespace StockTrack.Dto.CargoDetail
{
    public class SaveCargoReturnInfoDto 
    {
        public int Id { get; set; }
        public int StatusId { get; set; }
        
        public int ReceivedQuantity { get; set; }
        public int ZayiatQuantity { get; set; }
        public string? ControlResult { get; set; }
        
        public string? NewSerialNumber { get; set; }
        
        public string? ExtraProductName { get; set; }
        public int? ExtraProductQty { get; set; }

        public IFormFile? ReturnImage { get; set; }
    }
}