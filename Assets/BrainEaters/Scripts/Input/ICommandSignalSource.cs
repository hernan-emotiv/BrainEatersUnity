namespace BrainEaters.Input
{
    public interface ICommandSignalSource
    {
        bool HasSignal { get; }
        MentalCommandSignal LatestSignal { get; }
        string StatusText { get; }
    }
}
