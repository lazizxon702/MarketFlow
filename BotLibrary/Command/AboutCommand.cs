using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace BotLibrary.Command
{
    public class AboutCommand
    {
        public async Task Execute(ITelegramBotClient bot, Message message)
        {
            if (message.Chat.Type != ChatType.Private)
                return;

            string aboutText =
                "🤖 *Plum Market Bot*\n\n" +
                "📌 Maqsad: Eng zo'r startup qilish .\n" +
                "👤 Dasturchi: Lazizxon\n" +
                "🗓 Versiya: 1.0.0\n" +
                "⚙️ Qo‘shimcha: Agar savol yoki takliflar bo'lsa @sm7316 ga yozishingiz mumkin ✔\n" +
                "Xulosa ❗: Plum Market eng tog'ri tanlovdir! .";

            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: aboutText,
                parseMode: ParseMode.Markdown
            );
        }
    }
}