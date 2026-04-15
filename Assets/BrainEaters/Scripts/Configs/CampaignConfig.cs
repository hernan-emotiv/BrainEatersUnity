using System.Collections.Generic;
using UnityEngine;

namespace BrainEaters.Configs
{
    [CreateAssetMenu(fileName = "CampaignConfig", menuName = "Brain Eaters/Configs/Campaign Config")]
    public class CampaignConfig : ScriptableObject
    {
        [SerializeField] private string levelSelectSceneName = "LevelSelectScene";
        [SerializeField] private string gameplaySceneName = "GameScene";
        [SerializeField] private List<LevelConfig> levels = new List<LevelConfig>();

        public string LevelSelectSceneName => levelSelectSceneName;
        public string GameplaySceneName => gameplaySceneName;
        public IReadOnlyList<LevelConfig> Levels => levels;
    }
}
