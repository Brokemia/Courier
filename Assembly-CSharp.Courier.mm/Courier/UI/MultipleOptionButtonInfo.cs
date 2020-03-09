using System;
using UnityEngine.Events;

namespace Mod.Courier.UI {
    public class MultipleOptionButtonInfo : OptionsButtonInfo {
        // Takes this button info
        public Func<MultipleOptionButtonInfo, int> GetIndex;
        // Takes the current index
        public Func<int, string> GetTextForIndex;

        public Action<int> onSwitch;

        public MultipleOptionButtonInfo(Func<string> text, UnityAction onClick, Action<int> onSwitch, Func<MultipleOptionButtonInfo, int> GetIndex, Func<int, string> GetTextForIndex) : base(text, onClick) {
            this.GetIndex = GetIndex;
            this.GetTextForIndex = GetTextForIndex;
            this.onSwitch = onSwitch;
        }

        public override void OnInit(View view) {
            base.OnInit(view);
            LanguageSelector languageSelector = gameObject.transform.Find("Button").GetComponent<LanguageSelector>();
            MultiplerOptionSelector selector = gameObject.transform.Find("Button").gameObject.AddComponent<MultiplerOptionSelector>();
            selector.moveSFX = languageSelector.moveSFX;
            selector.buttonInfo = this;
            UnityEngine.Object.Destroy(languageSelector);
        }

        public override void UpdateStateText() {
            stateTextMesh.text = GetStateText();
        }

        public override string GetStateText() {
            return GetTextForIndex?.Invoke(GetIndex?.Invoke(this) ?? -1) ?? string.Empty;
        }
    }
}
