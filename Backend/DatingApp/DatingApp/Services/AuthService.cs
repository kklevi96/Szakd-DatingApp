using System.Security.Claims;
using DatingApp.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using DatingApp.Dtos.Auth;
using DatingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<DatingappUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly DatingAppDbContext _context;

    public AuthService(
        UserManager<DatingappUser> userManager,
        RoleManager<IdentityRole> roleManager,
        DatingAppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    public async Task<AuthResponse?> LoginAsync(AuthLoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            return null;

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Name, user.UserName ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.NameId, user.Id),
            new Claim("displayname", $"{user.FirstName} {user.LastName}")
        };

        foreach (var role in await _userManager.GetRolesAsync(user))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var signinKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes("nagyonhosszutitkoskodhelye123457890123457890")
        );
        var expiration = DateTime.Now.AddMinutes(1440);
        var token = new JwtSecurityToken(
            issuer: "http://www.security.org",
            audience: "http://www.security.org",
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            Id = user.Id,
            Email = user.Email ?? string.Empty
        };
    }

    public async Task<IdentityResult> RegisterAsync(AuthRegisterRequest request)
    {
        var user = new DatingappUser
        {
            Email = request.Email,
            UserName = request.Email,
            SecurityStamp = Guid.NewGuid().ToString(),

            FirstName = request.FirstName,
            LastName = request.LastName,
            BirthDay = request.BirthDay,

            InterestedinWoman = request.InterestedinWoman,
            InterestedinMan = request.InterestedinMan,

            Gender = request.Gender,
            MBTICode = request.MBTICode
        };

        await using var transaction = await _context.Database.BeginTransactionAsync();

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return result;

        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
            return roleResult;

        await transaction.CommitAsync();
        return result;
    }

    public async Task<UserMeResponse?> GetCurrentUserAsync(ClaimsPrincipal principal)
    {
        var user = await _userManager.GetUserAsync(principal);

        if (user == null)
            return null;

        return new UserMeResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            FirstName = user.FirstName,
            LastName = user.LastName,
            BirthDay = user.BirthDay,
            InterestedinWoman = user.InterestedinWoman,
            InterestedinMan = user.InterestedinMan,
            Gender = user.Gender,
            MBTICode = user.MBTICode,
            Roles = (await _userManager.GetRolesAsync(user)).ToList(),
            CanRequestAdmin = true
        };
    }

    public async Task<IdentityResult?> DelegateAdminAsync(
        ClaimsPrincipal principal, DelegateAdminRequest request)
    {
        if (request.Password != "DatingAppAdmin2026!")
        {
            return IdentityResult.Failed(new IdentityError
            {
                Code = "InvalidAdminPassword",
                Description = "Incorrect admin password."
            });
        }

        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
            return null;

        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            var role = new IdentityRole { Name = "Admin" };
            var roleResult = await _roleManager.CreateAsync(role);
            if (!roleResult.Succeeded)
                return roleResult;
        }

        if (await _userManager.IsInRoleAsync(user, "Admin"))
            return IdentityResult.Success;

        return await _userManager.AddToRoleAsync(user, "Admin");
    }

    public async Task<IdentityResult?> RemoveAdminAsync(ClaimsPrincipal principal)
    {
        var user = await _userManager.GetUserAsync(principal);
        if (user == null)
            return null;

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
            return IdentityResult.Success;

        return await _userManager.RemoveFromRoleAsync(user, "Admin");
    }
}
