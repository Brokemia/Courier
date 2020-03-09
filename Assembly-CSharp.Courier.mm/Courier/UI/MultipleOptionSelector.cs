using AdvancedInspector;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Mod.Courier.UI {
    public class MultiplerOptionSelector : MonoBehaviour, IMoveHandler, ISelectHandler, IDeselectHandler, IEventSystemHandler {
        public MultipleOptionButtonInfo buttonInfo;

        public AudioObjectDefinition moveSFX;

        public RectTransform leftArrow;

        public RectTransform rightArrow;
        
        private bool selected;
        
        private void Start() {
            leftArrow = gameObject.transform.Find("ButtonContent").Find("LeftArrow").GetComponent<RectTransform>();
            rightArrow = gameObject.transform.Find("ButtonContent").Find("RightArrow").GetComponent<RectTransform>();
            buttonInfo.UpdateStateText();
            UpdateArrows();
        }

        private void OnEnable() {
            if (leftArrow == null || rightArrow == null) {
                leftArrow = gameObject.transform.Find("ButtonContent").Find("LeftArrow").GetComponent<RectTransform>();
                rightArrow = gameObject.transform.Find("ButtonContent").Find("RightArrow").GetComponent<RectTransform>();
            }
            if (!selected) {
                leftArrow.gameObject.SetActive(false);
                rightArrow.gameObject.SetActive(false);
            }
        }

        public void OnSelect(BaseEventData eventData) {
            leftArrow.gameObject.SetActive(true);
            rightArrow.gameObject.SetActive(true);
            selected = true;
        }

        public void OnDeselect(BaseEventData eventData) {
            leftArrow.gameObject.SetActive(false);
            rightArrow.gameObject.SetActive(false);
            selected = false;
        }

        private void Update() {
            if (selected && Manager<InputManager>.Instance.GetConfirmDown()) {
                buttonInfo.onClick?.Invoke();
            }
        }

        public void OnMove(AxisEventData eventData) {
            if (eventData.moveDir == MoveDirection.Left || eventData.moveDir == MoveDirection.Right) {
                buttonInfo.onSwitch(buttonInfo.GetIndex(buttonInfo) + (eventData.moveDir == MoveDirection.Left ? -1 : 1));
                buttonInfo.UpdateStateText();
                UpdateArrows();
                SFXAudioObject sFXAudioObject = Manager<AudioManager>.Instance.PlaySoundEffect(moveSFX);
                sFXAudioObject.IgnoreListenerPause(true);
            }
        }

        private void UpdateArrows() {
            buttonInfo.stateTextMesh.ForceMeshUpdate();
            Vector2 anchoredPosition = leftArrow.anchoredPosition;
            Vector2 anchoredPosition2 = buttonInfo.stateTextMesh.GetComponent<RectTransform>().anchoredPosition;
            anchoredPosition.x = anchoredPosition2.x - buttonInfo.stateTextMesh.renderedWidth - 8f;
            leftArrow.anchoredPosition = anchoredPosition;
            Vector2 anchoredPosition3 = rightArrow.anchoredPosition;
            Vector2 anchoredPosition4 = buttonInfo.stateTextMesh.GetComponent<RectTransform>().anchoredPosition;
            anchoredPosition3.x = anchoredPosition4.x + 8f;
            rightArrow.GetComponent<RectTransform>().anchoredPosition = anchoredPosition3;
        }
    }
}