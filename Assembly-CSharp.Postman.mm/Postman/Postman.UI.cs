using System;
using System.Collections.Generic;
using Mod.Postman.UI;
using UnityEngine.Events;

namespace Mod.Postman {
    public partial class Postman {
        public static class UI {
            public static List<OptionsButtonInfo> OptionButtons = new List<OptionsButtonInfo>();

            public static void RegisterOptionButton(OptionsButtonInfo buttonInfo) {
                OptionButtons.Add(buttonInfo);
            }

            public static ToggleButtonInfo RegisterToggleOptionButton(string text, UnityAction onClick) {
                ToggleButtonInfo info = new ToggleButtonInfo(text, onClick,
                    (optionScreen) => Manager<LocalizationManager>.Instance.GetText(optionScreen.onLocID),
                    (optionScreen) => Manager<LocalizationManager>.Instance.GetText(optionScreen.offLocID)
                );
                RegisterOptionButton(info);
                return info;
            }

            public static ToggleButtonInfo RegisterToggleOptionButton(string text, UnityAction onClick, Func<OptionScreen, string> GetOnText, Func<OptionScreen, string> GetOffText) {
                ToggleButtonInfo info = new ToggleButtonInfo(text, onClick, GetOnText, GetOffText);
                RegisterOptionButton(info);
                return info;
            }

            public static void RegisterModOptionsButton() {
                throw new NotImplementedException();
            }
        }
    }
}
