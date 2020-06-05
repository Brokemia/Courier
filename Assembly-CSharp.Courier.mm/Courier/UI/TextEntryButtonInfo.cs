using System;
using Mod.Courier.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mod.Courier.UI {
    public class TextEntryButtonInfo : SubMenuButtonInfo {

        [Flags]
        public enum CharsetFlags {
            Letter = 1,
            Number = 2,
            Dash = 4,
            Space = 8,
            Dot = 16
        }

        public TextEntryPopup textEntryPopup { get; private set; }

        public Func<string> GetEntryText;

        public Func<string> GetInitialText;

        public Func<string, bool> onEntry;

        public CharsetFlags charsetFlags;

        public const CharsetFlags DEFAULT_CHARSET = CharsetFlags.Letter | CharsetFlags.Number | CharsetFlags.Dash | CharsetFlags.Space;

        /// <summary>
        /// The maximum number of characters that can be entered in the text field.
        /// Only matters until OnInit, don't try to set it after that
        /// </summary>
        public int maxCharacter;

        public TextEntryButtonInfo(Func<string> text, Func<string, bool> onEntry, int maxCharacters = 15, Func<string> GetEntryText = null, Func<string> GetInitialText = null, CharsetFlags charset = DEFAULT_CHARSET) : base(text, null) {
            onClick = onButtonClicked;
            this.GetEntryText = GetEntryText;
            this.GetInitialText = GetInitialText;
            maxCharacter = maxCharacters;
            this.onEntry = onEntry;
            charsetFlags = charset;
        }

        /// <summary>
        /// Calling classes should handle onBack and set the text entry popup to inactive themselves.
        /// </summary>
        /// <returns>The text entry popup.</returns>
        /// <param name="parentView">Parent view.</param>
        /// <param name="heading">Heading.</param>
        /// <param name="onEntry">On entry.</param>
        /// <param name="maxCharacter">Max character.</param>
        /// <param name="GetHeadingText">Get heading text.</param>
        /// <param name="charsetFlags">Charset flags.</param>
        public static TextEntryPopup InitTextEntryPopup(View parentView, string heading, Func<string, bool> onEntry, int maxCharacter, Func<string> GetHeadingText, CharsetFlags charsetFlags) {
            TextEntryPopup textEntryPopup = UnityEngine.Object.Instantiate(GameObjectTemplates.textEntryPopup);

            textEntryPopup.MaxCharacters = maxCharacter;
            textEntryPopup.charsetFlags = charsetFlags;
            textEntryPopup.transform.Find("BigFrame").Find("WhatIsYourName").GetComponent<TextMeshProUGUI>().SetText(GetHeadingText?.Invoke() ?? heading);
            textEntryPopup.entryTextfield = textEntryPopup.transform.Find("BigFrame").Find("WhatIsYourName").Find("ActualName").GetComponent<TextMeshProUGUI>();
            textEntryPopup.entryTextfield.name = "EntryTextfield";

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

            Manager<UIManager>.Instance.SetParentAndAlign(textEntryPopup.gameObject, parentView.gameObject);

            if ((charsetFlags & CharsetFlags.Dot) == CharsetFlags.Dot) {
                RectTransform dash = textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").Find("1") as RectTransform;
                GameObject dot = UnityEngine.Object.Instantiate(dash.gameObject);
                dot.name = ".";
                dot.transform.SetParent(textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid"));
                dot.transform.position = spaceIcon.transform.position;
                dot.GetComponent<TextMeshProUGUI>().SetText(".");
                dot.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                dot.GetComponent<Button>().onClick.AddListener(() => textEntryPopup.OnLetterSelected("."));
                dot.transform.localScale = Vector3.one;
                spaceIcon.transform.position += new Vector3(.7f, 0);
                eraseIcon.transform.position += new Vector3(.7f, 0);
            }

            // Remove numbers if they're disabled
            float numberRowY = textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").Find("1").position.y;
            if ((charsetFlags & CharsetFlags.Number) != CharsetFlags.Number) {
                foreach (Transform symbol in textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").GetChildren()) {
                    // Leave the dot alone, remove the dash only if it isn't in the charset
                    if (!symbol.name.Equals(".") && (!symbol.name.Equals("-") || (charsetFlags & CharsetFlags.Dash) != CharsetFlags.Dash)) {
                        UnityEngine.Object.Destroy(symbol.gameObject);
                    }
                }
                // If numbers are there, but the dash isn't
            } else if ((charsetFlags & CharsetFlags.Dash) != CharsetFlags.Dash) {
                UnityEngine.Object.Destroy(textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").Find("-").gameObject);
            }

            // Remove all the letters if they're disabled
            if ((charsetFlags & CharsetFlags.Letter) != CharsetFlags.Letter) {
                UnityEngine.Object.Destroy(textEntryPopup.transform.Find("BigFrame").Find("LetterGrid").gameObject);
                textEntryPopup.transform.Find("BigFrame").GetComponent<RectTransform>().sizeDelta -= new Vector2(0, 60);
                eraseIcon.position = new Vector3(eraseIcon.position.x, 6.669292f, eraseIcon.position.z);
                spaceIcon.position = new Vector3(spaceIcon.position.x, 6.669292f, spaceIcon.position.z);
            } else {
                foreach (Transform letter in textEntryPopup.transform.Find("BigFrame").Find("LetterGrid").GetChildren()) {
                    if (letter.name.Length == 1) {
                        letter.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                        letter.GetComponent<Button>().onClick.AddListener(() => textEntryPopup.OnLetterSelected(letter.name));
                    }
                }
            }

            if ((charsetFlags & CharsetFlags.Space) != CharsetFlags.Space) {
                UnityEngine.Object.Destroy(spaceIcon.gameObject);
                eraseIcon.Translate(new Vector2(-.7f, 0));
            }

            if ((charsetFlags & CharsetFlags.Dash) != CharsetFlags.Dash) {
                if ((charsetFlags & CharsetFlags.Space) == CharsetFlags.Space) {
                    spaceIcon.Translate(new Vector2(-.7f, 0));
                }
                eraseIcon.Translate(new Vector2(-.7f, 0));
            }

            SetInitialSelection(textEntryPopup, charsetFlags);

            textEntryPopup.onTextConfirmed += onEntry;

            return textEntryPopup;
        }

        public override void OnInit(View view) {
            base.OnInit(view);
            textEntryPopup = InitTextEntryPopup(addedTo, GetText?.Invoke() ?? "", onEntry, maxCharacter, GetEntryText, charsetFlags);

            textEntryPopup.onBack += OnCloseTextEntry;
            textEntryPopup.gameObject.SetActive(false);
        }

        public static void SetInitialSelection(TextEntryPopup textEntryPopup, CharsetFlags charsetFlags) {
            if (textEntryPopup == null)
                return;
            if ((charsetFlags & CharsetFlags.Letter) == CharsetFlags.Letter) {
                textEntryPopup.initialSelection = textEntryPopup.transform.Find("BigFrame").Find("LetterGrid").Find("A").gameObject;
            } else if ((charsetFlags & CharsetFlags.Number) == CharsetFlags.Number) {
                textEntryPopup.initialSelection = textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").Find("1").gameObject;
            } else if ((charsetFlags & CharsetFlags.Dash) == CharsetFlags.Dash) {
                textEntryPopup.initialSelection = textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").Find("-").gameObject;
            } else if ((charsetFlags & CharsetFlags.Dot) == CharsetFlags.Dot) {
                textEntryPopup.initialSelection = textEntryPopup.transform.Find("BigFrame").Find("SymbolsGrid").Find(".").gameObject;
            } else if ((charsetFlags & CharsetFlags.Space) == CharsetFlags.Space) {
                textEntryPopup.initialSelection = textEntryPopup.transform.Find("BigFrame").Find("SpaceIcon").gameObject;
            } else {
                textEntryPopup.initialSelection = textEntryPopup.transform.Find("BigFrame").Find("EraseIcon").gameObject;
            }
        }

        public static bool IsCharInCharset(char c, CharsetFlags charset) {
            return ((charset & CharsetFlags.Letter) == CharsetFlags.Letter && char.IsLetter(c)) ||
                ((charset & CharsetFlags.Number) == CharsetFlags.Number && char.IsDigit(c)) ||
                ((charset & CharsetFlags.Space) == CharsetFlags.Space && c == ' ') ||
                ((charset & CharsetFlags.Dash) == CharsetFlags.Dash && c == '-') ||
                ((charset & CharsetFlags.Dot) == CharsetFlags.Dot && c == '.');
        }

        protected void onButtonClicked() {
            textEntryPopup.Init(GetInitialText?.Invoke() ?? string.Empty);
            textEntryPopup.gameObject.SetActive(true);
            addedTo.gameObject.SetActive(false);
            textEntryPopup.transform.SetParent(addedTo.transform.parent);
            Canvas.ForceUpdateCanvases();
            if (!textEntryPopup.UseKeyboardInput) {
                textEntryPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = false;
                EventSystem.current.SetSelectedGameObject(textEntryPopup.initialSelection);
                textEntryPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = true;
            }
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
