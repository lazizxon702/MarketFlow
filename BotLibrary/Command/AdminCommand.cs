using BotLibrary.Services;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Command;

public class AdminCommand
{
    private readonly ITelegramBotClient _botClient;
    private readonly List<long> _adminIds;
    private readonly IServiceScopeFactory _scopeFactory;

    public AdminCommand(ITelegramBotClient botClient, List<long>? adminIds, IServiceScopeFactory scopeFactory)
    {
        _botClient = botClient;
        _adminIds = adminIds ?? new List<long>();
        _scopeFactory = scopeFactory;
    }

    public async Task ExecuteAsync(Message message)
    {
        if (!IsAdmin(message.From.Id))
        {
            await _botClient.SendMessage(
                chatId: message.Chat.Id,
                text: "⛔ Sizda admin paneliga kirish huquqi yo'q!"
            );
            return;
        }

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton("➕ Yangi mahsulot yaratish"), new KeyboardButton("👥 Foydalanuvchilar") },
            new[] { new KeyboardButton("🚪 Chiqish") }
        })
        {
            ResizeKeyboard = true
        };

        await _botClient.SendMessage(
            chatId: message.Chat.Id,
            text: "🔐 *Admin Panel*\n\nQuyidagi amallardan birini tanlang:",
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard
        );
    }


    public async Task HandleCallbackAsync(CallbackQuery callbackQuery)
    {
        await _botClient.AnswerCallbackQuery(callbackQuery.Id, "Bu menyu eskirgan. /admin ni yozing.");
    }

    public async Task StartProductCreation(long userId, long chatId)
    {
        AdminSession.SetState(userId, Enums.AdminState.WaitingForProductName);
        AdminSession.ProductDrafts[userId] = new RootLibrary.DTO.ProductDTO.ProductCreateDTO();

        await _botClient.SendMessage(
            chatId: chatId,
            text: "➕ *Yangi mahsulot yaratish*\n\nMahsulot nomini kiriting:",
            parseMode: ParseMode.Markdown,
            replyMarkup: new ReplyKeyboardRemove() 
        );
    }

    public async Task ShowUsers(long chatId)
    {
        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<RootLibrary.Interface.IUserService>();
        
        var response = await userService.GetAll();
        
        if (!response.Success || response.Data == null)
        {
             await _botClient.SendMessage(chatId, "Userlarni yuklashda xatolik!");
             return;
        }

        var users = response.Data;
        int total = users.Count;
        int active = users.Count(u => u.Role == "User");
        int admins = users.Count(u => u.Role == "Admin");

        var usersList = $"👥 *Foydalanuvchilar* ({total}):\n\n";

        foreach (var user in users)
        {
            usersList += $"👤 {EscapeMarkdown(user.Username)} | 📱 {EscapeMarkdown(user.PhoneNumber)}\n" +
                         $"🆔 {user.Id} | 🎭 {EscapeMarkdown(user.Role)}\n" +
                         $"-----------------------------\n";
        }
        
        if (usersList.Length > 4000)
        {
            usersList = usersList.Substring(0, 4000) + "...";
        }

        await _botClient.SendMessage(
            chatId: chatId,
            text: usersList,
            parseMode: ParseMode.Markdown
        );
    }
    
    private string EscapeMarkdown(string text)
    {
        return string.IsNullOrEmpty(text)
            ? "-"
            : text.Replace("_", "\\_")
                  .Replace("*", "\\*")
                  .Replace("[", "\\[")
                  .Replace("]", "\\]")
                  .Replace("`", "\\`");
    }

    public bool IsAdmin(long userId)
    {
        return _adminIds.Contains(userId);
    }
}
