using BuyMate.BLL.Contracts;
using BuyMate.DTO.ViewModels;
using Microsoft.AspNetCore.Identity;
using BuyMate.DTO.Common;

namespace BuyMate.BLL.Features.User.Register
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<Model.Entities.User> _userManager;

        public AuthService(UserManager<Model.Entities.User> userManager)
        {
            _userManager = userManager;
        }
        public async Task<Response<IdentityResult>> RegisterAsync(RegisterViewModel model)
        {

            //Check if  username is already taken
            var existingByUsername = await _userManager.FindByNameAsync(model.UserName);
            if (existingByUsername is not null)
            {
                return new Response<IdentityResult>
                {
                    Data = IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicateUserName",
                        Description = "Username is already taken."
                    }),
                    Status = false,
                    Message = "Registration failed due to duplicate username."
                };
            }

            // Friendly duplicate checks
            var existingByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingByEmail is not null)
            {
                return new Response<IdentityResult>
                {
                    Data = IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicateEmail",
                        Description = "Email is already taken."
                    }),
                    Status = false,
                    Message = "Registration failed due to duplicate email."
                };
            }

            var phoneExists = _userManager.Users.Any(u => u.PhoneNumber == model.Phone);
            if (phoneExists)
            {
                return new Response<IdentityResult>
                {
                    Data = IdentityResult.Failed(new IdentityError
                    {
                        Code = "DuplicatePhone",
                        Description = "Phone number is already used."
                    }),
                    Status = false,
                    Message = "Registration failed due to duplicate phone number."
                };
            }

            var user = new Model.Entities.User
            {
                UserName = model.UserName,
                Email = model.Email,
                PhoneNumber = model.Phone,
                FirstName = model.FirstName,
                LastName = model.LastName,
                EmailConfirmed = false,
                PhoneNumberConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            return new Response<IdentityResult>
            {
                Data = result,
                Status = true,
                Message = result.Succeeded ?
                 "User registered successfully." : "User registration failed."


            };
           
        }
    }
}
