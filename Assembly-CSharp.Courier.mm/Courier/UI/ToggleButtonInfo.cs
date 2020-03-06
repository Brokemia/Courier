using System;
using UnityEngine.Events;

namespace Mod.Courier.UI {
    public class ToggleButtonInfo : OptionsButtonInfo {
        // Takes this button info
        public Func<ToggleButtonInfo, bool> GetState;
        // Takes the default onLocID
        public Func<string, string> GetOnText;
        // Takes the default offLocID
        public Func<string, string> GetOffText;

        public ToggleButtonInfo(Func<string> text, UnityAction onClick, Func<ToggleButtonInfo, bool> GetState, Func<string, string> GetOnText, Func<string, string> GetOffText) : base(text, onClick) {
            this.GetOnText = GetOnText;
            this.GetOffText = GetOffText;
            this.GetState = GetState;
        }

        public override void UpdateStateText() {
            stateTextMesh.text = GetStateText();
        }

        public override string GetStateText() {
            return GetState?.Invoke(this) ?? false ? GetOnText(ModOptionScreen.onLocID) : GetOffText(ModOptionScreen.offLocID); // TODO Use Localization IDs instead
        }
    }
}
