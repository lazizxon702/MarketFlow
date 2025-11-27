using MarketPlace.DTO;
using MarketPlace.Interface;
using MarketPlace.Models.Response;
using MarketPlace.Enums;
using Microsoft.EntityFrameworkCore;

namespace MarketPlace.Services;

public class UserService(AppDbContext db) : IUserService
{
    public async Task<DefaultResponse<List<UserReadDTO>>> GetAll()
    {
        try
        {
            var users = await db.Users
                .Where(u => !u.IsDeleted)
                .Select(u => new UserReadDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedDate = u.CreatedDate
                })
                .ToListAsync();

            return new DefaultResponse<List<UserReadDTO>>(users, "Foydalanuvchilar topildi");
        }
        catch
        {
            var error = new ErrorResponse("Foydalanuvchilarni olishda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<List<UserReadDTO>>(error);
        }
    }

    public async Task<DefaultResponse<UserReadDTO>> GetById(int id)
    {
        try
        {
            var user = await db.Users
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(u => new UserReadDTO
                {
                    Id = u.Id,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role,
                    CreatedDate = u.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                var error = new ErrorResponse("Foydalanuvchi topilmadi", (int)ResponseCode.NotFound);
                return new DefaultResponse<UserReadDTO>(error);
            }

            return new DefaultResponse<UserReadDTO>(user, "Foydalanuvchi topildi");
        }
        catch
        {
            var error = new ErrorResponse("Foydalanuvchini olishda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<UserReadDTO>(error);
        }
    }

    
    public async Task<DefaultResponse<string>> CreateUser(UserCreateDTO dto)
    {
        try
        {
            var exists = await db.Users.AnyAsync(u => 
                u.Username == dto.Username || u.Email == dto.Email);

            if (exists)
            {
                var error = new ErrorResponse("Bu username yoki email allaqachon mavjud", (int)ResponseCode.ValidationError);
                return new DefaultResponse<string>(error);
            }

            var newUser = new User
            {
                Username = dto.Username,
                Password = dto.Password,
                Email = dto.Email,
                Role = dto.Role,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return new DefaultResponse<string>($"Foydalanuvchi '{newUser.Username}' muvaffaqiyatli yaratildi!");
        }
        catch
        {
            var error = new ErrorResponse("Foydalanuvchi yaratishda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<string>(error);
        }
    }

    public async Task<DefaultResponse<bool>> UpdateUser(int id, UserUpdateDTO dto)
    {
        try
        {
            var user = await db.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
            {
                var error = new ErrorResponse("Foydalanuvchi topilmadi", (int)ResponseCode.NotFound);
                return new DefaultResponse<bool>(error);
            }

            user.Username = dto.FullName;
            user.Password = dto.Password;
            user.Email = dto.Email;
            user.Role = dto.Role;

            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Foydalanuvchi muvaffaqiyatli yangilandi");
        }
        catch
        {
            var error = new ErrorResponse("Foydalanuvchini yangilashda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<bool>(error);
        }
    }

    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        try
        {
            var user = await db.Users
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted);
            
            if (user == null)
            {
                var error = new ErrorResponse("Foydalanuvchi topilmadi", (int)ResponseCode.NotFound);
                return new DefaultResponse<bool>(error);
            }

            user.IsDeleted = true;
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Foydalanuvchi muvaffaqiyatli o'chirildi");
        }
        catch
        {
            var error = new ErrorResponse("Foydalanuvchini o'chirishda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<bool>(error);
        }
    }
}