using BrainEaters.Configs;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public static class LevelProgressionService
    {
        private const string HighestUnlockedIndexKey = "BrainEaters.Levels.HighestUnlockedIndex";

        public static int GetHighestUnlockedIndex(CampaignConfig campaign)
        {
            if (campaign == null || campaign.Levels.Count == 0)
            {
                return -1;
            }

            int savedIndex = PlayerPrefs.GetInt(HighestUnlockedIndexKey, 0);
            return Mathf.Clamp(savedIndex, 0, campaign.Levels.Count - 1);
        }

        public static void UnlockAll(CampaignConfig campaign)
        {
            if (campaign == null || campaign.Levels.Count == 0)
            {
                return;
            }

            PlayerPrefs.SetInt(HighestUnlockedIndexKey, campaign.Levels.Count - 1);
            PlayerPrefs.Save();
        }

        public static void ResetProgress(CampaignConfig campaign)
        {
            if (campaign == null || campaign.Levels.Count == 0)
            {
                return;
            }

            PlayerPrefs.SetInt(HighestUnlockedIndexKey, 0);

            for (int i = 0; i < campaign.Levels.Count; i++)
            {
                LevelConfig level = campaign.Levels[i];
                if (level == null)
                {
                    continue;
                }

                PlayerPrefs.DeleteKey(GetCompletedKey(level));
            }

            PlayerPrefs.Save();
        }

        public static void RegisterVictory(CampaignConfig campaign, LevelConfig completedLevel)
        {
            if (campaign == null || completedLevel == null)
            {
                return;
            }

            int currentIndex = GetLevelIndex(campaign, completedLevel);
            if (currentIndex < 0)
            {
                return;
            }

            SetLevelCompleted(completedLevel, true);

            int highestUnlocked = GetHighestUnlockedIndex(campaign);
            if (currentIndex >= highestUnlocked && currentIndex + 1 < campaign.Levels.Count)
            {
                PlayerPrefs.SetInt(HighestUnlockedIndexKey, currentIndex + 1);
            }

            PlayerPrefs.Save();
        }

        public static bool IsAccessible(CampaignConfig campaign, LevelConfig levelConfig)
        {
            LevelAvailabilityState state = GetState(campaign, levelConfig);
            return state != LevelAvailabilityState.Locked;
        }

        public static bool IsFirstLevelCompleted(CampaignConfig campaign)
        {
            if (campaign == null || campaign.Levels.Count == 0 || campaign.Levels[0] == null)
            {
                return false;
            }

            return IsLevelCompleted(campaign.Levels[0]);
        }

        public static bool IsLevelCompleted(LevelConfig levelConfig)
        {
            return PlayerPrefs.GetInt(GetCompletedKey(levelConfig), 0) == 1;
        }

        public static LevelAvailabilityState GetState(CampaignConfig campaign, LevelConfig levelConfig)
        {
            if (campaign == null || levelConfig == null)
            {
                return LevelAvailabilityState.Locked;
            }

            int levelIndex = GetLevelIndex(campaign, levelConfig);
            if (levelIndex < 0)
            {
                return LevelAvailabilityState.Locked;
            }

            int highestUnlocked = GetHighestUnlockedIndex(campaign);
            if (levelIndex > highestUnlocked)
            {
                return LevelAvailabilityState.Locked;
            }

            if (IsLevelCompleted(levelConfig))
            {
                return LevelAvailabilityState.Completed;
            }

            return levelIndex == highestUnlocked ? LevelAvailabilityState.New : LevelAvailabilityState.Unlocked;
        }

        private static int GetLevelIndex(CampaignConfig campaign, LevelConfig levelConfig)
        {
            for (int i = 0; i < campaign.Levels.Count; i++)
            {
                if (campaign.Levels[i] == levelConfig)
                {
                    return i;
                }
            }

            return -1;
        }

        private static void SetLevelCompleted(LevelConfig levelConfig, bool completed)
        {
            PlayerPrefs.SetInt(GetCompletedKey(levelConfig), completed ? 1 : 0);
        }

        private static string GetCompletedKey(LevelConfig levelConfig)
        {
            string levelId = levelConfig != null ? levelConfig.LevelId : "unknown";
            return $"BrainEaters.Levels.Completed.{levelId}";
        }
    }
}
