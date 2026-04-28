using BrainEaters.Configs;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class PlayerPrefsUtility
    {
        private const string TutorialSeenKey = "BrainEaters.Onboarding.TutorialSeen";
        private const string HighestUnlockedIndexKey = "BrainEaters.Levels.HighestUnlockedIndex";

        [MenuItem("Brain Eaters/PlayerPrefs/Reset Tutorial Popup")]
        public static void ResetTutorialPopup()
        {
            PlayerPrefs.DeleteKey(TutorialSeenKey);
            PlayerPrefs.Save();
            Debug.Log($"Deleted PlayerPrefs key: {TutorialSeenKey}");
        }

        [MenuItem("Brain Eaters/PlayerPrefs/Reset Level Progress")]
        public static void ResetLevelProgress()
        {
            PlayerPrefs.DeleteKey(HighestUnlockedIndexKey);
            DeleteKnownCompletedLevelKeys();
            PlayerPrefs.Save();
            Debug.Log("Reset Brain Eaters level progression PlayerPrefs.");
        }

        [MenuItem("Brain Eaters/PlayerPrefs/Delete All PlayerPrefs")]
        public static void DeleteAllPlayerPrefs()
        {
            if (!EditorUtility.DisplayDialog(
                    "Delete All PlayerPrefs",
                    "This deletes every PlayerPrefs key for this Unity project, including keys outside Brain Eaters. Continue?",
                    "Delete All",
                    "Cancel"))
            {
                return;
            }

            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
            Debug.Log("Deleted all PlayerPrefs for this Unity project.");
        }

        private static void DeleteKnownCompletedLevelKeys()
        {
            foreach (string levelConfigGuid in AssetDatabase.FindAssets("t:LevelConfig"))
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(levelConfigGuid);
                LevelConfig levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(assetPath);
                if (levelConfig == null)
                {
                    continue;
                }

                PlayerPrefs.DeleteKey($"BrainEaters.Levels.Completed.{levelConfig.LevelId}");
            }
        }
    }
}
