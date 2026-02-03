using System.Text.RegularExpressions;
using BotLibrary.Command;
using BotLibrary.Enums;
using BotLibrary.Handlers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RootLibrary.Data;
using RootLibrary.Models.Inner;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotLibrary.Services;

public class BotService
{
    private readonly TelegramBotClient _botClient;
    private readonly IServiceScopeFactory _scopeFactory;

    public BotService(IOptions<BotOptions> options, IServiceScopeFactory scopeFactory)
    {
        _botClient = new TelegramBotClient(options.Value.BotToken);
        _scopeFactory = scopeFactory;
    }

    public void Start()
    {
        _botClient.StartReceiving(HandleUpdate, HandleError);
        Console.WriteLine(" BOT ISHLAYAPTI...");
    }

    private static string NormalizeButtonText(string? s)
    {
        if (string.IsNullOrWhiteSpace(s)) return "";

        s = s.Trim();
        s = s.Replace('\u00A0', ' ');
        s = s.Replace("’", "'").Replace("‘", "'").Replace("`", "'");

        s = Regex.Replace(s, @"\s+", " ");
        s = Regex.Replace(s, @"[^\p{L}\p{N}\s]", "");
        s = Regex.Replace(s, @"\s+", " ").Trim();

        return s.ToLowerInvariant();
    }

    private async Task HandleUpdate(ITelegramBotClient bot, Update update, CancellationToken ct)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            if (update.Type == UpdateType.CallbackQuery && update.CallbackQuery != null)
            {
                var callback = update.CallbackQuery;
                var data = callback.Data ?? "";
                var chatId = callback.Message?.Chat.Id ?? callback.From.Id;
                var telegramId = callback.From.Id;

                var userEntity = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == telegramId);

                if (data.StartsWith("admin_"))
                {
                    var adminCommand = scope.ServiceProvider.GetRequiredService<AdminCommand>();
                    await adminCommand.HandleCallbackAsync(callback);
                    await bot.AnswerCallbackQuery(callback.Id);
                    return;
                }

                if (data.StartsWith("category_"))
                {
                    var productCommand = scope.ServiceProvider.GetRequiredService<ProductCommand>();
                    int categoryId = int.Parse(data.Split('_')[1]);
                    await productCommand.Execute(bot, callback.Message!, categoryId);
                    await bot.AnswerCallbackQuery(callback.Id);
                    return;
                }

                if (data.StartsWith("product_"))
                {
                    var productCommand = scope.ServiceProvider.GetRequiredService<ProductCommand>();
                    int productId = int.Parse(data.Split('_')[1]);
                    await productCommand.ShowProductDetails(bot, chatId, productId);
                    await bot.AnswerCallbackQuery(callback.Id);
                    return;
                }

                if (data.StartsWith("add:"))
                {
                    if (userEntity == null)
                    {
                        await bot.AnswerCallbackQuery(callback.Id, "❌ User topilmadi. /start ni bosing.");
                        return;
                    }

                    int productId = int.Parse(data.Split(':')[1]);

                    var existing = await db.Carts.FirstOrDefaultAsync(c =>
                        c.UserId == userEntity.Id && c.ProductId == productId && !c.IsOrdered);

                    if (existing != null)
                        existing.Quantity += 1;
                    else
                        db.Carts.Add(new Cart
                        {
                            UserId = userEntity.Id,
                            ProductId = productId,
                            Quantity = 1,
                            IsOrdered = false
                        });

                    await db.SaveChangesAsync();
                    await bot.AnswerCallbackQuery(callback.Id, "✅ Mahsulot savatga qo‘shildi!");
                    return;
                }

                if (data.StartsWith("cart:inc") || data.StartsWith("cart:dec") || data.StartsWith("cart:del"))
                {
                    if (userEntity == null)
                    {
                        await bot.AnswerCallbackQuery(callback.Id, "❌ User topilmadi. /start ni bosing.");
                        return;
                    }

                    var parts = data.Split(':');
                    if (parts.Length < 3)
                    {
                        await bot.AnswerCallbackQuery(callback.Id, "❌ Noto‘g‘ri data");
                        return;
                    }

                    var action = parts[1];
                    int cartId = int.Parse(parts[2]);

                    var cart = await db.Carts.FirstOrDefaultAsync(c =>
                        c.Id == cartId && c.UserId == userEntity.Id && !c.IsOrdered);

                    if (cart == null)
                    {
                        await bot.AnswerCallbackQuery(callback.Id, "❌ Savat topilmadi");
                        return;
                    }

                    switch (action)
                    {
                        case "inc":
                            cart.Quantity++;
                            break;
                        case "dec":
                            cart.Quantity--;
                            if (cart.Quantity <= 0) db.Carts.Remove(cart);
                            break;
                        case "del":
                            db.Carts.Remove(cart);
                            break;
                    }

                    await db.SaveChangesAsync();
                    await bot.AnswerCallbackQuery(callback.Id, "✅ Yangilandi");
                    return;
                }

                if (data == "cart:clear")
                {
                    if (userEntity == null)
                    {
                        await bot.AnswerCallbackQuery(callback.Id, "❌ User topilmadi. /start ni bosing.");
                        return;
                    }

                    var carts = await db.Carts
                        .Where(c => c.UserId == userEntity.Id && !c.IsOrdered)
                        .ToListAsync();

                    if (carts.Count == 0)
                    {
                        await bot.AnswerCallbackQuery(callback.Id, "🛒 Savat bo‘sh");
                        return;
                    }

                    db.Carts.RemoveRange(carts);
                    await db.SaveChangesAsync();
                    await bot.AnswerCallbackQuery(callback.Id, "🗑 Savat tozalandi");
                    return;
                }

                if (data == "cart:order")
                {
                    var checkoutHandler = scope.ServiceProvider.GetRequiredService<CheckoutHandleInput>();
                    await checkoutHandler.StartCheckout(bot, chatId, telegramId);
                    await bot.AnswerCallbackQuery(callback.Id, "🧾 Buyurtma boshlandi");
                    return;
                }

                await bot.AnswerCallbackQuery(callback.Id);
                return;
            }

            if (update.Message == null)
                return;

            var message = update.Message;

            if (message.Type == MessageType.Contact)
            {
                var startCommand = scope.ServiceProvider.GetRequiredService<StartCommand>();
                await startCommand.HandleContact(bot, message);
                return;
            }

            var text = message.Text?.Trim();
            if (string.IsNullOrEmpty(text) && message.Type != MessageType.Photo)
                return;

            var key = NormalizeButtonText(text);

            if (UserSession.CheckoutState.TryGetValue(message.From!.Id, out var checkoutState) &&
                checkoutState != CheckoutState.None)
            {
                var checkoutHandler = scope.ServiceProvider.GetRequiredService<CheckoutHandleInput>();
                await checkoutHandler.HandleInput(bot, message);
                return;
            }

            if (text == "/admin")
            {
                AdminSession.ClearState(message.From!.Id);
                CheckoutHandleInput.ClearState(message.From!.Id);

                var adminCmd = scope.ServiceProvider.GetRequiredService<AdminCommand>();
                await adminCmd.ExecuteAsync(message);
                return;
            }

            var adminState = AdminSession.GetState(message.From!.Id);
            if (adminState != AdminState.None)
            {
                var adminHandler = scope.ServiceProvider.GetRequiredService<AdminHandleInput>();
                await adminHandler.HandleAdminInput(message, adminState);
                return;
            }

            var startCmd = scope.ServiceProvider.GetRequiredService<StartCommand>();
            var profileCommand = scope.ServiceProvider.GetRequiredService<ProfileCommand>();
            var categoryCmd = scope.ServiceProvider.GetRequiredService<CategoryCommand>();
            var cartCommand = scope.ServiceProvider.GetRequiredService<CartCommand>();

            switch (key)
            {
                case "start":
                    await startCmd.Execute(bot, message);
                    break;

                case "chiqish":
                    await startCmd.ShowMainMenu(bot, message.Chat.Id);
                    break;

                case "mahsulotlar":
                case "kategoriyalar":
                    await categoryCmd.Execute(bot, message);
                    break;

                case "buyurtmalar":
                {
                    var ordersCmd = scope.ServiceProvider.GetRequiredService<OrdersCommand>();
                    await ordersCmd.Execute(bot, message);
                    break;
                }

                case "profil":
                    await profileCommand.Execute(bot, message);
                    break;

                case "savat":
                    await cartCommand.Execute(bot, message);
                    break;

                case "orqaga":
                    await startCmd.ShowMainMenu(bot, message.Chat.Id);
                    break;

                case "about":
                    var aboutCommand = new AboutCommand();
                    await aboutCommand.Execute(bot, message);
                    break;

                case "foydalanuvchilar":
                {
                    var adminCmdUsers = scope.ServiceProvider.GetRequiredService<AdminCommand>();
                    if (adminCmdUsers.IsAdmin(message.From!.Id))
                        await adminCmdUsers.ShowUsers(message.Chat.Id);
                    break;
                }

                case "yangi mahsulot yaratish":
                {
                    var adminCmdCreate = scope.ServiceProvider.GetRequiredService<AdminCommand>();
                    if (adminCmdCreate.IsAdmin(message.From!.Id))
                        await adminCmdCreate.StartProductCreation(message.From!.Id, message.Chat.Id);
                    break;
                }

                case "savatga qoshish":
                {
                    if (!UserSession.ViewingProductId.TryGetValue(message.From!.Id, out int pid))
                    {
                        await bot.SendMessage(message.Chat.Id, "❌ Mahsulot tanlanmagan.");
                        break;
                    }

                    var userEntity = await db.Users.FirstOrDefaultAsync(u => u.TelegramId == message.From.Id);
                    if (userEntity == null)
                    {
                        await bot.SendMessage(message.Chat.Id, "❌ User topilmadi. /start ni bosing.");
                        break;
                    }

                    var existing = await db.Carts.FirstOrDefaultAsync(c =>
                        c.UserId == userEntity.Id && c.ProductId == pid && !c.IsOrdered);

                    if (existing != null)
                        existing.Quantity += 1;
                    else
                        db.Carts.Add(new Cart
                        {
                            UserId = userEntity.Id,
                            ProductId = pid,
                            Quantity = 1,
                            IsOrdered = false
                        });

                    await db.SaveChangesAsync();
                    await bot.SendMessage(message.Chat.Id, "✅ Mahsulot savatga qo‘shildi!");
                    break;
                }

                default:
                {
                    var catService = scope.ServiceProvider.GetRequiredService<RootLibrary.Interface.ICategoryService>();
                    var allCats = await catService.GetMainCategories();

                    if (allCats.Success && allCats.Data != null)
                    {
                        var cat = allCats.Data.FirstOrDefault(c =>
                            c.Keyword.Equals(text, StringComparison.OrdinalIgnoreCase));

                        if (cat != null)
                        {
                            var prodCmd = scope.ServiceProvider.GetRequiredService<ProductCommand>();
                            await prodCmd.Execute(bot, message, cat.Id);
                            return;
                        }
                    }

                    var foundProduct = await db.Products
                        .Where(p => !p.IsDeleted && p.NameS.ToLower() == text!.ToLower())
                        .FirstOrDefaultAsync();

                    if (foundProduct != null)
                    {
                        var prodCmd = scope.ServiceProvider.GetRequiredService<ProductCommand>();
                        await prodCmd.ShowProductDetailsSafe(bot, message.Chat.Id, foundProduct.Id);
                        return;
                    }

                    Console.WriteLine($"❌ No match for RAW: '{text}' | KEY: '{key}'");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ HandleUpdate Error: {ex.Message}");

            try
            {
                if (update.Message != null)
                    await bot.SendMessage(update.Message.Chat.Id,
                        "⚠️ Tizimda xatolik yuz berdi. Iltimos keyinroq urinib ko'ring.");
            }
            catch
            {
            }
        }
    }

    private Task HandleError(ITelegramBotClient bot, Exception ex, CancellationToken ct)
    {
        Console.WriteLine($"❌ Bot xatolik: {ex.Message}");
        return Task.CompletedTask;
    }
}
