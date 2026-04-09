using UnityEngine;

namespace BrainEaters.Player
{
    public class BombPulseVisual : MonoBehaviour
    {
        [SerializeField] private Transform effectOrigin;
        [SerializeField] private float durationSeconds = 0.25f;
        [SerializeField] private float verticalOffset = 0.1f;
        [SerializeField] private Color pulseColor = new Color(0.25f, 0.9f, 1f, 1f);

        private Transform activePulse;
        private float remainingTime;
        private float targetDiameter;

        private void Awake()
        {
            if (effectOrigin == null)
            {
                effectOrigin = transform;
            }
        }

        private void Update()
        {
            if (activePulse == null || remainingTime <= 0f)
            {
                return;
            }

            remainingTime -= Time.deltaTime;
            float progress = 1f - Mathf.Clamp01(remainingTime / durationSeconds);
            activePulse.localScale = Vector3.one * Mathf.Lerp(0.2f, targetDiameter, progress);

            if (remainingTime <= 0f)
            {
                activePulse.gameObject.SetActive(false);
            }
        }

        public void Play(float radius)
        {
            EnsurePulseObject();

            targetDiameter = radius * 2f;
            remainingTime = durationSeconds;
            activePulse.position = effectOrigin.position + Vector3.up * verticalOffset;
            activePulse.localScale = Vector3.one * 0.2f;
            activePulse.gameObject.SetActive(true);
        }

        private void EnsurePulseObject()
        {
            if (activePulse != null)
            {
                return;
            }

            GameObject pulse = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            pulse.name = "BombPulseVisual";
            pulse.transform.SetParent(null);

            Collider pulseCollider = pulse.GetComponent<Collider>();
            if (pulseCollider != null)
            {
                pulseCollider.enabled = false;
            }

            Renderer pulseRenderer = pulse.GetComponent<Renderer>();
            pulseRenderer.material.color = pulseColor;
            pulseRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            pulseRenderer.receiveShadows = false;

            activePulse = pulse.transform;
            activePulse.gameObject.SetActive(false);
        }
    }
}
