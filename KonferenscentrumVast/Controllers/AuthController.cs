using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;

namespace KonferenscentrumVast.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _users;
        private readonly SignInManager<IdentityUser> _signIn;
        private readonly IConfiguration _cfg;

        public AuthController(
            UserManager<IdentityUser> users,
            SignInManager<IdentityUser> signIn,
            IConfiguration cfg)
        {
            _users = users;
            _signIn = signIn;
            _cfg = cfg;
        }

        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var user = new IdentityUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
            var result = await _users.CreateAsync(user, dto.Password);
            if (!result.Succeeded) return BadRequest(result.Errors);
            return Ok(new { message = "User created" });
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var user = await _users.FindByEmailAsync(dto.Email);
            
            if (user is null) return Unauthorized();

            var result = await _signIn.CheckPasswordSignInAsync(user, dto.Password, false);
            if (!result.Succeeded) return Unauthorized();

            var token = GenerateJwt(user);
            return Ok(token);
        }

        public record RegisterDto(string Email, string Password);

        public record LoginDto(string Email, string Password);

        private string GenerateJwt(IdentityUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_cfg["JwtSettings:Secret"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? "")
            };

            var token = new JwtSecurityToken(
                issuer: _cfg["JwtSettings:Issuer"],
                audience: _cfg["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddSeconds(int.TryParse(_cfg["JwtSettings:ExpiryMinutes"], out var m)
                    ? m
                    : 60),
                signingCredentials: creds
            );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}