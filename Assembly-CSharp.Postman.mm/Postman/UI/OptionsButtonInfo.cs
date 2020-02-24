using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Mod.Postman.UI {
    public abstract class OptionsButtonInfo {
        public UnityAction onClick;
        public TextMeshProUGUI nameTextMesh;
        public TextMeshProUGUI stateTextMesh;
        public string text;
        public GameObject gameObject;
        public OptionScreen optionScreen;

        protected OptionsButtonInfo(string text, UnityAction onClick) {
            this.text = text;
            this.onClick = onClick;
        }

        public abstract void UpdateStateText();

        public abstract string GetStateText();
    }
}
