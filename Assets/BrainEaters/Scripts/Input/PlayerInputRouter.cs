using UnityEngine;

namespace BrainEaters.Input
{
    public class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour activeInputSource;

        private IGameplayInputSource cachedInputSource;

        public Vector2 Move => GetResolvedInputSource()?.Move ?? Vector2.zero;
        public Vector2 Look => GetResolvedInputSource()?.Look ?? Vector2.zero;
        public bool UsesFacingRelativeMovement => GetResolvedInputSource() != null && GetResolvedInputSource().UsesFacingRelativeMovement;
        public bool UsesDeltaLookInput => GetResolvedInputSource() != null && GetResolvedInputSource().UsesDeltaLookInput;
        public bool IsChargeHeld => GetResolvedInputSource() != null && GetResolvedInputSource().IsChargeHeld;
        public bool WasBombPressedThisFrame => GetResolvedInputSource() != null && GetResolvedInputSource().WasBombPressedThisFrame;

        private void Awake()
        {
            ResolveInputSource();
        }

        private void OnValidate()
        {
            ResolveInputSource();
        }

        public void SetInputSource(MonoBehaviour inputSource)
        {
            activeInputSource = inputSource;
            ResolveInputSource();
        }

        private IGameplayInputSource GetResolvedInputSource()
        {
            if (cachedInputSource == null || activeInputSource == null || !activeInputSource.isActiveAndEnabled)
            {
                ResolveInputSource();
            }

            return cachedInputSource;
        }

        private void ResolveInputSource()
        {
            cachedInputSource = null;

            if (activeInputSource != null && activeInputSource.isActiveAndEnabled)
            {
                cachedInputSource = activeInputSource as IGameplayInputSource;
                if (cachedInputSource != null)
                {
                    return;
                }
            }

            MonoBehaviour[] candidates = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour candidate in candidates)
            {
                if (candidate == null || !candidate.isActiveAndEnabled)
                {
                    continue;
                }

                if (candidate is IGameplayInputSource gameplayInputSource)
                {
                    activeInputSource = candidate;
                    cachedInputSource = gameplayInputSource;
                    return;
                }
            }
        }
    }
}
