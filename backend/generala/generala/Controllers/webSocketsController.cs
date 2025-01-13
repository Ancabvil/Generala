using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace generala.Controllers
{
    [ApiController]
    [Route("/ws")]
    public class webSocketsController : ControllerBase
    {
        public async Task get()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using webSocketsController websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await ProcessWebSocket(websocket);
            }
            else {
                HttpContext.Response.StatusCode = StatusCode.Status400BadRequest;
        }
    }
}
