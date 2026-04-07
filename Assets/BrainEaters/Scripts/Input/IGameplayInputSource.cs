using UnityEngine;

namespace BrainEaters.Input
{
    public interface IGameplayInputSource
    {
        Vector2 Move { get; }
        bool IsChargeHeld { get; }
        bool WasBombPressedThisFrame { get; }
    }
}
