using System.ComponentModel.DataAnnotations;

namespace StockTrack.Dto.UserManagement
{
    public class AssigningRoleToUser
    {
        public int UserId { get; set; }
        [Required(ErrorMessage = "Rol adı seçiniz")]
        public string RoleName { get; set; }
    }
}
