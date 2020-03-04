using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mod.Courier.UI {
    public class ModOptionScreen : View {
        public RectTransform backgroundFrame;

        public Transform optionMenuButtons;

        public Transform backButton;

        protected Vector3 topButtonPos = new Vector3(28.8f, 4.5f, 5.1f);

        public GameObject defaultSelection;

        public GameObject initialSelection;

        public static string onLocID = "OPTIONS_SCREEN_ON";

        public static string offLocID = "OPTIONS_SCREEN_OFF";

        public float heightPerButton = 18f;

        public static string sfxLocID = "OPTIONS_SCREEN_SOUND_FX";

        public static string musicLocID = "OPTIONS_SCREEN_MUSIC";

        public float initialHeight;
        
        public static ModOptionScreen BuildModOptionScreen(OptionScreen optionScreen) {
            GameObject gameObject = new GameObject();
            ModOptionScreen modOptionScreen = gameObject.AddComponent<ModOptionScreen>();
            OptionScreen newScreen = Instantiate(optionScreen);
            modOptionScreen.name = "ModOptionScreen";
            // Iterate backwards so elements don't shift as lower ones are removed
            // If you know, you know
            for (int i = newScreen.transform.childCount-1; i >= 0; i--) {
                newScreen.transform.GetChild(i).SetParent(modOptionScreen.transform, false);
            }
            modOptionScreen.optionMenuButtons = modOptionScreen.transform.Find("Container").Find("BackgroundFrame").Find("OptionsFrame").Find("OptionMenuButtons");
            modOptionScreen.backButton = modOptionScreen.optionMenuButtons.Find("Back");
            // Delete OptionScreen buttons except for the Back button
            foreach(Transform child in modOptionScreen.optionMenuButtons.GetChildren()) {
                if(!child.Equals(modOptionScreen.backButton))
                    Destroy(child.gameObject);
            }
            modOptionScreen.optionMenuButtons.DetachChildren();
            modOptionScreen.backButton.SetParent(modOptionScreen.optionMenuButtons);

            // Make back button take you to the OptionScreen instead of the pause menu
            Button button = modOptionScreen.backButton.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(modOptionScreen.BackToOptionMenu);
            
            modOptionScreen.InitStuffUnityWouldDo();

            modOptionScreen.gameObject.SetActive(value: false);
            Courier.UI.ModOptionScreenLoaded = true;
            return modOptionScreen;
        }

        private void Awake() {

        }

        // You heard me
        private void InitStuffUnityWouldDo() {
            transform.position -= new Vector3(0, 90 + heightPerButton * Courier.UI.ModOptionButtons.Count);
            backgroundFrame = (RectTransform)transform.Find("Container").Find("BackgroundFrame");
            initialHeight = backgroundFrame.sizeDelta.y;
            gameObject.AddComponent<Canvas>();
        }

        private void Start() {
            InitOptions();
        }

        public override void Init(IViewParams screenParams) {
            base.Init(screenParams);
            foreach (OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons) {
                OptionScreen optionScreen = Manager<UIManager>.Instance.GetView<OptionScreen>();
                if (buttonInfo is ToggleButtonInfo) {
                    buttonInfo.gameObject = Instantiate(optionScreen.fullscreenOption, transform.Find("Container").Find("BackgroundFrame"));
                } else if (buttonInfo is SubMenuButtonInfo) {
                    buttonInfo.gameObject = Instantiate(optionScreen.controlsButton.transform.parent.gameObject, transform.Find("Container").Find("BackgroundFrame"));
                } else {
                    // TODO Mods add their own ButtonInfo
                    Console.WriteLine(buttonInfo.GetType() + " not a known type of OptionsButtonInfo!");
                }
                buttonInfo.gameObject.transform.SetParent(transform.Find("Container").Find("BackgroundFrame").Find("OptionsFrame").Find("OptionMenuButtons"));
                buttonInfo.gameObject.name = buttonInfo.text;
                buttonInfo.gameObject.transform.name = buttonInfo.text;
                buttonInfo.addedTo = this;
                foreach (TextMeshProUGUI text in buttonInfo.gameObject.GetComponentsInChildren<TextMeshProUGUI>()) {
                    if (text.name.Equals("OptionState"))
                        buttonInfo.stateTextMesh = text;
                    if (text.name.Equals("OptionName"))
                        buttonInfo.nameTextMesh = text;
                }
                Button button = buttonInfo.gameObject.GetComponentInChildren<Button>();
                button.onClick = new Button.ButtonClickedEvent();
                button.onClick.AddListener(buttonInfo.onClick);

                buttonInfo.OnInit(this);
            }
            HideUnavailableOptions();
            InitOptions();
            SetInitialSelection();
        }

        private IEnumerator WaitAndSelectInitialButton() {
            yield return null;
            SetInitialSelection();
        }

        private void OnEnable() {
            if (transform.parent != null) {
                Manager<UIManager>.Instance.SetParentAndAlign(gameObject, transform.parent.gameObject);
            }
            EventSystem.current.SetSelectedGameObject(null);
            StartCoroutine(WaitAndSelectInitialButton());
        }

        private void HideUnavailableOptions() {
            foreach (OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons) {
                buttonInfo.gameObject.SetActive(true); // TODO Way to deactivate buttons
            }
            Vector2 sizeDelta = backgroundFrame.sizeDelta;
            backgroundFrame.sizeDelta = new Vector2(sizeDelta.x, 110 + heightPerButton * Courier.UI.ModOptionButtons.Count);
        }

        private void SetInitialSelection() {
            GameObject defaultSelectionButton = (initialSelection ?? defaultSelection).transform.Find("Button").gameObject;
            defaultSelectionButton.transform.GetComponent<UIObjectAudioHandler>().playAudio = false;
            EventSystem.current.SetSelectedGameObject(defaultSelectionButton);
            defaultSelectionButton.GetComponent<Button>().OnSelect(null);
            defaultSelectionButton.GetComponent<UIObjectAudioHandler>().playAudio = true;
            initialSelection = null;
        }

        public void GoOffscreenInstant() {
            gameObject.SetActive(false);
            Courier.UI.ModOptionScreenShowing = false;
        }

        private void LateUpdate() {
            if (Manager<InputManager>.Instance.GetBackDown()) {
                BackToOptionMenu();
            }
            // Line up all of the buttons
            backButton.position = topButtonPos + new Vector3(0, .45f * (Courier.UI.ModOptionButtons.Count-1));
            foreach (OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons) {
                buttonInfo.nameTextMesh.text = buttonInfo.text; // TODO Patch LoadGeneralLoc to load custom language files
                // Buttons are added in the same spot as the back button
                // Then the back button is shifted down to hold the place of the next button
                buttonInfo.gameObject.transform.position = backButton.transform.position;
                backButton.position = buttonInfo.gameObject.transform.position - new Vector3(0, .9f);
            }
        }

        private void InitOptions() {
            if (Courier.UI.ModOptionButtons.Count > 0) {
                defaultSelection = Courier.UI.ModOptionButtons[0].gameObject;
            } else {
                defaultSelection = backButton.gameObject;
            }

            backgroundFrame.Find("Title").GetComponent<TextMeshProUGUI>().SetText("Courier Mod Menu - Third Party Content");
            foreach (OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons) {
                buttonInfo.UpdateStateText();
            }
        }

        public void BackToOptionMenu() {
            //Manager<SaveManager>.Instance.SaveOptions(); TODO Mod options in save file
            Close(false);
            Manager<UIManager>.Instance.GetView<OptionScreen>().gameObject.SetActive(true);
            Courier.UI.ModOptionButton.gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>().playAudio = false;
            EventSystem.current.SetSelectedGameObject(Courier.UI.ModOptionButton.gameObject.transform.Find("Button").gameObject);
            Courier.UI.ModOptionButton.gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>().playAudio = true;
        }

        public override void Close(bool transitionOut) {
            base.Close(transitionOut);
            Courier.UI.ModOptionScreenShowing = false;
            Courier.UI.ModOptionScreenLoaded = false;
        }
    }
}
