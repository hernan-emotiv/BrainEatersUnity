using BrainEaters.Input;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainEaters.Cameras
{
    [ExecuteAlways]
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private bool lookAtTarget = true;
        [SerializeField] private float targetLookHeight = 1.5f;
        [SerializeField] private float yawSensitivity = 180f;
        [SerializeField] private float pitchSensitivity = 120f;
        [SerializeField] private Vector2 pitchLimits = new Vector2(15f, 65f);
        [SerializeField] private float singleJoystickFollowSpeed = 12f;
        [SerializeField] private bool enableEditorMouseDragLook = true;
        [SerializeField] private float editorMouseLookRangePixels = 180f;
        [SerializeField] private PlayerInputRouter inputRouter;

        private float orbitDistance;
        private float yaw;
        private float pitch;
        private MobileControlMode controlMode = MobileControlMode.DualJoystick;
        private bool orbitInitialized;
        private bool isEditorMouseDragging;
        private Vector2 editorMouseDragStartPosition;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
            if (inputRouter == null && target != null)
            {
                inputRouter = target.GetComponent<PlayerInputRouter>();
                if (inputRouter == null)
                {
                    inputRouter = target.GetComponentInParent<PlayerInputRouter>();
                }
            }

            orbitInitialized = false;
        }

        public void SetControlMode(MobileControlMode mode)
        {
            controlMode = mode;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            ResolveReferences();
            InitializeOrbitIfNeeded();
            float deltaTime = Application.isPlaying ? Time.unscaledDeltaTime : Time.deltaTime;
            UpdateOrbitAngles(deltaTime);

            Vector3 focusPoint = target.position + Vector3.up * targetLookHeight;
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            Vector3 desiredPosition = focusPoint + orbitRotation * (Vector3.back * orbitDistance);
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSpeed * deltaTime));

            if (lookAtTarget)
            {
                transform.LookAt(focusPoint);
            }
            else
            {
                transform.rotation = orbitRotation;
            }
        }

        private void OnValidate()
        {
            orbitInitialized = false;

            if (!Application.isPlaying)
            {
                ResolveReferences();
                InitializeOrbitIfNeeded();
                ApplyImmediatePreview();
            }
        }

        private void UpdateOrbitAngles(float deltaTime)
        {
            Vector2 lookInput = inputRouter != null ? inputRouter.Look : Vector2.zero;
            if (lookInput.sqrMagnitude <= 0.0001f)
            {
                lookInput = ReadEditorMouseDragLook();
            }

            float lookTimeFactor = inputRouter != null && inputRouter.UsesDeltaLookInput ? 1f : deltaTime;

            if (controlMode == MobileControlMode.SingleJoystick)
            {
                if (Mathf.Abs(lookInput.x) > 0.001f)
                {
                    float targetYaw = target.eulerAngles.y + (lookInput.x * yawSensitivity * lookTimeFactor);
                    target.rotation = Quaternion.Euler(0f, targetYaw, 0f);
                }

                yaw = Mathf.LerpAngle(yaw, target.eulerAngles.y, 1f - Mathf.Exp(-singleJoystickFollowSpeed * deltaTime));
                return;
            }

            yaw += lookInput.x * yawSensitivity * lookTimeFactor;
            pitch = Mathf.Clamp(pitch - (lookInput.y * pitchSensitivity * lookTimeFactor), pitchLimits.x, pitchLimits.y);
        }

        private Vector2 ReadEditorMouseDragLook()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying || !enableEditorMouseDragLook)
            {
                isEditorMouseDragging = false;
                return Vector2.zero;
            }

            Mouse mouse = Mouse.current;
            if (mouse == null || !mouse.leftButton.isPressed)
            {
                isEditorMouseDragging = false;
                return Vector2.zero;
            }

            Vector2 screenPosition = mouse.position.ReadValue();
            if (!isEditorMouseDragging || mouse.leftButton.wasPressedThisFrame)
            {
                isEditorMouseDragging = true;
                editorMouseDragStartPosition = screenPosition;
            }

            Vector2 dragDelta = screenPosition - editorMouseDragStartPosition;
            return Vector2.ClampMagnitude(dragDelta / Mathf.Max(1f, editorMouseLookRangePixels), 1f);
#else
            return Vector2.zero;
#endif
        }

        private void InitializeOrbitIfNeeded()
        {
            if (orbitInitialized)
            {
                return;
            }

            orbitDistance = offset.magnitude;
            if (orbitDistance <= 0.001f)
            {
                orbitDistance = 10f;
            }

            yaw = Mathf.Atan2(offset.x, -offset.z) * Mathf.Rad2Deg;
            pitch = Mathf.Asin(Mathf.Clamp(offset.y / orbitDistance, -1f, 1f)) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, pitchLimits.x, pitchLimits.y);
            orbitInitialized = true;
        }

        private void ResolveReferences()
        {
            if (inputRouter == null && target != null)
            {
                inputRouter = target.GetComponent<PlayerInputRouter>();
                if (inputRouter == null)
                {
                    inputRouter = target.GetComponentInParent<PlayerInputRouter>();
                }
            }
        }

        private void ApplyImmediatePreview()
        {
            if (target == null)
            {
                return;
            }

            Vector3 focusPoint = target.position + Vector3.up * targetLookHeight;
            Quaternion orbitRotation = Quaternion.Euler(pitch, yaw, 0f);
            transform.position = focusPoint + orbitRotation * (Vector3.back * orbitDistance);

            if (lookAtTarget)
            {
                transform.LookAt(focusPoint);
            }
            else
            {
                transform.rotation = orbitRotation;
            }
        }
    }
}
