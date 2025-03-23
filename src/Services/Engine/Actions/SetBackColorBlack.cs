using NetNitel.Services.Engine.Enums;
using NetNitel.Services.Engine.Interfaces;

namespace NetNitel.Services.Engine.Actions;

public class SetBackColorBlack(Minitel tel) : IMiniAction
{
    public Func<Task> Action => Act;
    
    private async Task Act()
    {
        await tel.Control.BackColor(MiniColor.Black);
    }
}