using UnityEngine;

namespace BrainEaters.Input
{
    public class PlayerInputRouter : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour activeInputSource;

        private IGameplayInputSource cachedInputSource;

        public Vector2 Move => cachedInputSource?.Move ?? Vector2.zero;
        public bool IsChargeHeld => cachedInputSource != null && cachedInputSource.IsChargeHeld;
        public bool WasBombPressedThisFrame => cachedInputSource != null && cachedInputSource.WasBombPressedThisFrame;

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

        private void ResolveInputSource()
        {
            cachedInputSource = activeInputSource as IGameplayInputSource;

            if (cachedInputSource != null)
            {
                return;
            }

            MonoBehaviour[] candidates = GetComponents<MonoBehaviour>();
            foreach (MonoBehaviour candidate in candidates)
            {
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
