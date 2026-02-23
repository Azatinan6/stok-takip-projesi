namespace StockTrack.Dto.RequestForm
{
    public class ResultRequestFormDto
    {
        public int Id { get; set; }
        public string MainRepoName { get; set; }
        public List<string> ImageUrl { get; set; }
        public List<string> CategoriName { get; set; }
        public List<string> ProductName { get; set; }
        public List<int> Quantity { get; set; }
        public string LocationName { get; set; }
        public string CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Type { get; set; }
        public List<string> Persons { get; set; }
        public DateTime? InstallationDate { get; set; }
        public string Description { get; set; }

    }
}
