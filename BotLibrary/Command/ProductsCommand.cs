using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Microsoft.EntityFrameworkCore;
using BotLibrary.Services;
using RootLibrary.Data;
using RootLibrary.Models;

namespace BotLibrary.Command;

public class ProductCommand
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductCommand(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

  
    public async Task Execute(ITelegramBotClient bot, Message? message, int categoryId)
    {
        long chatId = message.Chat.Id;

        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var products = db.Products
            .Where(p => p.CategoryId == categoryId && !p.IsDeleted)
            .Select(p => new { p.Id, p.NameS })
            .ToList();

        if (products.Count == 0)
        {
            await bot.SendMessage(chatId, "📦 Bu kategoriyada mahsulot topilmadi.");
            return;
        }

        var buttons = new List<KeyboardButton[]>();
        var row = new List<KeyboardButton>();
        foreach (var p in products)
        {
            row.Add(new KeyboardButton(p.NameS));
            if (row.Count == 2)
            {
                buttons.Add(row.ToArray());
                row = new List<KeyboardButton>();
            }
        }
        if (row.Count > 0) buttons.Add(row.ToArray());
        
        buttons.Add(new[] { new KeyboardButton("⬅️ Orqaga") });

        var keyboard = new ReplyKeyboardMarkup(buttons)
        {
            ResizeKeyboard = true
        };

        await bot.SendMessage(
            chatId: chatId,
            text: "📱 Mahsulotlar ro'yxati:",
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard
        );
    }

    public async Task ShowProductDetails(ITelegramBotClient bot, long chatId, int productId)
    {
        using var scope = _scopeFactory.CreateScope();
        var productService = scope.ServiceProvider.GetRequiredService<RootLibrary.Interface.IProductService>();
        var productRes = await productService.GetById(productId);
        
        if (!productRes.Success || productRes.Data == null)
        {
            await bot.SendMessage(chatId, "❌ Mahsulot ma'lumotlari topilmadi.");
            return;
        }

        var p = productRes.Data;
        
     
        string priceFormatted = $"{p.Price:N0}"; 
        
        string caption = $"🏷 *Nomi:* {p.NameS}\n" +
                         $"💰 *Narxi:* {priceFormatted} so'm\n" +
                         $"📝 *Tafsilot:* {p.Description}";

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton("➕ Savatga qo‘shish") },
            new[] { new KeyboardButton("⬅️ Orqaga") }
        })
        {
            ResizeKeyboard = true
        };
        
        UserSession.ViewingProductId[chatId] = p.Id;

        using var dbScope = _scopeFactory.CreateScope();
        var db = dbScope.ServiceProvider.GetRequiredService<AppDbContext>();
        var productDb = await db.Products.FirstOrDefaultAsync(x => x.Id == p.Id);
        if (productDb != null && !string.IsNullOrEmpty(productDb.ImageFileId))
        {
            try
            {
                await bot.SendPhoto(
                    chatId: chatId,
                    photo: InputFile.FromFileId(productDb.ImageFileId),
                    caption: caption,
                    parseMode: ParseMode.Markdown,
                    replyMarkup: keyboard
                );
            }
            catch
            {
                await bot.SendMessage(chatId, caption, ParseMode.Markdown, replyMarkup: keyboard);
            }
        }
        else
        {
            await bot.SendMessage(chatId, caption, ParseMode.Markdown, replyMarkup: keyboard);
        }
    }
    
    public async Task ShowProductDetailsSafe(ITelegramBotClient bot, long chatId, int productId)
    {
         using var scope = _scopeFactory.CreateScope();
         var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
         var p = await db.Products.FirstOrDefaultAsync(x => x.Id == productId);
         
         if (p == null) 
         {
             await bot.SendMessage(chatId, "❌ Mahsulot topilmadi.");
             return;
         }

         string priceFormatted = $"{p.Price:N0}"; 
         string caption = $"🏷 *Nomi:* {p.NameS}\n" +
                          $"💰 *Narxi:* {priceFormatted} so'm\n" +
                          $"📝 *Tafsilot:* {p.Description ?? "-"}";
         
         var keyboard = new ReplyKeyboardMarkup(new[]
         {
             new[] { new KeyboardButton("➕ Savatga qo‘shish") },
             new[] { new KeyboardButton("⬅️ Orqaga") }
         })
         {
             ResizeKeyboard = true
         };

         UserSession.ViewingProductId[chatId] = p.Id;

         if (!string.IsNullOrEmpty(p.ImageFileId))
         {
             try
             {
                 await bot.SendPhoto(
                     chatId: chatId,
                     photo: InputFile.FromFileId(p.ImageFileId),
                     caption: caption,
                     parseMode: ParseMode.Markdown,
                     replyMarkup: keyboard
                 );
             }
             catch
             {
                 await bot.SendMessage(chatId, caption, ParseMode.Markdown, replyMarkup: keyboard);
             }
         }
         else
         {
             await bot.SendMessage(chatId, caption, ParseMode.Markdown, replyMarkup: keyboard);
         }
    }
}