using UnityEngine;
using UnityEngine.EventSystems;

namespace BrainEaters.Input
{
    public class InvisibleTouchJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform regionRect;
        [SerializeField] private float dragRange = 120f;

        public Vector2 Value { get; private set; }

        private int activePointerId = int.MinValue;
        private Vector2 startScreenPosition;

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
            if (activePointerId != int.MinValue)
            {
                return;
            }

            activePointerId = eventData.pointerId;
            startScreenPosition = eventData.position;
            Value = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            if (dragRange <= 0.001f)
            {
                Value = Vector2.zero;
                return;
            }

            Vector2 delta = eventData.position - startScreenPosition;
            Value = Vector2.ClampMagnitude(delta / dragRange, 1f);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (eventData.pointerId != activePointerId)
            {
                return;
            }

            activePointerId = int.MinValue;
            Value = Vector2.zero;
        }

        private void ResolveReferences()
        {
            if (regionRect == null)
            {
                regionRect = transform as RectTransform;
            }
        }
    }
}
