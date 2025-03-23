using NetNitel.Services.Engine.Enums;
using NetNitel.Services.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NetNitel.Services.Engine;

public class MiniDrawer(Minitel tel)
{
    private readonly ImageService _imageService = new();

    /// <summary>
    /// Dessine du texte en mode graphique
    /// </summary>
    /// <param name="text"></param>
    /// <param name="font"></param>
    /// <param name="ligne"></param>
    /// <param name="colonne"></param>
    /// <param name="foreColor"></param>
    /// <param name="backColor"></param>
    public async Task Text(string text, MiniFont font = MiniFont.TheBigOne, int ligne = 1, int colonne = 1,
        MiniColor foreColor = MiniColor.White,
        MiniColor backColor = MiniColor.Black,
        bool disjoint = false)
    {
        var letters = new ImageTextWriter().GetImageLetters(text, font);
        var image = await _imageService.CombineImages(letters);

        using var rgba32Image = SixLabors.ImageSharp.Image.Load<Rgba32>(image);

        // Déterminer les dimensions de l'image en blocs
        var blocksWidth = rgba32Image.Width / 2;
        var blocksHeight = rgba32Image.Height / 3;

        // Pour chaque ligne de blocs
        for (var blockY = 0; blockY < blocksHeight; blockY++)
        {
            // Déplacer le curseur au début de la ligne
            await tel.Control.Move(ligne + blockY, colonne);

            await tel.Control.ForeColor(foreColor);
            await tel.Control.BackColor(backColor);

            // Pour chaque bloc dans la ligne
            for (var blockX = 0; blockX < blocksWidth; blockX++)
            {
                // Calculer la position du bloc dans l'image
                var pixelX = blockX * 2;
                var pixelY = blockY * 3;

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

                await tel.Control.Underline(disjoint);
                // Écrire le bloc (le curseur avance automatiquement)
                await tel.Control.WriteGraphic(pixelString);
            }
        }
    }


    /// <summary>
    /// Dessine une ligne horizontale
    /// </summary>
    /// <param name="ligne">Ligne entre 1 et 24</param>
    /// <param name="position">Position dans la ligne entre 1 et 3</param>
    /// <param name="backColor"></param>
    /// <param name="foreColor"></param>
    public async Task HorizontalLine(int ligne, MiniPosition position, MiniColor backColor = MiniColor.Black, MiniColor foreColor = MiniColor.White)
    {
        await tel.Control.Move(ligne);
        await tel.Control.BackColor(backColor);
        await tel.Control.ForeColor(foreColor);
        if (position == MiniPosition.Up) await tel.Control.Raw(0x7E);
        if (position == MiniPosition.Middle) await tel.Control.Raw(0x60);
        if (position == MiniPosition.Down) await tel.Control.Raw(0x5F);
        await tel.Control.Repeat(39);
    }
    
    /// <summary>
    /// Remplit l'écran avec une image en 80x72.
    /// </summary>
    /// <param name="imagePath"></param>
    public async Task Image(string imagePath)
    {
        // Placer le curseur en haut à gauche
        await tel.Control.Home();

        // Traiter l'image pour obtenir une version 80x72 en 8 couleurs
        var image = await File.ReadAllBytesAsync(imagePath);
        var miniImage = await _imageService.ConvertTo8Colors(image);
        
        // Charger l'image traitée
        using var rgba32Image = SixLabors.ImageSharp.Image.Load<Rgba32>(miniImage);
        
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
                await tel.Control.ForeColor(ConvertToMiniColor(color1));
                await tel.Control.BackColor(ConvertToMiniColor(color2));

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
                await tel.Control.WriteGraphic(pixelString);
            }
        }
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

    private bool IsPositivePixel(Rgba32 color, Rgba32 color1, Rgba32 color2)
    {
        // Calculer la distance entre la couleur du pixel et les deux couleurs dominantes
        var distance1 = CalculateColorDistance(color, color1);
        var distance2 = CalculateColorDistance(color, color2);
        
        // Si plus proche de la première couleur dominante, c'est un pixel positif
        return distance1 <= distance2;
    }
}