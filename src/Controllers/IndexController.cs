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
        var m = new NetNitel(webSocket);

        await m.Home();
        await m.ClearScreen();

        // Afficher un cadre de 40x23
        await m.MoveTo(1, 1);
        await m.Color(MiniColor.Blanc);
        await m.BackColor(MiniColor.Bleu);
        for (var i = 0; i < 40; i++)
        {
            await m.Print("▮");
        }

        await m.MoveTo(23, 1);
        for (var i = 0; i < 40; i++)
        {
            await m.Print("▮");
        }

        for (var i = 0; i < 23; i++)
        {
            await m.MoveTo(i + 1, 1);
            await m.Print("▮");
        }

        for (var i = 0; i < 23; i++)
        {
            await m.MoveTo(i + 1, 40);
            await m.Print("▮");
        }

        await m.MoveTo(0, 1);
        await m.Print("CX 0,05E puis 0,39E/min");
        
        
        
        
        // Test
        await m.MoveTo(3, 3);
        await m.Print("Valider code du service");
        await m.MoveTo(3, 31);
        await m.BackColor(MiniColor.Vert);
        await m.Color(MiniColor.Noir);
        await m.Print(" Envoi ");

    }
} 