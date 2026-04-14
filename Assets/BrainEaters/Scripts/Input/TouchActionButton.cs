using UnityEngine;
using UnityEngine.EventSystems;

namespace BrainEaters.Input
{
    public class TouchActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool IsPressed { get; private set; }

        private bool wasPressedThisFrame;

        public void OnPointerDown(PointerEventData eventData)
        {
            IsPressed = true;
            wasPressedThisFrame = true;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            IsPressed = false;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            IsPressed = false;
        }

        public bool ConsumePressedThisFrame()
        {
            bool pressed = wasPressedThisFrame;
            wasPressedThisFrame = false;
            return pressed;
        }
    }
}
