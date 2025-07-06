using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.JSInterop;

namespace VideoTube.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly IJSRuntime _jsRuntime;
    // Закомментировано для отключения авторизации
    // private UserInfo? _currentUser;
    
    // Создаем фиктивного пользователя для работы без авторизации
    private UserInfo _currentUser = new UserInfo
    {
        Email = "demo@videotube.com",
        Name = "Demo User",
        Picture = ""
    };

    public event Action<UserInfo?>? AuthenticationStateChanged;

    public AuthService(HttpClient httpClient, ILocalStorageService localStorage, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _jsRuntime = jsRuntime;
    }

    public UserInfo? CurrentUser => _currentUser;

    public async Task InitializeAsync()
    {
        // Закомментировано для отключения авторизации
        // var token = await _localStorage.GetItemAsync<string>("authToken");
        // if (!string.IsNullOrEmpty(token))
        // {
        //     await ValidateAndSetUserAsync(token);
        // }
        
        // Всегда считаем пользователя аутентифицированным
        AuthenticationStateChanged?.Invoke(_currentUser);
    }

    public async Task<bool> LoginAsync()
    {
        // Закомментировано для отключения авторизации
        // try
        // {
        //     // Redirect to Google login
        //     var loginUrl = $"{_httpClient.BaseAddress}auth/google-login";
        //     await _jsRuntime.InvokeVoidAsync("window.open", loginUrl, "_self");
        //     return true;
        // }
        // catch
        // {
        //     return false;
        // }
        
        // Просто возвращаем true, так как авторизация отключена
        return true;
    }

    public async Task<bool> ProcessAuthCallbackAsync(string token)
    {
        // Закомментировано для отключения авторизации
        // if (await ValidateAndSetUserAsync(token))
        // {
        //     await _localStorage.SetItemAsync("authToken", token);
        //     AuthenticationStateChanged?.Invoke(_currentUser);
        //     return true;
        // }
        // return false;
        
        // Всегда возвращаем true
        return true;
    }

    public async Task LogoutAsync()
    {
        // Закомментировано для отключения авторизации
        // _currentUser = null;
        // await _localStorage.RemoveItemAsync("authToken");
        // AuthenticationStateChanged?.Invoke(null);
        
        // Ничего не делаем, так как авторизация отключена
        await Task.CompletedTask;
    }

    private async Task<bool> ValidateAndSetUserAsync(string token)
    {
        // Закомментировано для отключения авторизации
        // try
        // {
        //     var response = await _httpClient.PostAsJsonAsync("auth/validate-token", new { Token = token });
        //     
        //     if (response.IsSuccessStatusCode)
        //     {
        //         var result = await response.Content.ReadFromJsonAsync<TokenValidationResult>();
        //         if (result?.IsValid == true && result.User != null)
        //         {
        //             _currentUser = result.User;
        //             return true;
        //         }
        //     }
        // }
        // catch
        // {
        //     // Token validation failed
        // }
        // 
        // return false;
        
        // Всегда возвращаем true
        return true;
    }

    // Всегда возвращаем true, так как авторизация отключена
    public bool IsAuthenticated => true; // _currentUser != null;

    public HttpClient AuthorizedClient => _httpClient;
}

public class UserInfo
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Picture { get; set; } = string.Empty;
}

public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public UserInfo? User { get; set; }
}

public interface ILocalStorageService
{
    Task<T?> GetItemAsync<T>(string key);
    Task SetItemAsync<T>(string key, T value);
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
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            return json == null ? default : JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        catch
        {
            // Handle error
        }
    }

    public async Task RemoveItemAsync(string key)
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
        catch
        {
            // Handle error
        }
    }
} 