#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable RECS0117 // Local variable has the same name as a member and hides it

using System;
using System.IO;
using Mod.Courier;
using Mod.Courier.Helpers;
using Mod.Courier.UI;
using UnityEngine;

public class patch_SaveGameSelectionScreen : SaveGameSelectionScreen {
    public extern void orig_Init(IViewParams screenParams);
    public new void Init(IViewParams screenParams) {
        NameSavePopup popupCopy = Instantiate(nameSavePopup);

        GameObject gameObject = new GameObject();
        GameObjectTemplates.textEntryPopup = gameObject.AddComponent<TextEntryPopup>();
        DontDestroyOnLoad(GameObjectTemplates.textEntryPopup);

        GameObjectTemplates.textEntryPopup.eraseLetterSFX = popupCopy.eraseLetterSFX;
        GameObjectTemplates.textEntryPopup.name = "ModdedTextEntry";
        // Iterate backwards so elements don't shift as lower ones are removed
        // If you know, you know
        for (int i = popupCopy.transform.childCount - 1; i >= 0; i--) {
            popupCopy.transform.GetChild(i).SetParent(GameObjectTemplates.textEntryPopup.transform, false);
        }
        GameObjectTemplates.textEntryPopup.gameObject.SetActive(false);
        GameObjectTemplates.textEntryPopup.transform.SetParent(null);

        orig_Init(screenParams);
    }
}