using System.Net.WebSockets;
using System.Text;

namespace netnitel.Services.Minitel;


/// <summary>
/// 
/// </summary>
/// <param name="webSocket"></param>
public class MiniRaw(WebSocket webSocket)
{
    /// <summary>
    /// Envoie un caractère ASCII
    /// </summary>
    public async Task SendChr(int ascii)
    {
        await Send(((char)ascii).ToString());
    }
    
    /// <summary>
    /// Envoie une séquence d'échappement
    /// </summary>
    public async Task SendEsc(string text)
    {
        await Send("\e" + text);
    }
    
    /// <summary>
    /// Envoie une séquence d'échappement
    /// </summary>
    public async Task SendEsc(int ascii)
    {
        await Send($"\e{(char)ascii}");
    }
    
    /// <summary>
    /// Envoie du texte au client
    /// </summary>
    public async Task Send(string text)
    {
        var buffer = Encoding.UTF8.GetBytes(text);
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async Task Send(byte[] buffer)
    {
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }

    public async Task Send(int charCode)
    {
        var buffer = new byte[] { (byte)charCode };
        await webSocket.SendAsync(
            new ArraySegment<byte>(buffer),
            WebSocketMessageType.Text,
            true,
            CancellationToken.None);
    }
}