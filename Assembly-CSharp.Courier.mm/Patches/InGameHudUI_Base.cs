#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Courier.Save;
using UnityEngine;

public class patch_InGameHudUI_Base : InGameHudUI_Base {
    private extern void orig_Start();
    private void Start() {
        // Hacky fix to make it not give an error for modded levels
        if(((patch_SaveManager)Manager<SaveManager>.Instance).GetSaveGameSlotIndex() == 3 && !Manager<DemoManager>.Instance.demoMode) {
            Manager<DemoManager>.Instance.demoMode = true;
            orig_Start();
            playerName.SetText(ModSaveGame.Instance.GetCurrentModSaveSlot().SlotName);
            Manager<DemoManager>.Instance.demoMode = false;
        } else {
            orig_Start();
        }
    }

    public extern void orig_RefreshTimeshards();
    public new void RefreshTimeshards() {
        if (((patch_SaveManager)Manager<SaveManager>.Instance).GetSaveGameSlotIndex() == 3) {
            coinCount.text = Manager<InventoryManager>.Instance.GetItemQuantity(EItems.TIME_SHARD).ToString();
            if (ModSaveGame.Instance.GetCurrentModSaveSlot().NewGamePlus) {
                if (Manager<InventoryManager>.Instance.GetItemQuantity(EItems.TIME_SHARD) >= Manager<GameManager>.Instance.GetNewGamePlusReviveCost() || Manager<ProgressionManager>.Instance.dealDone) {
                    coinCount.color = Color.white;
                } else {
                    coinCount.color = Color.red;
                }
            } else {
                coinCount.color = Color.white;
            }
        } else {
            orig_RefreshTimeshards();
        }
    }
}