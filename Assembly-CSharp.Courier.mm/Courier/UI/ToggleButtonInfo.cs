using System;
using UnityEngine.Events;

namespace Mod.Courier.UI {
    public class ToggleButtonInfo : OptionsButtonInfo {
        public bool state;
        // Takes the default onLocID
        public Func<string, string> GetOnText;
        // Takes the default offLocID
        public Func<string, string> GetOffText;

        public ToggleButtonInfo(string text, UnityAction onClick, Func<string, string> GetOnText, Func<string, string> GetOffText) : base(text, onClick) {
            this.GetOnText = GetOnText;
            this.GetOffText = GetOffText;
        }

        public override void UpdateStateText() {
            stateTextMesh.text = GetStateText();
        }

        public override string GetStateText() {
            return state ? GetOnText(ModOptionScreen.onLocID) : GetOffText(ModOptionScreen.offLocID); // TODO Use Localization IDs instead
        }
    }
}
