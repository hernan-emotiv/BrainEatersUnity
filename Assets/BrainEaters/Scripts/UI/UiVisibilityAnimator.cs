using UnityEngine;

namespace BrainEaters.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class UiVisibilityAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private Vector3 visibleScale = Vector3.one;
        [SerializeField] private Vector3 hiddenScale = new Vector3(0.86f, 0.86f, 0.86f);
        [SerializeField] private float visibleAlpha = 1f;
        [SerializeField] private float hiddenAlpha = 0f;
        [SerializeField] private float showDuration = 0.18f;
        [SerializeField] private float hideDuration = 0.14f;
        [SerializeField] private UiEase showEase = UiEase.EaseOutBack;
        [SerializeField] private UiEase hideEase = UiEase.EaseInOutQuad;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool deactivateWhenHidden = true;
        [SerializeField] private bool blockRaycastsWhenVisible = true;

        private Vector3 startScale;
        private Vector3 endScale;
        private float startAlpha;
        private float endAlpha;
        private float animationDuration;
        private float elapsedTime;
        private UiEase currentEase;
        private bool animating;
        private bool targetVisible;
        private bool hasInitializedState;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (!animating || targetRect == null || canvasGroup == null)
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
            float eased = UiEaseUtility.Evaluate(currentEase, t);

            targetRect.localScale = Vector3.LerpUnclamped(startScale, endScale, eased);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, eased);

            if (t >= 1f)
            {
                animating = false;
                ApplyState(targetVisible);

                if (!targetVisible && deactivateWhenHidden)
                {
                    targetRect.gameObject.SetActive(false);
                }
            }
        }

        public void Show(bool instant = false)
        {
            SetVisible(true, instant);
        }

        public void Hide(bool instant = false)
        {
            SetVisible(false, instant);
        }

        public void SetVisible(bool visible, bool instant = false)
        {
            ResolveReferences();
            if (targetRect == null || canvasGroup == null)
            {
                return;
            }

            if (hasInitializedState && targetVisible == visible && !instant)
            {
                return;
            }

            bool wasInactive = !targetRect.gameObject.activeSelf;
            targetVisible = visible;
            hasInitializedState = true;

            if (visible && wasInactive)
            {
                targetRect.gameObject.SetActive(true);
            }

            if (instant)
            {
                animating = false;
                ApplyState(visible);

                if (!visible && deactivateWhenHidden)
                {
                    targetRect.gameObject.SetActive(false);
                }

                return;
            }

            if (visible && wasInactive)
            {
                ApplyState(false);
            }

            startScale = targetRect.localScale;
            endScale = visible ? visibleScale : hiddenScale;
            startAlpha = canvasGroup.alpha;
            endAlpha = visible ? visibleAlpha : hiddenAlpha;
            animationDuration = visible ? showDuration : hideDuration;
            currentEase = visible ? showEase : hideEase;
            elapsedTime = 0f;
            animating = true;

            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void ApplyState(bool visible)
        {
            targetRect.localScale = visible ? visibleScale : hiddenScale;
            canvasGroup.alpha = visible ? visibleAlpha : hiddenAlpha;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible && blockRaycastsWhenVisible;
        }

        private void ResolveReferences()
        {
            if (targetRect == null)
            {
                targetRect = transform as RectTransform;
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }
        }
    }
}
