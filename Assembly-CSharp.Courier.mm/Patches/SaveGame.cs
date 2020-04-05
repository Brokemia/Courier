#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Courier;
using Mod.Courier.Save;

public class patch_SaveGame : SaveGame {
    public extern void orig_LoadOptions();
    public new void LoadOptions() {
        orig_LoadOptions();
        ModdedOptionsSave.Instance.LoadOptions();
    }

    public extern void orig_UpdateOptionsData();
    public new void UpdateOptionsData() {
        orig_UpdateOptionsData();
        ModdedOptionsSave.Instance.UpdateOptionsData();
    }
}