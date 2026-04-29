using System.Collections;
using BrainEaters.Turrets;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class OnboardingGateTarget : MonoBehaviour
    {
        [SerializeField] private TurretHealth targetHealth;
        [SerializeField] private Transform[] feedbackRoots = System.Array.Empty<Transform>();
        [SerializeField] private float hitWobbleAngle = 7f;
        [SerializeField] private float hitWobbleDuration = 0.18f;
        [SerializeField] private float hitWobbleFrequency = 26f;

        private Quaternion[] baseRotations = System.Array.Empty<Quaternion>();
        private Coroutine feedbackRoutine;

        private void Awake()
        {
            ResolveReferences();
            CacheBaseRotations();
            ResetGateTarget();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (targetHealth != null)
            {
                targetHealth.Damaged += HandleTargetDamaged;
            }
        }

        private void OnDisable()
        {
            if (targetHealth != null)
            {
                targetHealth.Damaged -= HandleTargetDamaged;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
            CacheBaseRotations();
        }

        public void ResetGateTarget()
        {
            CacheBaseRotations();
            ApplyBaseRotations();

            if (targetHealth != null)
            {
                targetHealth.ResetState();
                targetHealth.SetOnlineState(true);
            }
        }

        public void DisableTarget()
        {
            if (targetHealth != null)
            {
                targetHealth.SetOnlineState(false);
            }
        }

        private void HandleTargetDamaged(float _)
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
            }

            feedbackRoutine = StartCoroutine(PlayHitFeedback());
        }

        private IEnumerator PlayHitFeedback()
        {
            float elapsed = 0f;
            while (elapsed < hitWobbleDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, hitWobbleDuration));
                float falloff = 1f - normalized;
                float angle = Mathf.Sin(elapsed * hitWobbleFrequency) * hitWobbleAngle * falloff;

                for (int i = 0; i < feedbackRoots.Length; i++)
                {
                    Transform root = feedbackRoots[i];
                    if (root != null && i < baseRotations.Length)
                    {
                        root.localRotation = baseRotations[i] * Quaternion.Euler(0f, angle, 0f);
                    }
                }

                yield return null;
            }

            ApplyBaseRotations();
            feedbackRoutine = null;
        }

        private void ResolveReferences()
        {
            if (targetHealth == null)
            {
                targetHealth = GetComponent<TurretHealth>();
            }

            if ((feedbackRoots == null || feedbackRoots.Length == 0) && transform.childCount > 0)
            {
                feedbackRoots = new Transform[transform.childCount];
                for (int i = 0; i < transform.childCount; i++)
                {
                    feedbackRoots[i] = transform.GetChild(i);
                }
            }
        }

        private void CacheBaseRotations()
        {
            if (feedbackRoots == null)
            {
                feedbackRoots = System.Array.Empty<Transform>();
            }

            baseRotations = new Quaternion[feedbackRoots.Length];
            for (int i = 0; i < feedbackRoots.Length; i++)
            {
                baseRotations[i] = feedbackRoots[i] != null ? feedbackRoots[i].localRotation : Quaternion.identity;
            }
        }

        private void ApplyBaseRotations()
        {
            for (int i = 0; i < feedbackRoots.Length; i++)
            {
                if (feedbackRoots[i] != null && i < baseRotations.Length)
                {
                    feedbackRoots[i].localRotation = baseRotations[i];
                }
            }
        }
    }
}
