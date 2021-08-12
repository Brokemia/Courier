using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mod.Courier.Helpers;
using Mod.Courier.Module;
using Mod.Courier.Save;
using MonoMod.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Mod.Courier.UI {
    public class ModSaveSelectionScreen : View {
        public enum EScreenState {
            SELECT_SLOT,
            SELECTING_NAME,
            SELECTING_ITEM,
            ERASING_SLOT,
            SELECTING_NEW_GAME_PLUS_BASE,
            TRANSLATING,
            SELECTING_NEW_GAME_TYPE,
            SELECTING_CONTINUE_OR_RESET
        }

        public GameObject newCyclePopup;

        public GameObject newGamePlusPopup;

        public GameObject saveSlotPrefab;

        public GameObject normalNewGameButton;

        public GameObject continueButton;

        public List<RectTransform> saveSlotContainers;

        public List<RectTransform> newGamePlusContainers;

        public GameObject deleteInstructions;

        public GameObject confirmInstructions;

        [FormerlySerializedAs("deleteSavePopup")]
        public SaveGameSelectionConfirmPopup confirmPopup;

        public NameSavePopup nameSavePopup;

        public RectTransform focusSlotPosition;

        public RectTransform newGamePlusFocusPosition;

        public RectTransform newGamePlusBaseSlot_1;

        public RectTransform newGamePlusBaseSlot_2;

        public GameObject newGamePlusSelectBaseInstructions;

        public NewGamePlusSelectItemPopup selectItemPopup;

        public string deleteSaveConfirmationLocId;

        private bool loadingSaveFile;

        private bool selectingNewGamePlusBase;

        private const ELevel FIRST_LEVEL = ELevel.Level_01_NinjaVillage;

        private List<ModSaveSlotUI> slotsUI = new List<ModSaveSlotUI>();

        private ModSaveSlotUI focusedSlot;

        private Vector3 focusedSlotOriginalPos;

        private RectTransform activeFocusPosition;

        private ModSaveSlotUI newGameSelectedSlot;

        private ModSaveSlotUI loadGameSelectedSlot;

        private bool saveGameSelected;

        private bool isInFocus;

        private EScreenState state;

        public int numSlots => ModSaveGame.Instance.modSaveSlots.Count;

        public override void Init(IViewParams screenParams) {
            base.Init(screenParams);
            CreateSaveSlots();
            saveGameSelected = false;
            newGamePlusPopup.SetActive(false);
            newCyclePopup.SetActive(false);
            selectItemPopup.gameObject.SetActive(false);
            confirmPopup.gameObject.SetActive(false);
            nameSavePopup.gameObject.SetActive(false);
            newGamePlusSelectBaseInstructions.gameObject.SetActive(false);
        }

        private void CreateSaveSlots() {
            Transform containers = transform.Find("Container").Find("Background").Find("SaveSlotContainer");
            saveSlotContainers = new List<RectTransform> {
                (RectTransform)containers.Find("Slot_1Container").Find("SaveSlotContainer"),
                (RectTransform)containers.Find("Slot_2Container").Find("SaveSlotContainer"),
                (RectTransform)containers.Find("Slot_3Container").Find("SaveSlotContainer")
            };

            newGamePlusContainers = new List<RectTransform> {
                (RectTransform)containers.Find("Slot_1Container").Find("NewGamePlusContainer"),
                (RectTransform)containers.Find("Slot_2Container").Find("NewGamePlusContainer"),
                (RectTransform)containers.Find("Slot_3Container").Find("NewGamePlusContainer")
            };


            Vector3 lastSlotPos = containers.Find("Slot_3Container").transform.position;
            // Add any extra slot containers
            for (int i = 3; i < numSlots; i++) {
                RectTransform container = (RectTransform)Instantiate(containers.Find("Slot_1Container"), containers);
                container.position = lastSlotPos -= new Vector3(0, 5.25f, 0);
                saveSlotContainers.Add((RectTransform)container.Find("SaveSlotContainer"));
                newGamePlusContainers.Add((RectTransform)container.Find("NewGamePlusContainer"));
            }

            for (int i = 0; i < numSlots; i++) {
                SaveSlotUI vanillaSlot = Instantiate(saveSlotPrefab).GetComponent<SaveSlotUI>();
                ModSaveSlotUI slot = vanillaSlot.gameObject.AddComponent<ModSaveSlotUI>();
                CopyOverSaveSlotToModded(vanillaSlot, slot);
                slot.OnEnable();
                Destroy(vanillaSlot);

                RectTransform tf = slot.GetComponent<RectTransform>();
                tf.SetParent(saveSlotContainers[i], false);
                tf.localPosition = Vector3.zero;
                tf.localScale = Vector3.one;
                slot.SetSaveData(ModSaveGame.Instance.modSaveSlots[i], i);
                slot.newGamePlusPopupAnchor = newGamePlusContainers[i];
                slot.onSubmit += OnSlotSelected;
                slot.onEraseSlot += OnEraseSlot;
                slotsUI.Add(slot);
            }

            for (int i = 0; i < numSlots; i++) {
                Navigation navigation = slotsUI[i].button.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                if (i != 0) {
                    navigation.selectOnUp = slotsUI[i - 1].button;
                } else {
                    navigation.selectOnUp = null;
                }
                if (i < numSlots - 1) {
                    navigation.selectOnDown = slotsUI[i + 1].button;
                } else {
                    navigation.selectOnDown = null;
                }
                slotsUI[i].button.navigation = navigation;
            }
        }

        private void CopyOverSaveSlotToModded(SaveSlotUI saveSlot, ModSaveSlotUI modSlot) {
            modSlot.submitSFX = saveSlot.submitSFX;
            modSlot.saveGameName = saveSlot.saveGameName;
            modSlot.timeShardQuantity = saveSlot.timeShardQuantity;
            modSlot.timePlayedTitle = saveSlot.timePlayedTitle;
            modSlot.timePlayed = saveSlot.timePlayed;
            modSlot.locationTitle = saveSlot.locationTitle;
            modSlot.locationName = saveSlot.locationName;
            modSlot.powerSealQuantity = saveSlot.powerSealQuantity;
            modSlot.button = saveSlot.button;
            modSlot.itemIcons = saveSlot.itemIcons;
            modSlot.powerSeal = saveSlot.powerSeal;
            modSlot.playerImage = saveSlot.playerImage;
            modSlot.playerImage_16 = saveSlot.playerImage_16;
            modSlot.playerImage_16_Legs = saveSlot.playerImage_16_Legs;
            modSlot.playerImageNoScroll = saveSlot.playerImageNoScroll;
            modSlot.shardIcon = saveSlot.shardIcon;
            modSlot.lifePoints = saveSlot.lifePoints;
            modSlot.lifePointsRow2 = saveSlot.lifePointsRow2;
            modSlot.manaPoints = saveSlot.manaPoints;
            modSlot.newGamePlusAvailable = saveSlot.newGamePlusAvailable;
            modSlot.selectedArt = saveSlot.selectedArt;
            modSlot.newGamePlusHighScore = saveSlot.newGamePlusHighScore;
            modSlot.newGamePlusCurrentCount = saveSlot.newGamePlusCurrentCount;
            modSlot.newGamePlusCurrentText = saveSlot.newGamePlusCurrentText;
            modSlot.newGamePlusHighScoreText = saveSlot.newGamePlusHighScoreText;
            modSlot.eraseGameBackground = saveSlot.eraseGameBackground;
            modSlot.newGamePlusPopupAnchor = saveSlot.newGamePlusPopupAnchor;
        }

        private void ResetSlotsToDefaultPosition(ModSaveSlotUI exception = null) {
            for (int i = 0; i < numSlots; i++) {
                Navigation navigation = slotsUI[i].button.navigation;
                navigation.mode = Navigation.Mode.Explicit;
                if (i != 0) {
                    navigation.selectOnUp = slotsUI[i - 1].button;
                } else {
                    navigation.selectOnUp = null;
                }
                if (i < numSlots - 1) {
                    navigation.selectOnDown = slotsUI[i + 1].button;
                } else {
                    navigation.selectOnDown = null;
                }
                slotsUI[i].button.navigation = navigation;

                RectTransform tf = slotsUI[i].GetComponent<RectTransform>();
                tf.SetParent(saveSlotContainers[i], false);
                tf.localPosition = Vector3.zero;
                tf.localScale = Vector3.one;
            }
        }

        protected override void OnInDone() {
            base.OnInDone();
            isInFocus = true;
            state = EScreenState.SELECT_SLOT;
            if (slotsUI.Count > 0) {
                slotsUI[0].GetComponent<UIObjectAudioHandler>().playAudio = false;
                EventSystem.current.SetSelectedGameObject(slotsUI[0].gameObject);
                slotsUI[0].GetComponent<UIObjectAudioHandler>().playAudio = true;
            }
        }

        public void OnGetInScreenDone() {
            isInFocus = true;
            state = EScreenState.SELECT_SLOT;
            // Combat some weird bug
            animator.enabled = false;
            if (slotsUI.Count > 0) {
                slotsUI[0].GetComponent<UIObjectAudioHandler>().playAudio = false;
                EventSystem.current.SetSelectedGameObject(slotsUI[0].gameObject);
                slotsUI[0].GetComponent<UIObjectAudioHandler>().playAudio = true;
            }
        }

        public void OnGetOffscreenDone() {
            for (int i = 0; i < slotsUI.Count; i++) {
                slotsUI[i].gameObject.SetActive(false);
            }
        }

        private void OnEraseSlot(ModSaveSlotUI slot) {
            state = EScreenState.ERASING_SLOT;
            EventSystem.current.SetSelectedGameObject(null);
            SetFocusedSlot(slot);
            for (int num = slotsUI.Count - 1; num >= 0; num--) {
                slotsUI[num].gameObject.SetActive(false);
            }
            slot.gameObject.SetActive(true);
            StartCoroutine(TweenSlotToFocusPositionCoroutine(slot, ConfirmSaveDelete));
        }

        private void SetFocusedSlot(ModSaveSlotUI slot, bool saveOriginalPosition = true) {
            focusedSlot = slot;
            activeFocusPosition = focusSlotPosition;
            if (saveOriginalPosition) {
                focusedSlotOriginalPos = focusedSlot.transform.position;
            }
        }

        private void SetFocusedSlot(ModSaveSlotUI slot, RectTransform focusPosition, bool saveOriginalPosition = true) {
            focusedSlot = slot;
            activeFocusPosition = focusPosition;
            if (saveOriginalPosition) {
                focusedSlotOriginalPos = focusedSlot.transform.position;
            }
        }

        private void Update() {
            // Set the scrolling
            Transform saveSlotContainer = transform.Find("Container").Find("Background").Find("SaveSlotContainer");
            GameObject selectedSlot = EventSystem.current.currentSelectedGameObject;
            ModSaveSlotUI slot;
            // Scrolling specifically prohibited for 2 slots because it throws off my calculations
            if (slotsUI.Count != 2 && selectedSlot != null && (slot = selectedSlot.GetComponent<ModSaveSlotUI>()) != null) {
                saveSlotContainer.transform.localPosition = new Vector3(0, (Mathf.Clamp(slot.slotIndex, 1, slotsUI.Count - 2) - 1) * 105);
            }

            if (Manager<InputManager>.Instance.GetBackDown() && isInFocus && !saveGameSelected && state == EScreenState.SELECT_SLOT) {
                GoOffscreen();
                Manager<UIManager>.Instance.GetView<TitleScreen>().GetInScreen(((patch_TitleScreen)Manager<UIManager>.Instance.GetView<TitleScreen>()).modsModeButton);
            } else if (Manager<InputManager>.Instance.GetBackDown() && isInFocus && state == EScreenState.SELECTING_NEW_GAME_TYPE) {
                state = EScreenState.SELECT_SLOT;
                loadingSaveFile = false;
                saveGameSelected = false;
                for (int i = 0; i < slotsUI.Count; i++) {
                    slotsUI[i].button.interactable = true;
                }
                EventSystem.current.SetSelectedGameObject(newGameSelectedSlot.button.gameObject);
                newGamePlusPopup.SetActive(false);
            } else if (Manager<InputManager>.Instance.GetBackDown() && isInFocus && state == EScreenState.SELECTING_CONTINUE_OR_RESET) {
                state = EScreenState.SELECT_SLOT;
                loadingSaveFile = false;
                saveGameSelected = false;
                for (int j = 0; j < slotsUI.Count; j++) {
                    slotsUI[j].button.interactable = true;
                }
                EventSystem.current.SetSelectedGameObject(loadGameSelectedSlot.button.gameObject);
                newCyclePopup.SetActive(false);
            } else {
                if (!Manager<InputManager>.Instance.GetBackDown() || !isInFocus || state != EScreenState.SELECTING_NEW_GAME_PLUS_BASE) {
                    return;
                }
                state = EScreenState.TRANSLATING;
                loadingSaveFile = false;
                saveGameSelected = false;
                selectingNewGamePlusBase = false;
                EventSystem.current.SetSelectedGameObject(null);
                for (int k = 0; k < slotsUI.Count; k++) {
                    if (slotsUI[k] != newGameSelectedSlot) {
                        slotsUI[k].gameObject.SetActive(false);
                        slotsUI[k].button.interactable = false;
                    }
                }
                newGamePlusSelectBaseInstructions.SetActive(false);
                Vector3 position = newGameSelectedSlot.transform.position;
                ResetSlotsToDefaultPosition();
                newGameSelectedSlot.transform.position = position;
                StartCoroutine(ReturnFocusedSlotToOriginalPosition(OnExitNewGamePlusBaseSelection));
            }
        }

        private void OnCancelItemSelection(bool isNewGame) {
            selectItemPopup.onCancel -= OnCancelItemSelection;
            selectItemPopup.onItemSelected -= OnNewGamePlusItemSelected;
            state = EScreenState.SELECT_SLOT;
            loadingSaveFile = false;
            saveGameSelected = false;
            ModSaveSlotUI saveSlotUI = (!(newGameSelectedSlot == null)) ? newGameSelectedSlot : loadGameSelectedSlot;
            if (isNewGame) {
                saveSlotUI.saveGameSlot.Clear(true);
                saveSlotUI.SetSaveData(saveSlotUI.saveGameSlot, saveSlotUI.slotIndex);
            }
            for (int i = 0; i < slotsUI.Count; i++) {
                slotsUI[i].gameObject.SetActive(true);
                slotsUI[i].button.interactable = true;
            }
            ResetSlotsToDefaultPosition();
            EventSystem.current.SetSelectedGameObject((!(newGameSelectedSlot == null)) ? newGameSelectedSlot.button.gameObject : loadGameSelectedSlot.button.gameObject);
            selectItemPopup.gameObject.SetActive(false);
        }

        private void OnExitNewGamePlusBaseSelection(ModSaveSlotUI obj) {
            EventSystem.current.SetSelectedGameObject(newGameSelectedSlot.button.gameObject);
            for (int i = 0; i < slotsUI.Count; i++) {
                slotsUI[i].gameObject.SetActive(true);
                slotsUI[i].button.interactable = true;
            }
            state = EScreenState.SELECT_SLOT;
        }

        public void GoOffscreenInstant() {
            for (int i = 0; i < slotsUI.Count; i++) {
                slotsUI[i].gameObject.SetActive(false);
            }
            isInFocus = false;
            animator.SetTrigger("GoOffscreenInstant");
            animator.Update(1f);
            animator.enabled = true;
        }

        public void GoOffscreen() {
            isInFocus = false;
            EventSystem.current.SetSelectedGameObject(null);
            animator.SetTrigger("GoOffscreen");
            animator.enabled = true;
        }

        public void GetInScreen() {
            for (int i = 0; i < slotsUI.Count; i++) {
                slotsUI[i].gameObject.SetActive(true);
            }
            animator.SetTrigger("GetInScreen");
            animator.enabled = true;
        }

        private void ConfirmSaveDelete(ModSaveSlotUI slot) {
            confirmPopup.gameObject.SetActive(true);
            string text = Manager<LocalizationManager>.Instance.GetText(deleteSaveConfirmationLocId).Replace("\\n", "\n");
            confirmPopup.Init(text);
            confirmPopup.onChoiceDone += OnDeleteChoiceDone;
            confirmPopup.no.GetComponent<UIObjectAudioHandler>().playAudio = false;
            EventSystem.current.SetSelectedGameObject(confirmPopup.no);
            confirmPopup.no.GetComponent<UIObjectAudioHandler>().playAudio = true;
        }

        private void OnDeleteChoiceDone(bool delete) {
            confirmPopup.onChoiceDone -= OnDeleteChoiceDone;
            confirmPopup.gameObject.SetActive(value: false);
            EventSystem.current.SetSelectedGameObject(null);
            if (delete) {
                focusedSlot.Delete();
                ModSaveGame.Instance.modSaveSlots.RemoveAt(focusedSlot.slotIndex);
                // Adjust which slot starts a new game in a particular level set
                foreach (CourierModuleMetadata modMeta in Courier.Mods) {
                    foreach (CourierLevelSet levelSet in modMeta.LevelSets) {
                        levelSet.SlotID--;
                    }
                }
                for (int i = 0; i < slotsUI.Count - 1; i++) {
                    slotsUI[i].SetSaveData(ModSaveGame.Instance.modSaveSlots[i], i);
                }
                slotsUI.RemoveAt(slotsUI.Count - 1);
                
            }
            StartCoroutine(ReturnFocusedSlotToOriginalPosition(SelectSlot));
        }

        private void SelectSlot(ModSaveSlotUI slot) {
            state = EScreenState.SELECT_SLOT;
            for (int num = slotsUI.Count - 1; num >= 0; num--) {
                slotsUI[num].gameObject.SetActive(true);
            }
            deleteInstructions.SetActive(true);
            EventSystem.current.SetSelectedGameObject(null);
            slot.gameObject.GetComponent<UIObjectAudioHandler>().playAudio = false;
            EventSystem.current.SetSelectedGameObject(slot.gameObject);
            slot.gameObject.GetComponent<UIObjectAudioHandler>().playAudio = true;
        }

        private IEnumerator ReturnFocusedSlotToOriginalPosition(Action<ModSaveSlotUI> callback) {
            float progress = 0f;
            Vector3 startPosition = focusedSlot.transform.position;
            Vector3 targetPosition = focusedSlotOriginalPos;
            while (progress < 1f) {
                progress += Time.deltaTime / 0.6f;
                focusedSlot.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                yield return null;
            }
            callback?.Invoke(focusedSlot);
            state = EScreenState.SELECT_SLOT;
        }

        private IEnumerator TweenSlotToFocusPositionCoroutine(ModSaveSlotUI slot, Action<ModSaveSlotUI> callback) {
            float progress = 0f;
            Vector3 startPosition = slot.transform.position;
            Vector3 targetPosition = activeFocusPosition.position;
            while (progress < 1f) {
                progress += Time.deltaTime / 0.6f;
                slot.transform.position = Vector3.Lerp(startPosition, targetPosition, progress);
                yield return null;
            }
            callback?.Invoke(slot);
        }

        private void OnSlotSelected(ModSaveSlotUI slot) {
            if (selectingNewGamePlusBase) {
                OnNewGamePlusBaseSelected(slot);
            } else if (!loadingSaveFile) {
                loadingSaveFile = true;
                saveGameSelected = true;
                EventSystem.current.SetSelectedGameObject(null);
                StartCoroutine(BlinkSaveSlotCoroutine(slot));
            }
        }

        private IEnumerator BlinkSaveSlotCoroutine(ModSaveSlotUI slot) {
            int blinkCount = 15;
            while (blinkCount != 0) {
                slot.gameObject.SetActive(!slot.gameObject.activeSelf);
                blinkCount--;
                yield return new WaitForSeconds(0.1f);
            }
            slot.gameObject.SetActive(true);
            if (slot.saveGameSlot.IsEmpty()) {
                OnNewGame(slot);
            } else {
                OnLoadGame(slot.slotIndex);
            }
        }

        public void OnNewGame(ModSaveSlotUI slot) {
            loadGameSelectedSlot = null;
            newGameSelectedSlot = slot;
            if (slot.saveGameSlot.CanBeUsedAsNewGamePlusBase()) {
                state = EScreenState.SELECTING_NEW_GAME_TYPE;
                EventSystem.current.SetSelectedGameObject(null);
                for (int i = 0; i < slotsUI.Count; i++) {
                    slotsUI[i].button.interactable = false;
                }
                newGamePlusPopup.SetActive(value: true);
                newGamePlusPopup.GetComponent<RectTransform>().SetParent(slot.newGamePlusPopupAnchor, true);
                newGamePlusPopup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                EventSystem.current.SetSelectedGameObject(normalNewGameButton);
            } else {
                OnNormalNewGame();
            }
        }

        public void OnNormalNewGame() {
            newGamePlusPopup.SetActive(false);
            state = EScreenState.SELECTING_NAME;
            SetFocusedSlot(newGameSelectedSlot);
            for (int num = slotsUI.Count - 1; num >= 0; num--) {
                slotsUI[num].gameObject.SetActive(false);
            }
            deleteInstructions.SetActive(false);
            confirmInstructions.SetActive(true);
            newGameSelectedSlot.gameObject.SetActive(true);
            StartCoroutine(TweenSlotToFocusPositionCoroutine(newGameSelectedSlot, OnNewGameInFocus));
        }

        public void OnNewGamePlus() {
            newGamePlusPopup.SetActive(false);
            if (NewGamePlusBaseCount() > 1) {
                state = EScreenState.SELECTING_NAME;
                SetFocusedSlot(newGameSelectedSlot, newGamePlusFocusPosition);
                for (int num = slotsUI.Count - 1; num >= 0; num--) {
                    slotsUI[num].gameObject.SetActive(false);
                }
                deleteInstructions.SetActive(false);
                confirmInstructions.SetActive(false);
                newGameSelectedSlot.gameObject.SetActive(true);
                StartCoroutine(TweenSlotToFocusPositionCoroutine(newGameSelectedSlot, OnNewGamePlusBaseSelectionFocusDone));
                return;
            }
            int num2 = 0;
            while (true) {
                if (num2 < 3) {
                    if (slotsUI[num2].saveGameSlot.CanBeUsedAsNewGamePlusBase()) {
                        break;
                    }
                    num2++;
                    continue;
                }
                return;
            }
            OnNewGamePlusBaseSelected(slotsUI[num2]);
        }

        private void OnNewGamePlusBaseSelectionFocusDone(ModSaveSlotUI focusedSlot) {
            bool flag = false;
            focusedSlot.button.interactable = false;
            state = EScreenState.SELECTING_NEW_GAME_PLUS_BASE;
            for (int num = slotsUI.Count - 1; num >= 0; num--) {
                if (slotsUI[num] != focusedSlot) {
                    slotsUI[num].button.interactable = true;
                    slotsUI[num].GetComponent<RectTransform>().SetParent((!flag) ? newGamePlusBaseSlot_1 : newGamePlusBaseSlot_2);
                    slotsUI[num].GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slotsUI[num].gameObject.SetActive(value: true);
                    if (flag) {
                        Navigation navigation = slotsUI[num].button.navigation;
                        navigation.selectOnDown = null;
                        for (int i = 0; i < slotsUI.Count; i++) {
                            if (slotsUI[i] != focusedSlot && slotsUI[i] != slotsUI[num]) {
                                navigation.selectOnUp = slotsUI[i].button;
                                break;
                            }
                        }
                        slotsUI[num].button.navigation = navigation;
                    } else {
                        Navigation navigation2 = slotsUI[num].button.navigation;
                        for (int j = 0; j < slotsUI.Count; j++) {
                            if (slotsUI[j] != focusedSlot && slotsUI[j] != slotsUI[num]) {
                                navigation2.selectOnDown = slotsUI[j].button;
                                break;
                            }
                        }
                        navigation2.selectOnUp = null;
                        slotsUI[num].button.navigation = navigation2;
                    }
                    if (!flag) {
                        flag = true;
                        EventSystem.current.SetSelectedGameObject(slotsUI[num].button.gameObject);
                    }
                }
            }
            newGamePlusSelectBaseInstructions.SetActive(value: true);
            selectingNewGamePlusBase = true;
        }

        private void OnNewGamePlusBaseSelected(ModSaveSlotUI newGamePlusBase) {
            selectingNewGamePlusBase = false;
            Vector3 position = newGameSelectedSlot.transform.position;
            ResetSlotsToDefaultPosition();
            newGameSelectedSlot.transform.position = position;
            newGamePlusSelectBaseInstructions.SetActive(value: false);
            newGameSelectedSlot.saveGameSlot.CopyFromNewGamePlusBase(newGamePlusBase.saveGameSlot, keepLockedItems: false, resetNewGamePlushighScore: true);
            newGameSelectedSlot.saveGameSlot.SlotName = string.Empty;
            newGameSelectedSlot.saveGameSlot.DealDone = false;
            if (newGameSelectedSlot.saveGameSlot.EquippedSkin == ESkin.DARK_MESSENGER) {
                newGameSelectedSlot.saveGameSlot.EquippedSkin = ESkin.DEFAULT;
            }
            newGameSelectedSlot.saveGameSlot.SetAsNewGamePlus(resetHighScore: true);
            newGameSelectedSlot.SetSaveData(newGameSelectedSlot.saveGameSlot, newGameSelectedSlot.slotIndex);
            GoToItemSelection(isNewGame: true);
        }

        private void GoToItemSelection(bool isNewGame) {
            if (ModSaveGame.Instance.modSaveSlots[(!isNewGame) ? loadGameSelectedSlot.slotIndex : newGameSelectedSlot.slotIndex].GetNewGameplusItemListAvailable().Count <= 0) {
                SetupGameForNewCycle();
                loadGameSelectedSlot.saveGameSlot.MustSelectItem = false;
                // Indicate mods by selecting slot 3, which doesn't exist
                Manager<SaveManager>.Instance.SelectSaveGameSlot(3);
                ModSaveGame.Instance.currentModSaveSlotIndex = loadGameSelectedSlot.slotIndex;
                LoadNewGame(loadGameSelectedSlot.slotIndex);
                return;
            }
            EventSystem.current.SetSelectedGameObject(null);
            for (int num = slotsUI.Count - 1; num >= 0; num--) {
                slotsUI[num].gameObject.SetActive(value: false);
            }
            deleteInstructions.SetActive(value: false);
            selectItemPopup.gameObject.SetActive(value: true);
            state = EScreenState.SELECTING_ITEM;
            selectItemPopup.onCancel += OnCancelItemSelection;
            selectItemPopup.onItemSelected += OnNewGamePlusItemSelected;
            selectItemPopup.Init((!isNewGame) ? loadGameSelectedSlot.slotIndex : newGameSelectedSlot.slotIndex, isNewGame);
        }

        public void OnNewGamePlusItemSelected(bool isNewGame) {
            selectItemPopup.onCancel -= OnCancelItemSelection;
            selectItemPopup.onItemSelected -= OnNewGamePlusItemSelected;
            selectItemPopup.gameObject.SetActive(value: false);
            if (isNewGame) {
                confirmInstructions.SetActive(value: true);
                newGameSelectedSlot.gameObject.SetActive(value: true);
                state = EScreenState.SELECTING_NAME;
                SetFocusedSlot(newGameSelectedSlot, (NewGamePlusBaseCount(newGameSelectedSlot.slotIndex) <= 1) ? true : false);
                newGameSelectedSlot.SetSaveData(newGameSelectedSlot.saveGameSlot, newGameSelectedSlot.slotIndex);
                newGameSelectedSlot.gameObject.SetActive(value: true);
                newGameSelectedSlot.transform.position = focusSlotPosition.position;
                OnNewGameInFocus(newGameSelectedSlot);
            } else {
                bool mustSelectItem = loadGameSelectedSlot.saveGameSlot.MustSelectItem;
                loadGameSelectedSlot.saveGameSlot.MustSelectItem = false;
                // Indicate mods by selecting slot 3, which doesn't exist
                Manager<SaveManager>.Instance.SelectSaveGameSlot(3);
                ModSaveGame.Instance.currentModSaveSlotIndex = loadGameSelectedSlot.slotIndex;
                if (!mustSelectItem) {
                    SetupGameForNewCycle();
                } else {
                    ModSaveGame.Instance.modSaveSlots[ModSaveGame.Instance.currentModSaveSlotIndex].ResetNewGamePlus(true);
                    ModSaveGame.Instance.modSaveSlots[ModSaveGame.Instance.currentModSaveSlotIndex].MustSelectItem = false;
                }
                LoadNewGame(loadGameSelectedSlot.slotIndex);
            }
        }

        private void SetupGameForNewCycle() {
            SaveGameSlot baseSlot = new SaveGameSlot(loadGameSelectedSlot.saveGameSlot);
            int newGamePlusCount = loadGameSelectedSlot.saveGameSlot.NewGamePlusCount;
            int newGamePlusHighScore = loadGameSelectedSlot.saveGameSlot.NewGamePlusHighScore;
            loadGameSelectedSlot.saveGameSlot.CopyFromNewGamePlusBase(baseSlot, keepLockedItems: true, resetNewGamePlushighScore: false, copyTimeshards: true);
            loadGameSelectedSlot.saveGameSlot.NewGamePlus = true;
            loadGameSelectedSlot.saveGameSlot.NewGamePlusCount = newGamePlusCount + 1;
            loadGameSelectedSlot.saveGameSlot.NewGamePlusCount = ((loadGameSelectedSlot.saveGameSlot.NewGamePlusCount <= 999) ? loadGameSelectedSlot.saveGameSlot.NewGamePlusCount : 999);
            loadGameSelectedSlot.saveGameSlot.NewGamePlusHighScore = newGamePlusHighScore;
            if (loadGameSelectedSlot.saveGameSlot.NewGamePlusCount > loadGameSelectedSlot.saveGameSlot.NewGamePlusHighScore) {
                loadGameSelectedSlot.saveGameSlot.NewGamePlusHighScore = loadGameSelectedSlot.saveGameSlot.NewGamePlusCount;
            }
        }

        private int NewGamePlusBaseCount(int slotIndexToIgnore = -1) {
            int num = 0;
            for (int i = 0; i < 3; i++) {
                if (i != slotIndexToIgnore && ModSaveGame.Instance.modSaveSlots[i].CanBeUsedAsNewGamePlusBase()) {
                    num++;
                }
            }
            return num;
        }

        private void OnNewGameInFocus(ModSaveSlotUI slot) {
            slot.button.enabled = false;
            nameSavePopup.Init(new SaveSlotUI { slotIndex = slot.slotIndex, saveGameName = slot.saveGameName });
            nameSavePopup.onNameConfirmed += OnNameConfirmed;
            nameSavePopup.onBack += OnCloseNameSave;
            nameSavePopup.gameObject.SetActive(value: true);
            Canvas.ForceUpdateCanvases();
            nameSavePopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = false;
            EventSystem.current.SetSelectedGameObject(nameSavePopup.initialSelection);
            nameSavePopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = true;
        }

        // This method gets passed a dummy save slot so I don't have to rewrite NameSavePopup
        private void OnCloseNameSave(SaveSlotUI saveSlot) {
            nameSavePopup.gameObject.SetActive(false);
            ModSaveSlotUI realSlot = slotsUI[saveSlot.slotIndex];
            realSlot.button.enabled = true;
            loadingSaveFile = false;
            saveGameSelected = false;
            nameSavePopup.onNameConfirmed -= OnNameConfirmed;
            nameSavePopup.onBack -= OnCloseNameSave;
            if (realSlot.saveGameSlot.NewGamePlus) {
                realSlot.saveGameSlot.Clear(true);
                realSlot.SetSaveData(realSlot.saveGameSlot, realSlot.slotIndex);
            }
            confirmInstructions.SetActive(false);
            confirmPopup.gameObject.SetActive(false);
            EventSystem.current.SetSelectedGameObject(null);
            StartCoroutine(ReturnFocusedSlotToOriginalPosition(SelectSlot));
        }

        // This method gets passed a dummy save slot so I don't have to rewrite NameSavePopup
        private void OnNameConfirmed(SaveSlotUI saveSlot, string slotName) {
            ModSaveSlotUI realSlot = slotsUI[saveSlot.slotIndex];
            nameSavePopup.onNameConfirmed -= OnNameConfirmed;
            nameSavePopup.onBack -= OnCloseNameSave;
            // TODO Enable on supported versions only
            //TimeTrackerScreen.SpeedRunEnabled = (Manager<InputManager>.Instance.GetRightTrigger() && (realSlot.saveGameSlot.IsEmpty() || realSlot.saveGameSlot.NewGamePlus));
            // Indicate mods by selecting slot 3, which doesn't exist
            Manager<SaveManager>.Instance.SelectSaveGameSlot(3);
            ModSaveGame.Instance.currentModSaveSlotIndex = realSlot.slotIndex;
            if (!realSlot.saveGameSlot.NewGamePlus) {
                if (realSlot.saveGameSlot == null) {
                    realSlot.saveGameSlot = new SaveGameSlot();
                }
                realSlot.saveGameSlot.Create(true);
            }
            ModSaveGame.Instance.modSaveSlots[ModSaveGame.Instance.currentModSaveSlotIndex].SlotName = slotName;
            LoadNewGame(realSlot.slotIndex);
        }

        public void LoadNewGame(int slotIndex) {
            Manager<LevelManager>.Instance.onTransitionInDone += OnTransitionInDone;

            Manager<SaveManager>.Instance.LoadModSaveSlot(ModSaveGame.Instance.modSaveSlots[slotIndex]);

            ArmoireInteractionTrigger.currentDialogIndex = 0;

            CourierLevelSet levelSet = Courier.FindLevelSetWithSlotID(slotIndex);
            LaunchNewGame(levelSet, levelSet.StartingScene + "_Build");
        }

        public void OnLoadGame(int slotIndex) {
            newGameSelectedSlot = null;
            loadGameSelectedSlot = slotsUI[slotIndex];
            if (slotsUI[slotIndex].saveGameSlot.NewGamePlus && slotsUI[slotIndex].saveGameSlot.CanRunNewCycle()) {
                state = EScreenState.SELECTING_CONTINUE_OR_RESET;
                EventSystem.current.SetSelectedGameObject(null);
                for (int i = 0; i < slotsUI.Count; i++) {
                    slotsUI[i].button.interactable = false;
                }
                newCyclePopup.SetActive(value: true);
                newCyclePopup.GetComponent<RectTransform>().SetParent(slotsUI[slotIndex].newGamePlusPopupAnchor, worldPositionStays: true);
                newCyclePopup.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                EventSystem.current.SetSelectedGameObject(continueButton);
            } else if (ModSaveGame.Instance.modSaveSlots[slotIndex].MustSelectItem) {
                GoToItemSelection(false);
            } else {
                LoadGame(slotIndex);
            }
        }

        public void OnContinue() {
            newCyclePopup.SetActive(false);
            LoadGame(loadGameSelectedSlot.slotIndex);
        }

        public void OnNewCycle() {
            newCyclePopup.SetActive(false);
            GoToItemSelection(false);
        }

        private void LoadGame(int slotIndex) {
            // Indicate mods by selecting slot 3, which doesn't exist
            Manager<SaveManager>.Instance.SelectSaveGameSlot(3);
            ModSaveGame.Instance.currentModSaveSlotIndex = slotIndex;
            Manager<SaveManager>.Instance.LoadModSaveSlot(ModSaveGame.Instance.modSaveSlots[slotIndex]);
            Manager<LevelManager>.Instance.onTransitionInDone += OnTransitionInDone;
            string text = Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelName;
            if (text != ELevel.Level_15_Surf.ToString()) {
                text = Manager<LevelManager>.Instance.GetBuildSceneName(text);
            }
            LaunchGame(text, Manager<ProgressionManager>.Instance.checkpointSaveInfo);
        }

        private void LaunchNewGame(CourierLevelSet levelSet, string levelToLoad) {
            Manager<SaveManager>.Instance.ForceEnableSave();
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(levelToLoad, true, true, levelSet.StartingBits) {
                showLevelIntro = false,
                levelEntranceId = ELevelEntranceID.ENTRANCE_A
            }; // TODO Choose starting bits and inventory
            foreach(int item in levelSet.StartingInventory) {
                Manager<InventoryManager>.Instance.AddItem((EItems)item, 1);
            }
            Manager<ProgressionManager>.Instance.lastSaveTime = Time.time;
            Manager<InputManager>.Instance.CancelJumpTimeDown();
            levelLoadingInfo.bootInTotHQ = false;
            Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
        }

        private void LaunchGame(string levelToLoad, CheckpointSaveInfo checkpointSaveInfo) {
            Manager<SaveManager>.Instance.ForceEnableSave();
            LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(levelToLoad, true, true, checkpointSaveInfo.loadedLevelDimension);
            levelLoadingInfo.showLevelIntro = false;
            Manager<ProgressionManager>.Instance.lastSaveTime = Time.time;
            Manager<InputManager>.Instance.CancelJumpTimeDown();
            levelLoadingInfo.bootInTotHQ = (Manager<LevelManager>.Instance.GetEditorSceneName(checkpointSaveInfo.playerLocationSceneName) == ELevel.Level_13_TowerOfTimeHQ.ToString());
            levelLoadingInfo.levelInitializerParams = new LevelInitializerParams {
                playMusic = !levelLoadingInfo.bootInTotHQ,
                saveOnInitialize = false
            };
            Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
        }

        private void OnTransitionInDone() {
            Manager<LevelManager>.Instance.onTransitionInDone -= OnTransitionInDone;
            Manager<LevelManager>.Instance.AddPreLevelLoadingTask(Manager<LevelManager>.Instance.StartCoroutine(UnloadScreenBeforeLevelLoading()));
            Manager<LevelManager>.Instance.onLevelLoaded += OnLevelLoaded;
        }

        private IEnumerator UnloadScreenBeforeLevelLoading() {
            Manager<UIManager>.Instance.CloseAllScreensOfType<TitleScreen>(false);
            Manager<UIManager>.Instance.CloseScreen(this, false);
            Manager<UIManager>.Instance.CloseAllScreensOfType<OptionScreen>(false);
            yield return null;
            yield return null;
            yield return null;
        }

        private void OnLevelLoaded(string level) {
            Manager<LevelManager>.Instance.onLevelLoaded -= OnLevelLoaded;
            FixShaders();
        }

        private void FixShaders() {
            Manager<DimensionManager>.Instance.mask8.shader = Shader.Find("DimensionZone/Masks/8_Bit_SinWave");
            Manager<DimensionManager>.Instance.mask16.shader = Shader.Find("DimensionZone/Masks/16_Bit_SinWave");
            Manager<DimensionManager>.Instance.spriteMask8.shader = Shader.Find("DimensionZone/Masks/8_Bit_Sprite");
            Manager<DimensionManager>.Instance.spriteMask16.shader = Shader.Find("DimensionZone/Masks/16_Bit_Sprite");
            Manager<DimensionManager>.Instance.outlineMask8.shader = Shader.Find("DimensionZone/Masks/8_Bit_LucioleOutlineMask");
            Manager<DimensionManager>.Instance.outlineMask16.shader = Shader.Find("DimensionZone/Masks/16_Bit_LucioleOutlineMask");
            Manager<DimensionManager>.Instance.mask8Expanding.shader = Shader.Find("DimensionZone/Masks/8_Bit_Expanding");
            Manager<DimensionManager>.Instance.mask16Expanding.shader = Shader.Find("DimensionZone/Masks/16_Bit_Expanding");
        }
    }

    internal static class ModSaveSelectionScreenRelatedExtensions {
        internal static void LoadModSaveSlot(this SaveManager manager, SaveGameSlot slot) {
            slot.Load();
            Manager<SaveManager>.Instance.hasLoadedAtLeastOneSaveSlot = true;
            Manager<SaveManager>.Instance.Raise("onSaveSlotLoaded", EventArgs.Empty);
        }
    }
}