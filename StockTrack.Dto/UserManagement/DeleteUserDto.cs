namespace StockTrack.Dto.UserManagement
{
    public class DeleteUserDto
    {
        public int Id { get; set; }
        public string NameSurname { get; set; }
        public string RoleName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string DeletedBy { get; set; }
        public bool IsActive { get; set; }
    }
}
