using Microsoft.AspNetCore.Identity;

namespace StockTrack.Entity.Enitities
{
    public class AppUser : IdentityUser<int>
    {
        public string NameSurname { get; set; }
        public string? Title { get; set; }
        public string? ImageUrl { get; set; }
        public string CreatedBy { get; set; }
        public string? ModifiedBy { get; set; }
        public string? DeletedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsActive { get; set; }
    }
}
