using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.UI
{
    public class UiFillAnimator : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform fillArea;
        [SerializeField] private float animationDuration = 0.18f;
        [SerializeField] private float stepNormalized = 0.05f;
        [SerializeField] private UiEase ease = UiEase.EaseOutCubic;
        [SerializeField] private bool useUnscaledTime = true;

        private RectTransform fillRectTransform;
        private float currentNormalized;
        private float startNormalized;
        private float targetNormalized;
        private float elapsedTime;
        private bool isAnimating;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        private void Update()
        {
            if (!isAnimating)
            {
                return;
            }

            float deltaTime = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            if (deltaTime <= 0f)
            {
                return;
            }

            elapsedTime += deltaTime;
            float duration = Mathf.Max(0.0001f, animationDuration);
            float t = Mathf.Clamp01(elapsedTime / duration);
            float eased = UiEaseUtility.Evaluate(ease, t);
            currentNormalized = Mathf.Lerp(startNormalized, targetNormalized, eased);
            ApplyNormalized(currentNormalized);

            if (t >= 1f)
            {
                isAnimating = false;
                currentNormalized = targetNormalized;
                ApplyNormalized(currentNormalized);
            }
        }

        public void SetNormalized(float normalizedValue, bool instant = false)
        {
            CacheReferences();
            if (fillRectTransform == null || fillArea == null)
            {
                return;
            }

            float clamped = Mathf.Clamp01(normalizedValue);
            if (stepNormalized > 0.0001f)
            {
                clamped = Mathf.Round(clamped / stepNormalized) * stepNormalized;
                clamped = Mathf.Clamp01(clamped);
            }

            if (instant || animationDuration <= 0f)
            {
                isAnimating = false;
                currentNormalized = clamped;
                targetNormalized = clamped;
                ApplyNormalized(clamped);
                return;
            }

            if (Mathf.Approximately(clamped, targetNormalized) && isAnimating)
            {
                return;
            }

            startNormalized = currentNormalized;
            targetNormalized = clamped;
            elapsedTime = 0f;
            isAnimating = true;
        }

        private void ApplyNormalized(float normalizedValue)
        {
            float availableWidth = fillArea.rect.width;
            float rightInset = availableWidth * (1f - Mathf.Clamp01(normalizedValue));

            fillRectTransform.anchorMin = new Vector2(0f, 0f);
            fillRectTransform.anchorMax = new Vector2(1f, 1f);
            fillRectTransform.pivot = new Vector2(0f, 0.5f);
            fillRectTransform.offsetMin = Vector2.zero;
            fillRectTransform.offsetMax = new Vector2(-rightInset, 0f);
        }

        private void CacheReferences()
        {
            if (fillImage == null)
            {
                fillImage = GetComponentInChildren<Image>();
            }

            if (fillImage == null)
            {
                return;
            }

            fillRectTransform = fillImage.rectTransform;
            if (fillArea == null)
            {
                fillArea = fillRectTransform.parent as RectTransform;
            }
        }
    }
}
