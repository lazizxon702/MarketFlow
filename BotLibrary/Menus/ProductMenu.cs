using Telegram.Bot.Types.ReplyMarkups;

namespace BotLibrary.Menus;

public static class ProductMenu
{
    
    public static InlineKeyboardMarkup Build(List<(int Id, string Name)> products)
    {
        var buttons = products
            .Select(p => new[]
            {
                InlineKeyboardButton.WithCallbackData(p.Name, $"product_{p.Id}"),
            })
            .ToList();

      
        buttons.Add(new[] { InlineKeyboardButton.WithCallbackData("⬅️ Orqaga", "back_categories") });

        return new InlineKeyboardMarkup(buttons);
    }

   
    public static InlineKeyboardMarkup AddToCart(int productId)
    {
        return new InlineKeyboardMarkup(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("➕ Savatga qo‘shish", $"add:{productId}")
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData("⬅️ Orqaga", "back_products")
            }
        });
    }
}