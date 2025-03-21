using NetNitel.Services.Engine;

namespace NetNitel.Services.Applications;

public class MiniHomeScreen(Minitel tel)
{
    public async Task StartAsync()
    {
        await DrawTestScreen();
    }

    public async Task DrawTestScreen()
    {
        await tel.Control.Home();
        await tel.Control.ClearScreen();
        
        await tel.DrawImage("data/alita.png");
        await tel.DrawText("Alita", 20, 23,foreColor: MiniColor.Jaune, backColor: MiniColor.Bleu);
    }
    
    public async Task DrawHomeScreen()
    {
        await tel.Control.Home();
        await tel.Control.ClearScreen();
        
        await tel.DrawImage("data/image.png");

        await tel.Move(12, 4);
        await tel.Control.DoubleSizeText();
        await tel.BackColor(MiniColor.Noir);
        await tel.ForeColor(MiniColor.Blanc);
        await tel.Print("Inutile");
    }
}