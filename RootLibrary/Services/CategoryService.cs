using Microsoft.EntityFrameworkCore;
using RootLibrary.Data;
using RootLibrary.DTO.CategoryDTO;
using RootLibrary.DTO.ProductDTO;
using RootLibrary.Enums;
using RootLibrary.Interface;
using RootLibrary.Models.Inner;
using RootLibrary.Models.Response;

namespace RootLibrary.Services;

public class CategoryService(AppDbContext db) : ICategoryService
{
    public async Task<DefaultResponse<List<CategoryReadDTO>>> GetMainCategories()
    {
        try
        {
            var list = await db.Categories
                .Where(c => !c.IsDeleted )
                .Select(c => new CategoryReadDTO
                {
                    Id = c.Id,
                    Keyword = c.Keyword,
                  
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

    public async Task<DefaultResponse<CategoryChildDTO>> GetChildCategories(int id)
    {
        try
        {
            var category = await db.Categories
                .Where(c => c.Id == id && !c.IsDeleted)
                .Select(c => new CategoryChildDTO
                {
                    Id = c.Id,
                    Keyword = c.Keyword,
                    Products = c.Products
                        .Where(p => !p.IsDeleted)
                        .Select(p => new ProductReadDTO
                        {
                            Id = p.Id,
                            NameS = p.NameS,
                            Description = p.Description,
                            Price = p.Price,
                            CreatedDate = p.CreatedDate
                        })
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (category == null)
            {
                return new DefaultResponse<CategoryChildDTO>(
                    new ErrorResponse("Kategoriya topilmadi", (int)ResponseCode.NotFound)
                );
            }

            return new DefaultResponse<CategoryChildDTO>(
                category,
                "Kategoriya va mahsulotlar topildi"
            );
        }
        catch
        {
            return new DefaultResponse<CategoryChildDTO>(
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