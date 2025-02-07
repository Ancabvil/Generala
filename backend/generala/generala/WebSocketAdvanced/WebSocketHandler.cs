using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using generala.Models.Dtos;

namespace generala.Services
{
    public class WebSocketHandler
    {
        private readonly WebSocketNotificationService _notificationService;

        public WebSocketHandler(WebSocketNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, int userId, string nickname)
        {
            _notificationService.AddConnection(userId, webSocket, nickname);

            await _notificationService.BroadcastMessageAsync(new WebSocketMessageDto
            {
                Type = "user_connected",
                Content = JsonSerializer.Serialize(new { userId, nickname })
            });

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // ✅ Convertimos el mensaje a un DTO para procesarlo
                    var request = JsonSerializer.Deserialize<WebSocketMessageDto>(message);

                    if (request?.Type == "get_online_users")
                    {
                        await _notificationService.SendOnlineUsersList(userId);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en WebSocket: {ex.Message}");
            }
            finally
            {
                _notificationService.RemoveConnection(userId);
                await _notificationService.BroadcastMessageAsync(new WebSocketMessageDto
                {
                    Type = "user_disconnected",
                    Content = JsonSerializer.Serialize(new { userId, nickname })
                });

            }
        }

    }
}
