#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Courier.Helpers;
using Mod.Courier.Save;
using MonoMod;
using UnityEngine;

public class patch_SaveManager : SaveManager {

    [MonoModIgnore]
    private int saveGameSlotIndex;

    [MonoModIgnore]
    private extern void DoActualSaving(bool applySaveDelay = true);

    public int GetSaveGameSlotIndex() {
        return saveGameSlotIndex;
    }

    public extern SaveGameSlot orig_GetCurrentSaveGameSlot();
    public new SaveGameSlot GetCurrentSaveGameSlot() {
        if (saveGameSlotIndex == 3) {
            return ModSaveGame.Instance.GetCurrentModSaveSlot();
        }
        return orig_GetCurrentSaveGameSlot();
    }

    public extern void orig_Save(bool applySaveDelay);
    public new void Save(bool applySaveDelay = true) {
        if (saveGameSlotIndex == 3) {
            if (CanSave()) {
                this.Raise("onBeforeSave", EventArgs.Empty);
                SaveGameSlot currentSaveGameSlot = ModSaveGame.Instance.GetCurrentModSaveSlot();
                Manager<ProgressionManager>.Instance.UpdateFlagData();
                currentSaveGameSlot.UpdateSaveGameData();
                SaveCurrentGameSlot(applySaveDelay);
                Manager<ProgressionManager>.Instance.lastSaveTime = Time.time;
            }
        } else {
            orig_Save(applySaveDelay);
        }
    }

    public extern virtual void orig_SaveCurrentGameSlot(bool applySaveDelay);
    public new virtual void SaveCurrentGameSlot(bool applySaveDelay = true) {
        if (saveGameSlotIndex == 3) {
            SaveGameSlot currentSaveGameSlot = ModSaveGame.Instance.GetCurrentModSaveSlot();
            if (currentSaveGameSlot != null && CanSave()) {
                DoActualSaving(applySaveDelay);
            }
        } else {
            orig_SaveCurrentGameSlot(applySaveDelay);
        }
    }
}