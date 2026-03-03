namespace StockTrack.Dto.MainRepoLocation
{
    public class DeletedMainRepoLocationListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Adress { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string? DeletedBy { get; set; }

        public bool IsActive { get; set; }

    }
}
