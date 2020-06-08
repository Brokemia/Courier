#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0114 // Member hides inherited member; missing override keyword

using System;
using System.Collections;
using System.IO;
using Mod.Courier;
using Mod.Courier.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class patch_OptionScreen : OptionScreen {
    public extern void orig_Init(IViewParams screenParams);
    public void Init(IViewParams screenParams) {
        Courier.UI.InitOptionsViewWithModButtons(this, Courier.UI.OptionButtons);

        orig_Init(screenParams);
    }

    private extern void orig_OnEnable();
    private void OnEnable() {
        orig_OnEnable();

    }

    private extern void orig_InitOptions();
    private void InitOptions() {
        orig_InitOptions();
        foreach (OptionsButtonInfo buttonInfo in Courier.UI.OptionButtons)
            buttonInfo.UpdateStateText();
    }

    public void OnGUI() {
        // Needs to be changed when I get TextLocalizer to cooperate and stuff
        for (int i = 0; i < Courier.UI.OptionButtons.Count; i++) {
            OptionsButtonInfo buttonInfo = Courier.UI.OptionButtons[i];
            //buttonInfo.nameTextMesh.text = buttonInfo.GetText?.Invoke() ?? "";
        }
    }

    private extern void orig_HideUnavailableOptions();
    private void HideUnavailableOptions() {
        orig_HideUnavailableOptions();
        foreach (OptionsButtonInfo buttonInfo in Courier.UI.OptionButtons) {
            buttonInfo.gameObject?.SetActive(buttonInfo.IsEnabled?.Invoke() ?? true);
        }
        backgroundFrame.sizeDelta += new Vector2(0, heightPerButton * Courier.UI.EnabledCustomOptionButtonsCount());
    }

    private extern void orig_OnDisable();
    private void OnDisable() {
        foreach(OptionsButtonInfo buttonInfo in Courier.UI.OptionButtons) {
            //buttonInfo.gameObject?.SetActive(false);
        }
        orig_OnDisable();
    }

}
