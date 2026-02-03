using Microsoft.Extensions.DependencyInjection;
using RootLibrary.DTO.UserDTO;
using RootLibrary.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Command;

public class StartCommand
{
    private readonly IServiceScopeFactory _scopeFactory;

    public StartCommand(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(ITelegramBotClient bot, Message message)
    {
        if (message.Chat.Type != ChatType.Private)
            return;

        long telegramId = message.From.Id;

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        var userResponse = await userService.GetByTelegramId(telegramId);

        if (userResponse.Success && userResponse.Data != null)
        {
            await ShowMainMenu(bot, telegramId);
            return;
        }

        
        var contactKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new KeyboardButton("📱 Raqamni yuborish") { RequestContact = true }
        })
        {
            ResizeKeyboard = true,
            OneTimeKeyboard = true
        };

        await bot.SendMessage(
            telegramId,
            "👋 Assalomu alaykum! Botdan foydalanish uchun telefon raqamingizni yuboring.",
            replyMarkup: contactKeyboard
        );
    }

    public async Task HandleContact(ITelegramBotClient bot, Message message)
    {
        if (message.Contact == null) return;

        long telegramId = message.From.Id;
        var contact = message.Contact;

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

        
        var userResponse = await userService.GetByTelegramId(telegramId);
        if (userResponse.Success && userResponse.Data != null)
        {
            await ShowMainMenu(bot, telegramId);
            return;
        }

        string username = !string.IsNullOrWhiteSpace(message.From.Username)
            ? message.From.Username
            : $"{message.From.FirstName} {message.From.LastName}".Trim();

        if (string.IsNullOrWhiteSpace(username))
            username = $"user{telegramId}";

        var newUser = new TelegramUserDTO
        {
            TelegramId = telegramId,
            Username = username,
            PhoneNumber = contact.PhoneNumber,
            Role = "User",
            CreatedDate = DateTime.Now
        };

        var createResponse = await userService.CreateUser(newUser);

        if (!createResponse.Success)
        {
            await bot.SendMessage(
                telegramId,
                "❌ Xatolik yuz berdi. Keyinroq urinib ko‘ring."
            );
            return;
        }

        await bot.SendMessage(
            telegramId,
            $"👋 Salom {username}!\n✅ Siz muvaffaqiyatli ro‘yxatdan o‘tdingiz.",
            replyMarkup: new ReplyKeyboardRemove()
        );

        await ShowMainMenu(bot, telegramId);
    }

    public async Task ShowMainMenu(ITelegramBotClient bot, long userId)
    {
        var menuKeyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton("🛒 Mahsulotlar"), new KeyboardButton("📦 Buyurtmalar") },
            new[] { new KeyboardButton("👤 Profil"), new KeyboardButton("📚 Kategoriyalar") },
            new[] { new KeyboardButton("🛒 Savat") }
        })
        {
            ResizeKeyboard = true
        };

        await bot.SendMessage(
            userId,
            "🏠 Asosiy menyu",
            replyMarkup: menuKeyboard
        );
    }
}