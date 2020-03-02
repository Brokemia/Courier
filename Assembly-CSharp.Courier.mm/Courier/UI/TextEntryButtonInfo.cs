using System;
using Mod.Courier.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mod.Courier.UI {
    public class TextEntryButtonInfo : SubMenuButtonInfo {

        public TextEntryPopup textEntryPopup { get; private set; }

        public Func<string> GetEntryText;

        public Func<string> GetInitialText;

        public Action<string> onEntry;

        /// <summary>
        /// The maximum number of characters that can be entered in the text field.
        /// Only matters until OnInit, don't try to set it after that
        /// </summary>
        public int maxCharacter;

        public TextEntryButtonInfo(string text, Action<string> onEntry, int maxCharacters = 15, Func<string> GetEntryText = null, Func<string> GetInitialText = null) : base(text, null) {
            onClick = onButtonClicked;
            this.GetEntryText = GetEntryText;
            this.GetInitialText = GetInitialText;
            maxCharacter = maxCharacters;
            this.onEntry = onEntry;
        }

        public override void OnInit(View view) {
            base.OnInit(view);
            textEntryPopup = UnityEngine.Object.Instantiate(GameObjectTemplates.textEntryPopup);

            textEntryPopup.maxCharacter = maxCharacter;
            textEntryPopup.transform.Find("BigFrame").Find("WhatIsYourName").GetComponent<TextMeshProUGUI>().SetText(GetEntryText?.Invoke() ?? text);
            textEntryPopup.entryTextfield = textEntryPopup.transform.Find("BigFrame").Find("WhatIsYourName").Find("ActualName").GetComponent<TextMeshProUGUI>();
            textEntryPopup.entryTextfield.name = "EntryTextfield";
            textEntryPopup.initialSelection = textEntryPopup.transform.Find("BigFrame").Find("LetterGrid").Find("A").gameObject;
            foreach(Transform letter in textEntryPopup.transform.Find("BigFrame").Find("LetterGrid").GetChildren()) {
                if (letter.name.Length == 1) {
                    letter.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    letter.GetComponent<Button>().onClick.AddListener(() => textEntryPopup.OnLetterSelected(letter.name));
                }
            }
            foreach (Transform letter in textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").GetChildren()) {
                if (letter.name.Length == 1) {
                    letter.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    letter.GetComponent<Button>().onClick.AddListener(() => textEntryPopup.OnLetterSelected(letter.name));
                }
            }
            Transform eraseIcon = textEntryPopup.transform.Find("BigFrame").Find("EraseIcon");
            eraseIcon.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            eraseIcon.GetComponent<Button>().onClick.AddListener(textEntryPopup.OnLetterErased);
            Transform spaceIcon = textEntryPopup.transform.Find("BigFrame").Find("SpaceIcon");
            spaceIcon.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
            spaceIcon.GetComponent<Button>().onClick.AddListener(() => textEntryPopup.OnLetterSelected(" "));

            textEntryPopup.onTextConfirmed += onEntry;
            textEntryPopup.onBack += OnCloseTextEntry;
            textEntryPopup.gameObject.SetActive(false);
        }

        protected void onButtonClicked() {
            textEntryPopup.Init(GetInitialText?.Invoke() ?? string.Empty);
            textEntryPopup.gameObject.SetActive(true);
            Manager<UIManager>.Instance.SetParentAndAlign(textEntryPopup.gameObject, addedTo.gameObject);
            textEntryPopup.transform.SetParent(addedTo.transform.parent);
            addedTo.gameObject.SetActive(false);
            Canvas.ForceUpdateCanvases();
            textEntryPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = false;
            EventSystem.current.SetSelectedGameObject(textEntryPopup.initialSelection);
            textEntryPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = true;
        }

        protected void OnCloseTextEntry() {
            textEntryPopup.gameObject.SetActive(false);
            addedTo.gameObject.SetActive(true);
            gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>().playAudio = false;
            EventSystem.current.SetSelectedGameObject(gameObject.transform.Find("Button").gameObject);
            // Specific check for ModOptionScreen to stop it from overwriting selection one frame later
            // I don't like having the special case, but whatever
            if(addedTo is ModOptionScreen modOption) {
                modOption.initialSelection = gameObject;
            }
            gameObject.transform.Find("Button").GetComponent<UIObjectAudioHandler>().playAudio = true;
        }
    }
}
