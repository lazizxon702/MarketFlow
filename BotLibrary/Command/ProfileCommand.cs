using Microsoft.Extensions.DependencyInjection;
using RootLibrary.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Command
{
    public class ProfileCommand
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ProfileCommand(IServiceScopeFactory scopeFactory)
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

            if (!userResponse.Success || userResponse.Data == null)
            {
                await bot.SendMessage(telegramId, "❌ Profil topilmadi. /start ni qayta bosing.");
                return;
            }

            var user = userResponse.Data;
            
           
            var adminCommand = scope.ServiceProvider.GetRequiredService<AdminCommand>();
            string roleDisplay = user.Role;
            if (adminCommand.IsAdmin(telegramId))
            {
                roleDisplay = "Admin";
            }

            string profileText =
                $"👤 *Profil ma’lumotlari*\n\n" +
                $"🆔 *Telegram ID:* `{user.TelegramId}`\n" +
                $"👤 *Username:* {EscapeMarkdown(user.Username ?? "-")}\n" +
                $"📱 *Telefon:* {EscapeMarkdown(user.PhoneNumber ?? "-")}\n" +
                $"🎭 *Role:* {EscapeMarkdown(roleDisplay ?? "-")}\n" +
                $"📅 *Ro‘yxatdan o‘tgan:* {user.CreatedDate:dd.MM.yyyy}";

            var keyboard = new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("⬅️ Orqaga") }
            })
            {
                ResizeKeyboard = true,
                OneTimeKeyboard = true
            };

            await bot.SendMessage(
                telegramId,
                profileText,
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }

        private string EscapeMarkdown(string text)
        {
            return string.IsNullOrEmpty(text)
                ? "-"
                : text.Replace("_", "\\_").Replace("*", "\\*").Replace("[", "\\[").Replace("]", "\\]").Replace("`", "\\`");
        }
    }
}
