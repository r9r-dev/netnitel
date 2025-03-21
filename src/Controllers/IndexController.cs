using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
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

            await nitel.Move(12, 4);
            await nitel.Control.DoubleSizeText();
            await nitel.BackColor(MiniColor.Noir);
            await nitel.ForeColor(MiniColor.Blanc);
            await nitel.Print("Inutile");
            
            
            while (webSocket.State == WebSocketState.Open)
            {
                _ = await nitel.Input(0, 1, 1, "", "", false);
            }

        }
        catch (WebSocketException e)
        {
            Console.WriteLine(e.Message);
        }
        
    }
} 