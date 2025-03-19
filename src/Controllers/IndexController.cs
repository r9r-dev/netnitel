using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using netnitel.Services;
using netnitel.Services.MinitelEngine;

namespace netnitel.Controllers;

public class IndexController : ControllerBase
{
    private readonly MinitelService _minitelService;

    public IndexController(MinitelService minitelService)
    {
        _minitelService = minitelService;
    }

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
        var pynitel = new NetNitel(webSocket);

        // Afficher la page d'accueil
        await DisplayWelcome();

        // Gérer l'entrée utilisateur
        var buffer = new byte[1024 * 4];
        try
        {
            while (webSocket.State == WebSocketState.Open)
            {
                var input = await pynitel.Input(20, 20, 1, "", " ", false);

                if (input.SpecialKey == MiniKey.Envoi && string.IsNullOrEmpty(input.Message))
                {
                    // Afficher un message si ENVOI est pressé sans sélection
                    await pynitel.Message(0, 1, 3, "Veuillez choisir un service (1-2)");
                }
                else if (input.Message == "1")
                {
                    // Rediriger vers le service annuaire
                    await pynitel.ClearScreen();
                    await _minitelService.Annuaire(pynitel);
                    await DisplayWelcome();
                }
                else if (input.Message == "2")
                {
                    // Rediriger vers le jeu snake
                    await pynitel.ClearScreen();
                    await _minitelService.Snake(pynitel);
                    await DisplayWelcome();
                }
            }
        }
        catch (WebSocketException)
        {
            // Gérer la déconnexion
        }

        return;

        // Fonction pour afficher la page d'accueil
        async Task DisplayWelcome()
        {
            await pynitel.Home();
            await pynitel.ClearScreen();

            // Afficher le titre
            await pynitel.MoveTo(2, 15);
            await pynitel.Color(MiniColor.Blanc);
            await pynitel.BackColor(MiniColor.Bleu);
            await pynitel.Print(" 3615 SYMPA ");
            await pynitel.Normal();

            // Afficher le menu
            await pynitel.MoveTo(6, 10);
            await pynitel.Print("1 - Annuaire");

            await pynitel.MoveTo(8, 10);
            await pynitel.Print("2 - Snake");

            // Instructions
            await pynitel.MoveTo(12, 5);
            await pynitel.Print("Choisissez un service (1-2)");

            await pynitel.MoveTo(14, 5);
            await pynitel.Print("ou appuyez sur ENVOI");
        }
    }
} 