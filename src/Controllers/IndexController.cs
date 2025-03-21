using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using NetNitel.Services.Applications;
using NetNitel.Services.Engine;

namespace NetNitel.Controllers;

public class IndexController : ControllerBase
{
    
    [Route("/index")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            var connectionId = Guid.NewGuid();
            Console.WriteLine($"[{connectionId}] Socket Opened");
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleIndexWebSocket(webSocket, connectionId);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task HandleIndexWebSocket(WebSocket webSocket, Guid connectionId)
    {
        var nitel = new Minitel(webSocket, connectionId);
        try
        {
            var home = new MiniHomeScreen(nitel);
            await home.StartAsync();
            while (webSocket.State == WebSocketState.Open)
            {
                await Task.Delay(50);
            }
            Console.WriteLine($"[{connectionId}] Socket Closed");
        }
        catch (WebSocketException e)
        {
            Console.WriteLine(e.Message);
        }
        
    }
} 