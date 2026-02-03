using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RootLibrary.Data;
using RootLibrary.Models.Inner;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Command
{
    public class CartCommand
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CartCommand(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(ITelegramBotClient bot, Message message)
        {
            if (message.Chat.Type != ChatType.Private || message.From is null)
                return;

            long chatId = message.Chat.Id;
            long telegramId = message.From.Id;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
            if (user == null)
            {
                await bot.SendMessage(chatId, "❌ Foydalanuvchi topilmadi. /start ni bosing.");
                return;
            }

            var carts = await db.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == user.Id && !c.IsOrdered)
                .OrderBy(c => c.Id)
                .ToListAsync();

            if (!carts.Any())
            {
                await bot.SendMessage(chatId, "🛒 Savatchangiz bo‘sh");
                return;
            }

            var (text, keyboard) = BuildCartView(carts);

            await bot.SendMessage(
                chatId: chatId,
                text: text,
                parseMode: ParseMode.Markdown, // ✅ V2 emas
                replyMarkup: keyboard
            );
        }

        public static (string text, InlineKeyboardMarkup keyboard) BuildCartView(List<Cart> carts)
        {
            decimal total = 0;
            string text = "🛒 *Savatchangiz:*\n\n";

            foreach (var cart in carts)
            {
                var price = cart.Product.Price;
                var qty = cart.Quantity;
                var sum = qty * price;
                total += sum;

                // ✅ Markdown (oddiy) uchun minimal escape
                var safeName = EscapeMarkdown(cart.Product.NameS ?? "");

                text +=
                    $"📦 *{safeName}*\n" +
                    $"🔢 {qty} x {FormatSum(price)} = {FormatSum(sum)}\n\n";
            }

            text += $"💰 *Jami:* {FormatSum(total)} so‘m";

            var rows = new List<InlineKeyboardButton[]>();

            foreach (var c in carts)
            {
                rows.Add(new[]
                {
                    InlineKeyboardButton.WithCallbackData("➖", $"cart:dec:{c.Id}"),
                    InlineKeyboardButton.WithCallbackData("➕", $"cart:inc:{c.Id}"),
                    InlineKeyboardButton.WithCallbackData("❌", $"cart:del:{c.Id}")
                });
            }

            rows.Add(new[]
            {
                InlineKeyboardButton.WithCallbackData("🧾 Buyurtma berish", "cart:order"),
                InlineKeyboardButton.WithCallbackData("🗑 Savatchani tozalash", "cart:clear")
            });

            return (text, new InlineKeyboardMarkup(rows));
        }

        // ✅ 1200000 => "1 200 000"
        public static string FormatSum(decimal value)
        {
            return value.ToString("N0", CultureInfo.InvariantCulture).Replace(",", " ");
        }

        // ✅ Oddiy Markdown uchun escape (V2 emas)
        private static string EscapeMarkdown(string text)
        {
            // Markdown (classic) uchun asosan * _ ` [ ] belgilar xalaqit qiladi
            return text
                .Replace("\\", "\\\\")
                .Replace("*", "\\*")
                .Replace("_", "\\_")
                .Replace("`", "\\`")
                .Replace("[", "\\[")
                .Replace("]", "\\]");
        }
    }
}
