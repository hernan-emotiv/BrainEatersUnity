using UnityEngine;

namespace BrainEaters.Spawning
{
    public class SpawnPoint : MonoBehaviour
    {
        [SerializeField] private ParticleSystem ambientParticles;
        [SerializeField] private ParticleSystem spawnBurstParticles;
        [SerializeField] private float gizmoRadius = 0.5f;
        [SerializeField] private Color gizmoColor = new Color(0.9f, 0.2f, 0.2f, 0.9f);

        public Vector3 Position => transform.position;
        public Quaternion Rotation => transform.rotation;

        public void PlaySpawnFeedback()
        {
            if (spawnBurstParticles != null)
            {
                spawnBurstParticles.Play();
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = gizmoColor;
            Gizmos.DrawSphere(transform.position, gizmoRadius);
        }
    }
}
