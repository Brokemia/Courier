using System;
using System.Collections;
using System.Collections.Generic;
using Mod.Courier.Save;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mod.Courier.UI {
    public class ModSaveSlotUI : MonoBehaviour, ISelectHandler, IDeselectHandler, ISubmitHandler, IEventSystemHandler {
        public const string modLevelLocIDPrefix = "COURIER_MOD_LEVEL_";

        public AudioObjectDefinition submitSFX;

        public TextMeshProUGUI saveGameName;

        public TextMeshProUGUI timeShardQuantity;

        public TextMeshProUGUI timePlayedTitle;

        public TextMeshProUGUI timePlayed;

        public TextMeshProUGUI locationTitle;

        public TextMeshProUGUI locationName;

        public TextMeshProUGUI powerSealQuantity;

        public Button button;

        public List<SaveFileItemIcon> itemIcons;

        public Image powerSeal;

        public Image playerImage;

        public Image playerImage_16;

        public Image playerImage_16_Legs;

        public Image playerImageNoScroll;

        public GameObject shardIcon;

        public List<GameObject> lifePoints;

        public List<GameObject> lifePointsRow2;

        public List<GameObject> manaPoints;

        public GameObject newGamePlusAvailable;

        public GameObject selectedArt;

        public GameObject newGamePlusHighScore;

        public GameObject newGamePlusCurrentCount;

        public TextMeshProUGUI newGamePlusCurrentText;

        public TextMeshProUGUI newGamePlusHighScoreText;

        public Image eraseGameBackground;

        [HideInInspector]
        public RectTransform newGamePlusPopupAnchor;

        [HideInInspector]
        public SaveGameSlot saveGameSlot;

        [HideInInspector]
        public int slotIndex;

        private bool selected;

        private Coroutine eraseSlotCoroutine;

        public event Action<ModSaveSlotUI> onSubmit;

        public event Action<ModSaveSlotUI> onEraseSlot;

        public void SetSaveData(SaveGameSlot slot, int slotIndex) {
            saveGameSlot = slot;
            this.slotIndex = slotIndex;
            SkinPlayer();
            newGamePlusHighScore.SetActive(slot.NewGamePlus);
            newGamePlusCurrentCount.SetActive(slot.NewGamePlus);
            for (int i = 0; i < itemIcons.Count; i++) {
                itemIcons[i].SetState(slot);
            }
            if (slot.IsEmpty()) {
                SetDefaultName();
                locationName.text = Manager<LocalizationManager>.Instance.GetText(Courier.FindLevelSetWithSlotID(slotIndex).NameLocID);
                timePlayed.text = "000:00:00";
                timeShardQuantity.text = string.Empty;
                powerSealQuantity.text = string.Empty;
                playerImage_16.gameObject.SetActive(false);
                playerImageNoScroll.gameObject.SetActive(false);
                playerImage.gameObject.SetActive(false);
                shardIcon.SetActive(false);
                powerSeal.gameObject.SetActive(false);
                SetHP(0);
                SetMana(0);
                newGamePlusAvailable.SetActive(ModSaveGame.Instance.modSaveSlots[slotIndex].CanBeUsedAsNewGamePlusBase());
                return;
            }
            newGamePlusAvailable.SetActive(false);
            if (slot.NewGamePlus) {
                newGamePlusCurrentText.SetText("+" + slot.NewGamePlusCount);
                newGamePlusHighScoreText.SetText("+" + slot.NewGamePlusHighScore);
            }
            saveGameName.text = slot.SlotName;
            TimeSpan timeSpan = TimeSpan.FromSeconds(slot.SecondsPlayed);
            string text = $"{Mathf.FloorToInt((float)timeSpan.TotalHours):D3}:{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
            timePlayed.text = text;
            if (string.IsNullOrEmpty(slot.CheckpointSaveInfo.playerLocationSceneName)) {
                Debug.LogError("Null level name in slot: " + slotIndex);
                slot.CheckpointSaveInfo.playerLocationSceneName = "???";
            }
            string editorSceneName = Manager<LevelManager>.Instance.GetEditorSceneName(slot.CheckpointSaveInfo.playerLocationSceneName);
            locationName.text = Manager<LocalizationManager>.Instance.GetText(modLevelLocIDPrefix + editorSceneName);
            timeShardQuantity.text = GetItemQuantity(EItems.TIME_SHARD, slot.Items).ToString();
            if (Manager<LevelManager>.Instance.GetEditorSceneName(slot.CheckpointSaveInfo.playerLocationSceneName) == ELevel.Level_13_TowerOfTimeHQ.ToString() || slot.CheckpointSaveInfo.loadedLevelDimension == EBits.BITS_16) {
                playerImage_16.gameObject.SetActive(true);
                playerImageNoScroll.gameObject.SetActive(false);
                playerImage.gameObject.SetActive(false);
            } else if (GetItemQuantity(EItems.BASIC_SCROLL, slot.Items) >= 1 || GetItemQuantity(EItems.SCROLL_UPGRADE, slot.Items) >= 1) {
                playerImage_16.gameObject.SetActive(false);
                playerImageNoScroll.gameObject.SetActive(false);
                playerImage.gameObject.SetActive(true);
            } else {
                playerImageNoScroll.gameObject.SetActive(true);
                playerImage_16.gameObject.SetActive(false);
                playerImage.gameObject.SetActive(false);
            }
            shardIcon.SetActive(value: true);
            int num = PowerSealCollected(slot);
            powerSeal.gameObject.SetActive(num >= 1);
            powerSealQuantity.gameObject.SetActive(num >= 1);
            powerSealQuantity.text = "x" + num.ToString();
            int num2 = GetItemQuantity(EItems.HEART_CONTAINER, slot.Items) + GetItemQuantity(EItems.POTION_FULL_HEAL_AND_HP_UPGRADE, slot.Items) + Manager<PlayerManager>.Instance.baseMaxHP;
            num2 *= ((!saveGameSlot.DealDone) ? 1 : 2);
            SetHP(num2);
            SetMana(GetItemQuantity(EItems.MANA_CONTAINER, slot.Items) + Manager<PlayerManager>.Instance.baseMaxShuriken);
        }

        private void SkinPlayer() {
            ESkin equippedSkin = GetEquippedSkin();
            PlayerSkinDefinition skin = Manager<SkinManager>.Instance.GetSkin(equippedSkin);
            playerImageNoScroll.sprite = skin.baseSprites.Object.saveGameSprite_8_NoScroll;
            playerImage.sprite = skin.baseSprites.Object.saveGameSprite_8;
            playerImage_16.sprite = skin.baseSprites.Object.saveGameSprite_16;
            playerImage_16_Legs.sprite = skin.baseSprites.Object.saveGameSprite_16_Legs;
        }

        private ESkin GetEquippedSkin() {
            if (saveGameSlot.DealDone) {
                return ESkin.DARK_MESSENGER;
            }
            return saveGameSlot.EquippedSkin;
        }

        public void SetupLoc() {
            if (saveGameSlot == null) return;
            if (saveGameSlot.IsEmpty()) {
                SetDefaultName();
                return;
            }
            string editorSceneName = Manager<LevelManager>.Instance.GetEditorSceneName(saveGameSlot.CheckpointSaveInfo.playerLocationSceneName);
            locationName.text = Manager<LocalizationManager>.Instance.GetText("COURIER_MOD_LEVEL_" + editorSceneName);
        }

        public int PowerSealCollected(SaveGameSlot slot) {
            int num = 0;
            foreach (KeyValuePair<ELevel, int> item in slot.ChallengeRoomCompletedByLevel) {
                num += item.Value;
            }
            return num;
        }

        public void SetName(string saveName) {
            saveGameName.text = saveName;
        }

        public void SetDefaultName() {
            saveGameName.text = Manager<LocalizationManager>.Instance.GetText("NEW_GAME");
        }

        private void Update() {
            if (saveGameSlot != null && selected && !saveGameSlot.IsEmpty() && Manager<InputManager>.Instance.GetEraseSaveFileDown()) {
                if (eraseSlotCoroutine != null) {
                    StopCoroutine(eraseSlotCoroutine);
                }
                eraseSlotCoroutine = StartCoroutine(EraseSlotCoroutine());
            }
        }

        private IEnumerator EraseSlotCoroutine() {
            float progress = 0f;
            while (progress < 1f && Manager<InputManager>.Instance.GetEraseSaveFile()) {
                progress += Time.smoothDeltaTime / 1.5f;
                eraseGameBackground.fillAmount = progress;
                yield return null;
            }
            if (!Manager<InputManager>.Instance.GetEraseSaveFile()) {
                eraseGameBackground.fillAmount = 0f;
                yield break;
            }
            eraseGameBackground.fillAmount = 1f;
            onEraseSlot?.Invoke(this);
        }

        public void OnEnable() {
            SetupLoc();
            Manager<LocalizationManager>.Instance.onLanguageChanged += SetupLoc;
        }

        public void Delete() {
            ModSaveGame.Instance.modSaveSlots[slotIndex].Clear(true);
            Manager<SaveManager>.Instance.SaveGame();
            SetSaveData(saveGameSlot, slotIndex);
        }

        private int GetItemQuantity(EItems item, ItemQuantityByItemID items) {
            if (items.TryGetValue(item, out int value)) {
                return value;
            }
            return 0;
        }

        private void SetMana(int mana) {
            for (int i = 0; i < manaPoints.Count; i++) {
                manaPoints[i].SetActive(i < mana);
            }
        }

        private void SetHP(int hp) {
            for (int i = 0; i < lifePoints.Count; i++) {
                lifePoints[i].SetActive(i < hp);
            }
            for (int j = 0; j < lifePointsRow2.Count; j++) {
                lifePointsRow2[j].SetActive(j + lifePoints.Count < hp);
            }
        }

        public void OnSelect(BaseEventData eventData) {
            selectedArt.SetActive(value: true);
            selected = true;
        }

        public void OnDisable() {
            Manager<LocalizationManager>.Instance.onLanguageChanged -= SetupLoc;
        }

        public void OnDeselect(BaseEventData eventData) {
            if (eraseSlotCoroutine != null) {
                StopCoroutine(eraseSlotCoroutine);
                eraseGameBackground.fillAmount = 0f;
            }
            selectedArt.SetActive(false);
            selected = false;
        }

        public void OnSubmit(BaseEventData eventData) {
            // The gheesling cheat code
            // I'll leave it in, just for fun
            if (saveGameSlot.SlotName == "Cerso" && Manager<InputManager>.Instance.GetRightTrigger() && Manager<InputManager>.Instance.GetShuriken() && (saveGameSlot.EquippedSkin == ESkin.DEFAULT || saveGameSlot.EquippedSkin == ESkin.GHEESLING_RED || saveGameSlot.EquippedSkin == ESkin.GHEESLING_BLUE) && !Manager<ProgressionManager>.Instance.dealDone) {
                if (saveGameSlot.EquippedSkin == ESkin.GHEESLING_RED || saveGameSlot.EquippedSkin == ESkin.GHEESLING_BLUE) {
                    saveGameSlot.EquippedSkin = ESkin.DEFAULT;
                    Manager<AudioManager>.Instance.PlaySoundEffect(Manager<AudioManager>.Instance.pauseSFX);
                } else {
                    saveGameSlot.EquippedSkin = ESkin.GHEESLING_RED;
                    Manager<AudioManager>.Instance.PlaySoundEffect(Manager<AudioManager>.Instance.pauseSFX);
                }
                SkinPlayer();
            } else {
                Manager<AudioManager>.Instance.PlaySoundEffect(submitSFX);
                onSubmit?.Invoke(this);
            }
        }
    }
}