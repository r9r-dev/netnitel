using System.Net.WebSockets;
using System.Text;
using NetNitel.Services.Engine.Enums;

namespace NetNitel.Services.Engine;

public class MiniReader(Minitel tel, WebSocket socket)
{
    /// <summary>
    /// Attend une entrée clavier et retourne les informations
    /// </summary>
    /// <param name="ligne">Ligne ou positionner le curseur</param>
    /// <param name="colonne">Colonne ou positionner le curseur</param>
    /// <param name="longueur">Longueur du message</param>
    /// <param name="data">Prompt à afficher</param>
    /// <param name="caractere">Caractères de la ligne</param>
    /// <param name="redraw">Effacer la zone avant d'afficher le prompt</param>
    /// <returns></returns>
    /// <exception cref="WebSocketException"></exception>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public async Task<MiniInput> TextBox(
        int ligne,
        int colonne,
        int longueur,
        string data = "",
        string caractere = ".",
        bool redraw = true)
    {
        await tel.Control.Move(ligne, colonne);
        
        if (redraw)
        {
            // Effacer la zone de saisie
            var sb = new StringBuilder();
            for (var i = 0; i < longueur; i++)
            {
                sb.Append(caractere);
            }
            await tel.Write.Text(sb.ToString());
            await tel.Control.Move(ligne, colonne);
        }

        if (!string.IsNullOrEmpty(data))
        {
            await tel.Write.Text(data);
            await tel.Control.Move(ligne, colonne + data.Length);
        }

        await tel.Control.ShowCursor();

        var input = new StringBuilder(data);
        var finished = false;
        var key = MiniKey.None;

        while (!finished)
        {
            var messageInput = await Read();

            switch (messageInput.Type)
            {
                case MiniInputType.Text:
                    input.Append(messageInput.Message);
                    await tel.Write.Text(messageInput.Message);
                    break;
                case MiniInputType.SpecialKey:
                    if (messageInput.SpecialKey == MiniKey.Correction && input.Length > 0)
                    {
                        input.Length--;
                        await tel.Control.Move(ligne, colonne + input.Length);
                        await tel.Write.Text(caractere);
                        await tel.Control.Move(ligne, colonne + input.Length);
                    }
                    else
                    {
                        finished = true;
                    }
                    break;
                case MiniInputType.None:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        await tel.Control.HideCursor();
        
        return new MiniInput { Message = input.ToString(), SpecialKey = key};
    }

    public async Task<MiniInput> Key()
    {
        await tel.Control.HideCursor();
        return await Read();
    }

    private async Task<MiniInput> Read()
    {
        var buffer = new byte[1024];
        var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
        if (result.CloseStatus.HasValue)
        {
            throw new WebSocketException("WebSocket fermé");
        }

        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

        return MiniInput.Parse(message);
    }
}