﻿using Microsoft.AspNetCore.Identity;
using PBL3.Models.Domain;
using PBL3.Models.DTO;
using PBL3.Repositories.Abstract;
using System.Reflection.Metadata;
using System.Security.Claims;

namespace PBL3.Repositories.Implementation
{
    public class UserService : IUserService
    {
        private readonly UserManager<AppUser> userManager;
        private readonly SignInManager<AppUser> signInManager;

        public UserService(UserManager<AppUser> userManager
            , SignInManager<AppUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        public Task<AppUser> GetUserAsync(ClaimsPrincipal user)
        {
           return userManager.GetUserAsync(user);
        }
        public Task<AppUser> FindByIdAsync(string id)
        {
            return userManager.FindByIdAsync(id);
        }
        public async Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal user, string currentPassword, string newPassword)
        {
            AppUser appUser = await userManager.GetUserAsync(user);
            if (appUser == null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });
            }

            var result = await userManager.ChangePasswordAsync(appUser, currentPassword
                , newPassword);
            if (result.Succeeded)
            {
                // Làm mới thông tin đăng nhập của người dùng mà ko cần yêu cầu đăng nhập lại
                await signInManager.RefreshSignInAsync(appUser);
            }
            // Error
            return result;
        }
        public async Task<IdentityResult> ResetPasswordAsync(ClaimsPrincipal user, string newPassword)
        {
			AppUser appUser = await userManager.GetUserAsync(user);
            string token = await userManager.GeneratePasswordResetTokenAsync(appUser);
            return await userManager.ResetPasswordAsync(appUser, token, newPassword);
        }

        public async Task<IdentityResult> LoginAsync(string email, string password)
        {
            AppUser user = await userManager.FindByEmailAsync(email);
            if (user != null) 
            {
                await signInManager.SignOutAsync();
                SignInResult signInRes = await signInManager
                    .PasswordSignInAsync(user, password, false, false);
                
                if (signInRes.Succeeded)
                {
                    return IdentityResult.Success;
                }
                else
                {
                    return IdentityResult.Failed(new IdentityError()
                    {
                        Description = "Invalid password"
                    });
                }
            }
            return IdentityResult.Failed(new IdentityError() 
            { 
                Description = "Invalid email or email isn't registered" 
            });
        }

        public Task LogoutAsync()
        {
            return signInManager.SignOutAsync();
        }
        public async Task<IdentityResult> Register(AppUser newUser, string password)
        {
            // có validate thông qua IUserValidator và IPasswordValidator
            var res = await userManager.CreateAsync(newUser, password);
            if (res.Succeeded)
            {
                return await userManager.AddToRoleAsync(newUser, Role.Member);
            }
            return res;
        }
        public Task<IdentityResult> UpdateUserAsync(AppUser user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user));
            }
            // có validate thông qua IUserValidator và IPasswordValidator
            return userManager.UpdateAsync(user);
        }
        public Task<IList<AppUser>> GetUsersInRoleAsync(string roleName)
        {
            return userManager.GetUsersInRoleAsync(roleName);
        }
    }
}
