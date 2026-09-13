using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using DatingApp.Dtos.Auth;
using DatingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace DatingApp.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<DatingappUser> _userManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<DatingappUser> userManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _configuration = configuration;
    }

    public async Task<AuthResponse> LoginAsync(AuthLoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
            return null;

        var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);

        if (!passwordValid)
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

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
        );

        var expiration = DateTime.Now.AddMinutes(1440);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            Id = user.Id
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

        return await _userManager.CreateAsync(user, request.Password);
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
            MBTICode = user.MBTICode
        };
    }
}