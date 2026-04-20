using System.Collections.Generic;
using BrainEaters.Cameras;
using BrainEaters.Configs;
using BrainEaters.Player;
using BrainEaters.Spawning;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrainEaters.GameFlow
{
    public class GameManager : MonoBehaviour
    {
        public event System.Action<GameplayState> StateChanged;
        public event System.Action<GameplayReport> GameplayFinished;
        public event System.Action<GameModeType> ObjectiveModeChanged;
        public event System.Action<GameModeType, int, int> ObjectiveProgressChanged;

        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SpawnManager spawnManager;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private Transform levelRootParent;

        private LevelContext currentLevelInstance;
        private PlayerHealth cachedPlayerHealth;
        private float elapsedSurvivalTime;
        private float damageReceived;
        private int enemiesEliminated;
        private int capturedZonesCount;
        private int collectedPickupsCount;
        private readonly Dictionary<EnemyType, int> enemyKillCounts = new Dictionary<EnemyType, int>();
        private readonly Dictionary<EnemyType, string> enemyKillLabels = new Dictionary<EnemyType, string>();
        private bool levelRunning;
        private GameplayState currentState = GameplayState.None;

        public LevelConfig LevelConfig => levelConfig;
        public float ElapsedSurvivalTime => elapsedSurvivalTime;
        public float LevelDurationSeconds => levelConfig != null ? levelConfig.SurvivalDurationSeconds : 0f;
        public float RemainingSurvivalTime => Mathf.Max(0f, LevelDurationSeconds - elapsedSurvivalTime);
        public bool IsLevelRunning => levelRunning;
        public GameplayState CurrentState => currentState;
        public GameModeType ActiveGameMode => levelConfig != null ? levelConfig.GameModeType : GameModeType.Survival;
        public int CapturedZonesCount => capturedZonesCount;
        public int TotalCaptureZones => currentLevelInstance != null ? currentLevelInstance.CaptureZones.Count : 0;
        public int CollectedPickupsCount => collectedPickupsCount;
        public int TotalCollectPickups => currentLevelInstance != null ? currentLevelInstance.CollectPickups.Count : 0;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            if (LevelSession.SelectedLevel != null)
            {
                levelConfig = LevelSession.SelectedLevel;
            }
            else if (LevelSession.ActiveCampaign == null && levelConfig != null)
            {
                LevelSession.SetCampaign(null);
            }

            StartLevel(levelConfig);
        }

        private void OnDestroy()
        {
            UnsubscribeFromGameplayEvents();
        }

        private void Update()
        {
            if (!levelRunning || levelConfig == null || currentState != GameplayState.Running)
            {
                return;
            }

            elapsedSurvivalTime += Time.deltaTime;

            if (cachedPlayerHealth != null && !cachedPlayerHealth.IsAlive)
            {
                EndGameplay(GameplayState.Lost);
                return;
            }

            if (levelConfig.GameModeType == GameModeType.Survival)
            {
                if (elapsedSurvivalTime >= levelConfig.SurvivalDurationSeconds)
                {
                    EndGameplay(cachedPlayerHealth != null && cachedPlayerHealth.IsAlive ? GameplayState.Won : GameplayState.Lost);
                }
                return;
            }

            if (levelConfig.GameModeType == GameModeType.Capture)
            {
                TickCaptureMode(Time.deltaTime);
                return;
            }

            if (levelConfig.GameModeType == GameModeType.Collect)
            {
                TickCollectMode();
            }
        }

        public void StartLevel(LevelConfig config)
        {
            levelConfig = config;
            InitializeLevel();
        }

        public void InitializeLevel()
        {
            ResolveReferences();
            SetState(GameplayState.Initializing);
            UnsubscribeFromGameplayEvents();

            if (levelConfig == null)
            {
                levelRunning = false;
                Debug.LogError("GameManager requires a LevelConfig.", this);
                return;
            }

            if (playerController == null)
            {
                levelRunning = false;
                Debug.LogError("GameManager requires a PlayerController reference.", this);
                return;
            }

            if (spawnManager == null)
            {
                levelRunning = false;
                Debug.LogError("GameManager requires a SpawnManager reference.", this);
                return;
            }

            if (levelConfig.SpawnConfig == null)
            {
                levelRunning = false;
                Debug.LogError("LevelConfig requires a SpawnConfig.", this);
                return;
            }

            if (levelConfig.LevelPrefab == null)
            {
                levelRunning = false;
                Debug.LogError("LevelConfig requires a LevelPrefab.", this);
                return;
            }

            if (cameraFollow != null)
            {
                cameraFollow.SetTarget(playerController.transform);
                playerController.SetCamera(cameraFollow.transform);
            }
            else if (Camera.main != null)
            {
                playerController.SetCamera(Camera.main.transform);
            }

            InstantiateSelectedLevel();

            if (currentLevelInstance == null || currentLevelInstance.SpawnPoints.Count == 0)
            {
                levelRunning = false;
                Debug.LogError("GameManager could not resolve spawn points from the instantiated level.", this);
                return;
            }

            PositionPlayerAtSpawn();

            cachedPlayerHealth = playerController.GetComponent<PlayerHealth>();
            if (cachedPlayerHealth != null)
            {
                playerController.enabled = true;
                cachedPlayerHealth.ResetState();
            }

            spawnManager.Initialize(levelConfig, playerController.transform, new List<SpawnPoint>(currentLevelInstance.SpawnPoints));
            SubscribeToGameplayEvents();
            damageReceived = 0f;
            enemiesEliminated = 0;
            capturedZonesCount = 0;
            collectedPickupsCount = 0;
            ResetKillTracking();
            elapsedSurvivalTime = 0f;
            ConfigureLevelObjectives();
            NotifyObjectiveModeChanged();
            NotifyObjectiveProgressChanged();
            levelRunning = true;
            SetState(GameplayState.Running);

            Debug.Log($"Initialized level: {levelConfig.name}", this);
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (playerController == null)
            {
                playerController = FindFirstObjectByType<PlayerController>();
            }

            if (spawnManager == null)
            {
                spawnManager = FindFirstObjectByType<SpawnManager>();
            }

            if (cameraFollow == null)
            {
                cameraFollow = FindFirstObjectByType<CameraFollow>();
            }

            if (levelRootParent == null)
            {
                levelRootParent = transform;
            }
        }

        private void InstantiateSelectedLevel()
        {
            if (currentLevelInstance != null)
            {
                Destroy(currentLevelInstance.gameObject);
                currentLevelInstance = null;
            }

            currentLevelInstance = Instantiate(levelConfig.LevelPrefab, Vector3.zero, Quaternion.identity, levelRootParent);
            currentLevelInstance.name = $"{levelConfig.LevelPrefab.name}_Instance";
            currentLevelInstance.RefreshSpawnPointsIfNeeded();
        }

        private void PositionPlayerAtSpawn()
        {
            if (playerController == null || currentLevelInstance == null || currentLevelInstance.PlayerSpawnPoint == null)
            {
                return;
            }

            Transform playerTransform = playerController.transform;
            CharacterController characterController = playerController.GetComponent<CharacterController>();
            bool reenableCharacterController = characterController != null && characterController.enabled;
            if (reenableCharacterController)
            {
                characterController.enabled = false;
            }

            playerTransform.SetPositionAndRotation(currentLevelInstance.PlayerSpawnPoint.Position, currentLevelInstance.PlayerSpawnPoint.Rotation);

            if (reenableCharacterController)
            {
                characterController.enabled = true;
            }
        }

        private void EndGameplay(GameplayState resultState)
        {
            if (currentState == GameplayState.Won || currentState == GameplayState.Lost)
            {
                return;
            }

            levelRunning = false;

            if (spawnManager != null)
            {
                spawnManager.SetSpawningEnabled(false);
            }

            SetState(resultState);

            if (resultState == GameplayState.Won)
            {
                LevelProgressionService.RegisterVictory(LevelSession.ActiveCampaign, levelConfig);
            }

            GameplayReport report = new GameplayReport(
                resultState,
                enemiesEliminated,
                damageReceived,
                elapsedSurvivalTime,
                LevelDurationSeconds,
                BuildKillStats());

            GameplayFinished?.Invoke(report);
            Debug.Log($"Gameplay finished with state {resultState}. Kills: {enemiesEliminated}, damage received: {damageReceived:0.##}.", this);
        }

        public void RetryLevel()
        {
            InitializeLevel();
        }

        public void BackToMenu()
        {
            string sceneName = LevelSession.ActiveCampaign != null
                ? LevelSession.ActiveCampaign.LevelSelectSceneName
                : "LevelSelectScene";

            LevelSession.ClearSelectedLevel();
            SceneManager.LoadScene(sceneName);
        }

        private void SetState(GameplayState newState)
        {
            if (currentState == newState)
            {
                return;
            }

            currentState = newState;
            StateChanged?.Invoke(currentState);
        }

        private void SubscribeToGameplayEvents()
        {
            if (cachedPlayerHealth != null)
            {
                cachedPlayerHealth.Damaged += HandlePlayerDamaged;
                cachedPlayerHealth.Died += HandlePlayerDied;
            }

            if (spawnManager != null)
            {
                spawnManager.EnemyEliminated += HandleEnemyEliminated;
            }

            if (currentLevelInstance != null)
            {
                for (int i = 0; i < currentLevelInstance.CaptureZones.Count; i++)
                {
                    CaptureZone captureZone = currentLevelInstance.CaptureZones[i];
                    if (captureZone == null)
                    {
                        continue;
                    }

                    captureZone.Captured += HandleCaptureZoneCaptured;
                }

                for (int i = 0; i < currentLevelInstance.CollectPickups.Count; i++)
                {
                    CollectPickup collectPickup = currentLevelInstance.CollectPickups[i];
                    if (collectPickup == null)
                    {
                        continue;
                    }

                    collectPickup.Collected += HandleCollectPickupCollected;
                }
            }
        }

        private void UnsubscribeFromGameplayEvents()
        {
            if (cachedPlayerHealth != null)
            {
                cachedPlayerHealth.Damaged -= HandlePlayerDamaged;
                cachedPlayerHealth.Died -= HandlePlayerDied;
            }

            if (spawnManager != null)
            {
                spawnManager.EnemyEliminated -= HandleEnemyEliminated;
            }

            if (currentLevelInstance != null)
            {
                for (int i = 0; i < currentLevelInstance.CaptureZones.Count; i++)
                {
                    CaptureZone captureZone = currentLevelInstance.CaptureZones[i];
                    if (captureZone == null)
                    {
                        continue;
                    }

                    captureZone.Captured -= HandleCaptureZoneCaptured;
                }

                for (int i = 0; i < currentLevelInstance.CollectPickups.Count; i++)
                {
                    CollectPickup collectPickup = currentLevelInstance.CollectPickups[i];
                    if (collectPickup == null)
                    {
                        continue;
                    }

                    collectPickup.Collected -= HandleCollectPickupCollected;
                }
            }
        }

        private void ConfigureLevelObjectives()
        {
            if (currentLevelInstance == null)
            {
                return;
            }

            for (int i = 0; i < currentLevelInstance.CaptureZones.Count; i++)
            {
                CaptureZone captureZone = currentLevelInstance.CaptureZones[i];
                if (captureZone == null)
                {
                    continue;
                }

                captureZone.Configure(levelConfig.CaptureDurationSeconds);
                captureZone.ResetState();
            }

            for (int i = 0; i < currentLevelInstance.CollectPickups.Count; i++)
            {
                CollectPickup collectPickup = currentLevelInstance.CollectPickups[i];
                if (collectPickup == null)
                {
                    continue;
                }

                collectPickup.ResetState();
            }
        }

        private void TickCaptureMode(float deltaTime)
        {
            if (currentLevelInstance == null)
            {
                return;
            }

            int totalZones = currentLevelInstance.CaptureZones.Count;
            if (totalZones == 0)
            {
                return;
            }

            int capturedCount = 0;
            for (int i = 0; i < totalZones; i++)
            {
                CaptureZone captureZone = currentLevelInstance.CaptureZones[i];
                if (captureZone == null)
                {
                    continue;
                }

                captureZone.Tick(deltaTime);
                if (captureZone.IsCaptured)
                {
                    capturedCount++;
                }
            }

            capturedZonesCount = capturedCount;
            if (capturedZonesCount >= totalZones)
            {
                EndGameplay(GameplayState.Won);
            }
        }

        private void TickCollectMode()
        {
            if (currentLevelInstance == null)
            {
                return;
            }

            int totalPickups = currentLevelInstance.CollectPickups.Count;
            if (totalPickups == 0)
            {
                return;
            }

            if (collectedPickupsCount >= totalPickups)
            {
                EndGameplay(GameplayState.Won);
            }
        }

        private void NotifyObjectiveModeChanged()
        {
            ObjectiveModeChanged?.Invoke(ActiveGameMode);
        }

        private void NotifyObjectiveProgressChanged()
        {
            switch (ActiveGameMode)
            {
                case GameModeType.Capture:
                    ObjectiveProgressChanged?.Invoke(GameModeType.Capture, capturedZonesCount, TotalCaptureZones);
                    break;
                case GameModeType.Collect:
                    ObjectiveProgressChanged?.Invoke(GameModeType.Collect, collectedPickupsCount, TotalCollectPickups);
                    break;
            }
        }

        private void ResetKillTracking()
        {
            enemyKillCounts.Clear();
            enemyKillLabels.Clear();

            if (levelConfig == null)
            {
                return;
            }

            foreach (LevelEnemyDefinition definition in levelConfig.EnemyTypes)
            {
                if (definition == null || definition.EnemyConfig == null)
                {
                    continue;
                }

                EnemyType enemyType = definition.EnemyConfig.EnemyType;
                if (!enemyKillCounts.ContainsKey(enemyType))
                {
                    enemyKillCounts.Add(enemyType, 0);
                    enemyKillLabels.Add(enemyType, definition.EnemyConfig.DisplayName);
                }
            }
        }

        private List<GameplayKillStat> BuildKillStats()
        {
            List<GameplayKillStat> stats = new List<GameplayKillStat>(enemyKillCounts.Count);
            foreach (KeyValuePair<EnemyType, int> entry in enemyKillCounts)
            {
                string displayName = enemyKillLabels.TryGetValue(entry.Key, out string label) ? label : entry.Key.ToString();
                stats.Add(new GameplayKillStat(entry.Key, displayName, entry.Value));
            }

            stats.Sort((left, right) => left.EnemyType.CompareTo(right.EnemyType));
            return stats;
        }

        private void HandlePlayerDamaged(float amount)
        {
            damageReceived += amount;
        }

        private void HandlePlayerDied(PlayerHealth _)
        {
            EndGameplay(GameplayState.Lost);
        }

        private void HandleEnemyEliminated(EnemyConfig enemyConfig)
        {
            enemiesEliminated++;

            if (enemyConfig == null)
            {
                return;
            }

            EnemyType enemyType = enemyConfig.EnemyType;
            if (!enemyKillCounts.ContainsKey(enemyType))
            {
                enemyKillCounts.Add(enemyType, 0);
                enemyKillLabels[enemyType] = enemyConfig.DisplayName;
            }

            enemyKillCounts[enemyType]++;
        }

        private void HandleCaptureZoneCaptured(CaptureZone _)
        {
            if (currentLevelInstance == null)
            {
                return;
            }

            int capturedCount = 0;
            for (int i = 0; i < currentLevelInstance.CaptureZones.Count; i++)
            {
                CaptureZone captureZone = currentLevelInstance.CaptureZones[i];
                if (captureZone != null && captureZone.IsCaptured)
                {
                    capturedCount++;
                }
            }

            capturedZonesCount = capturedCount;
            NotifyObjectiveProgressChanged();
        }

        private void HandleCollectPickupCollected(CollectPickup _)
        {
            if (currentLevelInstance == null)
            {
                return;
            }

            int collectedCount = 0;
            for (int i = 0; i < currentLevelInstance.CollectPickups.Count; i++)
            {
                CollectPickup collectPickup = currentLevelInstance.CollectPickups[i];
                if (collectPickup != null && collectPickup.IsCollected)
                {
                    collectedCount++;
                }
            }

            collectedPickupsCount = collectedCount;
            NotifyObjectiveProgressChanged();
            if (collectedPickupsCount >= currentLevelInstance.CollectPickups.Count && currentLevelInstance.CollectPickups.Count > 0)
            {
                EndGameplay(GameplayState.Won);
            }
        }
    }
}
