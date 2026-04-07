using UnityEngine;

namespace BrainEaters.Spawning
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private float gizmoRadius = 0.5f;
        [SerializeField] private Color gizmoColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
        }
    }
}
