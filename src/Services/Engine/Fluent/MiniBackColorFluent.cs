namespace NetNitel.Services.Engine.Fluent;

public class MiniBackColorFluent
{
    public MiniSequencer Sequencer { get; set; }
    
    public MiniBackColorFluent(MiniSequencer sequencer)
    {
        Sequencer = sequencer;
    }
}