using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Mod.Courier.UI {
    public abstract class OptionsButtonInfo {
        public UnityAction onClick;
        public TextMeshProUGUI nameTextMesh;
        public TextMeshProUGUI stateTextMesh;
        public string text;
        public GameObject gameObject;
        public View addedTo;
        public Func<bool> IsEnabled;

        protected OptionsButtonInfo(string text, UnityAction onClick) {
            this.text = text;
            this.onClick = onClick;
        }

        public abstract void UpdateStateText();

        public abstract string GetStateText();

        /// <summary>
        /// Called during the Init method of the view this button is being added to.
        /// </summary>
        /// <param name="view">The view this button has been added to</param>
        public virtual void OnInit(View view) { }
    }
}
