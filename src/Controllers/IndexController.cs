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

            // Afficher un cadre de 40x23
            await nitel.Control.Move(1, 2);
            await nitel.Control.WriteGraphic("110000");
            await nitel.Control.Repeat(37);

            await nitel.Control.Move(23, 2);
            await nitel.Control.WriteGraphic("000011");
            await nitel.Control.Repeat(37);
            
            for (var i = 0; i < 23; i++)
            {
                await nitel.Control.Move(i + 1, 1);
                await nitel.Control.WriteGraphic("101010");
            }

            for (var i = 0; i < 23; i++)
            {
                await nitel.Control.Move(i + 1, 40);
                await nitel.Control.WriteGraphic("010101");
            }
            
            await nitel.Control.Move(1, 1);
            await nitel.Control.WriteGraphic("111010");
            await nitel.Control.Move(23, 1);
            await nitel.Control.WriteGraphic("101011");
            await nitel.Control.Move(1, 40);
            await nitel.Control.WriteGraphic("110101");
            await nitel.Control.Move(23, 40);
            await nitel.Control.WriteGraphic("010111");

            while (webSocket.State == WebSocketState.Open)
            {
                var input = await nitel.Input(22, 3, 36, "Youpi : ");
            }

        }
        catch (WebSocketException e)
        {
            Console.WriteLine(e.Message);
        }

    }
} 