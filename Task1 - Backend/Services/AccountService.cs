using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Task1___Backend;
using Task1Backend.Entities;
using Task1Backend.Models;

namespace Task1Backend.Services
{
    public interface IAccountService
    {
        void RegisterUser(RegisterUserDto dto);

        string GenerateJwt(LoginUserDto dto);
    }

    public class AccountService : IAccountService
    {
        private readonly DataContext _dataContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly AuthenticationSettings _authenticationSettings;

        public AccountService(DataContext dataContext, IPasswordHasher<User> passwordHasher, AuthenticationSettings authenticationSettings)
        {
            _dataContext = dataContext;
            _passwordHasher = passwordHasher;
            _authenticationSettings = authenticationSettings;
        }

        /// <summary>
        ///Taking data from user, hashing password and sends it to database
        /// </summary>
        /// <param name="dto"></param>

        public void RegisterUser(RegisterUserDto dto)
        {
            try
            {
                var newUser = new User()
                {
                    Name = dto.Name,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    RoleId = dto.RoleId,
                };
                var hashedPassword = _passwordHasher.HashPassword(newUser, dto.Password);

                newUser.PasswordHash = hashedPassword;
                _dataContext.Users.Add(newUser);
                _dataContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        ///  Login user and generating JWT token
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        /// <exception cref="BadHttpRequestException"></exception>

        public string GenerateJwt(LoginUserDto dto)
        {
            var user = _dataContext.Users.
                FirstOrDefault(x => x.Email == dto.Email);

            if (user is null)
            {
                throw new BadHttpRequestException("Invalid email or password");
            }

            user.Role = _dataContext.Roles.First(x => x.Id == user.RoleId);

            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
            if (result == PasswordVerificationResult.Failed)
            {
                throw new BadHttpRequestException("Invalid username or password");
            }

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, $"{user.Name} {user.LastName}"),
                new Claim(ClaimTypes.Role, $"{user.Role.Name}")
            };
            int keyLengthInBytes = 32; // 256-bit key length
            byte[] keyBytes = new byte[keyLengthInBytes];

            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(keyBytes);
            }

            var key = new SymmetricSecurityKey(keyBytes);
            //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authenticationSettings.JwtKey));
            var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddDays(_authenticationSettings.JwtExpireDays);

            var token = new JwtSecurityToken(_authenticationSettings.JwtIssuer,
                _authenticationSettings.JwtIssuer,
                claims,
                expires: expires,
                signingCredentials: cred);
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(token);
        }
    }
}