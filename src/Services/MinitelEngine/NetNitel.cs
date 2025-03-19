using System.Net.WebSockets;
using System.Text;

namespace netnitel.Services.MinitelEngine;

public class NetNitel(WebSocket webSocket)
{
    /// <summary>
    /// Efface l'écran et la ligne 0
    /// </summary>
    public async Task Home()
    {
        await Del(0, 1);
        await SendChr(12); // FF
        await Cursor(false); // Coff
    }

    /// <summary>
    /// Positionne le curseur au début d'une ligne
    /// </summary>
    public async Task VTab(int ligne)
    {
        await MoveTo(ligne);
    }

    /// <summary>
    /// Positionne le curseur à une ligne/colonne
    /// </summary>
    public async Task MoveTo(int ligne, int colonne = 1)
    {
        if (ligne == 1 && colonne == 1)
        {
            await SendChr(30);
        }
        else
        {
            await SendChr(31);
            await SendChr(64 + ligne);
            await SendChr(64 + colonne);
        }
    }

    /// <summary>
    /// Efface jusqu'à la fin de la ligne
    /// </summary>
    public async Task Del(int ligne, int colonne)
    {
        await MoveTo(ligne, colonne);
        await SendChr(24);
    }

    /// <summary>
    /// Passe à la vidéo normale
    /// </summary>
    public async Task Normal()
    {
        await SendEsc("I");
    }

    /// <summary>
    /// Change la couleur de fond
    /// </summary>
    public async Task BackColor(MiniColor couleur)
    {
        await SendEsc(char.ConvertFromUtf32(80 + (int)couleur));
    }

    /// <summary>
    /// Efface un bloc à l'écran
    /// </summary>
    public async Task CanBlock(int debut, int fin, int colonne, bool inverse = false)
    {
        if (!inverse)
        {
            await MoveTo(debut, colonne);
            await SendChr(24);
            for (var ligne = debut; ligne < fin; ligne++)
            {
                await SendChr(10);
                await SendChr(24);
            }
        }
        else
        {
            for (var ligne = debut; ligne < fin; ligne++)
            {
                await MoveTo(ligne, colonne);
                await Print(" ");
            }
        }
    }

    /// <summary>
    /// Efface jusqu'à la fin de la ligne
    /// </summary>
    public async Task CanEol(int ligne, int colonne)
    {
        await MoveTo(ligne, colonne);
        await SendChr(24);
    }

    /// <summary>
    /// Efface l'écran
    /// </summary>
    public async Task ClearScreen()
    {
        await SendChr(12);
    }

    /// <summary>
    /// Change la couleur d'avant-plan
    /// </summary>
    public async Task Color(MiniColor couleur)
    {
        await SendEsc(char.ConvertFromUtf32(64 + (int)couleur));
    }

    /// <summary>
    /// Active/désactive le curseur
    /// </summary>
    public async Task Cursor(bool visible)
    {
        if (visible)
        {
            await SendChr(17);
        }
        else
        {
            await SendChr(20);
        }
    }

    /// <summary>
    /// Active/désactive le clignotement
    /// </summary>
    public async Task Flash(bool clignote = true)
    {
        if (clignote)
        {
            await SendEsc("H");
        }
        else
        {
            await SendEsc("I");
        }
    }

    /// <summary>
    /// Attend une entrée clavier
    /// </summary>
    public async Task<MiniInput> Input(
        int ligne,
        int colonne,
        int longueur,
        string data = "",
        string caractere = ".",
        bool redraw = true)
    {
        await MoveTo(ligne, colonne);
        
        if (redraw)
        {
            // Effacer la zone de saisie
            var sb = new StringBuilder();
            for (var i = 0; i < longueur; i++)
            {
                sb.Append(caractere);
            }
            await Print(sb.ToString());
            await MoveTo(ligne, colonne);
        }

        if (!string.IsNullOrEmpty(data))
        {
            await Print(data);
            await MoveTo(ligne, colonne + data.Length);
        }

        await Cursor(true);

        var input = new StringBuilder(data);
        var finished = false;
        var key = MiniKey.None;

        while (!finished)
        {
            var buffer = new byte[1024];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
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
                        await MoveTo(ligne, colonne + input.Length);
                        await Print(caractere);
                        await MoveTo(ligne, colonne + input.Length);
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

        await Cursor(false);
        
        return new MiniInput { Message = input.ToString(), SpecialKey = key};
    }

    /// <summary>
    /// Active/désactive la vidéo inversée
    /// </summary>
    public async Task Inverse(bool inverse = true)
    {
        if (inverse)
        {
            await SendEsc("X");
        }
        else
        {
            await SendEsc("Y");
        }
    }

    /// <summary>
    /// Positionne le curseur sans mise à jour visuelle
    /// </summary>
    public async Task Locate(int ligne, int colonne)
    {
        await MoveTo(ligne, colonne);
    }

    /// <summary>
    /// Affiche un message temporaire
    /// </summary>
    public async Task Message(int ligne, int colonne, int delai, string message, bool bip = false)
    {
        await MoveTo(ligne, colonne);
        await Print(message);
        
        if (bip)
        {
            await Bip();
        }
        
        // Attendre le délai
        await Task.Delay(delai * 1000);
        
        // Effacer le message
        await MoveTo(ligne, colonne);
        var spaces = new string(' ', message.Length);
        await Print(spaces);
    }

    /// <summary>
    /// Active/désactive le soulignement
    /// </summary>
    public async Task Underline(bool souligne = true)
    {
        if (souligne)
        {
            await SendEsc("Z");
        }
        else
        {
            await SendEsc("[");
        }
    }

    /// <summary>
    /// Affiche du texte
    /// </summary>
    public async Task Print(string texte)
    {
        // Convertir les accents si nécessaire
        texte = ConvertAccents(texte);
        await Send(texte);
    }

    /// <summary>
    /// Envoie du texte au client
    /// </summary>
    private async Task Send(string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    /// <summary>
    /// Envoie un caractère ASCII
    /// </summary>
    private async Task SendChr(int ascii)
    {
        await Send(((char)ascii).ToString());
    }

    /// <summary>
    /// Envoie une séquence d'échappement
    /// </summary>
    private async Task SendEsc(string text)
    {
        await Send("\u001B" + text);
    }

    /// <summary>
    /// Émet un bip sonore
    /// </summary>
    public async Task Bip()
    {
        await SendChr(7);
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