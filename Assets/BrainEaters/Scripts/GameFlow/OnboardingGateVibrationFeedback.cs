using System.Collections;
using BrainEaters.Turrets;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class OnboardingGateVibrationFeedback : MonoBehaviour
    {
        [SerializeField] private TurretHealth targetHealth;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private float duration = 0.22f;
        [SerializeField] private float positionAmplitude = 0.045f;
        [SerializeField] private float rotationAmplitude = 3.5f;
        [SerializeField] private float frequency = 42f;

        private Vector3 baseLocalPosition;
        private Quaternion baseLocalRotation;
        private Coroutine feedbackRoutine;

        private void Awake()
        {
            ResolveReferences();
            CacheBaseTransform();
        }

        private void OnEnable()
        {
            ResolveReferences();
            CacheBaseTransform();
            if (targetHealth != null)
            {
                targetHealth.Damaged += HandleDamaged;
            }
        }

        private void OnDisable()
        {
            if (targetHealth != null)
            {
                targetHealth.Damaged -= HandleDamaged;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void Play()
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(PlayRoutine());
        }

        private void HandleDamaged(float _)
        {
            Play();
        }

        private IEnumerator PlayRoutine()
        {
            if (visualRoot == null)
            {
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, duration));
                float falloff = 1f - normalized;
                float pulse = Mathf.Sin(elapsed * frequency) * falloff;
                float secondaryPulse = Mathf.Sin(elapsed * frequency * 1.37f) * falloff;

                visualRoot.localPosition = baseLocalPosition + new Vector3(pulse * positionAmplitude, secondaryPulse * positionAmplitude * 0.5f, 0f);
                visualRoot.localRotation = baseLocalRotation * Quaternion.Euler(0f, pulse * rotationAmplitude, secondaryPulse * rotationAmplitude);
                yield return null;
            }

            visualRoot.localPosition = baseLocalPosition;
            visualRoot.localRotation = baseLocalRotation;
            feedbackRoutine = null;
        }

        private void ResolveReferences()
        {
            if (visualRoot == null)
            {
                visualRoot = transform;
            }

            if (targetHealth == null)
            {
                targetHealth = GetComponentInParent<TurretHealth>();
            }
        }

        private void CacheBaseTransform()
        {
            if (visualRoot == null)
            {
                return;
            }

            baseLocalPosition = visualRoot.localPosition;
            baseLocalRotation = visualRoot.localRotation;
        }
    }
}
