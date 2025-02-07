using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace generala.Controllers
{
    [Route("socket")]
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        [HttpGet]
        public async Task ConnectAsync()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleWebsocketAsync(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task HandleWebsocketAsync(WebSocket webSocket)
        {
            
            var buffer = new byte[4096];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var receiveTask = webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30)); // Espera 30 segundos

                    var completedTask = await Task.WhenAny(receiveTask, timeoutTask);

                    if (completedTask == timeoutTask)
                    {
                        
                        byte[] pingMessage = Encoding.UTF8.GetBytes("ping");
                        await webSocket.SendAsync(new ArraySegment<byte>(pingMessage), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    else
                    {
                        WebSocketReceiveResult result = await receiveTask;

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by client", CancellationToken.None);
                            break;
                        }

                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);

                        byte[] responseMessage = Encoding.UTF8.GetBytes($"Echo: {message}");
                        await webSocket.SendAsync(new ArraySegment<byte>(responseMessage), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error en WebSocket: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("WebSocket cerrado.");
            }
        }
    }
}
