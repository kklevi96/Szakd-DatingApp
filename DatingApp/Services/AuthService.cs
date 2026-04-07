using DatingApp.Dtos.Auth;
using DatingApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DatingApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<DatingappUser> _userManager;
        private readonly SignInManager<DatingappUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthService(
            UserManager<DatingappUser> userManager,
            SignInManager<DatingappUser> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        public async Task<AuthResponse> RegisterAsync(AuthRegisterRequest request)
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
                throw new Exception("Email is already used!");

            var user = new DatingappUser
            {
                UserName = request.Email,
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                BirthDay = request.BirthDay,
                InterestedinWoman = request.InterestedinWoman,
                InterestedinMan = request.InterestedinMan,
                Gender = request.Gender,
                MBTICode = request.MBTICode
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(" ", result.Errors.Select(e => e.Description));
                throw new Exception(errors);
            }

            return CreateAuthResponse(user);
        }

        public async Task<AuthResponse> LoginAsync(AuthLoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("Login failed");

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException("Login failed");

            return CreateAuthResponse(user);
        }

        public async Task<UserMeResponse?> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return null;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return null;

            return new UserMeResponse
            {
                Id = user.Id,
                Email = user.Email ?? "",
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirthDay = user.BirthDay,
                InterestedinWoman = user.InterestedinWoman,
                InterestedinMan = user.InterestedinMan,
                Gender = user.Gender,
                MBTICode = user.MBTICode
            };
        }

        private AuthResponse CreateAuthResponse(DatingappUser user)
        {
            var claim = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.NameId, user.Id)
            };

            var signinKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var credentials = new SigningCredentials(signinKey, SecurityAlgorithms.HmacSha256);

            var expiration = DateTime.UtcNow.AddDays(7);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claim,
                expires: expiration,
                signingCredentials: credentials
            );

            return new AuthResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                Expiration = token.ValidTo,
                Id = user.Id,
                Email = user.Email ?? ""
            };
        }
    }
}