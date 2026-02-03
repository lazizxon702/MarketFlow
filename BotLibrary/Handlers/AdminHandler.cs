using BotLibrary.Command;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using BotLibrary.Enums;
using BotLibrary.Services;

namespace BotLibrary.Handlers
{
    public class AdminHandler
    {
        private readonly ITelegramBotClient _botClient;
        private readonly AdminCommand _adminCommand;
    
        public AdminHandler(ITelegramBotClient botClient, AdminCommand adminCommand)
        {
            _botClient = botClient;
            _adminCommand = adminCommand;
        }

        public async Task HandleUpdateAsync(Update update)
        {
            if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
            {
                var message = update.Message;
                var state = AdminSession.GetState(message.From!.Id);
                
                if (state == AdminState.WaitingForProductName)
                {
                    string productName = message.Text;
                    
                    await _botClient.SendMessage(
                        chatId: message.Chat.Id,
                        text: $"✅ Mahsulot '{productName}' yaratildi."
                    );
                
                    AdminSession.ClearState(message.From.Id);
                
                  
                    await _adminCommand.ExecuteAsync(message);
                }
                else if (message.Text == "/admin")
                {
                    await _adminCommand.ExecuteAsync(message);
                }
            }
            else if (update.Type == UpdateType.CallbackQuery)
            {
                await _adminCommand.HandleCallbackAsync(update.CallbackQuery!);
            }
        }
    }
}