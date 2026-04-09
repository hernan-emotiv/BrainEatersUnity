using System.Collections.Generic;
using BrainEaters.Cameras;
using BrainEaters.Configs;
using BrainEaters.Player;
using BrainEaters.Spawning;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class GameManager : MonoBehaviour
    {
        public event System.Action<GameplayState> StateChanged;
        public event System.Action<GameplayReport> GameplayFinished;

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

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
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

            if (levelConfig.GameModeType != GameModeType.Survival)
            {
                return;
            }

            if (cachedPlayerHealth != null && !cachedPlayerHealth.IsAlive)
            {
                EndGameplay(GameplayState.Lost);
                return;
            }

            if (elapsedSurvivalTime >= levelConfig.SurvivalDurationSeconds)
            {
                EndGameplay(cachedPlayerHealth != null && cachedPlayerHealth.IsAlive ? GameplayState.Won : GameplayState.Lost);
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
            ResetKillTracking();
            elapsedSurvivalTime = 0f;
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
            Debug.Log("Back to menu requested. Not implemented yet.", this);
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
    }
}
