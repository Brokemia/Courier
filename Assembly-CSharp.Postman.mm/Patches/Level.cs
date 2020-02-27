#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Postman;

public class patch_Level : Level {
    private extern void orig_LateUpdate();
    private void LateUpdate() {
        // Essentially, make sure the ModOptionScreen isn't showing when trying to open the inventory or map
        if (Manager<PauseManager>.Instance.CanPause() && Manager<InputManager>.Instance.GetStartDown()) {
            orig_LateUpdate();
        } else if(!Postman.UI.ModOptionScreenShowing) {
            orig_LateUpdate();
        }
    }
}
