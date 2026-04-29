using BrainEaters.Player;
using BrainEaters.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.GameFlow
{
    public class OnboardingPopupTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text bodyText;
        [SerializeField] private Button continueButton;
        [SerializeField] private string title = "Mental Power";
        [SerializeField, TextArea] private string body = "Charge your Mind Power, then use the Brain Bomb near the bridge lever to launch the monsters away.";
        [SerializeField] private bool pauseGame = true;
        [SerializeField] private bool showOnlyOnce = true;

        private UiVisibilityAnimator popupAnimator;
        private bool hasShown;
        private bool isOpen;
        private float previousTimeScale = 1f;

        private void Awake()
        {
            ResolveReferences();
            ConfigureText();

            if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(Close);
            }
        }

        private void OnDisable()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveListener(Close);
            }

            if (isOpen && pauseGame)
            {
                Time.timeScale = previousTimeScale;
            }

            isOpen = false;
        }

        private void OnValidate()
        {
            ResolveReferences();
            ConfigureText();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (showOnlyOnce && hasShown)
            {
                return;
            }

            PlayerController playerController = other.GetComponentInParent<PlayerController>();
            if (playerController == null)
            {
                return;
            }

            Open();
        }

        public void Open()
        {
            if (isOpen || popupRoot == null)
            {
                return;
            }

            hasShown = true;
            isOpen = true;
            ConfigureText();

            if (pauseGame)
            {
                previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }

            popupRoot.SetActive(true);
            if (popupAnimator != null)
            {
                popupAnimator.PlayEntrance();
            }
        }

        public void Close()
        {
            if (!isOpen)
            {
                return;
            }

            if (pauseGame)
            {
                Time.timeScale = previousTimeScale;
            }

            isOpen = false;
            if (popupAnimator != null)
            {
                popupAnimator.Hide();
            }
            else if (popupRoot != null)
            {
                popupRoot.SetActive(false);
            }
        }

        private void ResolveReferences()
        {
            if (popupRoot != null && popupAnimator == null)
            {
                popupAnimator = popupRoot.GetComponent<UiVisibilityAnimator>();
            }

            if (continueButton == null && popupRoot != null)
            {
                continueButton = popupRoot.GetComponentInChildren<Button>(true);
            }
        }

        private void ConfigureText()
        {
            if (titleText != null)
            {
                titleText.text = title;
            }

            if (bodyText != null)
            {
                bodyText.text = body;
            }
        }
    }
}
