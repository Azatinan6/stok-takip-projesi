namespace StockTrack.WebUI.Models
{
    public class CargoMenuCountsVM
    {
        public int Requested { get; init; }   // Talepler
        public int AwaitingApproval { get; init; }   // Onay Bekleyenler
        public int ReadyForCargo { get; init; }      // Paketlenmiş - Kargoya Hazır
        public int InTransit { get; init; }          // Kargoda Olanlar
    }
}
