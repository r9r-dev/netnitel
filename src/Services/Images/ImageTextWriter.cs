using NetNitel.Services.Engine;
using NetNitel.Services.Engine.Enums;

namespace NetNitel.Services.Images;

public class ImageTextWriter
{

   public List<string> GetImageLetters(string text, MiniFont font = MiniFont.TheBigOne)
   {
      string basePath = string.Empty;
      switch (font)
      {
         case MiniFont.TheBigOne:
            basePath = "data/letters/";
            break;
         case MiniFont.TheSmallOne:
            basePath = "data/letters_small/";
            break;
      }
      
      var result = new List<string>();
      
      if (string.IsNullOrEmpty(text))
         return result;
      
      foreach (char c in text)
      {
         try
         {
            if (char.IsLetter(c))
            {
               if (char.IsUpper(c))
               {
                  // Format pour les majuscules: u[lettre].png
                  result.Add($"{basePath}u{c.ToString().ToLower()}.png");
                  continue;
               }

               // Format pour les minuscules: l[lettre].png
               result.Add($"{basePath}l{c}.png");
               continue;
            }

            if (char.IsWhiteSpace(c))
            {
               result.Add($"{basePath}space.png");
               continue;
            }

            if (char.IsNumber(c))
            {
               result.Add($"{basePath}{c}.png");
               continue;
            }

            switch (c)
            {
               case '_':
                  result.Add($"{basePath}underscore.png");
                  break;
               case '!':
                  result.Add($"{basePath}exclamation.png");
                  break;
               case '"':
                  result.Add($"{basePath}doublequote.png");
                  break;
               case '#':
                  result.Add($"{basePath}hash.png");
                  break;
               case '$':
                  result.Add($"{basePath}dollar.png");
                  break;
               case '%':
                  result.Add($"{basePath}percent.png");
                  break;
               case '&':
                  result.Add($"{basePath}ampersand.png");
                  break;
               case '\'':
                  result.Add($"{basePath}apostrophe.png");
                  break;
               case '(':
                  result.Add($"{basePath}leftparen.png");
                  break;
               case ')':
                  result.Add($"{basePath}rightparen.png");
                  break;
               case '×':
                  result.Add($"{basePath}times.png");
                  break;
               case '÷':
                  result.Add($"{basePath}divide.png");
                  break;
               case '+':
                  result.Add($"{basePath}plus.png");
                  break;
               case ',':
                  result.Add($"{basePath}comma.png");
                  break;
               case '-':
                  result.Add($"{basePath}minus.png");
                  break;
               case '.':
                  result.Add($"{basePath}dot.png");
                  break;
               case '/':
                  result.Add($"{basePath}slash.png");
                  break;
               case ':':
                  result.Add($"{basePath}colon.png");
                  break;
               case ';':
                  result.Add($"{basePath}semicolon.png");
                  break;
               case '<':
                  result.Add($"{basePath}leftangle.png");
                  break;
               case '=':
                  result.Add($"{basePath}equal.png");
                  break;
               case '>':
                  result.Add($"{basePath}rightangle.png");
                  break;
               case '?':
                  result.Add($"{basePath}question.png");
                  break;
               case '[':
                  result.Add($"{basePath}leftbracket.png");
                  break;
               case ']':
                  result.Add($"{basePath}rightbracket.png");
                  break;
               case '\\':
                  result.Add($"{basePath}backslash.png");
                  break;
               case '^':
                  result.Add($"{basePath}caret.png");
                  break;
               case '`':
                  result.Add($"{basePath}grave.png");
                  break;
               case '{':
                  result.Add($"{basePath}leftbrace.png");
                  break;
               case '}':
                  result.Add($"{basePath}rightbrace.png");
                  break;
               case '|':
                  result.Add($"{basePath}bar.png");
                  break;
               case '~':
                  result.Add($"{basePath}tilde.png");
                  break;
               case '@':
                  result.Add($"{basePath}at.png");
                  break;
               case '\u25ae':
                  result.Add($"{basePath}blackrectangle.png");
                  break;
               case '€':
                  result.Add($"{basePath}euro.png");
                  break;
               case 'µ':
                  result.Add($"{basePath}micro.png");
                  break;
               case '·':
                  result.Add($"{basePath}middlepoint.png");
                  break;
               default:
                  result.Add($"{basePath}unknown.png");
                  break;

            }
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
      }
      
      // check if file exist for each result
      for (int i = 0; i < result.Count; i++)
      {
         if (!File.Exists(result[i]))
         {
            result[i] = $"{basePath}blackrectangle.png";
         }
      }
      
      return result;
   }
}