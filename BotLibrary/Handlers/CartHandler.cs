using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RootLibrary.Data;
using RootLibrary.Enums;
using RootLibrary.Models.Inner;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotLibrary.Command
{
    public class CartHandler
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CartHandler(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Handle(ITelegramBotClient bot, CallbackQuery cq)
        {
            if (cq.Message is null || cq.From is null || string.IsNullOrWhiteSpace(cq.Data))
                return;

            if (!cq.Data.StartsWith("cart:"))
                return;

            long chatId = cq.Message.Chat.Id;
            int messageId = cq.Message.MessageId;
            long telegramId = cq.From.Id;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var user = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);
            if (user == null)
            {
                await bot.AnswerCallbackQuery(cq.Id, "Foydalanuvchi topilmadi. /start ni bosing.");
                return;
            }

            var parts = cq.Data.Split(':');
            var action = parts.Length > 1 ? parts[1] : "";
            long cartId = 0;
            if (parts.Length > 2) long.TryParse(parts[2], out cartId);

            try
            {
                switch (action)
                {
                    case "inc":
                        await Inc(db, user.Id, cartId);
                        await bot.AnswerCallbackQuery(cq.Id, "➕ Qo‘shildi");
                        break;

                    case "dec":
                        await Dec(db, user.Id, cartId);
                        await bot.AnswerCallbackQuery(cq.Id, "➖ Kamaytirildi");
                        break;

                    case "del":
                        await Del(db, user.Id, cartId);
                        await bot.AnswerCallbackQuery(cq.Id, "❌ O‘chirildi");
                        break;

                    case "clear":
                        await Clear(db, user.Id);
                        await bot.AnswerCallbackQuery(cq.Id, "🗑 Savat tozalandi");
                        break;

                    case "order":
                        var ok = await CreateOrderAndEmptyCart(db, user.Id);
                        await bot.AnswerCallbackQuery(cq.Id, ok ? "🧾 Buyurtma qabul qilindi" : "🛒 Savat bo‘sh");
                        break;

                    default:
                        await bot.AnswerCallbackQuery(cq.Id, "Noma’lum amal");
                        return;
                }

                var carts = await db.Carts
                    .Include(c => c.Product)
                    .Where(c => c.UserId == user.Id && !c.IsOrdered)
                    .OrderBy(c => c.Id)
                    .ToListAsync();

                if (!carts.Any())
                {
                    await bot.EditMessageText(
                        chatId: chatId,
                        messageId: messageId,
                        text: "🛒 Savatchangiz bo‘sh"
                    );
                    return;
                }

                var (text, keyboard) = CartCommand.BuildCartView(carts);

                await bot.EditMessageText(
                    chatId: chatId,
                    messageId: messageId,
                    text: text,
                    parseMode: ParseMode.MarkdownV2,
                    replyMarkup: keyboard
                );
            }
            catch
            {
                await bot.AnswerCallbackQuery(cq.Id, "Xatolik. Qayta urinib ko‘ring.");
            }
        }

        private static async Task Inc(AppDbContext db, long userId, long cartId)
        {
            var cart = await db.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId && !c.IsOrdered);
            if (cart == null) return;

            cart.Quantity += 1;
            await db.SaveChangesAsync();
        }

        private static async Task Dec(AppDbContext db, long userId, long cartId)
        {
            var cart = await db.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId && !c.IsOrdered);
            if (cart == null) return;

            cart.Quantity -= 1;

            if (cart.Quantity <= 0)
                db.Carts.Remove(cart);

            await db.SaveChangesAsync();
        }

        private static async Task Del(AppDbContext db, long userId, long cartId)
        {
            var cart = await db.Carts.FirstOrDefaultAsync(c => c.Id == cartId && c.UserId == userId && !c.IsOrdered);
            if (cart == null) return;

            db.Carts.Remove(cart);
            await db.SaveChangesAsync();
        }

        private static async Task Clear(AppDbContext db, long userId)
        {
            var carts = await db.Carts.Where(c => c.UserId == userId && !c.IsOrdered).ToListAsync();
            if (carts.Count == 0) return;

            db.Carts.RemoveRange(carts);
            await db.SaveChangesAsync();
        }


        private static async Task<bool> CreateOrderAndEmptyCart(AppDbContext db, long userId)
        {
            var carts = await db.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId && !c.IsOrdered)
                .ToListAsync();

            if (carts.Count == 0) return false;

            await using var tx = await db.Database.BeginTransactionAsync();

            var order = new Order
            {
                UserId = (int)userId,
                OrderDate = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                TotalAmount = carts.Sum(x => x.Quantity * x.Product.Price)
            };

            db.Orders.Add(order);
            await db.SaveChangesAsync();

            var items = carts.Select(c => new OrderItem
            {
                OrderId = order.Id,
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                ItemPrice = c.Product.Price,
                TotalPrice = c.Quantity * c.Product.Price
            }).ToList();

            db.OrderItems.AddRange(items);
            
            db.Carts.RemoveRange(carts);

            await db.SaveChangesAsync();
            await tx.CommitAsync();

            return true;
        }
    }

 

   
}
