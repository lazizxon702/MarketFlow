using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Menus
{
    public static class AdminMenu
    {
        public static ReplyKeyboardMarkup Main =>
            new ReplyKeyboardMarkup(new[]
            {
                new[] { new KeyboardButton("➕ Yangi mahsulot yaratish") },
                new[] { new KeyboardButton("👥 Foydalanuvchilar") },
                new[] { new KeyboardButton("❌ Chiqish") } 
            })
            {
                ResizeKeyboard = true,    
                OneTimeKeyboard = false   
            };
    }
}