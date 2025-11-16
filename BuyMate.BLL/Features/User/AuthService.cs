using BuyMate.BLL.Contracts;
using BuyMate.DTO.Common;
using BuyMate.DTO.ViewModels;
using BuyMate.Model.Entities;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace BuyMate.BLL.Features.User
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Model.Entities.User> _userManager;
        private readonly SignInManager<Model.Entities.User> _signInManager;

        public AuthService(UserManager<Model.Entities.User> userManager, SignInManager<Model.Entities.User> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<Response<bool>> RegisterAsync(RegisterViewModel model)
        {


            // Friendly duplicate checks
            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail is not null)
            {
                return new Response<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Email is already in use."
                };
            }

            var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == model.Phone);
            if (phoneExists)
            {
                return new Response<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Phone Number is already registered."
                };
            }

            var user = new Model.Entities.User
            {
                UserName = model.Email.Split('@')[0],
                Email = model.Email,
                PhoneNumber = model.Phone,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);



            return new Response<bool>
            {
                Data = true,
                Status = true,
                Message = result.Succeeded ?
                 "User registered successfully." : "User registration failed."
            };

        }

        public async Task<Response<bool>> LoginAsync(LoginViewModel model)
        {


            var userByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (userByEmail is null)
            {
                return new Response<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Login failed Invalide Email or Password"
                };
            }

            var passwordMatchFlag = await _userManager.CheckPasswordAsync(userByEmail, model.Password);

            if (!passwordMatchFlag)
            {
                return new Response<bool>
                {
                    Data = false,
                    Status = false,
                    Message = "Login failed Invalide Email or Password"
                };
            }

            var result = await _signInManager.PasswordSignInAsync(userByEmail, model.Password, model.RememberMe, false);


            //Adding avatar image to be used with user claim
            //  Remove old avatar claim(if exists)
            var oldClaim = (await _userManager.GetClaimsAsync(userByEmail))
                          .FirstOrDefault(c => c.Type == "avatar");
            if (oldClaim != null)
                await _userManager.RemoveClaimAsync(userByEmail, oldClaim);

            //Add new avatar claim
            var avatarUrl = userByEmail.ProfileImageUrl;

            await _userManager.AddClaimAsync(userByEmail, new Claim("avatar", avatarUrl));
            await _signInManager.RefreshSignInAsync(userByEmail);


            return new Response<bool>
            {
                Data = true,
                Status = true,
                Message = result.Succeeded ?
                 "Logined successfully." : "Login Failed Invalid Email or Password."


            };


        }

        public async Task<Response<bool>> LogoutAsync()
        {
            await _signInManager.SignOutAsync();

            return new Response<bool>
            {
                Status = true,
                Message = "Logged out successfully",
                Data = true
            };
        }

    }
}
