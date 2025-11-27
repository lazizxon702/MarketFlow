using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MarketFlow;
using MarketPlace.DTO.Auth;
using MarketPlace.DTO.AuthDTO;
using MarketPlace.Interface;
using MarketPlace.Models.Response;
using MarketPlace.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace MarketPlace.Services;

public class AuthService(AppDbContext db, IConfiguration config) : IAuthService
{
    public async Task<DefaultResponse<string>> Login(AuthLoginDTO dto)
    {
        try
        {
            var user = await db.Users.FirstOrDefaultAsync(u => 
                u.Username == dto.Username && u.Password == dto.Password && !u.IsDeleted);

            if (user == null)
            {
                var error = new ErrorResponse("Username yoki parol noto'g'ri", (int)ResponseCode.Unauthorized);
                return new DefaultResponse<string>(error);
            }

            var token = GenerateJwtToken(user);
            return new DefaultResponse<string>(token, "Login muvaffaqiyatli!");
        }
        catch
        {
            var error = new ErrorResponse("Login jarayonida xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<string>(error);
        }
    }

    
    public async Task<DefaultResponse<string>> SignUp(AuthRegisterDTO dto)
    {
        try
        {
         
            if (!dto.Email.EndsWith("@gmail.com"))
                return new DefaultResponse<string>(new ErrorResponse("Faqat '@gmail.com' email manzili qabul qilinadi!", (int)ResponseCode.ValidationError));

      
            if (await db.Users.AnyAsync(u => u.Username == dto.Username))
                return new DefaultResponse<string>(new ErrorResponse("Bu foydalanuvchi allaqachon mavjud!", (int)ResponseCode.ValidationError));

            
            if (dto.Password != dto.PasswordVerification)
                return new DefaultResponse<string>(new ErrorResponse("Parollar mos emas!", (int)ResponseCode.ValidationError));

    
            var adminUsername = config["Admin:Username"];
            var adminPassword = config["Admin:Password"];

            
            var role = (dto.Username == adminUsername && dto.Password == adminPassword) ? "Admin" : "User";

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = dto.Password,
                Role = role,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            return new DefaultResponse<string>($"{user.Username} muvaffaqiyatli ro‘yxatdan o‘tdi!");
        }
        catch
        {
            return new DefaultResponse<string>(new ErrorResponse("Ro‘yxatdan o‘tishda xatolik yuz berdi", (int)ResponseCode.ServerError));
        }
    }

    
    private string GenerateJwtToken(User user)
    {
        var secretKey = config["Jwt:SecretKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim("UserId", user.Id.ToString())
        };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}