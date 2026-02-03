using BotLibrary.Enums;
using BotLibrary.Services;
using Microsoft.Extensions.DependencyInjection;
using RootLibrary.Interface;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Handlers
{
    public class AdminHandleInput
    {
        private readonly ITelegramBotClient _botClient;
        private readonly IServiceScopeFactory _scopeFactory;

        public AdminHandleInput(ITelegramBotClient botClient, IServiceScopeFactory scopeFactory)
        {
            _botClient = botClient;
            _scopeFactory = scopeFactory;
        }
        
         public async Task HandleAdminInput(Message message, AdminState adminState)
        {
            long userId = message.From!.Id;

           
            if (!AdminSession.ProductDrafts.ContainsKey(userId))
            {
                AdminSession.ProductDrafts[userId] = new RootLibrary.DTO.ProductDTO.ProductCreateDTO();
            }
            var draft = AdminSession.ProductDrafts[userId];

            switch (adminState)
            {
                case AdminState.CreatingProduct:
                case AdminState.WaitingForProductName:
                    draft.NameS = message.Text;
                    draft.Description = "Ta'rif yo'q"; // Default
                    
                    AdminSession.SetState(userId, AdminState.ProductPrice);
                    await _botClient.SendMessage(message.Chat.Id, "✅ Nomi qabul qilindi.\nEndi narxini kiriting (faqat raqam):");
                    break;

                case AdminState.ProductPrice:
                    if (decimal.TryParse(message.Text, out decimal price))
                    {
                        draft.Price = price;
                        
                        AdminSession.SetState(userId, AdminState.ProductImage);
                        await _botClient.SendMessage(message.Chat.Id, "✅ Narx qabul qilindi.\nEndi mahsulot rasmini yuboring:");
                    }
                    else
                    {
                        await _botClient.SendMessage(message.Chat.Id, "❌ Iltimos, to'g'ri narx kiriting (faqat raqam).");
                    }
                    break;

                case AdminState.ProductImage:
                    if (message.Type == MessageType.Photo)
                    {
                       
                        var photo = message.Photo?.LastOrDefault();
                        if (photo != null)
                        {
                            draft.ImagePath = photo.FileId;
                            
                            using (var scope = _scopeFactory.CreateScope())
                            {
                                var catService = scope.ServiceProvider.GetRequiredService<ICategoryService>();
                                var catResponse = await catService.GetMainCategories();
                                
                                string catText = "✅ Rasm qabul qilindi.\nEndi Kategoriya ID sini tanlang:\n\n";
                                if (catResponse.Success && catResponse.Data != null)
                                {
                                    foreach (var c in catResponse.Data)
                                    {
                                        catText += $"{c.Id}. {c.Keyword}\n";
                                    }
                                }
                                catText += "\nID raqamini yuboring:";
                                
                                AdminSession.SetState(userId, AdminState.ChooseCategory);
                                await _botClient.SendMessage(message.Chat.Id, catText);
                            }
                        }
                    }
                    else
                    {
                         await _botClient.SendMessage(message.Chat.Id, "❌ Iltimos, rasm yuboring!");
                    }
                    break;

                case AdminState.ChooseCategory:
                    if (int.TryParse(message.Text, out int catId))
                    {
                        draft.CategoryId = catId;
                        
                        
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            try 
                            {
                                var productService = scope.ServiceProvider.GetRequiredService<IProductService>();
                                
                                Console.WriteLine($"🛠 Creating product in CategoryID: {draft.CategoryId} for User: {userId}");
                                
                                var createRes = await productService.Create(draft);
                                
                                if (createRes.Success)
                                {
                                    var keyboard = new ReplyKeyboardMarkup(new[]
                                    {
                                        new[] { new KeyboardButton("🔙 Admin Menyu"), new KeyboardButton("🚪 Chiqish") }
                                    })
                                    {
                                        ResizeKeyboard = true
                                    };

                                    await _botClient.SendMessage(
                                        chatId: message.Chat.Id,
                                        text: "✅ Mahsulot muvaffaqiyatli yaratildi!\n\nDavom etish uchun tanlang:",
                                        replyMarkup: keyboard
                                    );
                                    
                                    AdminSession.ClearState(userId);
                                    AdminSession.ProductDrafts.Remove(userId);
                                }
                                else
                                {
                                    await _botClient.SendMessage(message.Chat.Id, $"❌ Xatolik: {createRes.Message}");
                                    AdminSession.ClearState(userId);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"❌ EXCEPTION in AdminHandleInput: {ex}");
                                await _botClient.SendMessage(message.Chat.Id, $"❌ Dastur xatoligi: {ex.Message}");
                                AdminSession.ClearState(userId);
                            }
                        }
                    }
                    else
                    {
                        await _botClient.SendMessage(message.Chat.Id, "❌ Iltimos, kategoriya ID sini raqam sifatida yuboring.");
                    }
                    break;

                case AdminState.ManagingUsers:
                    await _botClient.SendMessage(message.Chat.Id, "👥 Userlarni boshqarish jarayoni boshlandi!");
                    break;

                default:
                    await _botClient.SendMessage(message.Chat.Id, "❌ Noma'lum admin komandasi.");
                    break;
            }
        }
    }
}