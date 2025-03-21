using System.Net.WebSockets;
using System.Text;
using NetNitel.Services.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NetNitel.Services.Engine;

public class Minitel
{
    public MiniControl Control { get; }
    private readonly WebSocket _webSocket;
    private readonly ImageService _imageService;
    private Guid _connectionId;
    
    public Minitel(WebSocket webSocket, Guid connectionId)
    {
        _connectionId = connectionId;
        _webSocket = webSocket;
        Control = new MiniControl(webSocket);
        _imageService = new ImageService();
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
    
    public async Task Move(int ligne, int colonne)
    {
        if (ligne > 24) ligne = 24;
        if (colonne > 40) colonne = 40;
        await Control.Move(ligne, colonne);
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

    public async Task DrawText(string text, int ligne = 1, int colonne = 1, MiniColor foreColor = MiniColor.Noir, MiniColor backColor = MiniColor.Blanc)
    {
        var letters = new ImageTextWriter().GetImageLetters(text);
        var image = await _imageService.CombineImages(letters);
        
        using var rgba32Image = Image.Load<Rgba32>(image);
        
        // Déterminer les dimensions de l'image en blocs
        var blocksWidth = rgba32Image.Width / 2;
        var blocksHeight = rgba32Image.Height / 3;
        
        // Pour chaque ligne de blocs
        for (var blockY = 0; blockY < blocksHeight; blockY++)
        {
            // Déplacer le curseur au début de la ligne
            await Control.Move(ligne + blockY, colonne);
            
            // Pour chaque bloc dans la ligne
            for (var blockX = 0; blockX < blocksWidth; blockX++)
            {
                // Calculer la position du bloc dans l'image
                var pixelX = blockX * 2;
                var pixelY = blockY * 3;
                
                
                await Control.ForeColor(foreColor);
                await Control.BackColor(backColor);
                
                var color1 = new Rgba32(0, 0, 0);
                var color2 = new Rgba32(255, 255, 255);
                
                // Lire les 6 pixels du bloc
                var pixels = new bool[6];
                for (var y = 0; y < 3; y++)
                {
                    for (var x = 0; x < 2; x++)
                    {
                        var color = rgba32Image[pixelX + x, pixelY + y];
                        // Si la couleur est plus proche de la première couleur dominante, c'est un pixel positif
                        pixels[y * 2 + x] = IsPositivePixel(color, color1, color2);
                    }
                }
                
                // Convertir les pixels en chaîne de 0 et 1
                var pixelString = string.Join("", pixels.Select(p => p ? "1" : "0"));
                
                // Écrire le bloc (le curseur avance automatiquement)
                await Control.WriteGraphic(pixelString);
            }
        }
    }

    /// <summary>
    /// Remplit l'écran avec une image en 80x72.
    /// </summary>
    /// <param name="imagePath"></param>
    public async Task DrawImage(string imagePath)
    {
        // Placer le curseur en haut à gauche
        await Control.Home();

        // Traiter l'image pour obtenir une version 80x72 en 8 couleurs
        var image = await File.ReadAllBytesAsync(imagePath);
        var miniImage = await _imageService.ConvertTo8Colors(image);
        
        // Charger l'image traitée
        using var rgba32Image = Image.Load<Rgba32>(miniImage);
        
        // Pour chaque ligne de 24 blocs
        for (var line = 0; line < 24; line++)
        {
            // Pour chaque bloc de 2x3 dans la ligne
            for (var block = 0; block < 40; block++)
            {
                // Calculer la position du bloc dans l'image
                var blockX = block * 2;
                var blockY = line * 3;

                // Définir les couleurs pour ce bloc
                var (color1, color2) = GetBlockColors(rgba32Image, blockX, blockY);
                await Control.ForeColor(ConvertToMiniColor(color1));
                await Control.BackColor(ConvertToMiniColor(color2));

                // Lire les 6 pixels du bloc
                var pixels = new bool[6];
                for (var y = 0; y < 3; y++)
                {
                    for (var x = 0; x < 2; x++)
                    {
                        var color = rgba32Image[blockX + x, blockY + y];
                        // Si la couleur est plus proche de la première couleur dominante, c'est un pixel positif
                        pixels[y * 2 + x] = IsPositivePixel(color, color1, color2);
                    }
                }

                // Convertir les pixels en chaîne de 0 et 1
                var pixelString = string.Join("", pixels.Select(p => p ? "1" : "0"));

                // Écrire le bloc
                await Control.WriteGraphic(pixelString);
            }
        }
    }

    private bool IsPositivePixel(Rgba32 color, Rgba32 color1, Rgba32 color2)
    {
        // Calculer la distance entre la couleur du pixel et les deux couleurs dominantes
        var distance1 = CalculateColorDistance(color, color1);
        var distance2 = CalculateColorDistance(color, color2);
        
        // Si plus proche de la première couleur dominante, c'est un pixel positif
        return distance1 <= distance2;
    }

    private (Rgba32 color1, Rgba32 color2) GetBlockColors(Image<Rgba32> image, int blockX, int blockY)
    {
        // Collecter toutes les couleurs du bloc
        var colors = new List<Rgba32>();
        for (var y = blockY; y < blockY + 3; y++)
        {
            for (var x = blockX; x < blockX + 2; x++)
            {
                colors.Add(image[x, y]);
            }
        }

        // Trouver les deux couleurs dominantes
        var avgColor = CalculateAverageColor(colors);
        var distances = ImageService.Palette
            .Select(color => (color, distance: CalculateColorDistance(avgColor, color)))
            .OrderBy(x => x.distance)
            .Take(2)
            .ToList();

        return (distances[0].color, distances[1].color);
    }

    private Rgba32 CalculateAverageColor(List<Rgba32> colors)
    {
        if (!colors.Any()) return new Rgba32(0, 0, 0);

        var r = (byte)colors.Average(c => c.R);
        var g = (byte)colors.Average(c => c.G);
        var b = (byte)colors.Average(c => c.B);

        return new Rgba32(r, g, b);
    }

    private double CalculateColorDistance(Rgba32 color1, Rgba32 color2)
    {
        // Utilisation de la distance euclidienne dans l'espace RGB
        var rDiff = color1.R - color2.R;
        var gDiff = color1.G - color2.G;
        var bDiff = color1.B - color2.B;
        
        return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }

    private MiniColor ConvertToMiniColor(Rgba32 color)
    {
        // Trouver l'index de la couleur dans la palette
        for (var i = 0; i < ImageService.Palette.Length; i++)
        {
            if (ImageService.Palette[i] == color)
            {
                // Convertir l'index en MiniColor (les valeurs correspondent)
                return (MiniColor)i;
            }
        }
        
        // Si la couleur n'est pas trouvée, trouver la plus proche
        var minDistance = double.MaxValue;
        var closestIndex = 0;
        
        for (var i = 0; i < ImageService.Palette.Length; i++)
        {
            var distance = CalculateColorDistance(color, ImageService.Palette[i]);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        
        return (MiniColor)closestIndex;
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
                case 'é': sb.Append("\eHe"); break;
                case 'è': sb.Append("\eIe"); break;
                case 'ê': sb.Append("\eJe"); break;
                case 'à': sb.Append("\eAa"); break;
                case 'â': sb.Append("\eBa"); break;
                case 'ç': sb.Append("\eKc"); break;
                case 'ù': sb.Append("\eUu"); break;
                case 'û': sb.Append("\eVu"); break;
                case 'ô': sb.Append("\eOu"); break;
                case 'î': sb.Append("\eLi"); break;
                case 'ï': sb.Append("\eMi"); break;
                case 'ë': sb.Append("\eNe"); break;
                case 'ü': sb.Append("\eWu"); break;
                default: sb.Append(c); break;
            }
        }
        return sb.ToString();
    }

    public async Task BackColor(MiniColor color)
    {
        await Control.BackColor(color);
    }

    public async Task ForeColor(MiniColor color)
    {
        await Control.ForeColor(color);
    }
} 