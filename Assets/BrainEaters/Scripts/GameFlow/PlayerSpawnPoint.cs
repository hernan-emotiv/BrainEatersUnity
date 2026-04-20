using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class PlayerSpawnPoint : MonoBehaviour
    {
        [SerializeField] private float gizmoRadius = 0.65f;
        [SerializeField] private Color gizmoColor = new Color(0.2f, 0.8f, 1f, 0.9f);

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
            Gizmos.DrawRay(transform.position + Vector3.up * 0.1f, transform.forward * 1.5f);
        }
    }
}
