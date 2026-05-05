using StockTrack.Dto.UserManagement;
using StockTrack.Entity.Enitities;
using System.Security.Claims;

namespace StockTrack.Business.Extension.UserManagement
{
    public interface IUserService
    {
        Task<List<UserWithRoleDto>> UserWithRoleList();
        Task<(bool Success, AppUser Appuser, string ErrorMessage)> CreateUserAsync(CreateUserDto createUserDto, ClaimsPrincipal user);
        Task<string> GenerateSecurePassword(int length = 12);
    }
}
