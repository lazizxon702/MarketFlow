using Microsoft.EntityFrameworkCore;
using RootLibrary.Data;
using RootLibrary.DTO.ProductDTO;
using RootLibrary.Enums;
using RootLibrary.Interface;
using RootLibrary.Models.Inner;
using RootLibrary.Models.Response;

namespace RootLibrary.Services;

public class ProductService(AppDbContext db) : IProductService
{
    public async Task<DefaultResponse<List<ProductReadDTO>>> GetAll()
    {
        try
        {
            var products = await db.Products
                .Where(p => !p.IsDeleted)
                .Select(p => new ProductReadDTO
                {
                    Id = p.Id,
                    NameS = p.NameS,
                    
                    Description = p.Description,
                    Price = p.Price, 
                    CreatedDate = p.CreatedDate
                })
                .ToListAsync();

            return new DefaultResponse<List<ProductReadDTO>>(products, "Mahsulotlar topildi");
        }
        catch
        {
            var error = new ErrorResponse("Mahsulotlarni olishda xatolik yuz ber2di", (int)ResponseCode.ServerError);
            return new DefaultResponse<List<ProductReadDTO>>(error);
        }
    }

    public async Task<DefaultResponse<ProductReadDTO>> GetById(int id)
    {
        try
        {
            var product = await db.Products
                .Where(p => p.Id == id && !p.IsDeleted)
                .Select(p => new ProductReadDTO
                {
                    Id = p.Id,
                    NameS = p.NameS,
                    Description = p.Description,
                    Price = p.Price,
                    CreatedDate = p.CreatedDate
                })
                .FirstOrDefaultAsync();

            if (product != null) return new DefaultResponse<ProductReadDTO>(product, "Mahsulot topildi");
            var error = new ErrorResponse("Mahsulot topilmadi", (int)ResponseCode.NotFound);
            return new DefaultResponse<ProductReadDTO>(error);
        }
        catch
        {
            var error = new ErrorResponse("Mahsulotni olishda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<ProductReadDTO>(error);
        }
    }


    public async Task<DefaultResponse<string>> Create(ProductCreateDTO dto)
    {
        try
        {
            var categoryExists = await db.Categories.AnyAsync(c => c.Id == dto.CategoryId && !c.IsDeleted);
            if (!categoryExists)
            {
                var error = new ErrorResponse("Kategoriya topilmadi", (int)ResponseCode.NotFound);
                return new DefaultResponse<string>(error);
            }

            var product = new Product
            {
                NameS = dto.NameS,
                Description = dto.Description,
                Price = dto.Price,
                CategoryId = dto.CategoryId,
                ImageFileId = dto.ImagePath ?? "",
                CreatedDate = DateTime.UtcNow,
                IsDeleted = false
            };

            db.Products.Add(product);
            await db.SaveChangesAsync();

            return new DefaultResponse<string>($"Mahsulot '{product.NameS}' muvaffaqiyatli yaratildi!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ ProductService.Create Exception: {ex}");
            var error = new ErrorResponse($"Mahsulot yaratishda xatolik: {ex.Message}", (int)ResponseCode.ServerError);
            return new DefaultResponse<string>(error);
        }
    }


    public async Task<DefaultResponse<bool>> Update(int id, ProductUpdateDTO dto)
    {
        try
        {
            var product = await db.Products.FindAsync(id);
            if (product == null || product.IsDeleted)
            {
                var error = new ErrorResponse("Mahsulot topilmadi", (int)ResponseCode.NotFound);
                return new DefaultResponse<bool>(error);
            }

            product.NameS = dto.NameS;
            product.Description = dto.Description;
            product.Price = dto.Price;

            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Mahsulot muvaffaqiyatli yangilandi");
        }
        catch
        {
            var error = new ErrorResponse("Mahsulotni yangilashda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<bool>(error);
        }
    }


    public async Task<DefaultResponse<bool>> Delete(int id)
    {
        try
        {
            var product = await db.Products.FindAsync(id);
            if (product == null || product.IsDeleted)
            {
                var error = new ErrorResponse("Mahsulot topilmadi", (int)ResponseCode.NotFound);
                return new DefaultResponse<bool>(error);
            }

            product.IsDeleted = true;
            await db.SaveChangesAsync();

            return new DefaultResponse<bool>(true, "Mahsulot muvaffaqiyatli o'chirildi");
        }
        catch
        {
            var error = new ErrorResponse("Mahsulotni o'chirishda xatolik yuz berdi", (int)ResponseCode.ServerError);
            return new DefaultResponse<bool>(error);
        }
    }
}