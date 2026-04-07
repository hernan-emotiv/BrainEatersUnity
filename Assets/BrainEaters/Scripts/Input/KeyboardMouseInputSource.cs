using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainEaters.Input
{
    public class KeyboardMouseInputSource : MonoBehaviour, IGameplayInputSource
    {
        public Vector2 Move { get; private set; }
        public bool IsChargeHeld { get; private set; }
        public bool WasBombPressedThisFrame { get; private set; }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            Move = ReadMove(keyboard);
            IsChargeHeld =
                (mouse != null && mouse.leftButton.isPressed) ||
                (keyboard != null && keyboard.spaceKey.isPressed);

            WasBombPressedThisFrame =
                (mouse != null && mouse.rightButton.wasPressedThisFrame) ||
                (keyboard != null && keyboard.qKey.wasPressedThisFrame);
        }

        private static Vector2 ReadMove(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float horizontal = 0f;
            float vertical = 0f;

            if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            {
                horizontal -= 1f;
            }

            if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            {
                horizontal += 1f;
            }

            if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
            {
                vertical -= 1f;
            }

            if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
            {
                vertical += 1f;
            }

            Vector2 input = new Vector2(horizontal, vertical);
            return input.sqrMagnitude > 1f ? input.normalized : input;
        }
    }
}
