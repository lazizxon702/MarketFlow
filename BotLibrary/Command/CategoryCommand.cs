using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Command
{
    public class CategoryCommand
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CategoryCommand(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task Execute(ITelegramBotClient bot, Message message)
        {
            if (message.Chat.Type != ChatType.Private)
                return;

            long telegramId = message.From.Id;

            using var scope = _scopeFactory.CreateScope();
            var categoryService = scope.ServiceProvider.GetRequiredService<RootLibrary.Interface.ICategoryService>();
            
           
            var catResponse = await categoryService.GetMainCategories();

            var buttons = new List<KeyboardButton[]>();
            
            if (catResponse.Success && catResponse.Data != null)
            {
                var row = new List<KeyboardButton>();
                foreach (var cat in catResponse.Data)
                {
                    row.Add(new KeyboardButton(cat.Keyword.Trim())); // Using Keyword/Name
                    if (row.Count == 2)
                    {
                        buttons.Add(row.ToArray());
                        row = new List<KeyboardButton>();
                    }
                }
                if (row.Count > 0) buttons.Add(row.ToArray());
            }

            
            buttons.Add(new[] { new KeyboardButton("⬅️ Orqaga") });

            var keyboard = new ReplyKeyboardMarkup(buttons)
            {
                ResizeKeyboard = true
            };

            await bot.SendMessage(
                telegramId,
                "📚 Kategoriyalar",
                parseMode: ParseMode.Markdown,
                replyMarkup: keyboard
            );
        }
    
    }
}