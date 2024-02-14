using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using tsuKeysAPIProject.DBContext.Models;
using tsuKeysAPIProject.DBContext;

namespace tsuKeysAPIProject.AdditionalServices.TokenHelpers
{
    public class TokenInteraction
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _secretKey;
        private readonly string _issuer;
        private readonly string _audience;

        public TokenInteraction(IConfiguration configuration, IServiceProvider serviceProvider, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;
            _secretKey = configuration.GetValue<string>("AppSettings:Secret");
            _issuer = configuration.GetValue<string>("AppSettings:Issuer");
            _audience = configuration.GetValue<string>("AppSettings:Audience");
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetUserEmailFromToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            string email = "";

            if (jwtToken.Payload.TryGetValue("email", out var emailObj) && emailObj is string emailValue)
            {
                email = emailValue;
            }

            return email;
        }
        public string GenerateToken(User user)
        {
            var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, user.Email)
        };

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(15),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = new(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
        public string GetTokenFromHeader()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDBContext>();

                string authorizationHeader = _serviceProvider.GetRequiredService<IHttpContextAccessor>().HttpContext.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authorizationHeader) && authorizationHeader.StartsWith("Bearer "))
                {
                    return authorizationHeader.Substring("Bearer ".Length);
                }
                return null;
            }
        }
    }
}
