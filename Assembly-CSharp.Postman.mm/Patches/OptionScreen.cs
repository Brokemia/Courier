#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword

using System;
using System.Collections;
using Mod.Postman;
using Mod.Postman.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class patch_OptionScreen : OptionScreen{
    public extern void orig_Init(IViewParams screenParams);
    public void Init(IViewParams screenParams) {
        foreach (OptionsButtonInfo buttonInfo in Postman.UI.OptionButtons) {
            buttonInfo.optionScreen = this;
            buttonInfo.gameObject = Instantiate(fullscreenOption, backgroundFrame);
            foreach (TextMeshProUGUI text in buttonInfo.gameObject.GetComponentsInChildren<TextMeshProUGUI>()) {
                if (text.name.Equals("OptionState"))
                    buttonInfo.stateTextMesh = text;
                if (text.name.Equals("OptionName"))
                    buttonInfo.nameTextMesh = text;
            }
            Button button = buttonInfo.gameObject.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(buttonInfo.onClick);
        }
        orig_Init(screenParams);
    }

    private extern void orig_InitOptions();
    private void InitOptions() {
        orig_InitOptions();
        foreach (OptionsButtonInfo buttonInfo in Postman.UI.OptionButtons)
            buttonInfo.UpdateStateText();
    }

    private extern void orig_LateUpdate();
    private void LateUpdate() {
        for (int i = 0; i < Postman.UI.OptionButtons.Count; i++) {
            OptionsButtonInfo buttonInfo = Postman.UI.OptionButtons[i]; 
            buttonInfo.nameTextMesh.text = buttonInfo.text; // TODO Patch LoadGeneralLoc to load custom language files
            // TODO Find an earlier place to set this. Currently, it flickers briefly before putting itself in the right spot
            buttonInfo.gameObject.transform.position = controlsButton.transform.position - new Vector3(20.2f, .9f * (i+1));
            foreach (GameObject obj in FindObjectsOfType<GameObject>()) {
                if (obj.activeInHierarchy && obj.name.Equals("Back")) {
                    obj.transform.position = buttonInfo.gameObject.transform.position + new Vector3(10.4f, -.9f);
                }
            }
        }
        orig_LateUpdate();
    }

    //private extern void orig_InitOptions();
    //private void InitOptions() {
    //    for (int i = 0; i < Postman.UI.OptionButtons.Count; i++) {
    //        OptionsButtonInfo buttonInfo = Postman.UI.OptionButtons[i];
    //        buttonInfo.gameObject.transform.position = controlsButton.transform.position - new Vector3(20.2f, .9f * (i + 1));
    //        foreach (GameObject obj in FindObjectsOfType<GameObject>()) {
    //            if (obj.activeInHierarchy && obj.name.Equals("Back")) {
    //                obj.transform.position = buttonInfo.gameObject.transform.position + new Vector3(10.4f, -.9f);
    //            }
    //        }
    //    }
    //    orig_InitOptions();Console.WriteLine("\n\n\n\n\n\n\n\n\n\n\n\nAwake");
    //    for (int i = 0; i < Postman.UI.OptionButtons.Count; i++) {
    //        OptionsButtonInfo buttonInfo = Postman.UI.OptionButtons[i];
    //        buttonInfo.gameObject.transform.position = controlsButton.transform.position - new Vector3(20.2f, .9f * (i + 1));
    //        foreach (GameObject obj in FindObjectsOfType<GameObject>()) {
    //            if (obj.activeInHierarchy && obj.name.Equals("Back")) {
    //                obj.transform.position = buttonInfo.gameObject.transform.position + new Vector3(10.4f, -.9f);
    //            }
    //        }
    //    }
    //}

    private extern void orig_HideUnavailableOptions();
    private void HideUnavailableOptions() {
        orig_HideUnavailableOptions();
        backgroundFrame.sizeDelta += new Vector2(0, heightPerButton * Postman.UI.OptionButtons.Count);
    }

}
