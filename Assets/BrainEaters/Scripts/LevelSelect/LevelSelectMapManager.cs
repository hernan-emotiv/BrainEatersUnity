using System.Collections.Generic;
using BrainEaters.Configs;
using BrainEaters.GameFlow;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BrainEaters.LevelSelect
{
    public class LevelSelectMapManager : MonoBehaviour
    {
        [SerializeField] private CampaignConfig campaignConfig;
        [SerializeField] private Button unlockAllButton;
        [SerializeField] private Button resetProgressButton;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private List<LevelSelectNode> levelNodes = new List<LevelSelectNode>();

        private void Awake()
        {
            ResolveReferences();
            BindButtons();
            LevelSession.SetCampaign(campaignConfig);
        }

        private void OnEnable()
        {
            ResolveReferences();
            BindButtons();
            RefreshNodes();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void UnlockAllLevels()
        {
            LevelProgressionService.UnlockAll(campaignConfig);
            RefreshNodes();
            SetStatus("All levels unlocked.");
        }

        public void ResetProgress()
        {
            LevelProgressionService.ResetProgress(campaignConfig);
            RefreshNodes();
            SetStatus("Progress reset.");
        }

        public void RefreshNodes()
        {
            for (int i = 0; i < levelNodes.Count; i++)
            {
                LevelSelectNode node = levelNodes[i];
                if (node == null || node.LevelConfig == null)
                {
                    continue;
                }

                LevelAvailabilityState state = LevelProgressionService.GetState(campaignConfig, node.LevelConfig);
                node.ApplyState(state);
                BindNode(node);
            }
        }

        private void TrySelectNode(LevelSelectNode node)
        {
            if (campaignConfig == null || node == null || node.LevelConfig == null)
            {
                return;
            }

            if (!LevelProgressionService.IsAccessible(campaignConfig, node.LevelConfig))
            {
                SetStatus($"{node.LevelConfig.DisplayName} is locked.");
                return;
            }

            LevelSession.SelectLevel(campaignConfig, node.LevelConfig);
            SetStatus($"Loading {node.LevelConfig.DisplayName}...");
            SceneManager.LoadScene(campaignConfig.GameplaySceneName);
        }

        private void BindButtons()
        {
            if (unlockAllButton != null)
            {
                unlockAllButton.onClick.RemoveListener(UnlockAllLevels);
                unlockAllButton.onClick.AddListener(UnlockAllLevels);
            }

            if (resetProgressButton != null)
            {
                resetProgressButton.onClick.RemoveListener(ResetProgress);
                resetProgressButton.onClick.AddListener(ResetProgress);
            }
        }

        private void BindNode(LevelSelectNode node)
        {
            if (node == null || node.Button == null)
            {
                return;
            }

            node.Button.onClick.RemoveAllListeners();
            node.Button.onClick.AddListener(() => TrySelectNode(node));
        }

        private void ResolveReferences()
        {
            if (levelNodes.Count == 0)
            {
                levelNodes.AddRange(FindObjectsByType<LevelSelectNode>(FindObjectsSortMode.None));
            }
        }

        private void SetStatus(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
        }
    }
}
