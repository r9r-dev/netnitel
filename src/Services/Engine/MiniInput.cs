namespace NetNitel.Services.Engine;

public class MiniInput
{
    public string Message { get; set; }
    public MiniKey SpecialKey { get; set; } = MiniKey.None;

    public MiniInput()
    {
        Message = string.Empty;
    }

    public MiniInput(MiniKey key)
    {
        SpecialKey = key;
    }
    
    public MiniInput(string message)
    {
        Message = message;
    }

    public MiniInputType Type
    {
        get
        {
            if (!string.IsNullOrEmpty(Message)) return MiniInputType.Text;
            if (SpecialKey != MiniKey.None) return MiniInputType.SpecialKey;
            return MiniInputType.None;
        }
    }

    public static MiniInput Parse(string input)
    {
        if (input.Length == 0) return new MiniInput();
        if (input.StartsWith('\u0013'))
        {
            var result = input switch {
                "\u0013A" => new MiniInput(MiniKey.Envoi),
                "\u0013B" => new MiniInput(MiniKey.Retour),
                "\u0013C" => new MiniInput(MiniKey.Repetition),
                "\u0013D" => new MiniInput(MiniKey.Guide),
                "\u0013E" => new MiniInput(MiniKey.Annulation),
                "\u0013F" => new MiniInput(MiniKey.Sommaire),
                "\u0013G" => new MiniInput(MiniKey.Correction),
                "\u0013H" => new MiniInput(MiniKey.Suite),
                "\u0013Y" => new MiniInput(MiniKey.Connexion),
                _ => new MiniInput(MiniKey.Unrecognised)
            };
            if (result.SpecialKey == MiniKey.Unrecognised)
                ConsoleParsed($"Unrecognised special key: {input[1..]}");
            else ConsoleParsed(result.SpecialKey.ToString());
            return result;
        }
        if (input.StartsWith('\e'))
        {
            var result = input switch
            {
                "\e[A" => new MiniInput(MiniKey.Haut),
                "\e[B" => new MiniInput(MiniKey.Bas),
                "\e[C" => new MiniInput(MiniKey.Droite),
                "\e[D" => new MiniInput(MiniKey.Gauche),
                "\e[H" => new MiniInput(MiniKey.Home),
                "\e[L" => new MiniInput(MiniKey.InserLigne),
                "\e[M" => new MiniInput(MiniKey.SupprLigne),
                "\e[P" => new MiniInput(MiniKey.SupprColonne),
                "\e[2J" => new MiniInput(MiniKey.EPage),
                "\e[4h" => new MiniInput(MiniKey.InserColonne),
                "\e" => new MiniInput(MiniKey.ESC),
                _ => new MiniInput(MiniKey.Unrecognised)
            };
            if (result.SpecialKey == MiniKey.Unrecognised)
                ConsoleParsed($"Unrecognised escape key: {input[1..]}");
            else ConsoleParsed(result.SpecialKey.ToString());
            return result;
        }

        var ch = input[0];
        var intch = (int)ch;

        switch ((int)ch)
        {
            case 8: // BS
                ConsoleParsed("Correction (BACKSPACE)");
                return new MiniInput(MiniKey.Correction);
            case 9: // TAB
                ConsoleParsed("TAB");
                return new MiniInput(MiniKey.Tab);
            case 10: // LF
                ConsoleParsed("LINE FEED");
                return new MiniInput(MiniKey.LineFeed);
            case 13: // CR
                ConsoleParsed("Enter");
                return new MiniInput(MiniKey.Envoi);
            case 127: // DEL
                ConsoleParsed("Delete");
                return new MiniInput(MiniKey.Delete);
        }
        
        if (intch is >= 32 and <= 126)
        {
            ConsoleParsed($"Char {ch}");
            return new MiniInput(ch.ToString());
        }
        
        ConsoleParsed($"Unknown {input}");
        
        return new MiniInput { Message = input };
    }

    private static void ConsoleParsed(string result)
    {
        Console.WriteLine($"[PARSED] {result}");
    }
}