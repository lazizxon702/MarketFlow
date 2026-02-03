using Microsoft.Extensions.DependencyInjection;
using RootLibrary.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Command;

public class OrdersCommand
{
    private readonly IServiceScopeFactory _scopeFactory;

    public OrdersCommand(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task Execute(ITelegramBotClient bot, Message message)
    {
        long telegramId = message.From.Id;

        using var scope = _scopeFactory.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

     
        var userRes = await userService.GetByTelegramId(telegramId);
        if (!userRes.Success || userRes.Data == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Foydalanuvchi topilmadi.");
            return;
        }

        var ordersRes = await orderService.GetUserOrder(userRes.Data.userId);
        if (!ordersRes.Success || ordersRes.Data == null)
        {
             await bot.SendMessage(message.Chat.Id, "❌ Buyurtmalarni yuklashda xatolik.");
             return;
        }

        var myOrders = ordersRes.Data.Where(o => o.UserId == userRes.Data.userId).ToList();

        var keyboard = new ReplyKeyboardMarkup(new[]
        {
            new[] { new KeyboardButton("⬅️ Orqaga") }
        })
        {
            ResizeKeyboard = true
        };

        if (myOrders.Count == 0)
        {
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: "📦 Sizda hozircha buyurtmalar yo'q.",
                replyMarkup: keyboard
            );
            return;
        }

        var text = "📦 *Sizning buyurtmalaringiz:*\n\n";
        foreach (var order in myOrders)
        {
            text += $"🆔 Buyurtma #{order.Id}\n" +
                    $"📅 Sana: {order:dd.MM.yyyy HH:mm}\n" +
                    $"💰 Jami: {order.TotalAmount} so'm\n" +
                    $"------------------------------\n";
        }

        await bot.SendMessage(
            message.Chat.Id,
            text,
            parseMode: ParseMode.Markdown,
            replyMarkup: keyboard
        );
    }
}