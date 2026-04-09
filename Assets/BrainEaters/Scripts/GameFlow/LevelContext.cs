using System.Collections.Generic;
using BrainEaters.Spawning;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class LevelContext : MonoBehaviour
    {
        [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        public IReadOnlyList<SpawnPoint> SpawnPoints => spawnPoints;

        private void Awake()
        {
            RefreshSpawnPointsIfNeeded();
        }

        private void OnValidate()
        {
            RefreshSpawnPointsIfNeeded();
        }

        public void RefreshSpawnPointsIfNeeded()
        {
            if (spawnPoints.Count > 0)
            {
                return;
            }

            spawnPoints.Clear();
            spawnPoints.AddRange(GetComponentsInChildren<SpawnPoint>(true));
        }
    }
}
