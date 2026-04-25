using BuyMate.BLL.Contracts;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BuyMate.ViewComponents
{
    public class UserAvatarViewComponent : ViewComponent
    {
        private readonly IUserProfileService _userProfile;

        public UserAvatarViewComponent(IUserProfileService userProfile)
        {
            _userProfile = userProfile;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            if (!User.Identity.IsAuthenticated)
                return View("Default", "UserProfileImages/Default.webp");

            var user = await _userProfile.GetProfileAsync(HttpContext.User);

            var avatar = user?.Data?.Avatar ?? "UserProfileImages/Default.webp";

            return View("Default", avatar);
        }
    }
}
