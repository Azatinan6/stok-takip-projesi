namespace StockTrack.Entity.Enitities
{
    public class PersonDetail
    {
        public int RequestFormDetailId { get; set; }
        public RequestFormDetail RequestFormDetail { get; set; }
        public int AppUserId { get; set; }
        public AppUser AppUser { get; set; }
    }
}
