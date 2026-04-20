using BrainEaters.GameFlow;
using BrainEaters.Spawning;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class CaptureArenaBuilder
    {
        private const string ArenaRootName = "Arena_Capture_01";

        [MenuItem("Brain Eaters/Build Capture Arena In Current Scene")]
        public static void BuildCaptureArenaInCurrentScene()
        {
            GameObject existingRoot = GameObject.Find(ArenaRootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
            }

            GameObject arenaRoot = new GameObject(ArenaRootName);
            Undo.RegisterCreatedObjectUndo(arenaRoot, "Create Capture Arena");
            arenaRoot.AddComponent<LevelContext>();

            CreateFloor(arenaRoot.transform);
            CreateWalls(arenaRoot.transform);
            CreateSlopes(arenaRoot.transform);
            CreateObstacles(arenaRoot.transform);
            CreateCaptureZones(arenaRoot.transform);
            CreatePlayerSpawnPoint(arenaRoot.transform);
            CreateSpawnPoints(arenaRoot.transform);

            Selection.activeGameObject = arenaRoot;
            EditorGUIUtility.PingObject(arenaRoot);
        }

        private static void CreateFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(parent);
            floor.transform.localPosition = new Vector3(0f, -0.5f, 0f);
            floor.transform.localScale = new Vector3(48f, 1f, 48f);
            RegisterCreatedObject(floor);
        }

        private static void CreateWalls(Transform parent)
        {
            CreateWall(parent, "Wall_North", new Vector3(0f, 2f, 24f), new Vector3(48f, 4f, 1f));
            CreateWall(parent, "Wall_South", new Vector3(0f, 2f, -24f), new Vector3(48f, 4f, 1f));
            CreateWall(parent, "Wall_East", new Vector3(24f, 2f, 0f), new Vector3(1f, 4f, 48f));
            CreateWall(parent, "Wall_West", new Vector3(-24f, 2f, 0f), new Vector3(1f, 4f, 48f));
        }

        private static void CreateWall(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.localPosition = localPosition;
            wall.transform.localScale = localScale;
            RegisterCreatedObject(wall);
        }

        private static void CreateSlopes(Transform parent)
        {
            GameObject slopesRoot = new GameObject("Slopes");
            slopesRoot.transform.SetParent(parent);
            RegisterCreatedObject(slopesRoot);

            CreateSlope(slopesRoot.transform, "Slope_01", new Vector3(-10f, 0.5f, -2f), new Vector3(10f, 1f, 8f), new Vector3(0f, 0f, 12f));
            CreateSlope(slopesRoot.transform, "Slope_02", new Vector3(12f, 0.5f, 8f), new Vector3(12f, 1f, 7f), new Vector3(0f, 180f, 10f));
        }

        private static void CreateSlope(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Vector3 localEulerAngles)
        {
            GameObject slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slope.name = name;
            slope.transform.SetParent(parent);
            slope.transform.localPosition = localPosition;
            slope.transform.localScale = localScale;
            slope.transform.localEulerAngles = localEulerAngles;
            RegisterCreatedObject(slope);
        }

        private static void CreateObstacles(Transform parent)
        {
            GameObject obstaclesRoot = new GameObject("Obstacles");
            obstaclesRoot.transform.SetParent(parent);
            RegisterCreatedObject(obstaclesRoot);

            CreateObstacle(obstaclesRoot.transform, "Obstacle_01", new Vector3(-15f, 1.5f, 10f), new Vector3(4f, 3f, 4f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_02", new Vector3(-2f, 1f, 14f), new Vector3(6f, 2f, 3f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_03", new Vector3(9f, 2f, -10f), new Vector3(5f, 4f, 5f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_04", new Vector3(16f, 1.5f, 2f), new Vector3(3f, 3f, 7f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_05", new Vector3(-8f, 1f, -14f), new Vector3(8f, 2f, 3f));
        }

        private static void CreateObstacle(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = name;
            obstacle.transform.SetParent(parent);
            obstacle.transform.localPosition = localPosition;
            obstacle.transform.localScale = localScale;
            RegisterCreatedObject(obstacle);
        }

        private static void CreateCaptureZones(Transform parent)
        {
            GameObject zonesRoot = new GameObject("CaptureZones");
            zonesRoot.transform.SetParent(parent);
            RegisterCreatedObject(zonesRoot);

            CreateCaptureZone(zonesRoot.transform, "CaptureZone_A", new Vector3(-12f, 0.05f, -12f), new Vector3(6f, 0.2f, 6f));
            CreateCaptureZone(zonesRoot.transform, "CaptureZone_B", new Vector3(0f, 0.05f, 14f), new Vector3(7f, 0.2f, 7f));
            CreateCaptureZone(zonesRoot.transform, "CaptureZone_C", new Vector3(14f, 0.05f, -4f), new Vector3(6f, 0.2f, 6f));
        }

        private static void CreateCaptureZone(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject zoneRoot = new GameObject(name);
            zoneRoot.transform.SetParent(parent);
            zoneRoot.transform.localPosition = localPosition;
            RegisterCreatedObject(zoneRoot);

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            visual.name = "Visual";
            visual.transform.SetParent(zoneRoot.transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = new Vector3(localScale.x, localScale.y, localScale.z);
            Object.DestroyImmediate(visual.GetComponent<CapsuleCollider>());
            MeshRenderer visualRenderer = visual.GetComponent<MeshRenderer>();
            visualRenderer.sharedMaterial = CreatePreviewMaterial("Capture_Zone", new Color(0.2f, 0.7f, 1f, 0.35f));
            RegisterCreatedObject(visual);

            BoxCollider trigger = zoneRoot.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(localScale.x * 2f, 2f, localScale.z * 2f);
            trigger.center = new Vector3(0f, 1f, 0f);

            CaptureZone captureZone = zoneRoot.AddComponent<CaptureZone>();
            SerializedObject serializedObject = new SerializedObject(captureZone);
            serializedObject.FindProperty("zoneRenderer").objectReferenceValue = visualRenderer;

            GameObject labelRoot = new GameObject("ProgressLabel");
            labelRoot.transform.SetParent(zoneRoot.transform);
            labelRoot.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            TextMeshPro label = labelRoot.AddComponent<TextMeshPro>();
            label.text = "0%";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 3f;
            label.color = Color.white;
            serializedObject.FindProperty("progressLabel").objectReferenceValue = label;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSpawnPoints(Transform parent)
        {
            GameObject spawnRoot = new GameObject("SpawnPoints");
            spawnRoot.transform.SetParent(parent);
            spawnRoot.transform.localPosition = Vector3.zero;
            RegisterCreatedObject(spawnRoot);

            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_01", new Vector3(-18f, 0f, -18f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_02", new Vector3(-18f, 0f, 18f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_03", new Vector3(18f, 0f, -18f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_04", new Vector3(18f, 0f, 18f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_05", new Vector3(0f, 0f, 22f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_06", new Vector3(22f, 0f, 0f));
        }

        private static void CreatePlayerSpawnPoint(Transform parent)
        {
            GameObject root = new GameObject("PlayerSpawn");
            root.transform.SetParent(parent);
            root.transform.localPosition = Vector3.zero;
            RegisterCreatedObject(root);

            GameObject point = PlayerSpawnPointBuilder.CreatePlayerSpawnPointObject(root.transform, "PlayerSpawnPoint", new Vector3(-20f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            RegisterCreatedObject(point);
        }

        private static void CreateSpawnPoint(Transform parent, string pointName, Vector3 localPosition)
        {
            GameObject point = SpawnPointBuilder.CreateSpawnPointObject(parent, pointName, localPosition);
            RegisterCreatedObject(point);
        }

        private static Material CreatePreviewMaterial(string materialName, Color color)
        {
            return EditorMaterialUtility.GetOrCreateLitMaterialAsset(materialName, color);
        }

        private static void RegisterCreatedObject(GameObject gameObject)
        {
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Capture Arena Piece");
        }
    }
}
