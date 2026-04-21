using UnityEngine;

namespace BrainEaters.Enemies
{
    public class EnemyDeathVisual : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float deathDurationSeconds = 0.3f;
        [SerializeField] private Vector3 collapsedScale = new Vector3(1.15f, 0.2f, 1.15f);
        [SerializeField] private Vector3 fallenEulerAngles = new Vector3(0f, 0f, 85f);
        [SerializeField] private Vector3 fallenLocalOffset = new Vector3(0f, -0.45f, 0.12f);

        private Vector3 initialLocalScale;
        private Quaternion initialLocalRotation;
        private Vector3 initialLocalPosition;
        private bool isPlayingDeath;
        private float deathElapsed;
        private bool hasCachedInitialState;

        private void Awake()
        {
            ResolveReferences();
            CacheInitialState();
        }

        private void OnValidate()
        {
            ResolveReferences();
            CacheInitialState();
        }

        private void Update()
        {
            if (!isPlayingDeath || visualRoot == null)
            {
                return;
            }

            deathElapsed += Time.deltaTime;
            float t = deathDurationSeconds <= 0.0001f ? 1f : Mathf.Clamp01(deathElapsed / deathDurationSeconds);

            visualRoot.localRotation = Quaternion.Slerp(initialLocalRotation, Quaternion.Euler(fallenEulerAngles), t);
            visualRoot.localScale = Vector3.Lerp(initialLocalScale, collapsedScale, t);
            visualRoot.localPosition = Vector3.Lerp(initialLocalPosition, initialLocalPosition + fallenLocalOffset, t);

            if (t >= 1f)
            {
                isPlayingDeath = false;
            }
        }

        public void PlayDeath()
        {
            ResolveReferences();
            CacheInitialState();
            isPlayingDeath = true;
            deathElapsed = 0f;
        }

        public void ResetState()
        {
            ResolveReferences();
            CacheInitialState();
            isPlayingDeath = false;
            deathElapsed = 0f;

            if (visualRoot != null)
            {
                visualRoot.localRotation = initialLocalRotation;
                visualRoot.localScale = initialLocalScale;
                visualRoot.localPosition = initialLocalPosition;
            }
        }

        private void ResolveReferences()
        {
            if (visualRoot == null)
            {
                Transform found = transform.Find("VisualRoot");
                if (found != null)
                {
                    visualRoot = found;
                }
            }
        }

        private void CacheInitialState()
        {
            if (visualRoot == null || hasCachedInitialState)
            {
                return;
            }

            initialLocalScale = visualRoot.localScale;
            initialLocalRotation = visualRoot.localRotation;
            initialLocalPosition = visualRoot.localPosition;
            hasCachedInitialState = true;
        }
    }
}
