using UnityEngine;

namespace BrainEaters.Player
{
    public class PlayerHitFeedback : MonoBehaviour
    {
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Color flashColor = new Color(1f, 0.35f, 0.35f, 1f);
        [SerializeField] private float flashDurationSeconds = 0.18f;
        [SerializeField] private float scaleMultiplier = 1.1f;

        private Renderer[] cachedRenderers = System.Array.Empty<Renderer>();
        private Color[] baseColors = System.Array.Empty<Color>();
        private Vector3 baseScale = Vector3.one;
        private float remainingFlashTime;

        private void Awake()
        {
            ResolveReferences();
            CacheVisualState();
        }

        private void OnEnable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (playerHealth != null)
            {
                playerHealth.Damaged -= HandleDamaged;
            }

            RestoreVisualState();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (visualRoot == null || remainingFlashTime <= 0f)
            {
                return;
            }

            remainingFlashTime -= Time.deltaTime;
            float normalized = Mathf.Clamp01(remainingFlashTime / flashDurationSeconds);

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                cachedRenderers[i].material.color = Color.Lerp(baseColors[i], flashColor, normalized);
            }

            visualRoot.localScale = Vector3.Lerp(baseScale, baseScale * scaleMultiplier, normalized);

            if (remainingFlashTime <= 0f)
            {
                RestoreVisualState();
            }
        }

        private void HandleDamaged(float _)
        {
            remainingFlashTime = flashDurationSeconds;
            CacheVisualState();
        }

        private void CacheVisualState()
        {
            if (visualRoot == null)
            {
                return;
            }

            cachedRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            baseColors = new Color[cachedRenderers.Length];
            baseScale = visualRoot.localScale;

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                baseColors[i] = cachedRenderers[i].material.color;
            }
        }

        private void RestoreVisualState()
        {
            if (visualRoot == null)
            {
                return;
            }

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].material.color = baseColors[i];
                }
            }

            visualRoot.localScale = baseScale;
        }

        private void ResolveReferences()
        {
            if (playerHealth == null)
            {
                playerHealth = GetComponent<PlayerHealth>();
            }

            if (visualRoot == null)
            {
                Transform visualTransform = transform.Find("Visual");
                visualRoot = visualTransform != null ? visualTransform : transform;
            }
        }
    }
}
