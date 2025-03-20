using System.Net.WebSockets;

namespace netnitel.Services.Minitel;

/// <summary>
/// Contrôle le minitel
/// Documentation : https://millevaches.hydraule.org/info/minitel/specs/norme.htm
/// </summary>
public class MiniControle
{
    private readonly MiniRaw _miniRaw;
    private bool _graphicMode = false;

    public MiniControle(WebSocket webSocket)
    {
        _miniRaw = new MiniRaw(webSocket);
    }

    /// <summary>
    /// Écrire du texte
    /// </summary>
    /// <param name="text"></param>
    public async Task Write(string text)
    {
        if (_graphicMode) await TextMode();
        await _miniRaw.Send(text);
    }

    /// <summary>
    /// Bip sonore
    /// </summary>
    public async Task Buzzer()
    {
        await _miniRaw.SendChr(0x07);
    }

    /// <summary>
    /// Déplace le curseur à gauche
    /// </summary>
    public async Task MoveLeft()
    {
        await _miniRaw.SendChr(0x08);
    }
    
    /// <summary>
    /// Déplace le curseur à droite
    /// </summary>
    public async Task MoveRight()
    {
        await _miniRaw.SendChr(0x09);
    }
    
    /// <summary>
    /// Déplace le curseur en bas
    /// </summary>
    public async Task MoveDown()
    {
        await _miniRaw.SendChr(0x0A);
    }
    
    /// <summary>
    /// Déplace le curseur en haut
    /// </summary>
    public async Task MoveUp()
    {
        await _miniRaw.SendChr(0x0B);
    }

    /// <summary>
    /// 
    /// </summary>
    private async Task GraphicMode()
    {
        _graphicMode = true;
        await _miniRaw.SendChr(0x0E);
    }

    private async Task TextMode()
    {
        _graphicMode = false;
        await _miniRaw.SendChr(0x0F);
    }

    public async Task WriteGraphic(string character)
    {
        if (character.Length != 6) throw new ArgumentException("Le caractère doit être de 6 bits");
        await GraphicMode();
        // le texte est au format 123456 en binaire
        // Par exemple 010101
        // il faut inverser le sens du texte et le convertir en binaire
        var text = character.Reverse().Aggregate(0, (current, c) => current * 2 + (c == '1' ? 1 : 0));
        await _miniRaw.Send(32 + text);
    }
    
    /// <summary>
    /// Efface l'écran et Home
    /// </summary>
    public async Task ClearScreen()
    {
        await _miniRaw.SendChr(0x0C);
    }

    /// <summary>
    /// Change la couleur de fond du texte
    /// </summary>
    /// <param name="color"></param>
    public async Task BackColor(MiniColor color)
    {
        await _miniRaw.SendEsc(0x50 + (int)color);
    }

    /// <summary>
    /// Change la couleur du texte
    /// </summary>
    /// <param name="color"></param>
    public async Task ForeColor(MiniColor color)
    {
        await _miniRaw.SendEsc(0x40 + (int)color);
    }

    /// <summary>
    /// Retour chariot
    /// </summary>
    public async Task CarriageReturn()
    {
        await _miniRaw.SendChr(0x0D);
    }

    /// <summary>
    /// Passage dans le jeu semi-graphique (G1)
    /// </summary>
    public async Task SemiGraphicalMode()
    {
        await _miniRaw.SendChr(0x0E);
    }

    /// <summary>
    /// Passage dans le jeu normal (G0)
    /// </summary>
    public async Task NormalMode()
    {
        await _miniRaw.SendChr(0x0F);
    }

    /// <summary>
    /// Affiche le curseur
    /// </summary>
    public async Task ShowCursor()
    {
        await _miniRaw.SendChr(0x11);
    }
    
    /// <summary>
    /// Masque le curseur
    /// </summary>
    public async Task HideCursor()
    {
        await _miniRaw.SendChr(0x14);
    }
    
    /// <summary>
    /// Répétition du dernier caractère
    /// </summary>
    /// <param name="count">Valeur entre 1 et 64</param>
    public async Task Repeat(int count)
    {
        await _miniRaw.SendChr(0x12);
        await _miniRaw.SendChr(64 + count);
    }

    /// <summary>
    /// Efface la fin de la ligne
    /// </summary>
    public async Task Cancel()
    {
        await _miniRaw.SendChr(0x18);
    }

    /// <summary>
    /// Introduit un caractère G2 (accents, signes spéciaux...)
    /// </summary>
    public async Task InsertSpecialCharacter()
    {
        await _miniRaw.SendChr(0x19);
    }

    /// <summary>
    /// Introduit une séquence d'échappement
    /// </summary>
    public async Task EscapeSequence()
    {
        await _miniRaw.SendChr(0x1B);
    }

    /// <summary>
    /// Passage à la ligne suivante
    /// </summary>
    public async Task CRLF()
    {
        await CarriageReturn();
        await MoveDown();
    }

    /// <summary>
    /// Home (1ere ligne, 1ere colonne)
    /// </summary>
    public async Task Home()
    {
        await _miniRaw.SendChr(0x1E);
    }

    public async Task Move(int ligne, int colonne = 1)
    {
        _graphicMode = false;
        if (ligne == 1 && colonne == 1)
        {
            await Home();
        }
        else
        {
            await _miniRaw.SendChr(0x1F);
            await _miniRaw.SendChr(64 + ligne);
            await _miniRaw.SendChr(64 + colonne);
        }
        
    }
}