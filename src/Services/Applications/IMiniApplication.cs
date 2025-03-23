using NetNitel.Services.Engine;

namespace NetNitel.Services.Applications;

public abstract class MiniApplication(Minitel tel)
{
    public abstract Task StartAsync();
}