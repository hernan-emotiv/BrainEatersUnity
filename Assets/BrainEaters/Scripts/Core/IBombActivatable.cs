namespace BrainEaters.Core
{
    public interface IBombActivatable
    {
        bool CanActivateBomb { get; }
        void ActivateBomb();
    }
}
