namespace StockTrack.Dto.LocationList
{
    public class ResultLocationListDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Adress { get; set; }
        public string ContactPerson { get; set; }
        public string Phone { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

    }
}
