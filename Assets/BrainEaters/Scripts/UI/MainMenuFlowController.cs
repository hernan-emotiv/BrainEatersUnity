using BrainEaters.Configs;
using BrainEaters.GameFlow;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BrainEaters.UI
{
    public class MainMenuFlowController : MonoBehaviour
    {
        private const string TutorialSeenKey = "BrainEaters.Onboarding.TutorialSeen";

        [SerializeField] private CampaignConfig campaignConfig;
        [SerializeField] private GameObject mainMenuRoot;
        [SerializeField] private GameObject levelSelectRoot;
        [SerializeField] private GameObject tutorialPopupRoot;
        [SerializeField] private GameObject howToPlayPopupRoot;
        [SerializeField] private Button playButton;
        [SerializeField] private Button howButton;
        [SerializeField] private Button tutorialStartButton;
        [SerializeField] private Button howBackButton;

        private void Awake()
        {
            BindButtons();
            LevelSession.SetCampaign(campaignConfig);
            ShowMainMenu();
        }

        private void OnEnable()
        {
            BindButtons();
        }

        public void ShowMainMenu()
        {
            SetActive(mainMenuRoot, true);
            SetActive(levelSelectRoot, false);
            SetActive(tutorialPopupRoot, false);
            SetActive(howToPlayPopupRoot, false);
        }

        public void HandlePlayPressed()
        {
            if (LevelProgressionService.IsFirstLevelCompleted(campaignConfig))
            {
                ShowLevelSelect();
                return;
            }

            if (!HasSeenTutorial())
            {
                ShowTutorial();
                return;
            }

            StartFirstLevel();
        }

        public void ShowLevelSelect()
        {
            SetActive(mainMenuRoot, false);
            SetActive(levelSelectRoot, true);
            SetActive(tutorialPopupRoot, false);
            SetActive(howToPlayPopupRoot, false);
        }

        public void ShowTutorial()
        {
            SetActive(mainMenuRoot, true);
            SetActive(levelSelectRoot, false);
            SetActive(tutorialPopupRoot, true);
            SetActive(howToPlayPopupRoot, false);
        }

        public void ShowHowToPlay()
        {
            SetActive(mainMenuRoot, true);
            SetActive(levelSelectRoot, false);
            SetActive(tutorialPopupRoot, false);
            SetActive(howToPlayPopupRoot, true);
        }

        public void StartFromTutorial()
        {
            MarkTutorialSeen();
            StartFirstLevel();
        }

        public void StartFirstLevel()
        {
            if (campaignConfig == null || campaignConfig.Levels.Count == 0 || campaignConfig.Levels[0] == null)
            {
                Debug.LogError("MainMenuFlowController requires a CampaignConfig with at least one level.", this);
                return;
            }

            LevelSession.SelectLevel(campaignConfig, campaignConfig.Levels[0]);
            SceneManager.LoadScene(campaignConfig.GameplaySceneName);
        }

        public void ResetTutorialSeen()
        {
            PlayerPrefs.DeleteKey(TutorialSeenKey);
            PlayerPrefs.Save();
        }

        private void BindButtons()
        {
            BindButton(playButton, HandlePlayPressed);
            BindButton(howButton, ShowHowToPlay);
            BindButton(tutorialStartButton, StartFromTutorial);
            BindButton(howBackButton, ShowMainMenu);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(callback);
            button.onClick.AddListener(callback);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }

        private static bool HasSeenTutorial()
        {
            return PlayerPrefs.GetInt(TutorialSeenKey, 0) == 1;
        }

        private static void MarkTutorialSeen()
        {
            PlayerPrefs.SetInt(TutorialSeenKey, 1);
            PlayerPrefs.Save();
        }
    }
}
