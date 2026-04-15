using UnityEngine;

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

        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool UsesFacingRelativeMovement => controlMode == MobileControlMode.SingleJoystick;
        public bool UsesDeltaLookInput => false;
        public bool IsChargeHeld { get; private set; }
        public bool WasBombPressedThisFrame { get; private set; }
        public MobileControlMode ControlMode => controlMode;

        public void SetControlMode(MobileControlMode mode)
        {
            controlMode = mode;
        }

        private void Update()
        {
            Vector2 moveValue = GetMoveInput();
            Vector2 lookValue = GetLookInput();

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

            IsChargeHeld = chargeButton != null && chargeButton.IsPressed;
            WasBombPressedThisFrame = bombButton != null && bombButton.ConsumePressedThisFrame();
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
                return invisibleLookJoystick != null ? invisibleLookJoystick.Value : Vector2.zero;
            }

            return lookJoystick != null ? lookJoystick.Value : Vector2.zero;
        }
    }
}
