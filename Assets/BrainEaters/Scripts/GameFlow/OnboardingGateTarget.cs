using System.Collections;
using BrainEaters.Turrets;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class OnboardingGateTarget : MonoBehaviour
    {
        public event System.Action Destroyed;

        [SerializeField] private TurretHealth targetHealth;
        [SerializeField] private Transform[] feedbackRoots = System.Array.Empty<Transform>();
        [SerializeField] private float hitWobbleAngle = 13f;
        [SerializeField] private float hitWobbleDuration = 0.28f;
        [SerializeField] private float hitRecoilAngle = 3f;

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
                targetHealth.Destroyed += HandleTargetDestroyed;
            }
        }

        private void OnDisable()
        {
            if (targetHealth != null)
            {
                targetHealth.Damaged -= HandleTargetDamaged;
                targetHealth.Destroyed -= HandleTargetDestroyed;
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

        public void DisableTarget(bool resetHealth = true)
        {
            if (targetHealth != null)
            {
                targetHealth.SetOnlineState(false, resetHealth);
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

        private void HandleTargetDestroyed(TurretHealth _)
        {
            if (feedbackRoutine != null)
            {
                StopCoroutine(feedbackRoutine);
                feedbackRoutine = null;
            }

            Destroyed?.Invoke();
        }

        private IEnumerator PlayHitFeedback()
        {
            float elapsed = 0f;
            while (elapsed < hitWobbleDuration)
            {
                elapsed += Time.deltaTime;
                float normalized = Mathf.Clamp01(elapsed / Mathf.Max(0.0001f, hitWobbleDuration));
                float falloff = 1f - normalized;
                float openPush = Mathf.Sin(normalized * Mathf.PI) * hitWobbleAngle;
                float recoil = Mathf.Sin(normalized * Mathf.PI * 4f) * hitRecoilAngle * falloff;

                for (int i = 0; i < feedbackRoots.Length; i++)
                {
                    Transform root = feedbackRoots[i];
                    if (root != null && i < baseRotations.Length)
                    {
                        float outwardSign = GetOutwardSign(root, i);
                        float angle = outwardSign * (openPush + recoil);
                        root.localRotation = baseRotations[i] * Quaternion.Euler(0f, angle, 0f);
                    }
                }

                yield return null;
            }

            ApplyBaseRotations();
            feedbackRoutine = null;
        }

        private static float GetOutwardSign(Transform root, int index)
        {
            if (root != null)
            {
                string lowerName = root.name.ToLowerInvariant();
                if (lowerName.Contains("left"))
                {
                    return 1f;
                }

                if (lowerName.Contains("right"))
                {
                    return -1f;
                }
            }

            return index == 0 ? 1f : -1f;
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
