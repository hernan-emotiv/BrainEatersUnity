using UnityEngine;

namespace BrainEaters.Enemies
{
    public class EnemyAnimatorDriver : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private string idleTriggerName = "idle";
        [SerializeField] private string walkTriggerName = "walk";
        [SerializeField] private string attackTriggerName = "attack";
        [SerializeField] private string dieTriggerName = "die";

        private string currentTriggerName;
        private bool isDead;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void PlayIdle()
        {
            if (isDead)
            {
                return;
            }

            Trigger(idleTriggerName);
        }

        public void PlayWalk()
        {
            if (isDead)
            {
                return;
            }

            Trigger(walkTriggerName);
        }

        public void PlayAttack()
        {
            if (isDead)
            {
                return;
            }

            Trigger(attackTriggerName);
        }

        public void PlayDeath()
        {
            if (isDead)
            {
                return;
            }

            isDead = true;
            Trigger(dieTriggerName);
        }

        public void ResetState()
        {
            isDead = false;
            currentTriggerName = null;
            PlayIdle();
        }

        private void Trigger(string triggerName)
        {
            if (animator == null || string.IsNullOrWhiteSpace(triggerName) || currentTriggerName == triggerName)
            {
                return;
            }

            ResetKnownTriggers();
            animator.SetTrigger(triggerName);
            currentTriggerName = triggerName;
        }

        private void ResetKnownTriggers()
        {
            if (animator == null)
            {
                return;
            }

            ResetTrigger(idleTriggerName);
            ResetTrigger(walkTriggerName);
            ResetTrigger(attackTriggerName);
            ResetTrigger(dieTriggerName);
        }

        private void ResetTrigger(string triggerName)
        {
            if (!string.IsNullOrWhiteSpace(triggerName))
            {
                animator.ResetTrigger(triggerName);
            }
        }

        private void ResolveReferences()
        {
            if (animator == null)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }
    }
}
