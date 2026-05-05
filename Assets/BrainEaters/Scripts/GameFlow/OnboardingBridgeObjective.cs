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
        [SerializeField] private float leftGateOpenAngle = 105f;
        [SerializeField] private float rightGateOpenAngle = -105f;
        [SerializeField] private bool returnBridgeAfterLaunch = true;
        [SerializeField] private float bridgeReturnDelaySeconds = 1.6f;
        [SerializeField] private float bridgeReturnDuration = 0.5f;
        [SerializeField] private bool stopSpawningOnActivation = true;
        [SerializeField] private bool killRemainingEnemiesOnActivation = true;
        [SerializeField] private float killRemainingEnemiesDelay = 1.25f;
        [SerializeField] private bool launchAllRemainingEnemiesOnActivation = true;
        [SerializeField] private float fallbackLaunchHorizontalImpulse = 10f;
        [SerializeField] private float fallbackLaunchUpwardImpulse = 6f;
        [SerializeField] private float fallbackLaunchTorqueImpulse = 8f;

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

        private void OnEnable()
        {
            ResolveReferences();
            if (gateTarget != null)
            {
                gateTarget.Destroyed += HandleGateDestroyed;
            }
        }

        private void OnDisable()
        {
            if (gateTarget != null)
            {
                gateTarget.Destroyed -= HandleGateDestroyed;
            }
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

            ResolveReferences();
            hasActivated = true;
            if (stopSpawningOnActivation)
            {
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

        private void HandleGateDestroyed()
        {
            if (hasActivated)
            {
                return;
            }

            hasActivated = true;
            if (activationIndicator != null)
            {
                activationIndicator.gameObject.SetActive(false);
            }

            if (activationRoutine != null)
            {
                StopCoroutine(activationRoutine);
            }

            activationRoutine = StartCoroutine(OpenGate());
        }

        private IEnumerator PlayActivation()
        {
            ResolveReferences();

            if (launchDelaySeconds > 0f)
            {
                yield return new WaitForSeconds(launchDelaySeconds);
            }

            int launchedCount = launchZone != null ? launchZone.LaunchTrackedEnemies() : 0;
            if (launchAllRemainingEnemiesOnActivation)
            {
                launchedCount += LaunchRemainingEnemies();
            }

            if (killRemainingEnemiesOnActivation || launchAllRemainingEnemiesOnActivation)
            {
                StartCoroutine(KillRemainingEnemiesAfterDelay());
            }

            if (bridgePivot == null)
            {
                Debug.LogWarning("Onboarding bridge activated, but no BridgePivot reference was found. The gate will open, but the bridge cannot launch.", this);
            }

            Debug.Log($"Onboarding bridge activated. Enemies launched: {launchedCount}. BridgePivot: {(bridgePivot != null ? bridgePivot.name : "None")}.", this);

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
                gateTarget.DisableTarget(!hasActivated);
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

        private int LaunchRemainingEnemies()
        {
            EnemyHealth[] enemies = FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None);
            Transform launchOrigin = gateRoot != null ? gateRoot : transform;
            int launchedCount = 0;

            for (int i = 0; i < enemies.Length; i++)
            {
                EnemyHealth enemy = enemies[i];
                if (enemy == null || !enemy.IsAlive)
                {
                    continue;
                }

                EnemyPhysicsLaunch launcher = enemy.GetComponent<EnemyPhysicsLaunch>();
                if (launcher != null && launcher.IsLaunching)
                {
                    continue;
                }

                if (launcher == null)
                {
                    launcher = enemy.gameObject.AddComponent<EnemyPhysicsLaunch>();
                }

                Vector3 direction = enemy.transform.position - launchOrigin.position;
                direction.y = 0f;
                if (direction.sqrMagnitude <= 0.0001f)
                {
                    direction = launchOrigin.forward;
                }

                direction.Normalize();
                Vector3 force = direction * fallbackLaunchHorizontalImpulse + Vector3.up * fallbackLaunchUpwardImpulse;
                Vector3 torque = Random.insideUnitSphere * fallbackLaunchTorqueImpulse;
                launcher.LaunchAndKill(force, torque, killRemainingEnemiesDelay);
                launchedCount++;
            }

            return launchedCount;
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
            Transform searchRoot = transform.parent != null ? transform.parent : transform.root;
            if (bridgePivot == null)
            {
                bridgePivot = FindChild(searchRoot, "BridgePivot");
            }

            if (gateRoot == null)
            {
                gateRoot = FindChild(searchRoot, "ClosedGate");
            }

            if (gateBlocker == null && gateRoot != null)
            {
                gateBlocker = gateRoot.GetComponent<Collider>();
            }

            if (launchZone == null)
            {
                launchZone = GetComponentInChildren<OnboardingBridgeLaunchZone>(true);
                if (launchZone == null && searchRoot != null)
                {
                    launchZone = searchRoot.GetComponentInChildren<OnboardingBridgeLaunchZone>(true);
                }
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

        private static Transform FindChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            if (root.name == childName)
            {
                return root;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                Transform match = FindChild(child, childName);
                if (match != null)
                {
                    return match;
                }
            }

            return null;
        }
    }
}
