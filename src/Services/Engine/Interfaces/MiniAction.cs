namespace NetNitel.Services.Engine.Interfaces;

public interface IMiniAction
{
    Func<Task> Action { get; }
}