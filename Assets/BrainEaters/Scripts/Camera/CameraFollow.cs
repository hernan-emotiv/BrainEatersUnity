using UnityEngine;

namespace BrainEaters.Cameras
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0f, 8f, -8f);
        [SerializeField] private float followSpeed = 10f;
        [SerializeField] private bool lookAtTarget = true;

        public void SetTarget(Transform followTarget)
        {
            target = followTarget;
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            transform.position = Vector3.Lerp(transform.position, desiredPosition, 1f - Mathf.Exp(-followSpeed * Time.deltaTime));

            if (lookAtTarget)
            {
                transform.LookAt(target.position + Vector3.up * 1.5f);
            }
        }
    }
}
