using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace BrainEaters.Enemies
{
    public class EnemyPhysicsLaunch : MonoBehaviour
    {
        [SerializeField] private Rigidbody body;
        [SerializeField] private float fallbackMass = 1.2f;

        private Coroutine killRoutine;

        public bool IsLaunching { get; private set; }

        public void LaunchAndKill(Vector3 force, Vector3 torque, float killDelaySeconds)
        {
            EnemyHealth health = GetComponent<EnemyHealth>();
            if (health == null || !health.IsAlive)
            {
                return;
            }

            DisableGameplayControl();
            EnsureBody();

            body.isKinematic = false;
            body.useGravity = true;
            body.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
            body.WakeUp();
            body.AddForce(force, ForceMode.Impulse);
            body.AddTorque(torque, ForceMode.Impulse);
            IsLaunching = true;

            if (killRoutine != null)
            {
                StopCoroutine(killRoutine);
            }

            killRoutine = StartCoroutine(KillAfterDelay(health, killDelaySeconds));
        }

        private void DisableGameplayControl()
        {
            EnemyController controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            EnemyMovement movement = GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.enabled = false;
            }

            EnemyAttack attack = GetComponent<EnemyAttack>();
            if (attack != null)
            {
                attack.enabled = false;
            }

            NavMeshAgent agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = false;
            }
        }

        private void EnsureBody()
        {
            if (body == null)
            {
                body = GetComponent<Rigidbody>();
            }

            if (body == null)
            {
                body = gameObject.AddComponent<Rigidbody>();
            }

            body.mass = Mathf.Max(0.1f, fallbackMass);
        }

        private IEnumerator KillAfterDelay(EnemyHealth health, float delaySeconds)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, delaySeconds));

            if (health != null)
            {
                health.Kill();
            }

            IsLaunching = false;
        }
    }
}
