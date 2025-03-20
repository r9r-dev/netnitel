using System.Drawing;
using System.Drawing.Imaging;

namespace netnitel.Services.Images;

public class ImageService
{
    private const int BLOCK_WIDTH = 2;
    private const int BLOCK_HEIGHT = 3;
    private const int IMAGE_WIDTH = 80;
    private const int IMAGE_HEIGHT = 72;

    private static readonly Color[] Palette = new[]
    {
        Color.Black,    // Noir
        Color.Red,      // Rouge
        Color.Green,    // Vert
        Color.Yellow,   // Jaune
        Color.Blue,     // Bleu
        Color.Magenta,  // Magenta
        Color.Cyan,     // Cyan
        Color.White     // Blanc
    };

    public byte[] ResizeImageToMaxDimensions(byte[] imageData)
    {
        using var originalImage = Image.FromStream(new MemoryStream(imageData));
        
        // Créer une nouvelle image avec les dimensions exactes requises
        using var resizedImage = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT);
        using var graphics = Graphics.FromImage(resizedImage);
        
        // Configurer la qualité du redimensionnement
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        
        // Calculer le ratio d'aspect pour maintenir les proportions
        var ratioX = (double)IMAGE_WIDTH / originalImage.Width;
        var ratioY = (double)IMAGE_HEIGHT / originalImage.Height;
        var ratio = Math.Min(ratioX, ratioY);
        
        var newWidth = (int)(originalImage.Width * ratio);
        var newHeight = (int)(originalImage.Height * ratio);
        
        // Calculer la position pour centrer l'image
        var x = (IMAGE_WIDTH - newWidth) / 2;
        var y = (IMAGE_HEIGHT - newHeight) / 2;
        
        // Dessiner l'image redimensionnée au centre
        graphics.DrawImage(originalImage, x, y, newWidth, newHeight);
        
        // Convertir en PNG
        using var ms = new MemoryStream();
        resizedImage.Save(ms, ImageFormat.Png);
        return ms.ToArray();
    }

    public byte[] ConvertTo8Colors(byte[] imageData)
    {
        // D'abord redimensionner l'image
        var resizedImageData = ResizeImageToMaxDimensions(imageData);
        
        // Créer une nouvelle image avec la taille exacte requise
        using var sourceBitmap = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT);
        using (var ms = new MemoryStream(resizedImageData))
        {
            using var tempImage = new Bitmap(ms);
            Console.WriteLine($"Dimensions de l'image temporaire: {tempImage.Width}x{tempImage.Height}");
            
            using var graphics = Graphics.FromImage(sourceBitmap);
            graphics.DrawImage(tempImage, 0, 0, IMAGE_WIDTH, IMAGE_HEIGHT);
        }
        
        Console.WriteLine($"Dimensions de l'image source: {sourceBitmap.Width}x{sourceBitmap.Height}");
        
        // Créer l'image de résultat
        using var resultBitmap = new Bitmap(IMAGE_WIDTH, IMAGE_HEIGHT);
        
        // Vérifier que les dimensions sont correctes
        if (sourceBitmap.Width != IMAGE_WIDTH || sourceBitmap.Height != IMAGE_HEIGHT)
        {
            throw new InvalidOperationException($"L'image source n'a pas les bonnes dimensions. Attendu: {IMAGE_WIDTH}x{IMAGE_HEIGHT}, Obtenu: {sourceBitmap.Width}x{sourceBitmap.Height}");
        }
        
        // Traiter chaque bloc de 2x3
        for (int blockY = 0; blockY < IMAGE_HEIGHT - BLOCK_HEIGHT + 1; blockY += BLOCK_HEIGHT)
        {
            for (int blockX = 0; blockX < IMAGE_WIDTH - BLOCK_WIDTH + 1; blockX += BLOCK_WIDTH)
            {
                try
                {
                    ProcessBlock(sourceBitmap, resultBitmap, blockX, blockY);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors du traitement du bloc à ({blockX}, {blockY}): {ex.Message}");
                    throw;
                }
            }
        }
        
        // Sauvegarder en PNG
        using var outputMs = new MemoryStream();
        resultBitmap.Save(outputMs, ImageFormat.Png);
        return outputMs.ToArray();
    }

    private void ProcessBlock(Bitmap source, Bitmap target, int blockX, int blockY)
    {
        // Vérifier les limites du bloc
        if (blockX + BLOCK_WIDTH > source.Width || blockY + BLOCK_HEIGHT > source.Height)
        {
            throw new ArgumentOutOfRangeException($"Le bloc ({blockX}, {blockY}) dépasse les limites de l'image {source.Width}x{source.Height}");
        }

        // Collecter toutes les couleurs du bloc
        var colors = new List<Color>();
        for (int y = blockY; y < blockY + BLOCK_HEIGHT; y++)
        {
            for (int x = blockX; x < blockX + BLOCK_WIDTH; x++)
            {
                try
                {
                    colors.Add(source.GetPixel(x, y));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de la lecture du pixel ({x}, {y}): {ex.Message}");
                    throw;
                }
            }
        }

        // Trouver les deux couleurs dominantes
        var (color1, color2) = FindDominantColors(colors);

        // Appliquer les couleurs au bloc
        for (int y = blockY; y < blockY + BLOCK_HEIGHT; y++)
        {
            for (int x = blockX; x < blockX + BLOCK_WIDTH; x++)
            {
                try
                {
                    var originalColor = source.GetPixel(x, y);
                    var newColor = FindClosestColor(originalColor, new[] { color1, color2 });
                    target.SetPixel(x, y, newColor);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'écriture du pixel ({x}, {y}): {ex.Message}");
                    throw;
                }
            }
        }
    }

    private (Color color1, Color color2) FindDominantColors(List<Color> colors)
    {
        // Calculer la moyenne des couleurs
        var avgColor = CalculateAverageColor(colors);
        
        // Trouver les deux couleurs de la palette les plus proches de la moyenne
        var distances = Palette
            .Select(color => (color, distance: CalculateColorDistance(avgColor, color)))
            .OrderBy(x => x.distance)
            .Take(2)
            .ToList();

        return (distances[0].color, distances[1].color);
    }

    private Color CalculateAverageColor(List<Color> colors)
    {
        if (!colors.Any()) return Color.Black;

        var r = colors.Average(c => c.R);
        var g = colors.Average(c => c.G);
        var b = colors.Average(c => c.B);

        return Color.FromArgb((int)r, (int)g, (int)b);
    }

    private Color FindClosestColor(Color color, Color[] allowedColors)
    {
        var minDistance = double.MaxValue;
        var closestColor = allowedColors[0];

        foreach (var allowedColor in allowedColors)
        {
            var distance = CalculateColorDistance(color, allowedColor);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestColor = allowedColor;
            }
        }

        return closestColor;
    }

    private static double CalculateColorDistance(Color color1, Color color2)
    {
        // Utilisation de la distance euclidienne dans l'espace RGB
        var rDiff = color1.R - color2.R;
        var gDiff = color1.G - color2.G;
        var bDiff = color1.B - color2.B;
        
        return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }

    public void ProcessImageFile(string inputPath, string outputPath)
    {
        // Lire le fichier PNG en bytes
        byte[] imageBytes = File.ReadAllBytes(inputPath);
        
        // Traiter l'image
        byte[] processedImage = ConvertTo8Colors(imageBytes);
        
        // Sauvegarder le résultat
        File.WriteAllBytes(outputPath, processedImage);
    }

    public void ResizeImageFile(string inputPath, string outputPath)
    {
        // Lire le fichier PNG en bytes
        byte[] imageBytes = File.ReadAllBytes(inputPath);
        
        // Redimensionner l'image
        byte[] resizedImage = ResizeImageToMaxDimensions(imageBytes);
        
        // Sauvegarder le résultat
        File.WriteAllBytes(outputPath, resizedImage);
    }
}