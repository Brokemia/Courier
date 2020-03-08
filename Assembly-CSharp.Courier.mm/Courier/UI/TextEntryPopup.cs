using System;
using System.Collections;
using Mod.Courier.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mod.Courier.UI {
    public class TextEntryPopup : MonoBehaviour {
        public int maxCharacter = 15;

        public bool blueFrame = true;

        public TextMeshProUGUI entryTextfield;

        public GameObject initialSelection;
        
        public AudioObjectDefinition eraseLetterSFX;
        
        protected string entryText = string.Empty;

        /// <summary>
        /// Called when the user hits confirm on the text entry. Takes the text entered and returns whether or not to call onBack.
        /// </summary>
        public event Func<string, bool> onTextConfirmed;

        public event Action onBack;

        public void Init() {
            Init(string.Empty);
        }

        public void Init(string initialText) {
            if (blueFrame) {
                Sprite borderSprite = transform.Find("BigFrame").GetComponent<Image>().sprite = Courier.EmbeddedSprites["Mod.Courier.UI.mod_options_frame"];
                borderSprite.bounds.extents.Set(1.7f, 1.7f, 0.1f);
                borderSprite.texture.filterMode = FilterMode.Point;

                // Make the selection frame blue
                //transform.Find("BigFrame").Find("LetterSelectionFrame").GetComponent<Image>().color = new Color(0, .633f, 1f);
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
                } catch {
                    Console.WriteLine("Image not Read/Writeable when recoloring selection frames in ModOptionScreen");
                }
            }
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
        }

        public string GetEntryText() {
            return entryText;
        }
    }
}