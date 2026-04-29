using System.Collections;
using BrainEaters.Core;
using BrainEaters.Enemies;
using BrainEaters.Spawning;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class OnboardingBridgeObjective : MonoBehaviour, IBombActivatable
    {
        [SerializeField] private Transform bridgePivot;
        [SerializeField] private Transform gateRoot;
        [SerializeField] private Transform leftGatePivot;
        [SerializeField] private Transform rightGatePivot;
        [SerializeField] private Collider gateBlocker;
        [SerializeField] private OnboardingGateTarget gateTarget;
        [SerializeField] private OnboardingBridgeLaunchZone launchZone;
        [SerializeField] private SpawnManager spawnManager;
        [SerializeField] private Transform activationIndicator;
        [SerializeField] private float bridgeRaiseAngle = -62f;
        [SerializeField] private float bridgeRaiseDuration = 0.42f;
        [SerializeField] private float launchDelaySeconds = 0.12f;
        [SerializeField] private float gateOpenDuration = 0.55f;
        [SerializeField] private float leftGateOpenAngle = -105f;
        [SerializeField] private float rightGateOpenAngle = 105f;
        [SerializeField] private bool returnBridgeAfterLaunch = true;
        [SerializeField] private float bridgeReturnDelaySeconds = 1.6f;
        [SerializeField] private float bridgeReturnDuration = 0.5f;
        [SerializeField] private bool stopSpawningOnActivation = true;
        [SerializeField] private bool killRemainingEnemiesOnActivation = true;
        [SerializeField] private float killRemainingEnemiesDelay = 1.25f;

        private Quaternion bridgeClosedRotation;
        private Vector3 gateClosedPosition;
        private Quaternion leftGateClosedRotation;
        private Quaternion rightGateClosedRotation;
        private bool hasActivated;
        private Coroutine activationRoutine;

        public bool CanActivateBomb => !hasActivated;

        private void Awake()
        {
            CacheInitialState();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void ActivateBomb()
        {
            if (!CanActivateBomb)
            {
                return;
            }

            hasActivated = true;
            if (stopSpawningOnActivation)
            {
                ResolveReferences();
                spawnManager?.SetSpawningEnabled(false);
            }

            if (activationIndicator != null)
            {
                activationIndicator.gameObject.SetActive(false);
            }

            if (activationRoutine != null)
            {
                StopCoroutine(activationRoutine);
            }

            activationRoutine = StartCoroutine(PlayActivation());
        }

        public void ResetObjective()
        {
            hasActivated = false;
            if (bridgePivot != null)
            {
                bridgePivot.localRotation = bridgeClosedRotation;
            }

            if (gateRoot != null)
            {
                gateRoot.localPosition = gateClosedPosition;
            }

            if (leftGatePivot != null)
            {
                leftGatePivot.localRotation = leftGateClosedRotation;
            }

            if (rightGatePivot != null)
            {
                rightGatePivot.localRotation = rightGateClosedRotation;
            }

            if (gateBlocker != null)
            {
                gateBlocker.enabled = true;
            }

            if (gateTarget != null)
            {
                gateTarget.ResetGateTarget();
            }

            if (activationIndicator != null)
            {
                activationIndicator.gameObject.SetActive(true);
            }
        }

        private IEnumerator PlayActivation()
        {
            if (launchDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(launchDelaySeconds);
            }

            launchZone?.LaunchTrackedEnemies();
            if (killRemainingEnemiesOnActivation)
            {
                StartCoroutine(KillRemainingEnemiesAfterDelay());
            }

            yield return RotateBridge(bridgeClosedRotation, bridgeClosedRotation * Quaternion.Euler(bridgeRaiseAngle, 0f, 0f), bridgeRaiseDuration);
            yield return OpenGate();

            if (returnBridgeAfterLaunch)
            {
                yield return new WaitForSeconds(Mathf.Max(0f, bridgeReturnDelaySeconds));
                yield return RotateBridge(bridgePivot != null ? bridgePivot.localRotation : bridgeClosedRotation, bridgeClosedRotation, bridgeReturnDuration);
            }
        }

        private IEnumerator RotateBridge(Quaternion from, Quaternion to, float duration)
        {
            if (bridgePivot == null)
            {
                yield break;
            }

            if (duration <= 0f)
            {
                bridgePivot.localRotation = to;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                bridgePivot.localRotation = Quaternion.Slerp(from, to, eased);
                yield return null;
            }

            bridgePivot.localRotation = to;
        }

        private IEnumerator OpenGate()
        {
            if (gateBlocker != null)
            {
                gateBlocker.enabled = false;
            }

            if (gateTarget != null)
            {
                gateTarget.DisableTarget();
            }

            bool hasDoorPivots = leftGatePivot != null || rightGatePivot != null;
            if (!hasDoorPivots && gateRoot == null)
            {
                yield break;
            }

            if (gateOpenDuration <= 0f)
            {
                ApplyGateOpenState(1f);
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < gateOpenDuration)
            {
                elapsed += Time.deltaTime;
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / gateOpenDuration));
                ApplyGateOpenState(eased);
                yield return null;
            }

            ApplyGateOpenState(1f);
        }

        private void ApplyGateOpenState(float normalized)
        {
            bool hasDoorPivots = leftGatePivot != null || rightGatePivot != null;
            if (hasDoorPivots)
            {
                if (leftGatePivot != null)
                {
                    Quaternion leftOpen = leftGateClosedRotation * Quaternion.Euler(0f, leftGateOpenAngle, 0f);
                    leftGatePivot.localRotation = Quaternion.Slerp(leftGateClosedRotation, leftOpen, normalized);
                }

                if (rightGatePivot != null)
                {
                    Quaternion rightOpen = rightGateClosedRotation * Quaternion.Euler(0f, rightGateOpenAngle, 0f);
                    rightGatePivot.localRotation = Quaternion.Slerp(rightGateClosedRotation, rightOpen, normalized);
                }

                return;
            }

            if (gateRoot != null)
            {
                gateRoot.localPosition = Vector3.Lerp(gateClosedPosition, gateClosedPosition + Vector3.up * 3.2f, normalized);
            }
        }

        private IEnumerator KillRemainingEnemiesAfterDelay()
        {
            yield return new WaitForSeconds(Mathf.Max(0f, killRemainingEnemiesDelay));

            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyHealth enemy = enemies[i];
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.Kill();
                }
            }
        }

        private void CacheInitialState()
        {
            ResolveReferences();
            bridgeClosedRotation = bridgePivot != null ? bridgePivot.localRotation : Quaternion.identity;
            gateClosedPosition = gateRoot != null ? gateRoot.localPosition : Vector3.zero;
            leftGateClosedRotation = leftGatePivot != null ? leftGatePivot.localRotation : Quaternion.identity;
            rightGateClosedRotation = rightGatePivot != null ? rightGatePivot.localRotation : Quaternion.identity;
            ResetObjective();
        }

        private void ResolveReferences()
        {
            if (launchZone == null)
            {
                launchZone = GetComponentInChildren<OnboardingBridgeLaunchZone>(true);
            }

            if (gateTarget == null && gateRoot != null)
            {
                gateTarget = gateRoot.GetComponentInChildren<OnboardingGateTarget>(true);
            }

            if (leftGatePivot == null && gateRoot != null)
            {
                Transform foundLeft = gateRoot.transform.Find("LeftDoorPivot");
                leftGatePivot = foundLeft != null ? foundLeft : null;
            }

            if (rightGatePivot == null && gateRoot != null)
            {
                Transform foundRight = gateRoot.transform.Find("RightDoorPivot");
                rightGatePivot = foundRight != null ? foundRight : null;
            }

            if (spawnManager == null)
            {
                spawnManager = FindFirstObjectByType<SpawnManager>();
            }
        }
    }
}
