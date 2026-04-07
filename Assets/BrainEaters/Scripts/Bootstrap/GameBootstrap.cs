using System.Collections.Generic;
using BrainEaters.Cameras;
using BrainEaters.Enemies;
using BrainEaters.Input;
using BrainEaters.Player;
using BrainEaters.Spawning;
using UnityEngine;

namespace BrainEaters.Bootstrap
{
    public class GameBootstrap : MonoBehaviour
    {
        [Header("Arena")]
        [SerializeField] private Vector2 arenaSize = new Vector2(28f, 28f);
        [SerializeField] private float wallHeight = 3f;
        [SerializeField] private float wallThickness = 1f;

        [Header("Defaults")]
        [SerializeField] private int defaultSpawnPointCount = 4;
        [SerializeField] private Vector3 playerStartPosition = new Vector3(0f, 1f, 0f);

        private PlayerController playerController;
        private SpawnManager spawnManager;

        private void Start()
        {
            EnsureArena();
            playerController = EnsurePlayer();
            EnsureCamera(playerController.transform);
            spawnManager = EnsureSpawnManager(playerController.transform);
        }

        private void EnsureArena()
        {
            if (GameObject.Find("ArenaFloor") != null)
            {
                return;
            }

            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "ArenaFloor";
            floor.transform.position = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(arenaSize.x, 1f, arenaSize.y);

            CreateWall("Wall_North", new Vector3(0f, wallHeight * 0.5f, arenaSize.y * 0.5f), new Vector3(arenaSize.x, wallHeight, wallThickness));
            CreateWall("Wall_South", new Vector3(0f, wallHeight * 0.5f, -arenaSize.y * 0.5f), new Vector3(arenaSize.x, wallHeight, wallThickness));
            CreateWall("Wall_East", new Vector3(arenaSize.x * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, arenaSize.y));
            CreateWall("Wall_West", new Vector3(-arenaSize.x * 0.5f, wallHeight * 0.5f, 0f), new Vector3(wallThickness, wallHeight, arenaSize.y));
        }

        private PlayerController EnsurePlayer()
        {
            PlayerController existingPlayer = FindFirstObjectByType<PlayerController>();
            if (existingPlayer != null)
            {
                return existingPlayer;
            }

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = playerStartPosition;

            CapsuleCollider capsuleCollider = player.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                capsuleCollider.enabled = false;
            }

            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = player.AddComponent<CharacterController>();
            }

            characterController.center = new Vector3(0f, 1f, 0f);
            characterController.height = 2f;
            characterController.radius = 0.45f;

            KeyboardMouseInputSource keyboardMouseInputSource = player.AddComponent<KeyboardMouseInputSource>();
            PlayerInputRouter inputRouter = player.AddComponent<PlayerInputRouter>();
            inputRouter.SetInputSource(keyboardMouseInputSource);

            player.AddComponent<PlayerMovement>();
            player.AddComponent<PlayerEnergyCharge>();
            player.AddComponent<PlayerBombAttack>();

            return player.AddComponent<PlayerController>();
        }

        private void EnsureCamera(Transform target)
        {
            CameraFollow existingFollow = FindFirstObjectByType<CameraFollow>();
            if (existingFollow != null)
            {
                existingFollow.SetTarget(target);
                playerController.SetCamera(existingFollow.transform);
                return;
            }

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }

            CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();
            if (cameraFollow == null)
            {
                cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
            }

            cameraFollow.SetTarget(target);
            mainCamera.transform.position = target.position + new Vector3(0f, 8f, -8f);
            mainCamera.transform.LookAt(target.position + Vector3.up * 1.5f);
            playerController.SetCamera(mainCamera.transform);
        }

        private SpawnManager EnsureSpawnManager(Transform playerTarget)
        {
            SpawnManager existingManager = FindFirstObjectByType<SpawnManager>();
            if (existingManager != null)
            {
                existingManager.SetPlayerTarget(playerTarget);
                SpawnPoint[] existingScenePoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
                if (existingScenePoints.Length == 0)
                {
                    existingScenePoints = CreateDefaultSpawnPoints().ToArray();
                }

                existingManager.SetSpawnPoints(new List<SpawnPoint>(existingScenePoints));
                existingManager.SetEnemyPrefab(CreateEnemyTemplate());
                return existingManager;
            }

            GameObject managerObject = new GameObject("SpawnManager");
            SpawnManager manager = managerObject.AddComponent<SpawnManager>();
            manager.SetPlayerTarget(playerTarget);

            SpawnPoint[] existingPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            if (existingPoints.Length == 0)
            {
                existingPoints = CreateDefaultSpawnPoints().ToArray();
            }

            manager.SetSpawnPoints(new List<SpawnPoint>(existingPoints));
            manager.SetEnemyPrefab(CreateEnemyTemplate());
            return manager;
        }

        private List<SpawnPoint> CreateDefaultSpawnPoints()
        {
            List<SpawnPoint> points = new List<SpawnPoint>(defaultSpawnPointCount);
            GameObject root = new GameObject("SpawnPoints");

            float xExtent = arenaSize.x * 0.35f;
            float zExtent = arenaSize.y * 0.35f;

            Vector3[] positions =
            {
                new Vector3(-xExtent, 0.5f, -zExtent),
                new Vector3(-xExtent, 0.5f, zExtent),
                new Vector3(xExtent, 0.5f, -zExtent),
                new Vector3(xExtent, 0.5f, zExtent)
            };

            int count = Mathf.Clamp(defaultSpawnPointCount, 1, positions.Length);
            for (int i = 0; i < count; i++)
            {
                GameObject pointObject = new GameObject($"SpawnPoint_{i + 1}");
                pointObject.transform.SetParent(root.transform);
                pointObject.transform.position = positions[i];
                points.Add(pointObject.AddComponent<SpawnPoint>());
            }

            return points;
        }

        private EnemyController CreateEnemyTemplate()
        {
            EnemyController existingTemplate = FindTemplateEnemy();
            if (existingTemplate != null)
            {
                return existingTemplate;
            }

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "EnemyTemplate";
            enemy.transform.position = new Vector3(0f, 0.75f, 0f);
            enemy.transform.localScale = new Vector3(1f, 1.5f, 1f);

            enemy.AddComponent<EnemyMovement>();
            enemy.AddComponent<EnemyHealth>();
            EnemyController enemyController = enemy.AddComponent<EnemyController>();
            enemy.SetActive(false);
            return enemyController;
        }

        private EnemyController FindTemplateEnemy()
        {
            EnemyController[] enemies = FindObjectsByType<EnemyController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (EnemyController enemy in enemies)
            {
                if (!enemy.gameObject.activeInHierarchy && enemy.name.StartsWith("EnemyTemplate"))
                {
                    return enemy;
                }
            }

            return null;
        }

        private void CreateWall(string wallName, Vector3 position, Vector3 scale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = wallName;
            wall.transform.position = position;
            wall.transform.localScale = scale;
        }
    }
}
