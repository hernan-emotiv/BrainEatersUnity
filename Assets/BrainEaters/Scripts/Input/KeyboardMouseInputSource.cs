using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainEaters.Input
{
    public class KeyboardMouseInputSource : MonoBehaviour, IGameplayInputSource
    {
        [SerializeField] private bool enableMouseLook = true;
        [SerializeField] private bool requireMouseButtonForLook = true;
        [SerializeField] private bool lockCursorWhileLooking = true;
        [SerializeField] private float mouseLookSensitivity = 0.015f;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool UsesFacingRelativeMovement => false;
        public bool UsesDeltaLookInput => true;
        public bool IsChargeHeld { get; private set; }
        public bool WasBombPressedThisFrame { get; private set; }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            UpdateCursorState(mouse);
            Move = ReadMove(keyboard);
            Look = ReadLook(mouse);
            IsChargeHeld =
                (mouse != null && mouse.leftButton.isPressed) ||
                (keyboard != null && keyboard.spaceKey.isPressed);

            WasBombPressedThisFrame =
                (mouse != null && mouse.rightButton.wasPressedThisFrame) ||
                (keyboard != null && keyboard.qKey.wasPressedThisFrame);
        }

        private void OnDisable()
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }

        private Vector2 ReadLook(Mouse mouse)
        {
            if (!enableMouseLook || mouse == null)
            {
                return Vector2.zero;
            }

            if (requireMouseButtonForLook && !IsAnyMouseButtonPressed(mouse))
            {
                return Vector2.zero;
            }

            return mouse.delta.ReadValue() * mouseLookSensitivity;
        }

        private void UpdateCursorState(Mouse mouse)
        {
            if (!enableMouseLook || !lockCursorWhileLooking || mouse == null || !Application.isPlaying)
            {
                return;
            }

            bool shouldLock = !requireMouseButtonForLook || IsAnyMouseButtonPressed(mouse);
            Cursor.lockState = shouldLock ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = !shouldLock;
        }

        private static bool IsAnyMouseButtonPressed(Mouse mouse)
        {
            return mouse.leftButton.isPressed || mouse.rightButton.isPressed || mouse.middleButton.isPressed;
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
