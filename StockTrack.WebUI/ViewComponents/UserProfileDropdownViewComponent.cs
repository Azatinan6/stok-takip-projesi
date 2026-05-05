using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StockTrack.Dto.UserProfile;
using StockTrack.Entity.Enitities;

namespace StockTrack.WebUI.ViewComponents
{
    public class UserProfileDropdownViewComponent: ViewComponent
    {
        private readonly UserManager<AppUser> _userManager;

        public UserProfileDropdownViewComponent(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var currentUser = await _userManager.GetUserAsync(HttpContext.User);
            UserProfileDropdownDto userProfileDropdownDto = new()
            {
                Email = currentUser.Email,
                NameSurname = currentUser.NameSurname,
                ImageUrl = currentUser.ImageUrl
            };
            return View(userProfileDropdownDto);

        }
    }
}
