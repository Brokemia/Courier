using System;
using System.Collections.Generic;
using Mod.Courier.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Mod.Courier.UI.TextEntryButtonInfo;

namespace Mod.Courier {
    public partial class Courier {
        public static class UI {
            public static List<OptionsButtonInfo> OptionButtons = new List<OptionsButtonInfo>();
            public static List<OptionsButtonInfo> ModOptionButtons = new List<OptionsButtonInfo>();

            public static OptionsButtonInfo ModOptionButton;
            public static ModOptionScreen ModOptionScreen;
            public static bool ModOptionScreenLoaded;

            public const string MOD_OPTIONS_BUTTON_LOC_ID = "COURIER_MOD_OPTIONS_BUTTON";
            public const string MOD_OPTIONS_MENU_TITLE_LOC_ID = "COURIER_MOD_OPTIONS_MENU_TITLE";

            public static int EnabledModOptionsCount() {
                int total = 0;
                foreach(OptionsButtonInfo buttonInfo in ModOptionButtons) {
                    if (buttonInfo.gameObject.activeInHierarchy)
                        total++;
                }
                return total;
            }

            public static int EnabledModOptionsBeforeButton(OptionsButtonInfo button) {
                int total = 0;
                foreach (OptionsButtonInfo buttonInfo in ModOptionButtons) {
                    if (buttonInfo.Equals(button)) return total;
                    if (buttonInfo.gameObject.activeInHierarchy)
                        total++;
                }
                return -1;
            }

            public static int EnabledCustomOptionButtonsCount() {
                int total = 0;
                foreach (OptionsButtonInfo buttonInfo in OptionButtons) {
                    if (buttonInfo.gameObject.activeInHierarchy)
                        total++;
                }
                return total;
            }

            public static int EnabledCustomOptionButtonsBeforeButton(OptionsButtonInfo button) {
                int total = 0;
                foreach (OptionsButtonInfo buttonInfo in OptionButtons) {
                    if (buttonInfo.Equals(button)) return total;
                    if (buttonInfo.gameObject.activeInHierarchy)
                        total++;
                }
                return -1;
            }

            public static void SetupModdedUI() {
                ModOptionButton = RegisterSubMenuOptionButton(() => Manager<LocalizationManager>.Instance.GetText(MOD_OPTIONS_BUTTON_LOC_ID), OnSelectModOptions);
            }

            public static void InitOptionsViewWithModButtons(View view, IEnumerable<OptionsButtonInfo> buttons) {
                OptionScreen optionScreen = Manager<UIManager>.Instance.GetView<OptionScreen>();
                if (view is OptionScreen)
                    optionScreen = (OptionScreen)view;

                foreach (OptionsButtonInfo buttonInfo in buttons) {
                    if (buttonInfo is ToggleButtonInfo) {
                        buttonInfo.gameObject = UnityEngine.Object.Instantiate(optionScreen.fullscreenOption, view.transform.Find("Container").Find("BackgroundFrame"));
                    } else if (buttonInfo is SubMenuButtonInfo) {
                        buttonInfo.gameObject = UnityEngine.Object.Instantiate(optionScreen.controlsButton.transform.parent.gameObject, view.transform.Find("Container").Find("BackgroundFrame"));
                    } else if (buttonInfo is MultipleOptionButtonInfo) {
                        buttonInfo.gameObject = UnityEngine.Object.Instantiate(optionScreen.languageOption, view.transform.Find("Container").Find("BackgroundFrame"));
                    } else {
                        // TODO Mods add their own ButtonInfo
                        Console.WriteLine(buttonInfo.GetType() + " not a known type of OptionsButtonInfo!");
                    }
                    buttonInfo.gameObject.transform.SetParent(view.transform.Find("Container").Find("BackgroundFrame").Find("OptionsFrame").Find("OptionMenuButtons"));
                    buttonInfo.gameObject.name = buttonInfo.GetText?.Invoke() ?? "";
                    buttonInfo.gameObject.transform.name = buttonInfo.GetText?.Invoke() ?? "";
                    buttonInfo.addedTo = view;
                    foreach (TextMeshProUGUI text in buttonInfo.gameObject.GetComponentsInChildren<TextMeshProUGUI>()) {
                        if (text.name.Equals("OptionState") || text.name.Equals("Text"))
                            buttonInfo.stateTextMesh = text;
                        if (text.name.Equals("OptionName"))
                            buttonInfo.nameTextMesh = text;
                    }
                    Button button = buttonInfo.gameObject.GetComponentInChildren<Button>();
                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener(buttonInfo.onClick);

                    buttonInfo.OnInit(view);
                }
            }

            public static void OnSelectModOptions() {
                Manager<UIManager>.Instance.GetView<OptionScreen>().gameObject.SetActive(false);
                if (!ModOptionScreenLoaded)
                    ModOptionScreen = ModOptionScreen.BuildModOptionScreen(Manager<UIManager>.Instance.GetView<OptionScreen>());

                ShowView(ModOptionScreen, EScreenLayers.PROMPT, null, false);
            }

            public static View ShowView(View view, EScreenLayers layer = EScreenLayers.PROMPT, IViewParams screenParams = null, bool transitionIn = true, AnimatorUpdateMode animUpdateMode = AnimatorUpdateMode.Normal) {
                GameObject gameObject = view.gameObject;
                gameObject.SetActive(true);
                view.layer = layer;
                if (!view.useScreenSpaceOverlay) {
                    Manager<UIManager>.Instance.SetParentAndAlign(gameObject, Manager<UIManager>.Instance.Layers[(int)layer].gameObject);
                }
                Canvas canvas = view.GetComponent<Canvas>();
                canvas.overrideSorting = true;
                canvas.sortingLayerName = "UI";
                canvas.sortingOrder = Manager<UIManager>.Instance.GetNextSortingOrderForLayer(layer);
                Manager<UIManager>.Instance.AddSortingOrderForLayer(canvas.sortingOrder, layer);
                view.Init(screenParams);
                Manager<UIManager>.Instance.StartCoroutine(view.Intro(transitionIn, animUpdateMode));
                return view;
            }

            public static void RegisterOptionButton(OptionsButtonInfo buttonInfo) {
                OptionButtons.Add(buttonInfo);
            }

            public static ToggleButtonInfo RegisterToggleOptionButton(Func<string> GetText, UnityAction onClick, Func<ToggleButtonInfo, bool> GetState) {
                ToggleButtonInfo info = new ToggleButtonInfo(GetText, onClick, GetState,
                    (defaultOnLocID) => Manager<LocalizationManager>.Instance.GetText(defaultOnLocID),
                    (defaultOffLocID) => Manager<LocalizationManager>.Instance.GetText(defaultOffLocID)
                );
                RegisterOptionButton(info);
                return info;
            }

            public static ToggleButtonInfo RegisterToggleOptionButton(Func<string> GetText, UnityAction onClick, Func<ToggleButtonInfo, bool> GetState, Func<string, string> GetOnText, Func<string, string> GetOffText) {
                ToggleButtonInfo info = new ToggleButtonInfo(GetText, onClick, GetState, GetOnText, GetOffText);
                RegisterOptionButton(info);
                return info;
            }

            public static SubMenuButtonInfo RegisterSubMenuOptionButton(Func<string> GetText, UnityAction onClick) {
                SubMenuButtonInfo info = new SubMenuButtonInfo(GetText, onClick);
                RegisterOptionButton(info);
                return info;
            }

            public static TextEntryButtonInfo RegisterTextEntryOptionButton(Func<string> GetText, Func<string, bool> onEntry, int maxCharacter = 15, Func<string> GetEntryText = null, Func<string> GetInitialText = null, CharsetFlags charset = TextEntryButtonInfo.DEFAULT_CHARSET) {
                TextEntryButtonInfo info = new TextEntryButtonInfo(GetText, onEntry, maxCharacter, GetEntryText, GetInitialText, charset);
                RegisterOptionButton(info);
                return info;
            }

            public static MultipleOptionButtonInfo RegisterMultipleOptionButton(Func<string> GetText, UnityAction onClick, Action<int> onSwitch, Func<MultipleOptionButtonInfo, int> GetIndex, Func<int, string> GetTextForIndex) {
                MultipleOptionButtonInfo info = new MultipleOptionButtonInfo(GetText, onClick, onSwitch, GetIndex, GetTextForIndex);
                RegisterOptionButton(info);
                return info;
            }

            public static void RegisterModOptionButton(OptionsButtonInfo buttonInfo) {
                ModOptionButtons.Add(buttonInfo);
            }

            public static ToggleButtonInfo RegisterToggleModOptionButton(Func<string> GetText, UnityAction onClick, Func<ToggleButtonInfo, bool> GetState) {
                ToggleButtonInfo info = new ToggleButtonInfo(GetText, onClick, GetState,
                    (optionScreen) => Manager<LocalizationManager>.Instance.GetText(ModOptionScreen.onLocID),
                    (optionScreen) => Manager<LocalizationManager>.Instance.GetText(ModOptionScreen.offLocID)
                );
                RegisterModOptionButton(info);
                return info;
            }

            public static ToggleButtonInfo RegisterToggleModOptionButton(Func<string> GetText, UnityAction onClick, Func<ToggleButtonInfo, bool> GetState, Func<string, string> GetOnText, Func<string, string> GetOffText) {
                ToggleButtonInfo info = new ToggleButtonInfo(GetText, onClick, GetState, GetOnText, GetOffText);
                RegisterModOptionButton(info);
                return info;
            }

            public static SubMenuButtonInfo RegisterSubMenuModOptionButton(Func<string> GetText, UnityAction onClick) {
                SubMenuButtonInfo info = new SubMenuButtonInfo(GetText, onClick);
                RegisterModOptionButton(info);
                return info;
            }

            public static TextEntryButtonInfo RegisterTextEntryModOptionButton(Func<string> GetText, Func<string, bool> onEntry, int maxCharacter = 15, Func<string> GetEntryText = null, Func<string> GetInitialText = null, CharsetFlags charset = TextEntryButtonInfo.DEFAULT_CHARSET) {
                TextEntryButtonInfo info = new TextEntryButtonInfo(GetText, onEntry, maxCharacter, GetEntryText, GetInitialText, charset);
                RegisterModOptionButton(info);
                return info;
            }

            public static MultipleOptionButtonInfo RegisterMultipleModOptionButton(Func<string> GetText, UnityAction onClick, Action<int> onSwitch, Func<MultipleOptionButtonInfo, int> GetIndex, Func<int, string> GetTextForIndex) {
                MultipleOptionButtonInfo info = new MultipleOptionButtonInfo(GetText, onClick, onSwitch, GetIndex, GetTextForIndex);
                RegisterModOptionButton(info);
                return info;
            }
        }
    }
}
