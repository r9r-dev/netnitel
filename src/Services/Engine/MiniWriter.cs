using System.Text;
using NetNitel.Services.Engine.Enums;
using NetNitel.Services.Images;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace NetNitel.Services.Engine;

public class MiniWriter(Minitel tel)
{
    
    
    /// <summary>
    /// Affiche du texte
    /// </summary>
    public async Task Text(string texte)
    {
        // Convertir les accents si nécessaire
        texte = ConvertAccents(texte);
        await tel.Control.Write(texte);
    }

    public async Task Error(string message)
    {
        await Message(
            message,
            true,
            MiniColor.Red);
    }

    public async Task Info(string message)
    {
        await Message(message);
    }
    
    


    /// <summary>
    /// Affiche un message d'information temporaire
    /// Par défaut, le message s'affice en haut de l'écran pendant 2 secondes
    /// </summary>
    /// <param name="message">Message à afficher</param>
    /// <param name="backColor"></param>
    /// <param name="duree">durée en secondes (2 secondes par défaut)</param>
    /// <param name="bip">faire un bip</param>
    /// <param name="ligne"></param>
    /// <param name="colonne"></param>
    /// <param name="foreColor"></param>
    public async Task Message(
        string message, 
        bool bip = false,
        MiniColor foreColor = MiniColor.White,
        MiniColor backColor = MiniColor.Black,
        int duree = 2, 
        int ligne = 0, 
        int colonne = 1)
    {
        await tel.Control.Move(ligne, colonne);
        await tel.Control.ForeColor(foreColor);
        await tel.Control.BackColor(backColor);
        await Text(message);
        
        if (bip)
        {
            //await tel.Control.Buzzer();
        }
        
        // Attendre le délai
        await Task.Delay(duree * 1000);
        
        // Effacer le message
        await tel.Control.Move(ligne, colonne);
        await tel.Control.ForeColor(MiniColor.White);
        await tel.Control.BackColor(MiniColor.Black);
        await Text(" ");
        await tel.Control.Repeat(message.Length - 1);
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
}