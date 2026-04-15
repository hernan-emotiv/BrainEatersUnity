using UnityEngine;

namespace BrainEaters.Input
{
    public interface IGameplayInputSource
    {
        Vector2 Move { get; }
        Vector2 Look { get; }
        bool UsesFacingRelativeMovement { get; }
        bool UsesDeltaLookInput { get; }
        bool IsChargeHeld { get; }
        bool WasBombPressedThisFrame { get; }
    }
}
