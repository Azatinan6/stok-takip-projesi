namespace StockTrack.Entity.Enitities
{
    public class MainRepoLocation: EntityBase
    {
        public string Name { get; set; }
        public string? Adress { get; set; }
        public string? PageUrl { get; set; }
        public ICollection<ProductMainRepoLocation> ProductMainRepoLocations { get; set; }
    }
}
