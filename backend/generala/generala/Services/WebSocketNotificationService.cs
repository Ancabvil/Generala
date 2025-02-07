using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using generala.Models.Dtos;

namespace generala.Services
{
    public class WebSocketNotificationService
    {
        private readonly ConcurrentDictionary<int, WebSocket> _connections = new();
        private readonly ConcurrentDictionary<int, string> _connectedUsers = new();

        public void AddConnection(int userId, WebSocket socket, string nickname)
        {
            _connections[userId] = socket;
            _connectedUsers[userId] = nickname; 
            NotifyUserStatus(userId, nickname, true);
        }

        public void RemoveConnection(int userId)
        {
            if (_connections.TryRemove(userId, out var socket) && socket.State == WebSocketState.Open)
            {
                socket.Abort();
            }

            if (_connectedUsers.TryRemove(userId, out var nickname))
            {
                NotifyUserStatus(userId, nickname, false);
            }
        }

        public async Task SendMessageAsync(int userId, WebSocketMessageDto message)
        {
            if (_connections.TryGetValue(userId, out var socket) && socket.State == WebSocketState.Open)
            {
                var json = JsonSerializer.Serialize(message);
                var buffer = Encoding.UTF8.GetBytes(json);
                await socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }

        public async Task BroadcastMessageAsync(WebSocketMessageDto message)
        {
            var json = JsonSerializer.Serialize(message);
            var buffer = Encoding.UTF8.GetBytes(json);

            foreach (var connection in _connections.Values)
            {
                if (connection.State == WebSocketState.Open)
                {
                    await connection.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        private async void NotifyUserStatus(int userId, string nickname, bool isConnected)
        {
            var message = new WebSocketMessageDto
            {
                Type = isConnected ? "user_connected" : "user_disconnected",
                Content = JsonSerializer.Serialize(new
                {
                    userId,
                    nickname,
                    totalOnline = _connectedUsers.Count
                })
            };

            await BroadcastMessageAsync(message);
        }

        public async Task SendOnlineUsersList(int userId)
        {

            var onlineUsers = _connectedUsers.Select(u => new { userId = u.Key, nickname = u.Value }).ToList();

            var message = new WebSocketMessageDto
            {
                Type = "online_users",
                Content = JsonSerializer.Serialize(onlineUsers)
            };

            await SendMessageAsync(userId, message);
        }




    }
}
