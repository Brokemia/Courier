using System;
using System.Collections.Generic;
using Mod.Courier.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
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
        }
    }
}
