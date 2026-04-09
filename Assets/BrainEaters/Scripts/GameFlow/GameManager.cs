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
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private PlayerController playerController;
        [SerializeField] private SpawnManager spawnManager;
        [SerializeField] private CameraFollow cameraFollow;
        [SerializeField] private Transform levelRootParent;

        private LevelContext currentLevelInstance;

        private float elapsedSurvivalTime;
        private bool levelRunning;

        public LevelConfig LevelConfig => levelConfig;
        public float ElapsedSurvivalTime => elapsedSurvivalTime;
        public float LevelDurationSeconds => levelConfig != null ? levelConfig.SurvivalDurationSeconds : 0f;
        public float RemainingSurvivalTime => Mathf.Max(0f, LevelDurationSeconds - elapsedSurvivalTime);
        public bool IsLevelRunning => levelRunning;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Start()
        {
            StartLevel(levelConfig);
        }

        private void Update()
        {
            if (!levelRunning || levelConfig == null)
            {
                return;
            }

            elapsedSurvivalTime += Time.deltaTime;

            if (levelConfig.GameModeType != GameModeType.Survival)
            {
                return;
            }

            if (elapsedSurvivalTime >= levelConfig.SurvivalDurationSeconds)
            {
                levelRunning = false;
                spawnManager.SetSpawningEnabled(false);
                Debug.Log($"Level complete. Survived {levelConfig.SurvivalDurationSeconds:0.0} seconds.", this);
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

            PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerController.enabled = true;
                playerHealth.ResetState();
            }

            spawnManager.Initialize(levelConfig, playerController.transform, new List<SpawnPoint>(currentLevelInstance.SpawnPoints));
            elapsedSurvivalTime = 0f;
            levelRunning = true;

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
    }
}
