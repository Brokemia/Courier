#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections;
using Mod.Postman;
using Mod.Postman.UI;

public class patch_DemoManager : DemoManager {

    private extern IEnumerator orig_UnloadScreenBeforeLevelLoading();
    private IEnumerator UnloadScreenBeforeLevelLoading() {
        Postman.UI.ModOptionScreen.Close(false);
        yield return orig_UnloadScreenBeforeLevelLoading();
        yield return null;
    }

}

