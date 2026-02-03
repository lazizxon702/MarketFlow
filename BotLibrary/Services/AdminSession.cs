using BotLibrary.Enums;
using RootLibrary.DTO.ProductDTO;

namespace BotLibrary.Services;

public class AdminSession
{
    public static Dictionary<long, AdminState> States = new();
    public static Dictionary<long, ProductCreateDTO> ProductDrafts = new();

    
    public static void SetState(long userId, AdminState state)
    {
        States[userId] = state;
    }

    public static AdminState GetState(long userId)
    {
        return States.ContainsKey(userId) ? States[userId] : AdminState.None;
    }

    public static void ClearState(long userId)
    {
        if (States.ContainsKey(userId))
            States.Remove(userId);
    }
}
