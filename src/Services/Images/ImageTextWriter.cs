namespace NetNitel.Services.Images;

public class ImageTextWriter
{
   private const string BASE_PATH = "data/letters/";


   public List<string> GetImageLetters(string text)
   {
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
                  result.Add($"{BASE_PATH}u{c.ToString().ToLower()}.png");
                  continue;
               }

               // Format pour les minuscules: l[lettre].png
               result.Add($"{BASE_PATH}l{c}.png");
               continue;
            }

            if (char.IsWhiteSpace(c))
            {
               result.Add($"{BASE_PATH}space.png");
               continue;
            }

            if (char.IsNumber(c))
            {
               result.Add($"{BASE_PATH}{c}.png");
               continue;
            }

            switch (c)
            {
               case '_':
                  result.Add($"{BASE_PATH}underscore.png");
                  break;
               case '!':
                  result.Add($"{BASE_PATH}exclamation.png");
                  break;
               case '"':
                  result.Add($"{BASE_PATH}doublequote.png");
                  break;
               case '#':
                  result.Add($"{BASE_PATH}hash.png");
                  break;
               case '$':
                  result.Add($"{BASE_PATH}dollar.png");
                  break;
               case '%':
                  result.Add($"{BASE_PATH}percent.png");
                  break;
               case '&':
                  result.Add($"{BASE_PATH}ampersand.png");
                  break;
               case '\'':
                  result.Add($"{BASE_PATH}apostrophe.png");
                  break;
               case '(':
                  result.Add($"{BASE_PATH}leftparen.png");
                  break;
               case ')':
                  result.Add($"{BASE_PATH}rightparen.png");
                  break;
               case '×':
                  result.Add($"{BASE_PATH}times.png");
                  break;
               case '÷':
                  result.Add($"{BASE_PATH}divide.png");
                  break;
               case '+':
                  result.Add($"{BASE_PATH}plus.png");
                  break;
               case ',':
                  result.Add($"{BASE_PATH}comma.png");
                  break;
               case '-':
                  result.Add($"{BASE_PATH}minus.png");
                  break;
               case '.':
                  result.Add($"{BASE_PATH}dot.png");
                  break;
               case '/':
                  result.Add($"{BASE_PATH}slash.png");
                  break;
               case ':':
                  result.Add($"{BASE_PATH}colon.png");
                  break;
               case ';':
                  result.Add($"{BASE_PATH}semicolon.png");
                  break;
               case '<':
                  result.Add($"{BASE_PATH}leftangle.png");
                  break;
               case '=':
                  result.Add($"{BASE_PATH}equal.png");
                  break;
               case '>':
                  result.Add($"{BASE_PATH}rightangle.png");
                  break;
               case '?':
                  result.Add($"{BASE_PATH}question.png");
                  break;
               case '[':
                  result.Add($"{BASE_PATH}leftbracket.png");
                  break;
               case ']':
                  result.Add($"{BASE_PATH}rightbracket.png");
                  break;
               case '\\':
                  result.Add($"{BASE_PATH}backslash.png");
                  break;
               case '^':
                  result.Add($"{BASE_PATH}caret.png");
                  break;
               case '`':
                  result.Add($"{BASE_PATH}grave.png");
                  break;
               case '{':
                  result.Add($"{BASE_PATH}leftbrace.png");
                  break;
               case '}':
                  result.Add($"{BASE_PATH}rightbrace.png");
                  break;
               case '|':
                  result.Add($"{BASE_PATH}bar.png");
                  break;
               case '~':
                  result.Add($"{BASE_PATH}tilde.png");
                  break;
               case '@':
                  result.Add($"{BASE_PATH}at.png");
                  break;
               case '\u25ae':
                  result.Add($"{BASE_PATH}blackrectangle.png");
                  break;
               case '€':
                  result.Add($"{BASE_PATH}euro.png");
                  break;
               case 'µ':
                  result.Add($"{BASE_PATH}micro.png");
                  break;
               default:
                  result.Add($"{BASE_PATH}unknown.png");
                  break;

            }
         }
         catch (Exception e)
         {
            Console.WriteLine(e);
         }
      }
      
      return result;
   }
}