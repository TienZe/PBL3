﻿using Microsoft.AspNetCore.Identity;
using PBL3.Models.Domain;
using System.Security.Claims;

namespace PBL3.Repositories.Abstract
{
	public interface IUserService
    {
        Task<AppUser> GetUserAsync(ClaimsPrincipal user);
        Task<AppUser> FindByIdAsync(string id);
        Task<IList<AppUser>> GetUsersInRoleAsync(string roleName);
        Task<IdentityResult> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<IdentityResult> Register(AppUser newUser, string password);
        Task<IdentityResult> ChangePasswordAsync(ClaimsPrincipal user, string currentPassword
            , string newPassword);
        Task<IdentityResult> ResetPasswordAsync(ClaimsPrincipal user, string newPassword);

		Task<IdentityResult> UpdateUserAsync(AppUser user);
    }
}
