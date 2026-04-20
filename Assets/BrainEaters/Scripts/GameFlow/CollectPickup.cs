using BrainEaters.Player;
using TMPro;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class CollectPickup : MonoBehaviour
    {
        public event System.Action<CollectPickup> Collected;

        [SerializeField] private Transform visualRoot;
        [SerializeField] private MeshRenderer pickupRenderer;
        [SerializeField] private TMP_Text stateLabel;
        [SerializeField] private Collider triggerCollider;
        [SerializeField] private float rotationSpeed = 180f;
        [SerializeField] private float bobAmplitude = 0.2f;
        [SerializeField] private float bobFrequency = 2.5f;
        [SerializeField] private Color idleColor = new Color(1f, 0.82f, 0.2f, 1f);
        [SerializeField] private Color collectedColor = new Color(0.35f, 1f, 0.45f, 0.45f);

        private Vector3 initialVisualLocalPosition;
        private bool isCollected;

        public bool IsCollected => isCollected;

        private void Awake()
        {
            ResolveReferences();
            CacheVisualPosition();
            RefreshVisuals();
        }

        private void OnValidate()
        {
            ResolveReferences();
            CacheVisualPosition();
            RefreshVisuals();
        }

        private void Update()
        {
            if (isCollected || visualRoot == null)
            {
                return;
            }

            float bobOffset = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            visualRoot.localPosition = initialVisualLocalPosition + Vector3.up * bobOffset;
            visualRoot.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        }

        public void ResetState()
        {
            ResolveReferences();
            CacheVisualPosition();
            isCollected = false;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = true;
            }

            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(true);
                visualRoot.localPosition = initialVisualLocalPosition;
                visualRoot.localRotation = Quaternion.identity;
            }

            RefreshVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCollected)
            {
                return;
            }

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player == null)
            {
                return;
            }

            isCollected = true;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            if (visualRoot != null)
            {
                visualRoot.gameObject.SetActive(false);
            }

            RefreshVisuals();
            Collected?.Invoke(this);
        }

        private void ResolveReferences()
        {
            if (visualRoot == null)
            {
                Transform visualTransform = transform.Find("VisualRoot");
                if (visualTransform != null)
                {
                    visualRoot = visualTransform;
                }
            }

            if (pickupRenderer == null && visualRoot != null)
            {
                pickupRenderer = visualRoot.GetComponentInChildren<MeshRenderer>();
            }

            if (stateLabel == null)
            {
                stateLabel = GetComponentInChildren<TMP_Text>(true);
            }

            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<Collider>();
            }
        }

        private void CacheVisualPosition()
        {
            if (visualRoot == null)
            {
                return;
            }

            initialVisualLocalPosition = visualRoot.localPosition;
        }

        private void RefreshVisuals()
        {
            if (pickupRenderer != null)
            {
                Material material = Application.isPlaying ? pickupRenderer.material : pickupRenderer.sharedMaterial;
                if (material != null)
                {
                    material.color = isCollected ? collectedColor : idleColor;
                }
            }

            if (stateLabel != null)
            {
                stateLabel.text = isCollected ? "DONE" : "COLLECT";
            }
        }
    }
}
