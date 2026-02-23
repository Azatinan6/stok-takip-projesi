namespace StockTrack.Dto.UserProfile
{
    public class UserProfilDetailDto
    {
        public string NameSurname { get; set; }
        public string Username { get; set; }
        public string ImageUrl { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
