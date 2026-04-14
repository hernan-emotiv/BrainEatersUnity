using UnityEngine;
using UnityEngine.EventSystems;

namespace BrainEaters.Input
{
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform backgroundRect;
        [SerializeField] private RectTransform handleRect;
        [SerializeField] private float handleRange = 60f;

        public Vector2 Value { get; private set; }

        private Canvas parentCanvas;
        private Camera eventCamera;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (backgroundRect == null)
            {
                return;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(backgroundRect, eventData.position, eventCamera, out Vector2 localPoint))
            {
                return;
            }

            float radius = backgroundRect.rect.width * 0.5f;
            if (radius <= 0.001f)
            {
                Value = Vector2.zero;
                return;
            }

            Value = Vector2.ClampMagnitude(localPoint / radius, 1f);

            if (handleRect != null)
            {
                handleRect.anchoredPosition = Value * handleRange;
            }
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            Value = Vector2.zero;

            if (handleRect != null)
            {
                handleRect.anchoredPosition = Vector2.zero;
            }
        }

        private void ResolveReferences()
        {
            if (backgroundRect == null)
            {
                backgroundRect = transform as RectTransform;
            }

            if (handleRect == null && transform.childCount > 0)
            {
                handleRect = transform.GetChild(0) as RectTransform;
            }

            parentCanvas = GetComponentInParent<Canvas>();
            eventCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay ? parentCanvas.worldCamera : null;
        }
    }
}
