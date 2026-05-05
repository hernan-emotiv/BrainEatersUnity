using BrainEaters.Core;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class BombActivationProxy : MonoBehaviour, IBombActivatable
    {
        [SerializeField] private MonoBehaviour activationTarget;

        private IBombActivatable target;

        public bool CanActivateBomb
        {
            get
            {
                ResolveTarget();
                return target != null && target.CanActivateBomb;
            }
        }

        private void Awake()
        {
            ResolveTarget();
        }

        private void OnValidate()
        {
            ResolveTarget();
        }

        public void ActivateBomb()
        {
            ResolveTarget();
            target?.ActivateBomb();
        }

        private void ResolveTarget()
        {
            target = activationTarget as IBombActivatable;
            if (target == null && activationTarget != null)
            {
                Debug.LogWarning($"{activationTarget.name} does not implement IBombActivatable.", this);
            }
        }
    }
}
