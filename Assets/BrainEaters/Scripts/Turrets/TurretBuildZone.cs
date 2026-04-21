using BrainEaters.Player;
using TMPro;
using UnityEngine;

namespace BrainEaters.Turrets
{
    public class TurretBuildZone : MonoBehaviour
    {
        public event System.Action<float, float> ProgressChanged;
        public event System.Action BuildCompleted;

        [SerializeField] private float buildDurationSeconds = 3f;
        [SerializeField] private Collider triggerCollider;
        [SerializeField] private MeshRenderer zoneRenderer;
        [SerializeField] private TMP_Text progressLabel;
        [SerializeField] private Color idleColor = new Color(1f, 0.65f, 0.2f, 0.25f);
        [SerializeField] private Color buildingColor = new Color(1f, 0.9f, 0.25f, 0.5f);
        [SerializeField] private Color completedColor = new Color(0.25f, 1f, 0.4f, 0.55f);

        private PlayerController playerInside;
        private float progressSeconds;

        public bool IsCompleted { get; private set; }
        public float ProgressSeconds => progressSeconds;
        public float BuildDurationSeconds => buildDurationSeconds;

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

        private void Update()
        {
            if (IsCompleted)
            {
                return;
            }

            if (playerInside != null)
            {
                progressSeconds = Mathf.Min(buildDurationSeconds, progressSeconds + Time.deltaTime);
                ProgressChanged?.Invoke(progressSeconds, buildDurationSeconds);
                RefreshVisuals();

                if (progressSeconds >= buildDurationSeconds)
                {
                    CompleteBuild();
                }
            }
            else if (progressSeconds > 0f)
            {
                progressSeconds = Mathf.Max(0f, progressSeconds - Time.deltaTime);
                ProgressChanged?.Invoke(progressSeconds, buildDurationSeconds);
                RefreshVisuals();
            }
        }

        public void ResetState()
        {
            ResolveReferences();
            IsCompleted = false;
            progressSeconds = 0f;
            playerInside = null;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = true;
            }

            ProgressChanged?.Invoke(progressSeconds, buildDurationSeconds);
            RefreshVisuals();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (IsCompleted)
            {
                return;
            }

            PlayerController player = other.GetComponentInParent<PlayerController>();
            if (player != null)
            {
                playerInside = player;
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (IsCompleted)
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

        private void CompleteBuild()
        {
            IsCompleted = true;
            progressSeconds = buildDurationSeconds;

            if (triggerCollider != null)
            {
                triggerCollider.enabled = false;
            }

            ProgressChanged?.Invoke(progressSeconds, buildDurationSeconds);
            RefreshVisuals();
            BuildCompleted?.Invoke();
        }

        private void ResolveReferences()
        {
            if (triggerCollider == null)
            {
                triggerCollider = GetComponent<Collider>();
            }

            if (zoneRenderer == null)
            {
                zoneRenderer = GetComponentInChildren<MeshRenderer>();
            }

            if (progressLabel == null)
            {
                progressLabel = GetComponentInChildren<TMP_Text>(true);
            }
        }

        private void RefreshVisuals()
        {
            if (zoneRenderer != null)
            {
                Material targetMaterial = Application.isPlaying ? zoneRenderer.material : zoneRenderer.sharedMaterial;
                if (targetMaterial != null)
                {
                    targetMaterial.color = IsCompleted
                        ? completedColor
                        : playerInside != null ? buildingColor : idleColor;
                }
            }

            if (progressLabel != null)
            {
                if (IsCompleted)
                {
                    progressLabel.text = "ONLINE";
                }
                else
                {
                    float normalized = buildDurationSeconds <= 0f ? 1f : progressSeconds / buildDurationSeconds;
                    progressLabel.text = $"BUILD {Mathf.RoundToInt(normalized * 100f)}%";
                }
            }
        }
    }
}
