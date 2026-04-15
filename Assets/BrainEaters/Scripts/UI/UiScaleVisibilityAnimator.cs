using UnityEngine;

namespace BrainEaters.UI
{
    public class UiScaleVisibilityAnimator : MonoBehaviour
    {
        [SerializeField] private RectTransform targetRect;
        [SerializeField] private Vector3 visibleScale = Vector3.one;
        [SerializeField] private Vector3 hiddenScale = new Vector3(0.2f, 0.2f, 0.2f);
        [SerializeField] private float showDuration = 0.18f;
        [SerializeField] private float hideDuration = 0.22f;
        [SerializeField] private UiEase showEase = UiEase.EaseOutBack;
        [SerializeField] private UiEase hideEase = UiEase.EaseInOutQuad;
        [SerializeField] private bool useUnscaledTime = true;
        [SerializeField] private bool deactivateWhenHidden = true;

        private Vector3 startScale;
        private Vector3 endScale;
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
            if (!animating || targetRect == null)
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

            if (t >= 1f)
            {
                animating = false;
                targetRect.localScale = endScale;

                if (!targetVisible && deactivateWhenHidden)
                {
                    targetRect.gameObject.SetActive(false);
                }
            }
        }

        public void SetVisible(bool visible, bool instant = false)
        {
            ResolveReferences();
            if (targetRect == null)
            {
                return;
            }

            if (hasInitializedState && targetVisible == visible && !instant)
            {
                return;
            }

            targetVisible = visible;
            hasInitializedState = true;

            if (visible && !targetRect.gameObject.activeSelf)
            {
                targetRect.gameObject.SetActive(true);
            }

            Vector3 desiredScale = visible ? visibleScale : hiddenScale;
            if (instant)
            {
                animating = false;
                targetRect.localScale = desiredScale;

                if (!visible && deactivateWhenHidden)
                {
                    targetRect.gameObject.SetActive(false);
                }
                else if (visible)
                {
                    targetRect.gameObject.SetActive(true);
                }

                return;
            }

            startScale = targetRect.localScale;
            endScale = desiredScale;
            animationDuration = visible ? showDuration : hideDuration;
            currentEase = visible ? showEase : hideEase;
            elapsedTime = 0f;
            animating = true;
        }

        private void ResolveReferences()
        {
            if (targetRect == null)
            {
                targetRect = transform as RectTransform;
            }
        }
    }
}
