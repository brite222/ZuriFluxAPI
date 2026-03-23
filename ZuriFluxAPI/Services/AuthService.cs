namespace ZuriFluxAPI.Services
{
    using BCrypt.Net;
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;
    using System.Text;
    using ZuriFluxAPI.DTOs;
    using ZuriFluxAPI.Models;
    using ZuriFluxAPI.Repositories;
    using BCrypt.Net;
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ICreditRepository _creditRepository;
        public AuthService(IUserRepository userRepository, IConfiguration configuration, ICreditRepository creditRepository)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _creditRepository = creditRepository;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetByEmailAsync(dto.Email);
            if (existingUser != null)
                throw new Exception("A user with this email already exists.");

            var referralCode = GenerateReferralCode(dto.FullName);

            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email.ToLower(),
                PasswordHash = BCrypt.HashPassword(dto.Password),
                Role = dto.Role ?? "citizen",
                CreditBalance = 0,
                ReferralCode = referralCode,
                TotalReferrals = 0,
                CreatedAt = DateTime.UtcNow
            };

            // Check referral code BEFORE saving user
            User referrer = null;
            if (!string.IsNullOrEmpty(dto.ReferralCode))
            {
                referrer = await _userRepository
                    .GetByReferralCodeAsync(dto.ReferralCode);

                if (referrer != null && referrer.Email != dto.Email.ToLower())
                {
                    // Give new user welcome bonus
                    user.CreditBalance = 20;
                }
            }

            // Save the user FIRST so they get an ID
            var created = await _userRepository.CreateAsync(user);

            // NOW handle referral transactions (user has ID now)
            if (referrer != null)
            {
                // Award referrer
                referrer.CreditBalance += 50;
                referrer.TotalReferrals += 1;
                await _userRepository.UpdateAsync(referrer);

                // Log referrer's transaction
                await _creditRepository.CreateAsync(new CreditTransaction
                {
                    UserId = referrer.Id,
                    Amount = 50,
                    Reason = $"Referral bonus — referred {dto.FullName}",
                    TransactedAt = DateTime.UtcNow
                });

                // Log new user's welcome transaction
                await _creditRepository.CreateAsync(new CreditTransaction
                {
                    UserId = created.Id,   // ← now has a valid ID
                    Amount = 20,
                    Reason = "Welcome bonus — joined via referral",
                    TransactedAt = DateTime.UtcNow
                });
            }

            var token = GenerateJwtToken(created);

            return new AuthResponseDto
            {
                Token = token,
                FullName = created.FullName,
                Email = created.Email,
                Role = created.Role
            };
        }
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // Find user by email
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null)
                throw new Exception("Invalid email or password.");

            // Verify password against stored hash
            bool passwordValid = BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!passwordValid)
                throw new Exception("Invalid email or password.");

            var token = GenerateJwtToken(user);

            return new AuthResponseDto
            {
                Token = token,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }

        public async Task<UserResponseDto> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null) return null;

            return new UserResponseDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CreditBalance = user.CreditBalance,
                ReferralCode = user.ReferralCode,       // ← add
                TotalReferrals = user.TotalReferrals,   // ← add
                CreatedAt = user.CreatedAt
            };
        }

        // Private method - generates the actual JWT token
        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]));

            // Claims are pieces of info embedded inside the token
            var claims = new[]
            {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(ClaimTypes.Name, user.FullName)
        };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(
                    double.Parse(jwtSettings["ExpiryInHours"])),
                signingCredentials: new SigningCredentials(
                    key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateReferralCode(string fullName)
        {
            var namePart = fullName.Split(' ')[0].ToUpper()
                .Substring(0, Math.Min(5, fullName.Split(' ')[0].Length));
            var randomPart = Guid.NewGuid().ToString("N")
                .Substring(0, 4).ToUpper();
            return $"{namePart}{randomPart}";
        }
    }

}
