using System.Collections.Generic;
using BrainEaters.Spawning;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class LevelContext : MonoBehaviour
    {
        [SerializeField] private PlayerSpawnPoint playerSpawnPoint;
        [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        [SerializeField] private List<CaptureZone> captureZones = new List<CaptureZone>();
        [SerializeField] private List<CollectPickup> collectPickups = new List<CollectPickup>();

        public PlayerSpawnPoint PlayerSpawnPoint => playerSpawnPoint;
        public IReadOnlyList<SpawnPoint> SpawnPoints => spawnPoints;
        public IReadOnlyList<CaptureZone> CaptureZones => captureZones;
        public IReadOnlyList<CollectPickup> CollectPickups => collectPickups;

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
            if (playerSpawnPoint == null)
            {
                playerSpawnPoint = GetComponentInChildren<PlayerSpawnPoint>(true);
            }

            spawnPoints.Clear();
            spawnPoints.AddRange(GetComponentsInChildren<SpawnPoint>(true));
            captureZones.Clear();
            captureZones.AddRange(GetComponentsInChildren<CaptureZone>(true));
            collectPickups.Clear();
            collectPickups.AddRange(GetComponentsInChildren<CollectPickup>(true));
        }
    }
}
