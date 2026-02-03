using Microsoft.EntityFrameworkCore;
using RootLibrary.Data;
using RootLibrary.DTO.UserDTO;
using RootLibrary.Enums;
using RootLibrary.Interface;
using RootLibrary.Models.Inner;
using RootLibrary.Models.Response;

namespace RootLibrary.Services;

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
                    PhoneNumber = u.PhoneNumber,
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
                    PhoneNumber = u.PhoneNumber,
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

    public async Task<DefaultResponse<TelegramUserDTO>> GetByTelegramId(long telegramId)
    {
        try
        {
            var user = await db.Users
                .Where(u => u.TelegramId == telegramId)
                .Select(u => new TelegramUserDTO
                {
                    TelegramId = u.TelegramId,
                    Username = u.Username,
                    PhoneNumber = u.PhoneNumber,
                    Role = u.Role,
                    CreatedDate = u.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new DefaultResponse<TelegramUserDTO>(
                    new ErrorResponse("User topilmadi", (int)ResponseCode.NotFound)
                );
            }

            return new DefaultResponse<TelegramUserDTO>(user, "User topildi");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<TelegramUserDTO>(
                new ErrorResponse($"Xatolik: {ex.Message}", (int)ResponseCode.BadRequest )
            );
        }
    }


    public async Task<DefaultResponse<string>> CreateUser(TelegramUserDTO dto)
    {
        try
        {
            var exists = await db.Users.AnyAsync(u => u.TelegramId == dto.TelegramId);

            if (exists)
                return new DefaultResponse<string>(
                    new ErrorResponse("Siz allaqachon ro‘yxatdan o‘tgan ekansiz", 400)
                );

            var newUser = new User
            {
                TelegramId = dto.TelegramId,
                Username = dto.Username,
                PhoneNumber = dto.PhoneNumber,
                Role = dto.Role ,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            db.Users.Add(newUser);
            await db.SaveChangesAsync();

            return new DefaultResponse<string>($"Foydalanuvchi '{newUser.Username}' muvaffaqiyatli yaratildi!");
        }
        catch (Exception ex)
        {
            return new DefaultResponse<string>(
                new ErrorResponse($"Foydalanuvchi yaratishda xatolik: {ex.Message}", 500)
            );
        }
    }
    public async Task<DefaultResponse<bool>> UpdateUser(int id, TelegramUserDTO dto)
    {
        try
        {
            var user = await db.Users.FindAsync(id);
            if (user == null || user.IsDeleted)
            {
                var error = new ErrorResponse("Foydalanuvchi topilmadi", (int)ResponseCode.NotFound);
                return new DefaultResponse<bool>(error);
            }

            user.Username = dto.Username;
            user.PhoneNumber = dto.PhoneNumber;
            

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