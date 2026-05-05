namespace StockTrack.Dto.UserManagement
{
    public class UserWithRoleDto
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? RoleName { get; set; }
        public bool IsActive { get; set; }
    }
}
