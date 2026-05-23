using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace WebYTE.Client.Services;

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T item);
    Task RemoveItemAsync(string key);
}

public class LocalStorageService : ILocalStorageService
{
    private readonly IJSRuntime _jsRuntime;

    public LocalStorageService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task<T?> GetItemAsync<T>(string key)
    {
        try
        {
            // Dùng string? để an toàn khi JS trả về null
            var json = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            if (string.IsNullOrEmpty(json))
                return default;

            // Nếu T là string, trả về thẳng không cần deserialize
            if (typeof(T) == typeof(string))
                return (T)(object)json;

            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T item)
    {
        try
        {
            // Nếu T là string thì lưu thẳng không wrap JSON
            if (typeof(T) == typeof(string))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, item);
            }
            else
            {
                var json = System.Text.Json.JsonSerializer.Serialize(item);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
        }
        catch { }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch { }
    }
}
