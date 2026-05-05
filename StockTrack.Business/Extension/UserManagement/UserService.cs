using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StockTrack.Business.Extension.ImageManagement;
using StockTrack.DataAccess.Context;
using StockTrack.Dto.UserManagement;
using StockTrack.Entity.Enitities;
using System.Security.Claims;
using System.Text;

namespace StockTrack.Business.Extension.UserManagement
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IImageService _imageService;

        public UserService(AppDbContext appDbContext, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IImageService imageService)
        {
            _appDbContext = appDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _imageService = imageService;
        }

        public async Task<(bool Success, AppUser Appuser, string ErrorMessage)> CreateUserAsync(CreateUserDto createUserDto, ClaimsPrincipal user)
        {

            var currentUser = await _userManager.GetUserAsync(user);
            var userAdd = new AppUser
            {
                NameSurname = createUserDto.NameSurname,
                Email = createUserDto.Email,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                UserName = createUserDto.Username,
                CreatedBy = currentUser.NameSurname,
                IsActive = true,
            };

            if (createUserDto.ImageUrl != null)
            {
                var imageUrl = await _imageService.SaveImageAsync(createUserDto.ImageUrl, "uploaduserimages");
                userAdd.ImageUrl = imageUrl;
            }

            // Kullanıcıyı oluştur
            var createResult = await _userManager.CreateAsync(userAdd, createUserDto.Password);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                return (false, null, errors);
            }

            // Kullanıcıya rol ata
            if (!string.IsNullOrEmpty(createUserDto.RoleName))
            {
                var roleExists = await _roleManager.RoleExistsAsync(createUserDto.RoleName);
                if (!roleExists)
                {
                    return (false, null, $"'{createUserDto.RoleName}' rolü mevcut değil.");
                }

                var addRoleResult = await _userManager.AddToRoleAsync(userAdd, createUserDto.RoleName);
                if (!addRoleResult.Succeeded)
                {
                    var errors = string.Join(", ", addRoleResult.Errors.Select(e => e.Description));
                    return (false, null, errors);
                }
            }


            return (true, userAdd, null);
        }


        //Kullanıcı rolleri ile beraber listeleme
        public async Task<List<UserWithRoleDto>> UserWithRoleList()
        {

            var userWithRoleList = await (from user in _appDbContext.Users
                                          where user.IsDeleted != true
                                          join userRole in _appDbContext.UserRoles on user.Id equals userRole.UserId into userRoleGroup
                                          from userRole in userRoleGroup.DefaultIfEmpty()
                                          join role in _appDbContext.Roles on userRole.RoleId equals role.Id into roleGroup
                                          from role in roleGroup.DefaultIfEmpty()
                                          select new UserWithRoleDto
                                          {
                                              Id = Convert.ToInt32(user.Id),
                                              FullName = user.NameSurname,
                                              Email = user.Email,
                                              CreatedDate = user.CreatedDate,
                                              RoleName = role.Name,
                                              IsActive = user.IsActive
                                          }).OrderByDescending(u => u.IsActive).ToListAsync();
            return userWithRoleList;
        }

        //Şifre oluşturma
        public async Task<string> GenerateSecurePassword(int length = 12)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*()-_=+[]{}<>?";

            string allChars = upper + lower + digits + special;
            var random = new Random();

            // Şifrede en az birer karakter bulunmasını garanti et
            var password = new StringBuilder();
            password.Append(upper[random.Next(upper.Length)]);
            password.Append(lower[random.Next(lower.Length)]);
            password.Append(digits[random.Next(digits.Length)]);
            password.Append(special[random.Next(special.Length)]);

            // Kalan karakterleri rastgele doldur
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Karışık hale getir
            var newPassword = new string(password.ToString().OrderBy(_ => Guid.NewGuid()).ToArray());
            return newPassword;
        }


    }
}
