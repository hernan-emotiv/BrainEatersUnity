using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainEaters.Input
{
    public class MobileGameplayInputSource : MonoBehaviour, IGameplayInputSource
    {
        [SerializeField] private VirtualJoystick moveJoystick;
        [SerializeField] private VirtualJoystick lookJoystick;
        [SerializeField] private InvisibleTouchJoystick invisibleMoveJoystick;
        [SerializeField] private InvisibleTouchJoystick invisibleLookJoystick;
        [SerializeField] private TouchActionButton chargeButton;
        [SerializeField] private TouchActionButton bombButton;
        [SerializeField] private MobileControlMode controlMode = MobileControlMode.DualJoystick;
        [SerializeField] private float editorMouseLookRangePixels = 180f;

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool UsesFacingRelativeMovement => controlMode == MobileControlMode.SingleJoystick;
        public bool UsesDeltaLookInput => false;
        public bool IsChargeHeld { get; private set; }
        public bool WasBombPressedThisFrame { get; private set; }
        public MobileControlMode ControlMode => controlMode;

        private bool isEditorMouseLooking;
        private Vector2 editorMouseLookStartPosition;

        public void SetControlMode(MobileControlMode mode)
        {
            controlMode = mode;
        }

        private void Update()
        {
            ResolveReferences();

            Vector2 moveValue = CombineMoveInput(GetMoveInput(), ReadKeyboardMove());
            Vector2 lookValue = GetLookInput();
            Keyboard keyboard = Keyboard.current;

            if (controlMode == MobileControlMode.DualJoystick || controlMode == MobileControlMode.InvisibleJoysticks)
            {
                Move = moveValue;
                Look = lookValue;
            }
            else
            {
                Move = new Vector2(0f, moveValue.y);
                Look = new Vector2(moveValue.x, 0f);
            }

            IsChargeHeld =
                (chargeButton != null && chargeButton.IsPressed) ||
                (keyboard != null && keyboard.spaceKey.isPressed);

            WasBombPressedThisFrame =
                (bombButton != null && bombButton.ConsumePressedThisFrame()) ||
                (keyboard != null && keyboard.qKey.wasPressedThisFrame);
        }

        private Vector2 GetMoveInput()
        {
            if (controlMode == MobileControlMode.InvisibleJoysticks)
            {
                return invisibleMoveJoystick != null ? invisibleMoveJoystick.Value : Vector2.zero;
            }

            return moveJoystick != null ? moveJoystick.Value : Vector2.zero;
        }

        private Vector2 GetLookInput()
        {
            if (controlMode == MobileControlMode.InvisibleJoysticks)
            {
                if (invisibleLookJoystick != null)
                {
                    return invisibleLookJoystick.Value;
                }

                return ReadEditorMouseLookFallback();
            }

            if (lookJoystick != null)
            {
                return lookJoystick.Value;
            }

            return ReadEditorMouseLookFallback();
        }

        private Vector2 ReadEditorMouseLookFallback()
        {
#if UNITY_EDITOR
            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.isPressed)
            {
                isEditorMouseLooking = false;
                return Vector2.zero;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            if (!isEditorMouseLooking || mouse.leftButton.wasPressedThisFrame)
            {
                isEditorMouseLooking = true;
                editorMouseLookStartPosition = screenPosition;
            }

            Vector2 dragDelta = screenPosition - editorMouseLookStartPosition;
            return Vector2.ClampMagnitude(dragDelta / Mathf.Max(1f, editorMouseLookRangePixels), 1f);
#else
            return Vector2.zero;
#endif
        }

        private void ResolveReferences()
        {
            if (moveJoystick == null)
            {
                moveJoystick = FindFirstObjectByType<VirtualJoystick>();
            }

            if (lookJoystick == null)
            {
                VirtualJoystick[] joysticks = FindObjectsByType<VirtualJoystick>(FindObjectsSortMode.None);
                for (int i = 0; i < joysticks.Length; i++)
                {
                    if (joysticks[i] != null && joysticks[i].name.Contains("Right"))
                    {
                        lookJoystick = joysticks[i];
                        break;
                    }
                }
            }

            if (invisibleMoveJoystick == null || invisibleLookJoystick == null)
            {
                InvisibleTouchJoystick[] invisibleJoysticks = FindObjectsByType<InvisibleTouchJoystick>(FindObjectsSortMode.None);
                for (int i = 0; i < invisibleJoysticks.Length; i++)
                {
                    InvisibleTouchJoystick joystick = invisibleJoysticks[i];
                    if (joystick == null)
                    {
                        continue;
                    }

                    if (invisibleMoveJoystick == null && joystick.name.Contains("Left"))
                    {
                        invisibleMoveJoystick = joystick;
                    }

                    if (invisibleLookJoystick == null && joystick.name.Contains("Right"))
                    {
                        invisibleLookJoystick = joystick;
                    }
                }
            }

            if (chargeButton == null || bombButton == null)
            {
                TouchActionButton[] buttons = FindObjectsByType<TouchActionButton>(FindObjectsSortMode.None);
                for (int i = 0; i < buttons.Length; i++)
                {
                    TouchActionButton button = buttons[i];
                    if (button == null)
                    {
                        continue;
                    }

                    if (chargeButton == null && button.name.Contains("Charge"))
                    {
                        chargeButton = button;
                    }

                    if (bombButton == null && button.name.Contains("Bomb"))
                    {
                        bombButton = button;
                    }
                }
            }
        }

        private static Vector2 CombineMoveInput(Vector2 primaryInput, Vector2 keyboardInput)
        {
            Vector2 combined = primaryInput + keyboardInput;
            return combined.sqrMagnitude > 1f ? combined.normalized : combined;
        }

        private static Vector2 ReadKeyboardMove()
        {
            Keyboard keyboard = Keyboard.current;
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
