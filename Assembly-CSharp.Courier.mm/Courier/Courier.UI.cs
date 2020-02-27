using System;
using System.Collections.Generic;
using Mod.Courier.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Mod.Courier {
    public partial class Courier {
        public static class UI {
            public static List<OptionsButtonInfo> OptionButtons = new List<OptionsButtonInfo>();
            public static List<OptionsButtonInfo> ModOptionButtons = new List<OptionsButtonInfo>();

            public static OptionsButtonInfo ModOptionButton;
            public static ModOptionScreen ModOptionScreen;
            public static bool ModOptionScreenLoaded;
            public static bool ModOptionScreenShowing;

            public static void SetupModdedUI() {
                ModOptionButton = RegisterSubMenuOptionButton("Mod Options", OnSelectModOptions);
            }

            public static void OnSelectModOptions() {
                Manager<UIManager>.Instance.GetView<OptionScreen>().gameObject.SetActive(false);
                if (!ModOptionScreenLoaded)
                    ModOptionScreen = ModOptionScreen.BuildModOptionScreen(Manager<UIManager>.Instance.GetView<OptionScreen>());

                ModOptionScreenShowing = true;
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

            public static ToggleButtonInfo RegisterToggleOptionButton(string text, UnityAction onClick) {
                ToggleButtonInfo info = new ToggleButtonInfo(text, onClick,
                    (defaultOnLocID) => Manager<LocalizationManager>.Instance.GetText(defaultOnLocID),
                    (defaultOffLocID) => Manager<LocalizationManager>.Instance.GetText(defaultOffLocID)
                );
                RegisterOptionButton(info);
                return info;
            }

            public static ToggleButtonInfo RegisterToggleOptionButton(string text, UnityAction onClick, Func<string, string> GetOnText, Func<string, string> GetOffText) {
                ToggleButtonInfo info = new ToggleButtonInfo(text, onClick, GetOnText, GetOffText);
                RegisterOptionButton(info);
                return info;
            }

            public static SubMenuButtonInfo RegisterSubMenuOptionButton(string text, UnityAction onClick) {
                SubMenuButtonInfo info = new SubMenuButtonInfo(text, onClick);
                RegisterOptionButton(info);
                return info;
            }

            public static void RegisterModOptionButton(OptionsButtonInfo buttonInfo) {
                ModOptionButtons.Add(buttonInfo);
            }

            public static ToggleButtonInfo RegisterToggleModOptionButton(string text, UnityAction onClick) {
                ToggleButtonInfo info = new ToggleButtonInfo(text, onClick,
                    (optionScreen) => Manager<LocalizationManager>.Instance.GetText(ModOptionScreen.onLocID),
                    (optionScreen) => Manager<LocalizationManager>.Instance.GetText(ModOptionScreen.offLocID)
                );
                RegisterModOptionButton(info);
                return info;
            }

            public static ToggleButtonInfo RegisterToggleModOptionButton(string text, UnityAction onClick, Func<string, string> GetOnText, Func<string, string> GetOffText) {
                ToggleButtonInfo info = new ToggleButtonInfo(text, onClick, GetOnText, GetOffText);
                RegisterModOptionButton(info);
                return info;
            }

            public static SubMenuButtonInfo RegisterSubMenuModOptionButton(string text, UnityAction onClick) {
                SubMenuButtonInfo info = new SubMenuButtonInfo(text, onClick);
                RegisterModOptionButton(info);
                return info;
            }
        }
    }
}
