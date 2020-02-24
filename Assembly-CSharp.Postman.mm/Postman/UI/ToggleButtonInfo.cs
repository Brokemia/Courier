using System;
using UnityEngine.Events;

namespace Mod.Postman.UI {
    public class ToggleButtonInfo : OptionsButtonInfo {
        public bool state;
        public Func<OptionScreen, string> GetOnText;
        public Func<OptionScreen, string> GetOffText;

        public ToggleButtonInfo(string text, UnityAction onClick, Func<OptionScreen, string> GetOnText, Func<OptionScreen, string> GetOffText) : base(text, onClick) {
            this.GetOnText = GetOnText;
            this.GetOffText = GetOffText;
        }

        public override void UpdateStateText() {
            stateTextMesh.text = GetStateText();
        }

        public override string GetStateText() {
            return state ? GetOnText(optionScreen) : GetOffText(optionScreen); // TODO Use Localization IDs instead
        }
    }
}
