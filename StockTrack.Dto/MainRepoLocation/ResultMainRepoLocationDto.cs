namespace StockTrack.Dto.MainRepoLocation
{
    public class ResultMainRepoLocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Adress { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }
    }
}
