using BrainEaters.Spawning;
using BrainEaters.GameFlow;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class ArenaBuilder
    {
        private const string ArenaRootName = "Arena_Level01";

        [MenuItem("Brain Eaters/Build Arena In Current Scene")]
        public static void BuildArenaInCurrentScene()
        {
            GameObject existingRoot = GameObject.Find(ArenaRootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
            }

            GameObject arenaRoot = new GameObject(ArenaRootName);
            Undo.RegisterCreatedObjectUndo(arenaRoot, "Create Brain Eaters Arena");
            arenaRoot.AddComponent<LevelContext>();

            CreateFloor(arenaRoot.transform);
            CreateWalls(arenaRoot.transform);
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
            floor.transform.localScale = new Vector3(28f, 1f, 28f);
            RegisterCreatedObject(floor);
        }

        private static void CreateWalls(Transform parent)
        {
            CreateWall(parent, "Wall_North", new Vector3(0f, 1.5f, 14f), new Vector3(28f, 3f, 1f));
            CreateWall(parent, "Wall_South", new Vector3(0f, 1.5f, -14f), new Vector3(28f, 3f, 1f));
            CreateWall(parent, "Wall_East", new Vector3(14f, 1.5f, 0f), new Vector3(1f, 3f, 28f));
            CreateWall(parent, "Wall_West", new Vector3(-14f, 1.5f, 0f), new Vector3(1f, 3f, 28f));
        }

        private static void CreateWall(Transform parent, string wallName, Vector3 localPosition, Vector3 localScale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = wallName;
            wall.transform.SetParent(parent);
            wall.transform.localPosition = localPosition;
            wall.transform.localScale = localScale;
            RegisterCreatedObject(wall);
        }

        private static void CreateSpawnPoints(Transform parent)
        {
            GameObject spawnRoot = new GameObject("SpawnPoints");
            spawnRoot.transform.SetParent(parent);
            spawnRoot.transform.localPosition = Vector3.zero;
            RegisterCreatedObject(spawnRoot);

            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_01", new Vector3(-8f, 0f, -8f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_02", new Vector3(-8f, 0f, 8f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_03", new Vector3(8f, 0f, -8f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_04", new Vector3(8f, 0f, 8f));
        }

        private static void CreatePlayerSpawnPoint(Transform parent)
        {
            GameObject root = new GameObject("PlayerSpawn");
            root.transform.SetParent(parent);
            root.transform.localPosition = Vector3.zero;
            RegisterCreatedObject(root);

            GameObject point = PlayerSpawnPointBuilder.CreatePlayerSpawnPointObject(root.transform, "PlayerSpawnPoint", new Vector3(0f, 0f, -10f), Quaternion.identity);
            RegisterCreatedObject(point);
        }

        private static void CreateSpawnPoint(Transform parent, string pointName, Vector3 localPosition)
        {
            GameObject point = SpawnPointBuilder.CreateSpawnPointObject(parent, pointName, localPosition);
            RegisterCreatedObject(point);
        }

        private static void RegisterCreatedObject(GameObject gameObject)
        {
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Brain Eaters Arena Piece");
        }
    }
}
