using BrainEaters.Cameras;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BrainEaters.Input
{
    public class MobileControlsManager : MonoBehaviour
    {
        [SerializeField] private bool forceMobileControlsInEditor;
        [SerializeField] private MobileControlMode initialControlMode = MobileControlMode.DualJoystick;
        [SerializeField] private GameObject controlsRoot;
        [SerializeField] private GameObject visibleJoysticksRoot;
        [SerializeField] private GameObject rightJoystickRoot;
        [SerializeField] private GameObject invisibleJoysticksRoot;
        [SerializeField] private Button modeToggleButton;
        [SerializeField] private TMP_Text modeLabel;
        [SerializeField] private PlayerInputRouter playerInputRouter;
        [SerializeField] private KeyboardMouseInputSource keyboardMouseInputSource;
        [SerializeField] private MobileGameplayInputSource mobileGameplayInputSource;
        [SerializeField] private CameraFollow cameraFollow;

        private bool useMobileControls;
        private MobileControlMode currentControlMode;

        private void Awake()
        {
            ResolveReferences();
            BindButton();
            useMobileControls = ShouldUseMobileControls();
            ApplyInputSource();
            SetControlMode(initialControlMode);
        }

        private void OnEnable()
        {
            ResolveReferences();
            BindButton();
            useMobileControls = ShouldUseMobileControls();
            ApplyInputSource();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (!useMobileControls)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.tabKey.wasPressedThisFrame)
            {
                CycleControlMode();
            }
        }

        public void CycleControlMode()
        {
            MobileControlMode nextMode = currentControlMode switch
            {
                MobileControlMode.DualJoystick => MobileControlMode.SingleJoystick,
                MobileControlMode.SingleJoystick => MobileControlMode.InvisibleJoysticks,
                _ => MobileControlMode.DualJoystick
            };

            SetControlMode(nextMode);
        }

        public void SetControlMode(MobileControlMode controlMode)
        {
            currentControlMode = controlMode;

            if (mobileGameplayInputSource != null)
            {
                mobileGameplayInputSource.SetControlMode(controlMode);
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetControlMode(controlMode);
            }

            if (visibleJoysticksRoot != null)
            {
                visibleJoysticksRoot.SetActive(useMobileControls && controlMode != MobileControlMode.InvisibleJoysticks);
            }

            if (rightJoystickRoot != null)
            {
                rightJoystickRoot.SetActive(useMobileControls && controlMode == MobileControlMode.DualJoystick);
            }

            if (invisibleJoysticksRoot != null)
            {
                invisibleJoysticksRoot.SetActive(useMobileControls && controlMode == MobileControlMode.InvisibleJoysticks);
            }

            if (modeLabel != null)
            {
                modeLabel.text = controlMode switch
                {
                    MobileControlMode.DualJoystick => "2 Joysticks",
                    MobileControlMode.SingleJoystick => "1 Joystick",
                    _ => "Invisible Joysticks"
                };
            }
        }

        private void ApplyInputSource()
        {
            if (controlsRoot != null)
            {
                controlsRoot.SetActive(useMobileControls);
            }

            if (playerInputRouter == null)
            {
                return;
            }

            if (useMobileControls && mobileGameplayInputSource != null)
            {
                playerInputRouter.SetInputSource(mobileGameplayInputSource);
                if (cameraFollow != null)
                {
                    cameraFollow.SetControlMode(currentControlMode);
                }
            }
            else if (keyboardMouseInputSource != null)
            {
                playerInputRouter.SetInputSource(keyboardMouseInputSource);
                if (cameraFollow != null)
                {
                    cameraFollow.SetControlMode(MobileControlMode.DualJoystick);
                }
            }
        }

        private bool ShouldUseMobileControls()
        {
#if UNITY_EDITOR
            return forceMobileControlsInEditor;
#else
            return Application.isMobilePlatform;
#endif
        }

        private void BindButton()
        {
            if (modeToggleButton == null)
            {
                return;
            }

            modeToggleButton.onClick.RemoveListener(CycleControlMode);
            modeToggleButton.onClick.AddListener(CycleControlMode);
        }

        private void ResolveReferences()
        {
            if (playerInputRouter == null)
            {
                playerInputRouter = FindFirstObjectByType<PlayerInputRouter>();
            }

            if (keyboardMouseInputSource == null)
            {
                keyboardMouseInputSource = FindFirstObjectByType<KeyboardMouseInputSource>();
            }

            if (mobileGameplayInputSource == null)
            {
                mobileGameplayInputSource = FindFirstObjectByType<MobileGameplayInputSource>();
            }

            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow>();
            }

            if (controlsRoot == null)
            {
                Transform visuals = transform.Find("ControlsVisuals");
                controlsRoot = visuals != null ? visuals.gameObject : null;
            }

            if (visibleJoysticksRoot == null)
            {
                Transform visible = transform.Find("ControlsVisuals/VisibleJoysticksRoot");
                visibleJoysticksRoot = visible != null ? visible.gameObject : null;
            }

            if (invisibleJoysticksRoot == null)
            {
                Transform invisible = transform.Find("ControlsVisuals/InvisibleJoysticksRoot");
                invisibleJoysticksRoot = invisible != null ? invisible.gameObject : null;
            }

            if (rightJoystickRoot == null)
            {
                Transform right = transform.Find("ControlsVisuals/VisibleJoysticksRoot/RightJoystickRoot");
                rightJoystickRoot = right != null ? right.gameObject : null;
            }
        }
    }
}
