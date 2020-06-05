using System;
using System.Collections;
using Mod.Courier.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mod.Courier.UI {
    public class TextEntryPopup : MonoBehaviour {
        public int MaxCharacters = 15;

        public bool BlueFrame = true;

        /// <summary>
        /// Whether or not to allow users to input text using the keyboard rather than moving a selection. Incompatible with controllers. <see langword="true"/> by default.
        /// </summary>
        public bool UseKeyboardInput = true;

        public TextEntryButtonInfo.CharsetFlags charsetFlags = TextEntryButtonInfo.DEFAULT_CHARSET;

        public TextMeshProUGUI entryTextfield;

        public GameObject initialSelection;
        
        public AudioObjectDefinition eraseLetterSFX;
        
        protected string entryText = string.Empty;

        private Coroutine backWhenBackReleasedRoutine;

        /// <summary>
        /// Called when the user hits confirm on the text entry. Takes the text entered and returns whether or not to call onBack.
        /// </summary>
        public event Func<string, bool> onTextConfirmed;

        public event Action onBack;

        public void Init() {
            Init(string.Empty);
        }

        public void Init(string initialText) {
            if (BlueFrame) {
                Sprite borderSprite = transform.Find("BigFrame").GetComponent<Image>().sprite = Courier.EmbeddedSprites["Mod.Courier.UI.mod_options_frame"];
                borderSprite.bounds.extents.Set(1.7f, 1.7f, 0.1f);
                borderSprite.texture.filterMode = FilterMode.Point;

                // Make the selection frame blue
                // Unnecessary for just keyboard input
                if (!UseKeyboardInput) {
                    Image image = transform.Find("BigFrame").Find("LetterSelectionFrame").GetComponent<Image>();
                    try {
                        if (image.overrideSprite != null && image.overrideSprite.name != "Empty") {
                            RenderTexture rt = new RenderTexture(image.overrideSprite.texture.width, image.overrideSprite.texture.height, 0);
                            RenderTexture.active = rt;
                            Graphics.Blit(image.overrideSprite.texture, rt);

                            Texture2D res = new Texture2D(rt.width, rt.height, TextureFormat.RGBA32, true);
                            res.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0, false);

                            Color[] pxls = res.GetPixels();
                            for (int i = 0; i < pxls.Length; i++) {
                                if (Math.Abs(pxls[i].r - .973) < .01 && Math.Abs(pxls[i].g - .722) < .01) {
                                    pxls[i].r = 0;
                                    pxls[i].g = .633f;
                                    pxls[i].b = 1;
                                }
                            }
                            res.SetPixels(pxls);
                            res.Apply();

                            Sprite sprite = image.overrideSprite = Sprite.Create(res, new Rect(0, 0, res.width, res.height), new Vector2(16, 16), 20, 1, SpriteMeshType.FullRect, new Vector4(5, 5, 5, 5));
                            sprite.bounds.extents.Set(.8f, .8f, 0.1f);
                            sprite.texture.filterMode = FilterMode.Point;
                        }
                    } catch(Exception e) {
                        CourierLogger.Log(LogType.Exception, "TextEntryPopup", "Image not Read/Writeable when recoloring selection frames in TextEntryPopup");
                        CourierLogger.LogDetailed(e);
                    }
                } else {
                    transform.Find("BigFrame").Find("LetterSelectionFrame").gameObject.SetActive(false);
                    // Disable all the letters and numbers
                    foreach(Button b in transform.Find("BigFrame").GetComponentsInChildren<Button>()) {
                        b.interactable = false;
                    }
                }
            }
            entryText = initialText;
            UpdateNameField();
        }

        public void OnLetterSelected(string letter) {
            if (entryText.Length >= MaxCharacters) {
                entryText = entryText.Remove(entryText.Length - 1, 1);
            }
            entryText += letter;
            UpdateNameField();
        }

        public void OnLetterErased() {
            if (entryText.Length > 0) {
                entryText = entryText.Remove(entryText.Length - 1, 1);
                UpdateNameField();
            } else if (backWhenBackReleasedRoutine == null) {
                backWhenBackReleasedRoutine = StartCoroutine(BackWhenBackButtonReleased());
            }
        }

        /// <summary>
        /// Waits until the back button is released then exits the text entry popup
        /// We do this to make sure it doesn't back out of the menu we came from too
        /// </summary>
        /// <returns>The when back button released.</returns>
        public IEnumerator BackWhenBackButtonReleased() {
            while (Manager<InputManager>.Instance.GetBackDown())
                yield return null;
            gameObject.SetActive(false);
            onBack?.Invoke();
            backWhenBackReleasedRoutine = null;
        }

        protected void UpdateNameField() {
            string entryTextFieldText = entryText;
            for (int i = entryTextFieldText.Length - 1; i < MaxCharacters - 1; i++) {
                entryTextFieldText += "_";
            }
            entryTextfield.text = entryTextFieldText;
        }

        protected void Update() {
            if (!UseKeyboardInput) {
                if (Manager<InputManager>.Instance.GetStartDown() && !string.IsNullOrEmpty(entryText)) {
                    if (onTextConfirmed?.Invoke(entryText) ?? true) {
                        gameObject.SetActive(false);
                        onBack?.Invoke();
                    }
                    return;
                }
                if (Manager<InputManager>.Instance.GetBackDown()) {
                    Manager<AudioManager>.Instance.PlaySoundEffect(eraseLetterSFX);
                    OnLetterErased();
                }
            } else {
                // Handle typing with a keyboard instead of painstakingly selecting letters
                // Loop through every character typed this frame
                foreach (char c in Input.inputString) {
                    // Deal with special situations like backspace or enter
                    if (c == '\b') {
                        Manager<AudioManager>.Instance.PlaySoundEffect(eraseLetterSFX);
                        OnLetterErased();
                    } else if ((c == '\n' || c == '\r') && !string.IsNullOrEmpty(entryText)) {
                        if (onTextConfirmed?.Invoke(entryText) ?? true) {
                            gameObject.SetActive(false);
                            onBack?.Invoke();
                        }
                        return;
                    } else { // Handle all other letters
                        // Check to see if the letter is in the allowable characters
                        if (TextEntryButtonInfo.IsCharInCharset(c, charsetFlags)) {
                            OnLetterSelected(c.ToString());
                        }
                    }
                }
            }
        }

        public string GetEntryText() {
            return entryText;
        }
    }
}