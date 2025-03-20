using System.Net.WebSockets;
using System.Text;

namespace netnitel.Services.Minitel;

public class NetNitel
{
    public MiniControle Control { get; }
    private readonly WebSocket _webSocket;
    
    public NetNitel(WebSocket webSocket)
    {
        _webSocket = webSocket;
        Control = new MiniControle(webSocket);
    }
    
    /// <summary>
    /// Efface l'écran et la ligne 0
    /// </summary>
    public async Task Reset()
    {
        await ClearFrom(0, 1);
        await Control.ClearScreen();
        await Control.Home();
        await Control.HideCursor();
    }

    /// <summary>
    /// Efface jusqu'à la fin de la ligne
    /// </summary>
    public async Task ClearFrom(int ligne, int colonne)
    {
        await Control.Move(ligne, colonne);
        await Control.Cancel();
    }

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
    public async Task<MiniInput> Input(
        int ligne,
        int colonne,
        int longueur,
        string data = "",
        string caractere = ".",
        bool redraw = true)
    {
        await Control.Move(ligne, colonne);
        
        if (redraw)
        {
            // Effacer la zone de saisie
            var sb = new StringBuilder();
            for (var i = 0; i < longueur; i++)
            {
                sb.Append(caractere);
            }
            await Print(sb.ToString());
            await Control.Move(ligne, colonne);
        }

        if (!string.IsNullOrEmpty(data))
        {
            await Print(data);
            await Control.Move(ligne, colonne + data.Length);
        }

        await Control.ShowCursor();

        var input = new StringBuilder(data);
        var finished = false;
        var key = MiniKey.None;

        while (!finished)
        {
            var buffer = new byte[1024];
            var result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            if (result.CloseStatus.HasValue)
            {
                throw new WebSocketException("WebSocket fermé");
            }

            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);

            var messageInput = MiniInput.Parse(message);

            switch (messageInput.Type)
            {
                case MiniInputType.Text:
                    input.Append(messageInput.Message);
                    await Print(messageInput.Message);
                    break;
                case MiniInputType.SpecialKey:
                    if (messageInput.SpecialKey == MiniKey.Correction && input.Length > 0)
                    {
                        input.Length--;
                        await Control.Move(ligne, colonne + input.Length);
                        await Print(caractere);
                        await Control.Move(ligne, colonne + input.Length);
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

        await Control.HideCursor();
        
        return new MiniInput { Message = input.ToString(), SpecialKey = key};
    }

    /// <summary>
    /// Affiche un message temporaire
    /// </summary>
    public async Task Message(int ligne, int colonne, int delai, string message, bool bip = false)
    {
        await Control.Move(ligne, colonne);
        await Print(message);
        
        if (bip)
        {
            await Control.Buzzer();
        }
        
        // Attendre le délai
        await Task.Delay(delai * 1000);
        
        // Effacer le message
        await Control.Move(ligne, colonne);
        var spaces = new string(' ', message.Length);
        await Print(spaces);
    }

    public async Task DrawImage(string imagePath)
    {
        await Control.Home();
    }
    
    /// <summary>
    /// Affiche du texte
    /// </summary>
    public async Task Print(string texte)
    {
        // Convertir les accents si nécessaire
        texte = ConvertAccents(texte);
        await Control.Write(texte);
    }

    /// <summary>
    /// Convertit les accents pour le Minitel
    /// </summary>
    private string ConvertAccents(string text)
    {
        // Conversion simple des accents pour l'affichage Minitel
        var sb = new StringBuilder(text.Length);
        foreach (var c in text)
        {
            switch (c)
            {
                case 'é': sb.Append("\u001BHe"); break;
                case 'è': sb.Append("\u001BIe"); break;
                case 'ê': sb.Append("\u001BJe"); break;
                case 'à': sb.Append("\u001BAa"); break;
                case 'â': sb.Append("\u001BBa"); break;
                case 'ç': sb.Append("\u001BKc"); break;
                case 'ù': sb.Append("\u001BUu"); break;
                case 'û': sb.Append("\u001BVu"); break;
                case 'ô': sb.Append("\u001BOu"); break;
                case 'î': sb.Append("\u001BLi"); break;
                case 'ï': sb.Append("\u001BMi"); break;
                case 'ë': sb.Append("\u001BNe"); break;
                case 'ü': sb.Append("\u001BWu"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }
} 