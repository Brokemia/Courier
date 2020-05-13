#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections;
using Mod.Courier;
using Mod.Courier.UI;
using MonoMod;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class patch_TitleScreen : TitleScreen {

    public const string PLAY_MODS_BUTTON_LOC_ID = "COURIER_PLAY_MODS_BUTTON";

    public GameObject modsModeButton;

    public GameObject modsModeVisualContainer;

    private extern IEnumerator orig_TrackStart();
    private IEnumerator TrackStart() {
        yield return orig_TrackStart();
        /*if (modsModeButton == null) {
            modsModeButton = Instantiate(normalModeButton, playModeSelectionObject.transform);
            modsModeVisualContainer = modsModeButton.transform.Find("VisualContainer").gameObject;
            // Set it as the third button, after the demo and play buttons
            modsModeButton.transform.SetSiblingIndex(2);
            TextLocalizer modsModeTextLocalizer = modsModeVisualContainer.transform.Find("Text").GetComponent<TextLocalizer>();
            modsModeTextLocalizer.locID = PLAY_MODS_BUTTON_LOC_ID;
            modsModeTextLocalizer.Refresh();
            Button buttonComp = modsModeButton.GetComponent<Button>();
            buttonComp.onClick = new Button.ButtonClickedEvent();
            buttonComp.onClick.AddListener(BootModsMode);

            // Shift the whole set of buttons up a little so the Quit To Desktop doesn't overlap the copyright stuff
            playModeSelectionObject.transform.Translate(new Vector3(0, .5f));
        }*/
    }

    public void BootModsMode() {
        Manager<DemoManager>.Instance.demoMode = false;
        EventSystem.current.SetSelectedGameObject(null);
        StartCoroutine(ModsModeSelectionCoroutine());
    }

    private IEnumerator ModsModeSelectionCoroutine() {
        Manager<AudioManager>.Instance.PlaySoundEffect(pressStartSound);
        int blinkCount = 15;
        while (blinkCount != 0) {
            modsModeVisualContainer.SetActive(!modsModeVisualContainer.activeSelf);
            blinkCount--;

            yield return new WaitForSeconds(0.1f);
        }
        modsModeVisualContainer.SetActive(false);

        GameObject saveScreenObject = Instantiate(Manager<UIManager>.Instance.GetView<SaveGameSelectionScreen>().gameObject);
        SaveGameSelectionScreen saveScreen = saveScreenObject.GetComponent<SaveGameSelectionScreen>();
        ModSaveSelectionScreen modSaveScreen = saveScreenObject.AddComponent<ModSaveSelectionScreen>();
        CopyOverSaveScreenToModded(saveScreen, modSaveScreen);
        Destroy(saveScreen);
        Courier.UI.ShowView(modSaveScreen, EScreenLayers.MAIN, null, false);
        modSaveScreen.GoOffscreenInstant();

        modSaveScreen.GetInScreen();
        //saveScreen.GetInScreen();
        MoveOffscreen();
    }

    [MonoModIgnore]
    private extern void MoveOffscreen();

    private void CopyOverSaveScreenToModded(SaveGameSelectionScreen saveScreen, ModSaveSelectionScreen modSaveScreen) {
        modSaveScreen.animator = saveScreen.animator;
        modSaveScreen.useScreenSpaceOverlay = saveScreen.useScreenSpaceOverlay;
        modSaveScreen.layer = saveScreen.layer;
        modSaveScreen.newCyclePopup = saveScreen.newCyclePopup;
        modSaveScreen.newGamePlusPopup = saveScreen.newGamePlusPopup;
        modSaveScreen.saveSlotPrefab = saveScreen.saveSlotPrefab;
        modSaveScreen.normalNewGameButton = saveScreen.normalNewGameButton;
        modSaveScreen.continueButton = saveScreen.continueButton;
        modSaveScreen.deleteInstructions = saveScreen.deleteInstructions;
        modSaveScreen.confirmInstructions = saveScreen.confirmInstructions;
        modSaveScreen.confirmPopup = saveScreen.confirmPopup;
        modSaveScreen.nameSavePopup = saveScreen.nameSavePopup;
        modSaveScreen.focusSlotPosition = saveScreen.focusSlotPosition;
        modSaveScreen.newGamePlusFocusPosition = saveScreen.newGamePlusFocusPosition;
        modSaveScreen.newGamePlusBaseSlot_1 = saveScreen.newGamePlusBaseSlot_1;
        modSaveScreen.newGamePlusBaseSlot_2 = saveScreen.newGamePlusBaseSlot_2;
        modSaveScreen.newGamePlusSelectBaseInstructions = saveScreen.newGamePlusSelectBaseInstructions;
        modSaveScreen.selectItemPopup = saveScreen.selectItemPopup;
        modSaveScreen.deleteSaveConfirmationLocId = saveScreen.deleteSaveConfirmationLocId;
    }

    public extern void orig_GetInScreen(GameObject buttonToSelect);
    public new void GetInScreen(GameObject buttonToSelect) {
        orig_GetInScreen(buttonToSelect);
        //modsModeButton.SetActive(true);
        //modsModeVisualContainer.SetActive(true);
    }
    public extern void orig_MoveOffscreenDone();
    public new void MoveOffscreenDone() {
        orig_MoveOffscreenDone();
        //modsModeButton.SetActive(false);
    }
}