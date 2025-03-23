using NetNitel.Services.Engine;
using NetNitel.Services.Engine.Enums;

namespace NetNitel.Services.Applications;

public class MiniHomeScreen(Minitel tel)
{
    private bool _test = true;
    
    public async Task StartAsync()
    {
        if (_test)
        {
            DrawTestScreen();
        }
        else
        {
            //await DrawSplashScreen();
            var dalle = new MiniDallE(tel);
            await dalle.StartAsync();
        }
    }

    public async Task DrawTestScreen()
    {
        //await tel.BlackBG.White.Line(1).Left("Hello World");
    }
    
    public async Task DrawSplashScreen()
    {
        await tel.Control.Home();
        await tel.Control.ClearScreen();
        
        await tel.Draw.Image("data/alita.png");
        await tel.Draw.Text("Alita", MiniFont.TheBigOne, 20, 23,foreColor: MiniColor.Yellow, backColor: MiniColor.Blue);

        await tel.Write.Error("Erreur de connexion");
        
        await Task.Delay(2000);
    }
}