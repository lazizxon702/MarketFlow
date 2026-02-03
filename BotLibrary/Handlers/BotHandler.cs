using BotLibrary.Command;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

public class BotHandler
{
    private readonly ITelegramBotClient _botClient;
    private readonly AdminCommand _adminCommand;

    public BotHandler(ITelegramBotClient botClient, AdminCommand adminCommand)
    {
        _botClient = botClient;
        _adminCommand = adminCommand;
    }

    public async Task HandleUpdateAsync(Update update)
    {
        if (update.Type == UpdateType.Message && update.Message!.Type == MessageType.Text)
        {
            var message = update.Message;

            if (message.Text == "/admin")
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