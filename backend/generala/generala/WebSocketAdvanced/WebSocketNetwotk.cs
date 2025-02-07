using System.Net.WebSockets;
using generala.Services;

namespace generala.Network
{
    public class WebSocketNetwork
    {
        private readonly WebSocketHandler _handler;

        public WebSocketNetwork(WebSocketHandler handler)
        {
            _handler = handler;
        }

        public async Task HandleRequestAsync(WebSocket webSocket, int userId, string nickname) 
        {
           

            try
            {
                await _handler.HandleWebSocketAsync(webSocket, userId, nickname); 
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error en WebSocket: {ex.Message}");
            }
            finally
            {
            }
        }
    }
}
