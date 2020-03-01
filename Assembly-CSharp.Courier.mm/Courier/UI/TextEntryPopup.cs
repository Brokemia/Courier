using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mod.Courier.UI {
    public class TextEntryPopup : MonoBehaviour {
        public int maxCharacter = 15;

        public TextMeshProUGUI entryTextfield;

        public GameObject initialSelection;
        
        public AudioObjectDefinition eraseLetterSFX;
        
        protected string entryText = string.Empty;
        
        public event Action<string> onTextConfirmed;

        public event Action onBack;

        public void Init() {
            Init(string.Empty);
        }

        public void Init(string initialText) {
            entryText = initialText;
            UpdateNameField();
        }

        public void OnLetterSelected(string letter) {
            if (entryText.Length >= maxCharacter) {
                entryText = entryText.Remove(entryText.Length - 1, 1);
            }
            entryText += letter;
            UpdateNameField();
        }

        public void OnLetterErased() {
            if (entryText.Length > 0) {
                entryText = entryText.Remove(entryText.Length - 1, 1);
                UpdateNameField();
            } else {
                StartCoroutine(BackWhenBackButtonReleased());
            }
        }

        /// <summary>
        /// Waits until the back button is released then exits the text entry popup
        /// We do this to make sure it doesn't back out of the menu we came from
        /// </summary>
        /// <returns>The when back button released.</returns>
        public IEnumerator BackWhenBackButtonReleased() {
            while (Manager<InputManager>.Instance.GetBackDown())
                yield return null;
            gameObject.SetActive(false);
            onBack?.Invoke();
        }

        protected void UpdateNameField() {
            string entryTextFieldText = entryText;
            for (int i = entryTextFieldText.Length - 1; i < maxCharacter - 1; i++) {
                entryTextFieldText += "_";
            }
            entryTextfield.text = entryTextFieldText;
        }

        protected void Update() {
            if (Manager<InputManager>.Instance.GetStartDown() && !string.IsNullOrEmpty(entryText)) {
                gameObject.SetActive(false);
                onTextConfirmed?.Invoke(entryText);
                onBack?.Invoke();
                return;
            }
            if (Manager<InputManager>.Instance.GetBackDown()) {
                Manager<AudioManager>.Instance.PlaySoundEffect(eraseLetterSFX);
                OnLetterErased();
            }
        }

        public string GetEntryText() {
            return entryText;
        }
    }
}