using UnityEngine;

namespace BrainEaters.Player
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float turnSpeed = 720f;
        [SerializeField] private float gravity = -20f;

        private float verticalVelocity;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void SetCamera(Transform targetCamera)
        {
            cameraTransform = targetCamera;
        }

        public void Tick(Vector2 moveInput, float deltaTime, bool useFacingRelativeMovement = false)
        {
            if (deltaTime <= 0f)
            {
                return;
            }

            if (cameraTransform == null && Camera.main != null)
            {
                cameraTransform = Camera.main.transform;
            }

            Vector3 planarMove = useFacingRelativeMovement
                ? GetFacingRelativeMove(moveInput)
                : GetPlanarMove(moveInput);

            if (!useFacingRelativeMovement && planarMove.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(planarMove, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * deltaTime);
            }

            if (characterController.isGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f;
            }

            verticalVelocity += gravity * deltaTime;

            Vector3 velocity = planarMove * moveSpeed;
            velocity.y = verticalVelocity;

            characterController.Move(velocity * deltaTime);
        }

        private Vector3 GetFacingRelativeMove(Vector2 moveInput)
        {
            float forwardAmount = Mathf.Clamp(moveInput.y, -1f, 1f);
            if (Mathf.Abs(forwardAmount) <= 0.001f)
            {
                return Vector3.zero;
            }

            Vector3 forward = transform.forward;
            forward.y = 0f;
            forward.Normalize();
            return forward * forwardAmount;
        }

        private Vector3 GetPlanarMove(Vector2 moveInput)
        {
            Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
            if (inputDirection.sqrMagnitude <= 0.001f)
            {
                return Vector3.zero;
            }

            inputDirection = Vector3.ClampMagnitude(inputDirection, 1f);

            if (cameraTransform == null)
            {
                return inputDirection;
            }

            Vector3 cameraForward = cameraTransform.forward;
            cameraForward.y = 0f;
            cameraForward.Normalize();

            Vector3 cameraRight = cameraTransform.right;
            cameraRight.y = 0f;
            cameraRight.Normalize();

            Vector3 move = (cameraForward * inputDirection.z) + (cameraRight * inputDirection.x);
            return move.normalized;
        }

        private void ResolveReferences()
        {
            if (characterController == null)
            {
                characterController = GetComponent<CharacterController>();
            }
        }
    }
}
