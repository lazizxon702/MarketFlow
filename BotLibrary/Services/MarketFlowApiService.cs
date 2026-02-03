using System.Net.Http;
using System.Net.Http.Json;
using RootLibrary.DTO.CategoryDTO;

namespace BotLibrary.Services;

public class MarketFlowApiService
{
    private readonly HttpClient _httpClient;

    public MarketFlowApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<CategoryReadDTO>> GetCategoriesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<CategoryReadDTO>>("api/categories") ?? new();
    }

    public async Task CreateProductAsync(TempProductDTO dto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/products", dto);
        response.EnsureSuccessStatusCode();
    }
}
