using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;
using WebYTE.Application.DTOs.Notification;

namespace WebYTE.Client.Services;

public class NotificationService : IAsyncDisposable
{
    private readonly HttpClient _http;
    private readonly NavigationManager _navigation;
    private HubConnection? _hubConnection;
    private readonly AuthService _authService;

    public event Action<NotificationDto>? OnNotificationReceived;
    public event Action? OnNotificationsChanged;

    public List<NotificationDto> Notifications { get; private set; } = new();
    public int UnreadCount => Notifications.Count(n => !n.IsRead);

    public NotificationService(HttpClient http, NavigationManager navigation, AuthService authService)
    {
        _http = http;
        _navigation = navigation;
        _authService = authService;
    }

    public async Task InitializeAsync()
    {
        var token = await _authService.GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("No token found, skipping notification initialization");
            return;
        }

        // SignalR Hub is on the API server, not the client
        var hubUrl = "http://localhost:5109/notificationHub";
        
        _hubConnection = new HubConnectionBuilder()
            .WithUrl(hubUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(token)!;
            })
            .WithAutomaticReconnect()
            .Build();

        _hubConnection.On<NotificationDto>("ReceiveNotification", notification =>
        {
            Notifications.Insert(0, notification);
            OnNotificationReceived?.Invoke(notification);
            OnNotificationsChanged?.Invoke();
        });

        try
        {
            await _hubConnection.StartAsync();
            Console.WriteLine("SignalR connected successfully");
            await LoadNotificationsAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to notification hub: {ex.Message}");
        }
    }

    public async Task LoadNotificationsAsync()
    {
        try
        {
            var notifications = await _http.GetFromJsonAsync<List<NotificationDto>>("api/notification");
            if (notifications != null)
            {
                Notifications = notifications;
                OnNotificationsChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading notifications: {ex.Message}");
        }
    }

    public async Task MarkAsReadAsync(Guid notificationId)
    {
        try
        {
            var response = await _http.PutAsync($"api/notification/{notificationId}/read", null);
            if (response.IsSuccessStatusCode)
            {
                var notification = Notifications.FirstOrDefault(n => n.Id == notificationId);
                if (notification != null)
                {
                    notification.IsRead = true;
                    OnNotificationsChanged?.Invoke();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking notification as read: {ex.Message}");
        }
    }

    public async Task MarkAllAsReadAsync()
    {
        try
        {
            var response = await _http.PutAsync("api/notification/read-all", null);
            if (response.IsSuccessStatusCode)
            {
                foreach (var notification in Notifications)
                {
                    notification.IsRead = true;
                }
                OnNotificationsChanged?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error marking all notifications as read: {ex.Message}");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }
}
