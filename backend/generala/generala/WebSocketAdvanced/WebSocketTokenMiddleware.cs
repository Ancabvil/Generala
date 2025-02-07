using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using generala.Network;

namespace generala.Middleware
{
    public class WebSocketTokenMiddleware
    {
        private readonly RequestDelegate _next;

        public WebSocketTokenMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, WebSocketNetwork wsNetwork)
        {

            if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
            {
                var token = context.Request.Query["token"];

                if (string.IsNullOrEmpty(token))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.CompleteAsync();
                    return;
                }

                var (userId, nickname) = GetUserIdAndNicknameFromToken(token);

                if (userId == null)
                {
                    context.Response.StatusCode = 401;
                    await context.Response.CompleteAsync();
                    return;
                }

                WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();

                await wsNetwork.HandleRequestAsync(webSocket, userId.Value, nickname);
                return;
            }
            await _next(context);
        }



        private (int?, string) GetUserIdAndNicknameFromToken(string token)
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    return (null, null);
                }

                var userIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");
                var nicknameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "nickname");

                if (userIdClaim == null)
                {
                    return (null, null);
                }

                int userId = int.Parse(userIdClaim.Value);
                string nickname = nicknameClaim?.Value ?? "Usuario";

                return (userId, nickname);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR al decodificar el token JWT: {ex.Message}");
                return (null, null);
            }
        }


    }
}
