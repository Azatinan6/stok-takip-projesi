namespace StockTrack.Dto.CargoDetail
{
    public class SaveCargoInfoDto
    {
        public int Id { get; set; }  

        public string? CargoPreparerUserId { get; set; }
        public int StatusId { get; set; }
        public int? CargoNameId { get; set; }
        public DateTime? CargoDeliveredDate { get; set; }
        public DateTime? CargoGivenDate { get; set; }

        public bool? IsOfficeDelivery { get; set; }
        public string? TrackingNumber { get; set; }
        public string? CancelDescription { get; set; }
        public string? CargoCompany { get; set; }
    }
}
