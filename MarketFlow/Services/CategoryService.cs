using MarketFlow.Data;
using MarketFlow.DTO.CategoryDTO;
using MarketFlow.Enums;
using MarketFlow.Interface;
using MarketFlow.Models.Inner;
using MarketFlow.Models.Response;
using Microsoft.EntityFrameworkCore;

namespace MarketFlow.Services;

public class CategoryService(AppDbContext db) : ICategoryService
{
    public async Task<DefaultResponse<List<CategoryReadDTO>>> GetAll()
    {
        try
        {
            var list = await db.Categories
                .Where(c => !c.IsDeleted)
                .Select(c => new CategoryReadDTO
                {
                    Id = c.Id,
                    Keyword = c.Keyword
                })
                .ToListAsync();

            return new DefaultResponse<List<CategoryReadDTO>>(list, "Kategoriyalar mavjud");
        }
        catch
        {
            return new DefaultResponse<List<CategoryReadDTO>>(
                new ErrorResponse("Kategoriyalarni olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<List<CategoryReadDTO>>> GetById(int id)
    {
        try
        {
            var categoryList = await db.Categories
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CategoryReadDTO
                {
                    Id = c.Id,
                    Keyword = c.Keyword
                })
                .ToListAsync();

            if (categoryList == null || categoryList.Count == 0)
                return new DefaultResponse<List<CategoryReadDTO>>(
                    new ErrorResponse("Kategoriya topilmadi", (int)ResponseCode.NotFound)
                );

            return new DefaultResponse<List<CategoryReadDTO>>(categoryList);
        }
        catch
        {
            return new DefaultResponse<List<CategoryReadDTO>>(
                new ErrorResponse("Kategoriya olishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<string>> Create(CategoryCreateDTO dto)
    {
        try
        {
            var exists = await db.Categories.AnyAsync(c => c.Keyword == dto.Keyword);
            if (exists)
                return new DefaultResponse<string>(
                    new ErrorResponse("Bu kategoriya allaqachon mavjud!", (int)ResponseCode.ValidationError)
                );

            var category = new Category
            {
                Keyword = dto.Keyword,
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            db.Categories.Add(category);
            await db.SaveChangesAsync();

            return new DefaultResponse<string>($"Category '{category.Keyword}' muvaffaqiyatli yaratildi!");
        }
        catch
        {
            return new DefaultResponse<string>(
                new ErrorResponse("Kategoriya yaratishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<bool>> Update(int id, CategoryUpdateDTO dto)
    {
        try
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
                return new DefaultResponse<bool>(
                    new ErrorResponse("Kategoriya topilmadi", (int)ResponseCode.NotFound)
                );

            category.Keyword = dto.Keyword;
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Kategoriya muvaffaqiyatli yangilandi");
        }
        catch
        {
            return new DefaultResponse<bool>(
                new ErrorResponse("Kategoriya yangilashda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }

    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        try
        {
            var category = await db.Categories.FindAsync(id);
            if (category == null || category.IsDeleted)
                return new DefaultResponse<bool>(
                    new ErrorResponse("Kategoriya topilmadi", (int)ResponseCode.NotFound)
                );

            category.IsDeleted = true;
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Kategoriya muvaffaqiyatli o'chirildi");
        }
        catch
        {
            return new DefaultResponse<bool>(
                new ErrorResponse("Kategoriya o'chirishda xatolik yuz berdi", (int)ResponseCode.ServerError)
            );
        }
    }
}