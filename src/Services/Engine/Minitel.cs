using System.Net.WebSockets;
using System.Text;
using NetNitel.Services.Engine.Actions;
using NetNitel.Services.Engine.Fluent;
using NetNitel.Services.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NetNitel.Services.Engine;

public class Minitel
{
    public MiniControl Control { get; }
    
    /// <summary>
    /// Lecture des entrées utilisateur
    /// </summary>
    public MiniReader Read { get; }
    
    /// <summary>
    /// Écriture de texte
    /// </summary>
    public MiniWriter Write { get; }

    /// <summary>
    /// Dessin / Graphisme
    /// </summary>
    public MiniDrawer Draw { get; }

    public MiniBackColorFluent BlackBG
    {
        get
        {
            var action = new SetBackColorBlack(this);
            var sequencer = new MiniSequencer();
            sequencer.Add(action);
            var fluent = new MiniBackColorFluent(sequencer);
            return fluent;
        }
    }

    private readonly WebSocket _webSocket;
    private Guid _connectionId;
    
    public Minitel(WebSocket webSocket, Guid connectionId)
    {
        _connectionId = connectionId;
        _webSocket = webSocket;
        Control = new MiniControl(webSocket);
        Write = new MiniWriter(this);
        Read = new MiniReader(this, webSocket);
        Draw = new MiniDrawer(this);
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
} 