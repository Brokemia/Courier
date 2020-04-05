using System;
using Mod.Courier.Save;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

namespace Mod.Courier.UI {
    public abstract class OptionsButtonInfo {
        public UnityAction onClick;
        public TextMeshProUGUI nameTextMesh;
        public TextMeshProUGUI stateTextMesh;
        public Func<string> GetText;
        public GameObject gameObject;
        public View addedTo;
        public Func<bool> IsEnabled;
        public OptionSaveMethod SaveMethod = new OptionSaveMethod();

        protected OptionsButtonInfo(Func<string> GetText, UnityAction onClick) {
            this.GetText = GetText;
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
