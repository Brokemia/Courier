using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mod.Courier.Helpers;
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

        // The least y value the menu will be at without scrolling
        public float startYMax = -90 - 18f * 10;

        public Vector3 defaultPos = new Vector3(28.2f, 5.5f, 5.1f);

        public static ModOptionScreen BuildModOptionScreen(OptionScreen optionScreen) {
            GameObject gameObject = new GameObject();
            ModOptionScreen modOptionScreen = gameObject.AddComponent<ModOptionScreen>();
            OptionScreen newScreen = Instantiate(optionScreen);
            modOptionScreen.name = "ModOptionScreen";
            // Iterate backwards so elements don't shift as lower ones are removed
            // If you know, you know
            for (int i = newScreen.transform.childCount - 1; i >= 0; i--) {
                newScreen.transform.GetChild(i).SetParent(modOptionScreen.transform, false);
            }
            modOptionScreen.optionMenuButtons = modOptionScreen.transform.Find("Container").Find("BackgroundFrame").Find("OptionsFrame").Find("OptionMenuButtons");
            modOptionScreen.backButton = modOptionScreen.optionMenuButtons.Find("Back");
            // Delete OptionScreen buttons except for the Back button
            foreach (Transform child in modOptionScreen.optionMenuButtons.GetChildren()) {
                if (!child.Equals(modOptionScreen.backButton))
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
            //transform.position = new Vector3(0, Math.Max(-90 - heightPerButton * Courier.UI.ModOptionButtons.Count, startYMax));
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
                buttonInfo.gameObject.name = buttonInfo.GetText?.Invoke() ?? "";
                buttonInfo.gameObject.transform.name = buttonInfo.GetText?.Invoke() ?? "";
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
            
            // Make the border frames blue
            Sprite borderSprite = backgroundFrame.GetComponent<Image>().sprite = Courier.EmbeddedSprites["Mod.Courier.UI.mod_options_frame"];
            borderSprite.bounds.extents.Set(1.7f, 1.7f, 0.1f);
            borderSprite.texture.filterMode = FilterMode.Point;

            borderSprite = backgroundFrame.Find("OptionsFrame").GetComponent<Image>().sprite = Courier.EmbeddedSprites["Mod.Courier.UI.mod_options_frame"];
            borderSprite.bounds.extents.Set(1.7f, 1.7f, 0.1f);
            borderSprite.texture.filterMode = FilterMode.Point;

            HideUnavailableOptions();
            InitOptions();
            SetInitialSelection();

            // Make the selection frames blue
            foreach (Image image in transform.GetComponentsInChildren<Image>().Where((c) => c.name.Equals("SelectionFrame"))) {
                try {
                    if (image.overrideSprite != null && image.overrideSprite.name != "Empty") {
                        RenderTexture rt = new RenderTexture(image.overrideSprite.texture.width, image.overrideSprite.texture.height, 0);
                        RenderTexture.active = rt;
                        Graphics.Blit(image.overrideSprite.texture, rt);

                        Texture2D res = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, true);
                        res.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);

                        Color[] pxls = res.GetPixels();
                        for (int i = 0; i < pxls.Length; i++) {
                            if (Math.Abs(pxls[i].r - .973) < .01 && Math.Abs(pxls[i].g - .722) < .01) {
                                pxls[i].r = 0;
                                pxls[i].g = .633f;
                                pxls[i].b = 1;
                            }
                        }
                        res.SetPixels(pxls);
                        res.Apply();

                        Sprite sprite = image.overrideSprite = Sprite.Create(res, new Rect(0, 0, res.width, res.height), new Vector2(16, 16), 20, 1, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
                        sprite.bounds.extents.Set(.8f, .8f, 0.1f);
                        sprite.texture.filterMode = FilterMode.Point;
                    }
                } catch {
                    Console.WriteLine("Image not Read/Writeable when recoloring selection frames in ModOptionScreen");
                }
            }
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
                buttonInfo.gameObject.SetActive(buttonInfo.IsEnabled?.Invoke() ?? true);
            }
            Vector2 sizeDelta = backgroundFrame.sizeDelta;
            backgroundFrame.sizeDelta = new Vector2(sizeDelta.x, 110 + heightPerButton * Courier.UI.EnabledModOptionsCount());
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
            Courier.UI.ModOptionScreenLoaded = false;
        }

        public int GetSelectedButtonIndex() {
            if (backButton.Find("Button").gameObject.Equals(EventSystem.current.currentSelectedGameObject)) return Courier.UI.EnabledModOptionsCount();
            foreach(OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons) {
                if (buttonInfo.gameObject.transform.Find("Button").gameObject.Equals(EventSystem.current.currentSelectedGameObject)) {
                    return Courier.UI.EnabledModOptionsBeforeButton(buttonInfo);
                }
            }
            return -1;
        }

        private void LateUpdate() {
            if (Manager<InputManager>.Instance.GetBackDown()) {
                BackToOptionMenu();
            }

            Vector3 windowOffset = new Vector3(0, Math.Min(GetSelectedButtonIndex(), Math.Max(0, Courier.UI.EnabledModOptionsCount() - 10)) * .9f) - new Vector3(0, Math.Max(0, Courier.UI.EnabledModOptionsCount() - 11) * .45f);
            transform.position = defaultPos + windowOffset;

            // Line up all of the buttons
            backButton.position = topButtonPos + new Vector3(0, .45f * (Courier.UI.EnabledModOptionsCount() - 1)) + windowOffset;
            foreach (OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons) {
                buttonInfo.nameTextMesh.text = buttonInfo.GetText?.Invoke() ?? "";
                if (buttonInfo.gameObject.activeInHierarchy) {
                    // Buttons are added in the same spot as the back button
                    // Then the back button is shifted down to hold the place of the next button
                    buttonInfo.gameObject.transform.position = backButton.transform.position;
                    backButton.position = buttonInfo.gameObject.transform.position - new Vector3(0, .9f);
                }
            }

            // Make the selection frames blue
            foreach (Image image in transform.GetComponentsInChildren<Image>().Where((c) => c.name.Equals("SelectionFrame"))) {
                try {
                    if (image.overrideSprite != null && image.overrideSprite.name.Equals("ShopItemSelectionFrame")) {
                        RenderTexture rt = new RenderTexture(image.overrideSprite.texture.width, image.overrideSprite.texture.height, 0);
                        RenderTexture.active = rt;
                        Graphics.Blit(image.overrideSprite.texture, rt);

                        Texture2D res = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, true);
                        res.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);

                        Color[] pxls = res.GetPixels();
                        for (int i = 0; i < pxls.Length; i++) {
                            if (Math.Abs(pxls[i].r - .973) < .01 && Math.Abs(pxls[i].g - .722) < .01) {
                                pxls[i].r = 0;
                                pxls[i].g = .633f;
                                pxls[i].b = 1;
                            }
                        }
                        res.SetPixels(pxls);
                        res.Apply();

                        Sprite sprite = image.overrideSprite = Sprite.Create(res, new Rect(0, 0, res.width, res.height), new Vector2(16, 16), 20, 1, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
                        sprite.bounds.extents.Set(.8f, .8f, 0.1f);
                        sprite.texture.filterMode = FilterMode.Point;
                    }
                } catch {
                    Console.WriteLine("Image not Read/Writeable when recoloring selection frames in ModOptionScreen");
                }
            }
        }

        private void InitOptions() {
            if (Courier.UI.ModOptionButtons.Count > 0) {
                defaultSelection = Courier.UI.ModOptionButtons[0].gameObject;
            } else {
                defaultSelection = backButton.gameObject;
            }

            backgroundFrame.Find("Title").GetComponent<TextMeshProUGUI>().SetText(Manager<LocalizationManager>.Instance.GetText(Courier.UI.MOD_OPTIONS_MENU_TITLE_LOC_ID));
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
            Courier.UI.ModOptionScreenLoaded = false;
        }
    }
}
