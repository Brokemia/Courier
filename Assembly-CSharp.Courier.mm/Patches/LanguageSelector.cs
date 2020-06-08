#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using Mod.Courier;
using Mod.Courier.UI;
using MonoMod;
using UnityEngine;

public class patch_LanguageSelector : LanguageSelector {

    [MonoModIgnore]
    private bool selected;

    private extern void orig_Update();
    private void Update() {
        orig_Update();
        if (selected && Manager<InputManager>.Instance.GetConfirmDown()) {
            // Update all the modded options text
            foreach(OptionsButtonInfo buttonInfo in Courier.UI.OptionButtons) {
                buttonInfo.UpdateNameText();
            }
            foreach (OptionsButtonInfo buttonInfo in Courier.UI.ModOptionButtons) {
                buttonInfo.UpdateNameText();
            }
        }
    }
}
