using NetNitel.Services.Engine.Interfaces;

namespace NetNitel.Services.Engine;

public class MiniSequencer
{
    private readonly List<IMiniAction> _actions = new();
    
    public MiniSequencer Add(IMiniAction action)
    {
        _actions.Add(action);
        return this;
    }
    
    public async Task Execute()
    {
        foreach (var action in _actions)
        {
            await action.Action();
        }
    }
}