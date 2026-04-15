using BrainEaters.Player;
using TMPro;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class CaptureZone : MonoBehaviour
    {
        public event System.Action<CaptureZone> Captured;

        [SerializeField] private float captureDurationSeconds = 3f;
        [SerializeField] private MeshRenderer zoneRenderer;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Color idleColor = new Color(0.2f, 0.7f, 1f, 0.35f);
        [SerializeField] private Color capturingColor = new Color(1f, 0.82f, 0.2f, 0.6f);
        [SerializeField] private Color capturedColor = new Color(0.25f, 1f, 0.4f, 0.8f);

        private PlayerController playerInside;
        private float captureProgress;
        private bool isCaptured;

        public bool IsCaptured => isCaptured;

        private void Awake()
        {
            ResolveReferences();
            RefreshVisuals();
        }

        private void OnValidate()
        {
            ResolveReferences();
            RefreshVisuals();
        }

        public void Configure(float durationSeconds)
        {
            captureDurationSeconds = Mathf.Max(0.1f, durationSeconds);
            captureProgress = 0f;
            isCaptured = false;
            RefreshVisuals();
        }

        public void ResetState()
        {
            captureProgress = 0f;
            isCaptured = false;
            playerInside = null;
            RefreshVisuals();
        }

        public void Tick(float deltaTime)
        {
            if (isCaptured)
            {
                return;
            }

            if (playerInside == null)
            {
                if (captureProgress > 0f)
                {
                    captureProgress = Mathf.Max(0f, captureProgress - deltaTime);
                    RefreshVisuals();
                }

                return;
            }

            captureProgress = Mathf.Min(captureDurationSeconds, captureProgress + deltaTime);
            if (captureProgress >= captureDurationSeconds)
            {
                isCaptured = true;
                captureProgress = captureDurationSeconds;
                RefreshVisuals();
                Captured?.Invoke(this);
                return;
            }

            RefreshVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (isCaptured)
            {
                return;
            }

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                playerInside = player;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null && player == playerInside)
            {
                playerInside = null;
            }
        }

        private void ResolveReferences()
        {
            if (zoneRenderer == null)
            {
                zoneRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (progressLabel == null)
            {
                progressLabel = GetComponentInChildren<TMP_Text>();
            }
        }

        private void RefreshVisuals()
        {
            if (zoneRenderer != null)
            {
                zoneRenderer.material.color = isCaptured
                    ? capturedColor
                    : playerInside != null ? capturingColor : idleColor;
            }

            if (progressLabel != null)
            {
                if (isCaptured)
                {
                    progressLabel.text = "CAPTURED";
                }
                else
                {
                    float normalized = captureDurationSeconds <= 0f ? 1f : captureProgress / captureDurationSeconds;
                    progressLabel.text = $"{Mathf.RoundToInt(normalized * 100f)}%";
                }
            }
        }
    }
}
