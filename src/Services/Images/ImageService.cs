using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Drawing.Processing;

namespace NetNitel.Services.Images;

public class ImageService
{
    private const int BLOCK_WIDTH = 2;
    private const int BLOCK_HEIGHT = 3;
    private const int IMAGE_WIDTH = 80;
    private const int IMAGE_HEIGHT = 72;

    public static readonly Rgba32[] Palette =
    [
        new(0, 0, 0),       // Noir
        new(255, 0, 0),     // Rouge
        new(0, 255, 0),     // Vert
        new(255, 255, 0),   // Jaune
        new(0, 0, 255),     // Bleu
        new(255, 0, 255),   // Magenta
        new(0, 255, 255),   // Cyan
        new(255, 255, 255)  // Blanc
    ];

    /// <summary>
    /// Redimensionne une image en conservant les proportions et en la centrant vers le format Minitel (80x72)
    /// </summary>
    /// <param name="imageData"></param>
    /// <returns></returns>
    private byte[] ResizeImageToMaxDimensions(byte[] imageData)
    {
        using var image = Image.Load<Rgba32>(imageData);
        
        Console.WriteLine($"Resizing image to {IMAGE_WIDTH}x{IMAGE_HEIGHT}");
        Console.WriteLine($"Source image is {image.Width}x{image.Height}");
        
        // Calculer le ratio d'aspect pour maintenir les proportions
        var ratioX = (double)IMAGE_WIDTH / image.Width;
        var ratioY = (double)IMAGE_HEIGHT / image.Height;
        var ratio = Math.Min(ratioX, ratioY);
        
        var newWidth = (int)(image.Width * ratio);
        var newHeight = (int)(image.Height * ratio);
        
        // Calculer la position pour centrer l'image
        var x = (IMAGE_WIDTH - newWidth) / 2;
        var y = (IMAGE_HEIGHT - newHeight) / 2;
        
        // Créer une nouvelle image avec les dimensions exactes requises
        using var resizedImage = new Image<Rgba32>(IMAGE_WIDTH, IMAGE_HEIGHT);
        
        // Redimensionner l'image source
        image.Mutate(ctx => ctx.Resize(newWidth, newHeight));
        
        // Dessiner l'image redimensionnée au centre
        resizedImage.Mutate(ctx => ctx.DrawImage(image, new Point(x, y), 1f));
        
        // Convertir en PNG
        using var ms = new MemoryStream();
        resizedImage.SaveAsPng(ms);
        return ms.ToArray();
    }
    
    /// <summary>
    /// Convertis une image en 8 couleurs en utilisant la palette Minitel
    /// L'image est redimensionnée en 80x72
    /// </summary>
    /// <param name="imageData"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<byte[]> ConvertTo8Colors(byte[] imageData)
    {
        // D'abord redimensionner l'image
        var resizedImageData = ResizeImageToMaxDimensions(imageData);
        
        // Créer une nouvelle image avec la taille exacte requise
        using var sourceImage = Image.Load<Rgba32>(resizedImageData);
        
        // Créer l'image de résultat
        using var resultImage = new Image<Rgba32>(IMAGE_WIDTH, IMAGE_HEIGHT);
        
        // Vérifier que les dimensions sont correctes
        if (sourceImage.Width != IMAGE_WIDTH || sourceImage.Height != IMAGE_HEIGHT)
        {
            throw new InvalidOperationException($"L'image source n'a pas les bonnes dimensions. Attendu: {IMAGE_WIDTH}x{IMAGE_HEIGHT}, Obtenu: {sourceImage.Width}x{sourceImage.Height}");
        }
        
        // Traiter chaque bloc de 2x3
        for (var blockY = 0; blockY < IMAGE_HEIGHT - BLOCK_HEIGHT + 1; blockY += BLOCK_HEIGHT)
        {
            for (var blockX = 0; blockX < IMAGE_WIDTH - BLOCK_WIDTH + 1; blockX += BLOCK_WIDTH)
            {
                try
                {
                    ProcessBlock(sourceImage, resultImage, blockX, blockY);
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
        await resultImage.SaveAsPngAsync(outputMs);
        return outputMs.ToArray();
    }

    private void ProcessBlock(Image<Rgba32> source, Image<Rgba32> target, int blockX, int blockY)
    {
        // Vérifier les limites du bloc
        if (blockX + BLOCK_WIDTH > source.Width || blockY + BLOCK_HEIGHT > source.Height)
        {
            throw new ArgumentOutOfRangeException($"Le bloc ({blockX}, {blockY}) dépasse les limites de l'image {source.Width}x{source.Height}");
        }

        // Collecter toutes les couleurs du bloc
        var colors = new List<Rgba32>();
        for (var y = blockY; y < blockY + BLOCK_HEIGHT; y++)
        {
            for (var x = blockX; x < blockX + BLOCK_WIDTH; x++)
            {
                try
                {
                    colors.Add(source[x, y]);
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
        for (var y = blockY; y < blockY + BLOCK_HEIGHT; y++)
        {
            for (var x = blockX; x < blockX + BLOCK_WIDTH; x++)
            {
                try
                {
                    var originalColor = source[x, y];
                    var newColor = FindClosestColor(originalColor, [color1, color2]);
                    target[x, y] = newColor;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Erreur lors de l'écriture du pixel ({x}, {y}): {ex.Message}");
                    throw;
                }
            }
        }
    }

    private (Rgba32 color1, Rgba32 color2) FindDominantColors(List<Rgba32> colors)
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

    private Rgba32 CalculateAverageColor(List<Rgba32> colors)
    {
        if (colors.Count == 0) return new Rgba32(0, 0, 0);

        var r = (byte)colors.Average(c => c.R);
        var g = (byte)colors.Average(c => c.G);
        var b = (byte)colors.Average(c => c.B);

        return new Rgba32(r, g, b);
    }

    private Rgba32 FindClosestColor(Rgba32 color, Rgba32[] allowedColors)
    {
        var minDistance = double.MaxValue;
        var closestColor = allowedColors[0];

        foreach (var allowedColor in allowedColors)
        {
            var distance = CalculateColorDistance(color, allowedColor);
            if (!(distance < minDistance)) continue;
            minDistance = distance;
            closestColor = allowedColor;
        }

        return closestColor;
    }

    private static double CalculateColorDistance(Rgba32 color1, Rgba32 color2)
    {
        // Utilisation de la distance euclidienne dans l'espace RGB
        var rDiff = color1.R - color2.R;
        var gDiff = color1.G - color2.G;
        var bDiff = color1.B - color2.B;
        
        return Math.Sqrt(rDiff * rDiff + gDiff * gDiff + bDiff * bDiff);
    }

    public async Task ProcessImageFile(string inputPath, string outputPath)
    {
        // Lire le fichier PNG en bytes
        var imageBytes = await File.ReadAllBytesAsync(inputPath);
        
        // Traiter l'image
        var processedImage = await ConvertTo8Colors(imageBytes);
        
        // Sauvegarder le résultat
        await File.WriteAllBytesAsync(outputPath, processedImage);
    }

    public async Task ResizeImageFile(string inputPath, string outputPath)
    {
        // Lire le fichier PNG en bytes
        var imageBytes = await File.ReadAllBytesAsync(inputPath);
        
        // Redimensionner l'image
        var resizedImage = ResizeImageToMaxDimensions(imageBytes);
        
        // Sauvegarder le résultat
        await File.WriteAllBytesAsync(outputPath, resizedImage);
    }
    
    /// <summary>
    /// Combine plusieurs images en une seule grande image
    /// </summary>
    /// <param name="imagePaths">Liste des chemins vers les fichiers PNG à combiner</param>
    /// <returns>Un tableau d'octets représentant l'image combinée</returns>
    public async Task<byte[]> CombineImages(List<string> imagePaths)
    {
        if (imagePaths == null || !imagePaths.Any())
            throw new ArgumentException("La liste des images ne peut pas être vide");
            
        // Charger toutes les images
        var images = new List<Image<Rgba32>>();
        foreach (var path in imagePaths)
        {
            var imageBytes = await File.ReadAllBytesAsync(path);
            images.Add(Image.Load<Rgba32>(imageBytes));
        }
        
        // Organisation horizontale: toutes les images sur une seule ligne
        var columns = images.Count;
        var rows = 1;
        
        // Trouver la hauteur maximale pour la ligne
        var maxHeight = images.Max(img => img.Height);
        
        // Calculer la largeur totale de l'image de sortie
        var totalWidth = images.Sum(img => img.Width);
        var totalHeight = maxHeight;
        
        // Créer l'image résultante
        using var resultImage = new Image<Rgba32>(totalWidth, totalHeight);
        
        // Placer chaque image de gauche à droite
        int currentX = 0;
        for (int i = 0; i < images.Count; i++)
        {
            var image = images[i];
            resultImage.Mutate(ctx => ctx.DrawImage(image, new Point(currentX, 0), 1f));
            currentX += image.Width;
        }
        
        // Libérer les ressources des images sources
        foreach (var img in images)
        {
            img.Dispose();
        }
        
        // Convertir en PNG et retourner
        using var ms = new MemoryStream();
        await resultImage.SaveAsPngAsync(ms);
        return ms.ToArray();
    }
}