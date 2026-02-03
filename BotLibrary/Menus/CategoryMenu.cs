using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Menus;

public static class CategoryMenu
{
    public static InlineKeyboardMarkup Keyboard =>
        new InlineKeyboardMarkup(new[]
        {
            new [] { InlineKeyboardButton.WithCallbackData("Elektronika", "category_1"),
                InlineKeyboardButton.WithCallbackData("Kiyim-kechak", "category_2") },
            new [] { InlineKeyboardButton.WithCallbackData("Kitoblar", "category_3"),
                InlineKeyboardButton.WithCallbackData("Uy-ro‘zg‘or", "category_4") },
            new [] { InlineKeyboardButton.WithCallbackData("⬅️ Orqaga", "back_main") }
        });
}