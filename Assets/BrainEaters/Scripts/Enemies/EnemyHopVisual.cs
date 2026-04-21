using UnityEngine;

namespace BrainEaters.Enemies
{
    public class EnemyHopVisual : MonoBehaviour
    {
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float hopAmplitude = 0.22f;
        [SerializeField] private float hopFrequency = 8f;
        [SerializeField] private float landingSmoothing = 10f;

        private Vector3 baseLocalPosition;
        private bool isMoving;
        private bool hasCachedBasePosition;

        private void Awake()
        {
            ResolveReferences();
            CacheBasePosition();
        }

        private void OnValidate()
        {
            ResolveReferences();
            CacheBasePosition();
        }

        private void Update()
        {
            if (visualRoot == null)
            {
                return;
            }

            if (!isMoving)
            {
                visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, baseLocalPosition, Time.deltaTime * landingSmoothing);
                return;
            }

            float hopOffset = Mathf.Abs(Mathf.Sin(Time.time * hopFrequency)) * hopAmplitude;
            visualRoot.localPosition = baseLocalPosition + Vector3.up * hopOffset;
        }

        public void SetMoving(bool moving)
        {
            isMoving = moving;
        }

        public void ResetState()
        {
            ResolveReferences();
            CacheBasePosition();
            isMoving = false;

            if (visualRoot != null)
            {
                visualRoot.localPosition = baseLocalPosition;
            }
        }

        private void ResolveReferences()
        {
            if (visualRoot == null)
            {
                Transform visualTransform = transform.Find("VisualRoot");
                if (visualTransform != null)
                {
                    visualRoot = visualTransform;
                }
            }
        }

        private void CacheBasePosition()
        {
            if (visualRoot == null || hasCachedBasePosition)
            {
                return;
            }

            baseLocalPosition = visualRoot.localPosition;
            hasCachedBasePosition = true;
        }
    }
}
