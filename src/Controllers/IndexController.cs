using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using netnitel.Services;
using netnitel.Services.Minitel;

namespace netnitel.Controllers;

public class IndexController : ControllerBase
{

    [Route("/index")]
    public async Task Get()
    {
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await HandleIndexWebSocket(webSocket);
        }
        else
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }

    private async Task HandleIndexWebSocket(WebSocket webSocket)
    {
        var nitel = new NetNitel(webSocket);
        try
        {
            await nitel.Control.Home();
            await nitel.Control.ClearScreen();

            await nitel.DrawImage("image.png");
            
            while (webSocket.State == WebSocketState.Open)
            {
                var input = await nitel.Input(0, 1, 1, "", "", false);
            }

        }
        catch (WebSocketException e)
        {
            Console.WriteLine(e.Message);
        }

    }
} 