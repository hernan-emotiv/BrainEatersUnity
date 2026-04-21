using BrainEaters.GameFlow;
using UnityEngine;

namespace BrainEaters.Turrets
{
    public class TurretController : MonoBehaviour
    {
        [SerializeField] private TurretActivationMode activationMode = TurretActivationMode.BuildZone;
        [SerializeField] private bool canBeDamaged = true;
        [SerializeField] private TurretBuildZone buildZone;
        [SerializeField] private CaptureZone captureZone;
        [SerializeField] private TurretHealth turretHealth;
        [SerializeField] private TurretTargeting targeting;
        [SerializeField] private TurretWeapon weapon;
        [SerializeField] private GameObject[] offlineObjects;
        [SerializeField] private GameObject[] onlineObjects;
        [SerializeField] private GameObject[] buildZoneObjects;

        public bool IsActive { get; private set; }
        private bool requiresRebuild;

        private void Awake()
        {
            ResolveReferences();
            ResetState();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            RefreshVisualState();
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void OnValidate()
        {
            ResolveReferences();
            RefreshVisualState();
        }

        private void Update()
        {
            if (!IsActive || targeting == null || weapon == null)
            {
                return;
            }

            var target = targeting.AcquireTarget();
            if (target == null || !target.IsAlive)
            {
                return;
            }

            Vector3 aimPoint = targeting.GetAimPoint(target);
            bool aligned = targeting.AimAt(aimPoint, Time.deltaTime);
            if (aligned)
            {
                weapon.TryFireAt(aimPoint);
            }
        }

        public void ResetState()
        {
            requiresRebuild = false;
            IsActive = activationMode == TurretActivationMode.Immediate;

            if (buildZone != null)
            {
                buildZone.ResetState();
            }

            if (weapon != null)
            {
                weapon.ResetState();
            }

            if (turretHealth != null)
            {
                turretHealth.ResetState();
            }

            RefreshVisualState();
        }

        public void Activate()
        {
            if (IsActive)
            {
                return;
            }

            requiresRebuild = false;
            IsActive = true;

            if (turretHealth != null)
            {
                turretHealth.SetOnlineState(true);
            }

            RefreshVisualState();
        }

        public void DeactivateForRebuild()
        {
            IsActive = false;
            requiresRebuild = true;

            if (weapon != null)
            {
                weapon.ResetState();
            }

            if (buildZone != null)
            {
                buildZone.ResetState();
            }

            if (turretHealth != null)
            {
                turretHealth.SetOnlineState(false);
            }

            RefreshVisualState();
        }

        private void HandleBuildCompleted()
        {
            if (ShouldUseBuildZone() || requiresRebuild)
            {
                Activate();
            }
        }

        private void HandleCaptureZoneCaptured(CaptureZone _)
        {
            if (activationMode == TurretActivationMode.CaptureZone)
            {
                RefreshVisualState();
            }
        }

        private void HandleTurretDestroyed(TurretHealth _)
        {
            if (canBeDamaged)
            {
                DeactivateForRebuild();
            }
        }

        private void Subscribe()
        {
            if (buildZone != null)
            {
                buildZone.BuildCompleted += HandleBuildCompleted;
            }

            if (captureZone != null)
            {
                captureZone.Captured += HandleCaptureZoneCaptured;
            }

            if (turretHealth != null)
            {
                turretHealth.Destroyed += HandleTurretDestroyed;
            }
        }

        private void Unsubscribe()
        {
            if (buildZone != null)
            {
                buildZone.BuildCompleted -= HandleBuildCompleted;
            }

            if (captureZone != null)
            {
                captureZone.Captured -= HandleCaptureZoneCaptured;
            }

            if (turretHealth != null)
            {
                turretHealth.Destroyed -= HandleTurretDestroyed;
            }
        }

        private void ResolveReferences()
        {
            if (buildZone == null)
            {
                buildZone = GetComponentInChildren<TurretBuildZone>(true);
            }

            if (targeting == null)
            {
                targeting = GetComponentInChildren<TurretTargeting>(true);
            }

            if (weapon == null)
            {
                weapon = GetComponentInChildren<TurretWeapon>(true);
            }

            if (turretHealth == null)
            {
                turretHealth = GetComponent<TurretHealth>();
            }

            if (buildZoneObjects == null || buildZoneObjects.Length == 0)
            {
                buildZoneObjects = FindChildObjectArray("BuildZone");
            }

            if (offlineObjects == null || offlineObjects.Length == 0)
            {
                offlineObjects = FindChildObjectArray("OfflineVisual");
            }

            if (onlineObjects == null || onlineObjects.Length == 0)
            {
                onlineObjects = FindChildObjectArray("OnlineVisual");
            }
        }

        private void RefreshVisualState()
        {
            bool captureUnlocked = IsCaptureUnlocked();
            bool showBuildZone = !IsActive && (ShouldUseBuildZone() || requiresRebuild || captureUnlocked);
            SetActiveForArray(buildZoneObjects, showBuildZone);
            SetActiveForArray(offlineObjects, !IsActive);
            SetActiveForArray(onlineObjects, IsActive);
        }

        private bool ShouldUseBuildZone()
        {
            return activationMode == TurretActivationMode.BuildZone ||
                (activationMode == TurretActivationMode.CaptureZone && captureZone == null);
        }

        private bool IsCaptureUnlocked()
        {
            return activationMode == TurretActivationMode.CaptureZone && captureZone != null && captureZone.IsCaptured;
        }

        private GameObject[] FindChildObjectArray(string childName)
        {
            Transform child = transform.Find(childName);
            if (child == null)
            {
                return System.Array.Empty<GameObject>();
            }

            return new[] { child.gameObject };
        }

        private static void SetActiveForArray(GameObject[] targets, bool active)
        {
            if (targets == null)
            {
                return;
            }

            for (int i = 0; i < targets.Length; i++)
            {
                if (targets[i] != null)
                {
                    targets[i].SetActive(active);
                }
            }
        }
    }
}
