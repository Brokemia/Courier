#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Courier;
using Mod.Courier.Helpers;
using TMPro;
using UnityEngine;

public class patch_PauseScreen : PauseScreen {

    string fullVersionString;

    private extern void orig_LateUpdate();
    private void LateUpdate() {
        orig_LateUpdate();
        versionText.enabled = true;
        versionText.text = fullVersionString;
    }

    private extern void orig_Start();
    private void Start() {
        fullVersionString = versionText.text;
        if(!string.IsNullOrEmpty(fullVersionString)) {
            fullVersionString += " + ";
        }
        fullVersionString += Courier.CourierVersionString;
        orig_Start();
    }
}
