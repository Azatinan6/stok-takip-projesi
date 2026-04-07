namespace StockTrack.Dto.UserManagement
{
    public class EditUserDetailDto
    {
        public int Id { get; set; }
        public string NameSurname { get; set; }
        public string Email { get; set; }
        public string Username { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? CreatedBy { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
