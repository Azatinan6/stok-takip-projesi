namespace StockTrack.Entity.Enitities
{
    public class CargoDefinition : EntityBase
    {
        // Örn: "Paket Hasarlı", "Tutanak Tutuldu", "Eksik Ürün"
        public string Name { get; set; }

        // Bu alanı gruplama için kullanacağız (1 = Kontrol Sonucu)
        public int DefinitionType { get; set; }
    }
}