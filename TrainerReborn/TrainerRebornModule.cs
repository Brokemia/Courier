using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Mod.Courier;
using Mod.Courier.Module;
using Mod.Courier.UI;
using MonoMod.RuntimeDetour;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using static Mod.Courier.UI.TextEntryButtonInfo;

namespace TrainerReborn {
    public class TrainerRebornModule : CourierModule {
        public const string INF_HEALTH_BUTTON_LOC_ID = "TRAINER_REBORN_INF_HEALTH_BUTTON";
        public const string INF_SHURIKEN_BUTTON_LOC_ID = "TRAINER_REBORN_INF_SHURIKEN_BUTTON";
        public const string INF_JUMP_BUTTON_LOC_ID = "TRAINER_REBORN_INF_JUMP_BUTTON";
        public const string NO_BOUNDS_BUTTON_LOC_ID = "TRAINER_REBORN_NO_BOUNDS_BUTTON";
        public const string DEBUG_POS_BUTTON_LOC_ID = "TRAINER_REBORN_DEBUG_POS_BUTTON";
        public const string DEBUG_BOSS_BUTTON_LOC_ID = "TRAINER_REBORN_DEBUG_BOSS_BUTTON";
        public const string TOGGLE_COLLISIONS_BUTTON_LOC_ID = "TRAINER_REBORN_TOGGLE_COLLISIONS_BUTTON";
        public const string SECOND_QUEST_BUTTON_LOC_ID = "TRAINER_REBORN_SECOND_QUEST_BUTTON";
        public const string RELOAD_BUTTON_LOC_ID = "TRAINER_REBORN_RELOAD_BUTTON";
        public const string SAVE_BUTTON_LOC_ID = "TRAINER_REBORN_SAVE_BUTTON";
        public const string SPEED_MULT_BUTTON_LOC_ID = "TRAINER_REBORN_SPEED_MULT_BUTTON";
        public const string DEBUG_TEXT_COLOR_BUTTON_LOC_ID = "TRAINER_REBORN_DEBUG_TEXT_COLOR_BUTTON";
        public const string TP_BUTTON_LOC_ID = "TRAINER_REBORN_TP_BUTTON";
        public const string GET_ITEM_BUTTON_LOC_ID = "TRAINER_REBORN_GET_ITEM_BUTTON";
        public const string TP_LEVEL_ENTRY_LOC_ID = "TRAINER_REBORN_TP_LEVEL_ENTRY";
        public const string TP_LOCATION_ENTRY_LOC_ID = "TRAINER_REBORN_TP_LOCATION_ENTRY";
        public const string ITEM_NAME_ENTRY_LOC_ID = "TRAINER_REBORN_ITEM_NAME_ENTRY";
        public const string ITEM_NUMBER_ENTRY_LOC_ID = "TRAINER_REBORN_ITEM_NUMBER_ENTRY";

        public const string POS_DEBUG_TEXT_LOC_ID = "TRAINER_REBORN_POS_DEBUG_TEXT";
        public const string CAMERA_UNLOCKED_DEBUG_TEXT_LOC_ID = "TRAINER_REBORN_CAMERA_UNLOCKED_DEBUG_TEXT";
        public const string NO_COLLISIONS_DEBUG_TEXT_LOC_ID = "TRAINER_REBORN_NO_COLLISIONS_DEBUG_TEXT";
        public const string INF_SHURIKEN_DEBUG_TEXT_LOC_ID = "TRAINER_REBORN_INF_SHURIKEN_DEBUG_TEXT";
        public const string INF_HEALTH_DEBUG_TEXT_LOC_ID = "TRAINER_REBORN_INF_HEALTH_DEBUG_TEXT";
        public const string INF_JUMP_DEBUG_TEXT_LOC_ID = "TRAINER_REBORN_INF_JUMP_DEBUG_TEXT";
        public const string SPEED_DEBUG_TEXT_LOC_ID = "TRAINER_REBORN_SPEED_DEBUG_TEXT";

        public bool noBounds;

        public bool debugPos;

        public bool debugBoss;

        public bool infJump;

        public bool infHealth;

        public bool infShuriken;

        public bool collisionsDisabled;

        public float speedMult = 1;

        public Color debugTextColor = Color.white;
        
        private TextMeshProUGUI debugText8;

        private TextMeshProUGUI debugText16;
        
        private static MethodInfo get_PlayerShurikensInfo = typeof(PlayerManager).GetProperty("PlayerShurikens", BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty).GetGetMethod();
        private static MethodInfo get_PlayerShurikensHookInfo = typeof(TrainerRebornModule).GetMethod(nameof(PlayerManager_get_PlayerShurikens), BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.InvokeMethod);

        ToggleButtonInfo infHealthButton;
        ToggleButtonInfo infShurikenButton;
        ToggleButtonInfo infJumpButton;
        ToggleButtonInfo noBoundsButton;
        ToggleButtonInfo debugPosButton;
        ToggleButtonInfo debugBossButton;
        ToggleButtonInfo toggleCollisionsButton;
        ToggleButtonInfo secondQuestButton;
        SubMenuButtonInfo reloadButton;
        SubMenuButtonInfo saveButton;
        TextEntryButtonInfo speedMultButton;
        TextEntryButtonInfo debugTextColorButton;
        TextEntryButtonInfo tpButton;
        TextEntryButtonInfo getItemButton;

        public override void Load() {
            On.InGameHud.OnGUI += InGameHud_OnGUI;
            On.PlayerController.CanJump += PlayerController_CanJump;
            On.PlayerController.Awake += PlayerController_Awake;
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
            new Hook(get_PlayerShurikensInfo, get_PlayerShurikensHookInfo, this);
            // Stuff that doesn't always call orig(self)
            using (new DetourContext("TrainerReborn") {
                Before = { "*" }
            }) {
                On.PlayerController.Damage += PlayerController_Damage;
                On.RetroCamera.SnapPositionToCameraBounds += RetroCamera_SnapPositionToCameraBounds;
            }

            infHealthButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(INF_HEALTH_BUTTON_LOC_ID), OnInfHealth, (b) => infHealth);
            infShurikenButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(INF_SHURIKEN_BUTTON_LOC_ID), OnInfShuriken, (b) => infShuriken);
            infJumpButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(INF_JUMP_BUTTON_LOC_ID), OnInfJump, (b) => infJump);
            noBoundsButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(NO_BOUNDS_BUTTON_LOC_ID), OnNoBounds, (b) => noBounds);
            debugPosButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(DEBUG_POS_BUTTON_LOC_ID), OnDebugPos, (b) => debugPos);
            debugBossButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(DEBUG_BOSS_BUTTON_LOC_ID), OnDebugBoss, (b) => debugBoss);
            toggleCollisionsButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(TOGGLE_COLLISIONS_BUTTON_LOC_ID), OnToggleCollisions, (b) => !collisionsDisabled);
            secondQuestButton = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(SECOND_QUEST_BUTTON_LOC_ID), OnSecondQuest, (b) => Manager<ProgressionManager>.Instance.secondQuest);
            speedMultButton = Courier.UI.RegisterTextEntryModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(SPEED_MULT_BUTTON_LOC_ID), OnEnterSpeed, 4, null, () => Manager<PlayerManager>.Instance?.Player?.RunSpeedMultiplier.ToString() ?? "" + speedMult, CharsetFlags.Number | CharsetFlags.Dot);
            debugTextColorButton = Courier.UI.RegisterTextEntryModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(DEBUG_TEXT_COLOR_BUTTON_LOC_ID), OnEnterDebugTextColor, 7, null, () => "", CharsetFlags.Letter);
            tpButton = Courier.UI.RegisterTextEntryModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(TP_BUTTON_LOC_ID), OnEnterTeleportLevel, 17, () => Manager<LocalizationManager>.Instance.GetText(TP_LEVEL_ENTRY_LOC_ID), () => GetLevelNameFromEnum(Manager<LevelManager>.Instance?.GetCurrentLevelEnum() ?? ELevel.NONE), CharsetFlags.Letter);
            getItemButton = Courier.UI.RegisterTextEntryModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(GET_ITEM_BUTTON_LOC_ID), OnEnterItemToGive, 16, () => Manager<LocalizationManager>.Instance.GetText(ITEM_NAME_ENTRY_LOC_ID), () => "", CharsetFlags.Letter);
            reloadButton = Courier.UI.RegisterSubMenuModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(RELOAD_BUTTON_LOC_ID), OnReloadButton);
            saveButton = Courier.UI.RegisterSubMenuModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(SAVE_BUTTON_LOC_ID), OnSaveButton);

            // Disable certain features until we enter the level
            secondQuestButton.IsEnabled += () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;
            tpButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;
            getItemButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;
            reloadButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;
            saveButton.IsEnabled = () => Manager<LevelManager>.Instance.GetCurrentLevelEnum() != ELevel.NONE;

            if (Dicts.tpDict == null) {
                Dicts.InitTpDict();
            }
            if (Dicts.itemDict == null) {
                Dicts.InitItemDict();
            }
            if (Dicts.levelDict == null) {
                Dicts.InitLevelDict();
            }
        }

        static string GetLevelNameFromEnum(ELevel levelEnum) {
            if (Dicts.levelDict == null) {
                Dicts.InitLevelDict();
            }
            foreach (KeyValuePair<string, string> kvp in Dicts.levelDict) {
                if (levelEnum.ToString().Equals(kvp.Value))
                    return kvp.Key;
            }
            return "";
        }

        void PlayerController_Awake(On.PlayerController.orig_Awake orig, PlayerController self) {
            orig(self);
            self.SetRunSpeedMultiplier(speedMult);
            List<LayerMask> collisionMaskList = Manager<PlayerManager>.Instance.Player.Controller.collisionMaskList;
            // Remove collisions if they were disabled earlier
            if (collisionsDisabled) {
                collisionMaskList[0] = 0;
                collisionMaskList[1] = 0;
            }
        }

        void OnInfHealth() {
            infHealth = !infHealth;
            infHealthButton.UpdateStateText();
            Console.WriteLine("Infinite Health: " + infHealth);
        }

        void OnInfShuriken() {
            infShuriken = !infShuriken;
            infShurikenButton.UpdateStateText();
            Console.WriteLine("Infinite Shurikens: " + infShuriken);
        }

        void OnInfJump() {
            infJump = !infJump;
            infJumpButton.UpdateStateText();
            Console.WriteLine("Infinite Jumps: " + infJump);
        }

        void OnNoBounds() {
            noBounds = !noBounds;
            noBoundsButton.UpdateStateText();
            Console.WriteLine("No Camera Bounds: " + noBounds);
        }

        void OnDebugPos() {
            debugPos = !debugPos;
            debugPosButton.UpdateStateText();
            Console.WriteLine("Position Debug Display: " + debugPos);
        }

        void OnDebugBoss() {
            debugBoss = !debugBoss;
            debugBossButton.UpdateStateText();
            Console.WriteLine("Boss Debug Display: " + debugBoss);
        }

        void OnReloadButton() {
            Console.WriteLine("Reloading to last checkpoint");
            Manager<LevelManager>.Instance.LoadToLastCheckpoint(false, false);
        }

        void OnToggleCollisions() {
            collisionsDisabled = !collisionsDisabled;
            List<LayerMask> collisionMaskList = Manager<PlayerManager>.Instance?.Player?.Controller?.collisionMaskList;
            if (collisionMaskList != null) {
                // Only add collisions if they are turned off
                if (!collisionsDisabled) {
                    collisionMaskList[0] = 4608;
                    collisionMaskList[1] = 25165824;
                } else if (collisionsDisabled) {
                    collisionMaskList[0] = 0;
                    collisionMaskList[1] = 0;
                }
            }
            toggleCollisionsButton.UpdateStateText();
            Console.WriteLine("Collisions: " + !collisionsDisabled);
        }

        void OnSaveButton() {
            Console.WriteLine("Instant Saving");
            Vector3 loadedLevelPlayerPosition = new Vector2(Manager<PlayerManager>.Instance.Player.transform.position.x, Manager<PlayerManager>.Instance.Player.transform.position.y);
            Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = loadedLevelPlayerPosition;
            Manager<SaveManager>.Instance.Save();
        }

        void OnSecondQuest() {
            Manager<ProgressionManager>.Instance.secondQuest = !Manager<ProgressionManager>.Instance.secondQuest;
            secondQuestButton.UpdateStateText();
            Console.WriteLine("Second Quest: " + Manager<ProgressionManager>.Instance.secondQuest);
        }

        bool OnEnterSpeed(string entry) {
            if (float.TryParse(entry, out speedMult)) {
                Manager<PlayerManager>.Instance?.Player?.SetRunSpeedMultiplier(speedMult);
                Console.WriteLine("Speed Multiplier: " + speedMult);
            } else {
                Console.WriteLine("Speed Multiplier set to invalid value");
            }
            return true;
        }

        bool OnEnterDebugTextColor(string entry) {
            if (entry.Equals("White", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.white;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Black", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.black;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Red", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.red;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Green", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.green;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Blue", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.blue;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Cyan", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.cyan;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Gray", StringComparison.InvariantCultureIgnoreCase) || entry.Equals("Grey", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.gray;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Magenta", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.magenta;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            } else if (entry.Equals("Yellow", StringComparison.InvariantCultureIgnoreCase)) {
                debugTextColor = Color.yellow;
                Console.WriteLine("Debug text color set to: " + debugTextColor);
            }
            if (debugText8 != null)
                debugText8.color = debugTextColor;
            if (debugText16 != null)
                debugText16.color = debugTextColor;
            return true;
        }

        // When they enter the level to load into
        bool OnEnterTeleportLevel(string level) {
            if (Dicts.tpDict == null) {
                Dicts.InitTpDict();
            }
            level = level.Replace(" ", "");
            if (Dicts.tpDict.ContainsKey(level)) {
                TextEntryPopup locationPopup = InitTextEntryPopup(tpButton.addedTo, Manager<LocalizationManager>.Instance.GetText(TP_LOCATION_ENTRY_LOC_ID), (entry) => OnEnterTeleportLocation(level, entry), 2, null, CharsetFlags.Number | CharsetFlags.Dash);
                locationPopup.onBack += () => {
                    locationPopup.gameObject.SetActive(false);
                    tpButton.textEntryPopup.gameObject.SetActive(true);
                    tpButton.textEntryPopup.StartCoroutine(tpButton.textEntryPopup.BackWhenBackButtonReleased());
                };
                tpButton.textEntryPopup.gameObject.SetActive(false);
                locationPopup.Init(string.Empty);
                locationPopup.gameObject.SetActive(true);
                locationPopup.transform.SetParent(tpButton.addedTo.transform.parent);
                tpButton.addedTo.gameObject.SetActive(false);
                Canvas.ForceUpdateCanvases();
                locationPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = false;
                EventSystem.current.SetSelectedGameObject(locationPopup.initialSelection);
                locationPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = true;
                return false;
            }
            Console.WriteLine("Teleport Level set to an invalid value");
            return false;
        }

        bool OnEnterTeleportLocation(string level, string location) {
            if(int.TryParse(location, out int tpLoc) && Dicts.tpDict[level].TryGetValue(tpLoc, out float[] loadPos)) {
                EBits dimension = Manager<DimensionManager>.Instance.currentDimension;
                // TODO tp to other dimension
                //if (array2.Length == 4) {
                //    if (array2[3].Equals("8")) {
                //        dimension = EBits.BITS_8;
                //    }
                //    if (array2[3].Equals("16")) {
                //        dimension = EBits.BITS_16;
                //    }
                //}
                Manager<PauseManager>.Instance.Resume();
                Manager<UIManager>.Instance.GetView<OptionScreen>().Close(false);
                string levelName = level.Equals("Surf", StringComparison.InvariantCultureIgnoreCase) ? Dicts.levelDict[level] : (Dicts.levelDict[level] + "_Build");
                Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = new Vector2(loadPos[0], loadPos[1]);
                LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(levelName, false, true, LoadSceneMode.Single, ELevelEntranceID.NONE, dimension);
                Console.WriteLine("Teleporting to location " + tpLoc + " in " + level);
                // Close mod options menu before TPing out
                Courier.UI.ModOptionScreen?.Close(false);

                Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
                return true;
            }
            Console.WriteLine("Teleport Location set to an invalid value");
            return false;
        }

        // When they enter the name of the item to give
        bool OnEnterItemToGive(string item) {
            if (Dicts.itemDict == null) {
                Dicts.InitItemDict();
            }
            item = item.Replace(" ", "");
            if (Dicts.itemDict.ContainsKey(item)) {
                TextEntryPopup quantityPopup = InitTextEntryPopup(getItemButton.addedTo, Manager<LocalizationManager>.Instance.GetText(ITEM_NUMBER_ENTRY_LOC_ID), (entry) => OnEnterItemQuantity(item, entry), 4, null, CharsetFlags.Number | CharsetFlags.Dash);
                quantityPopup.onBack += () => {
                    quantityPopup.gameObject.SetActive(false);
                    getItemButton.textEntryPopup.gameObject.SetActive(true);
                    getItemButton.textEntryPopup.StartCoroutine(getItemButton.textEntryPopup.BackWhenBackButtonReleased());
                };
                getItemButton.textEntryPopup.gameObject.SetActive(false);
                quantityPopup.Init(string.Empty);
                quantityPopup.gameObject.SetActive(true);
                quantityPopup.transform.SetParent(getItemButton.addedTo.transform.parent);
                getItemButton.addedTo.gameObject.SetActive(false);
                Canvas.ForceUpdateCanvases();
                quantityPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = false;
                EventSystem.current.SetSelectedGameObject(quantityPopup.initialSelection);
                quantityPopup.initialSelection.GetComponent<UIObjectAudioHandler>().playAudio = true;
                return false;
            }
            Console.WriteLine("Item Name To Give set to an invalid value");
            return false;
        }

        bool OnEnterItemQuantity(string item, string number) {
            if (int.TryParse(number, out int quantity)) {
                if (item.Equals("TimeShard", StringComparison.InvariantCultureIgnoreCase)) {
                    if (quantity >= 0) {
                        Manager<InventoryManager>.Instance.CollectTimeShard(quantity);
                    } else {
                        Manager<InventoryManager>.Instance.SpendTimeShard(-quantity);
                    }
                } else {
                    string[] itemIDs = Dicts.itemDict[item].Split('-');
                    if (quantity >= 1) {
                        if (itemIDs.Length == 1) {
                            Manager<InventoryManager>.Instance.AddItem((EItems)int.Parse(itemIDs[0]), 1);
                        } else {
                            for (int i = int.Parse(itemIDs[0]); i <= int.Parse(itemIDs[1]); i++) {
                                Manager<InventoryManager>.Instance.AddItem((EItems)i, 1);
                            }
                        }
                    }
                    if (quantity <= 0) {
                        if (itemIDs.Length == 1) {
                            Manager<InventoryManager>.Instance.RemoveItem((EItems)int.Parse(itemIDs[0]), 1);
                        } else {
                            for (int i = int.Parse(itemIDs[0]); i <= int.Parse(itemIDs[1]); i++) {
                                Manager<InventoryManager>.Instance.RemoveItem((EItems)i, 1);
                            }
                        }
                    }
                }
                Console.WriteLine("Giving " + quantity + "x " + item);
                return true;
            }
            Console.WriteLine("Item Quantity set to an invalid value");
            return false;
        }

        Vector3 RetroCamera_SnapPositionToCameraBounds(On.RetroCamera.orig_SnapPositionToCameraBounds orig, RetroCamera self, Vector3 pos) {
            if (!noBounds) {
                return orig(self, pos);
            }
            return pos;
        }


        int PlayerManager_get_PlayerShurikens(Func<PlayerManager, int> orig, PlayerManager self) {
            if (!infShuriken) {
                return orig(self);
            }
            return self.GetMaxShuriken();
        }

        void PlayerController_Damage(On.PlayerController.orig_Damage orig, PlayerController self, int amount) {
            if (!infHealth) {
                orig(self, amount);
            }
        }


        bool PlayerController_CanJump(On.PlayerController.orig_CanJump orig, PlayerController self) {
            if (self.IsDucking && !self.CanUnduck()) {
                return orig(self);
            }
            if (infJump) {
                return true;
            }
            return orig(self);
        }


        public void InGameHud_OnGUI(On.InGameHud.orig_OnGUI orig, InGameHud self) {
            orig(self);
            if (debugText8 == null) {
                debugText8 = UnityEngine.Object.Instantiate(self.hud_8.coinCount, self.hud_8.gameObject.transform);
                debugText16 = UnityEngine.Object.Instantiate(self.hud_16.coinCount, self.hud_16.gameObject.transform);
                debugText8.transform.Translate(0f, -110f, 0f);
                debugText16.transform.Translate(0f, -110f, 0f);
                debugText8.fontSize = 7f;
                debugText16.fontSize = 7f;
                debugText8.alignment = TextAlignmentOptions.TopRight;
                debugText16.alignment = TextAlignmentOptions.TopRight;
                debugText8.enableWordWrapping = false;
                debugText16.enableWordWrapping = false;
                debugText8.color = debugTextColor;
                debugText16.color = debugTextColor;
            }
            debugText8.text = debugText16.text = string.Empty;
            UpdateDebugText();
        }

        private void AddToDebug(string debug) {
            debugText8.text += debug;
            debugText16.text += debug;
        }

        private void UpdateDebugText() { // TODO
            if (debugPos) {
                Vector2 playerPos = Manager<PlayerManager>.Instance.Player.transform.position;
                string posText = Manager<LocalizationManager>.Instance.GetText(POS_DEBUG_TEXT_LOC_ID);
                posText = posText.Replace("[posX]", playerPos.x.ToString("F1"));
                posText = posText.Replace("[posY]", playerPos.y.ToString("F1"));
                AddToDebug("\r\n" + posText);
            }
            if (noBounds) {
                AddToDebug("\r\n" + Manager<LocalizationManager>.Instance.GetText(CAMERA_UNLOCKED_DEBUG_TEXT_LOC_ID));
            }
            if (collisionsDisabled) {
                AddToDebug("\r\n" + Manager<LocalizationManager>.Instance.GetText(NO_COLLISIONS_DEBUG_TEXT_LOC_ID));
            }
            if (infShuriken) {
                AddToDebug("\r\n" + Manager<LocalizationManager>.Instance.GetText(INF_SHURIKEN_DEBUG_TEXT_LOC_ID));
            }
            if (infHealth) {
                AddToDebug("\r\n" + Manager<LocalizationManager>.Instance.GetText(INF_HEALTH_DEBUG_TEXT_LOC_ID));
            }
            if (infJump) {
                AddToDebug("\r\n" + Manager<LocalizationManager>.Instance.GetText(INF_JUMP_DEBUG_TEXT_LOC_ID));
            }
            if (Manager<PlayerManager>.Instance.Player.RunSpeedMultiplier > 1f) {
                string speedText = Manager<LocalizationManager>.Instance.GetText(SPEED_DEBUG_TEXT_LOC_ID);
                speedText = speedText.Replace("[Speed]", Manager<PlayerManager>.Instance.Player.RunSpeedMultiplier.ToString());
                AddToDebug("\r\n" + speedText);
            }
            if (debugBoss) {
                AddToDebug(GetDebugBossString());
            }
        }

        private string GetDebugBossString() { // TODO
            string bossName = "";
            string bossState = "";
            string bossHealth = "";
            string currentSceneName = Manager<LevelManager>.Instance.CurrentSceneName;
            if (currentSceneName == "Level_02_AutumnHills_Build") {
                bossName = "Leaf Golem";
                if (Manager<LeafGolemFightManager>.Instance != null) {
                    bossState = Manager<LeafGolemFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<LeafGolemFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<LeafGolemFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_03_ForlornTemple_Build") {
                bossName = "Demon King";
                if (Manager<DemonKingFightManager>.Instance != null) {
                    bossState = Manager<DemonKingFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<DemonKingFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<DemonKingFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_04_Catacombs_Build") {
                bossName = "Ruxxtin";
                if (Manager<NecromancerFightManager>.Instance != null) {
                    bossState = Manager<NecromancerFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<NecromancerFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<NecromancerFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_05_A_HowlingGrotto_Build") {
                bossName = "Emerald Golem";
                if (Manager<EmeraldGolemFightManager>.Instance != null) {
                    if (Manager<EmeraldGolemFightManager>.Instance.EssenceComponent == null) {
                        bossState = Manager<EmeraldGolemFightManager>.Instance.bossComponent.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                        bossHealth = "B: " + Manager<EmeraldGolemFightManager>.Instance.bossComponent.CurrentHP + "/" + Manager<EmeraldGolemFightManager>.Instance.bossComponent.maxHP + " G: " + Manager<EmeraldGolemFightManager>.Instance.bossComponent.gemHP + "/" + Manager<EmeraldGolemFightManager>.Instance.bossComponent.gemMaxHP;
                    } else {
                        bossState = "MovementCoroutine";
                        bossHealth = "E: " + Manager<EmeraldGolemFightManager>.Instance.EssenceComponent.CurrentHP.ToString() + "/" + Manager<EmeraldGolemFightManager>.Instance.EssenceComponent.maxHP;
                    }
                }
            }
            if (currentSceneName == "Level_07_QuillshroomMarsh_Build") {
                bossName = "Queen Of Quills";
                if (Manager<QueenOfQuillsFightManager>.Instance != null) {
                    bossState = Manager<QueenOfQuillsFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<QueenOfQuillsFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<QueenOfQuillsFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_08_SearingCrags_Build") {
                bossName = "Colos & Suses";
                if (Manager<SearingCragsBossFightManager>.Instance != null && Manager<SearingCragsBossFightManager>.Instance.colossusesInstance.stateMachine != null) {
                    bossState = string.Concat(new object[]
                    {
                    bossState,
                    Manager<SearingCragsBossFightManager>.Instance.colossusesInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0],
                    " C: ",
                    Manager<SearingCragsBossFightManager>.Instance.colosInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0],
                    " S: ",
                    Manager<SearingCragsBossFightManager>.Instance.susesInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0]
                    });
                    if (Manager<SearingCragsBossFightManager>.Instance.colosInstance != null && Manager<SearingCragsBossFightManager>.Instance.susesInstance != null) {
                        bossHealth = bossHealth + "C: " + Manager<SearingCragsBossFightManager>.Instance.colosInstance.CurrentHP + "/" + Manager<SearingCragsBossFightManager>.Instance.colosInstance.maxHP + " S: " + Manager<SearingCragsBossFightManager>.Instance.susesInstance.CurrentHP + "/" + Manager<SearingCragsBossFightManager>.Instance.susesInstance.maxHP;
                    }
                }
            }
            if (currentSceneName == "Level_10_A_TowerOfTime_Build") {
                bossName = "Arcane Golem";
                if (Manager<ArcaneGolemBossFightManager>.Instance != null) {
                    bossState = Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.head.CurrentHP.ToString() + "/" + Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.head.maxHP;
                    bossHealth = bossHealth + "  P2: " + Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.secondPhaseStartHP;
                }
            }
            if (currentSceneName == "Level_11_A_CloudRuins_Build") {
                bossName = "Manfred";
                if (Manager<ManfredBossfightManager>.Instance != null) {
                    bossState = Manager<ManfredBossfightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<ManfredBossfightManager>.Instance.bossInstance.head.hittable.CurrentHP.ToString() + "/" + Manager<ManfredBossfightManager>.Instance.bossInstance.head.hittable.maxHP;
                }
            }
            if (currentSceneName == "Level_12_UnderWorld_Build") {
                bossName = "Demon General";
                if (Manager<DemonGeneralFightManager>.Instance != null) {
                    bossState = Manager<DemonGeneralFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<DemonGeneralFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<DemonGeneralFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_04_C_RiviereTurquoise_Build") {
                bossName = "Butterfly Matriarch";
                if (Manager<ButterflyMatriarchFightManager>.Instance != null) {
                    bossState = Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.maxHP;
                    bossHealth = bossHealth + "  P2: " + Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.phase1MaxHP;
                    bossHealth = bossHealth + ", P3: " + Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.phase2MaxHP;
                }
            }
            if (currentSceneName == "Level_09_B_ElementalSkylands_Build") {
                bossName = "Clockwork Concierge";
                if (Manager<ConciergeFightManager>.Instance != null) {
                    bossState = string.Concat(new object[]
                    {
                    bossState,
                    Manager<ConciergeFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0],
                    " B: ",
                    Manager<ConciergeFightManager>.Instance.bossInstance.bodyStateMachine.CurrentState.ToString().Split(' ', '(')[0],
                    " H: ",
                    Manager<ConciergeFightManager>.Instance.bossInstance.headStateMachine.CurrentState.ToString().Split(' ', '(')[0]
                    });
                    bossHealth = ((!Manager<ConciergeFightManager>.Instance.bossInstance.opened) ? (bossHealth + "H: " + Manager<ConciergeFightManager>.Instance.bossInstance.head.CurrentHP + " C: " + Manager<ConciergeFightManager>.Instance.bossInstance.bodyCanon_1.CurrentHP + "|" + Manager<ConciergeFightManager>.Instance.bossInstance.bodyCanon_2.CurrentHP + "|" + Manager<ConciergeFightManager>.Instance.bossInstance.bodyCanon_3.CurrentHP + " T: " + Manager<ConciergeFightManager>.Instance.bossInstance.sideTrap.CurrentHP) : ("H: " + Manager<ConciergeFightManager>.Instance.bossInstance.heart.CurrentHP + "/" + Manager<ConciergeFightManager>.Instance.bossInstance.heart.maxHP));
                }
            }
            if (currentSceneName == "Level_11_B_MusicBox_Build") {
                bossName = "Phantom";
                if (Manager<PhantomFightManager>.Instance != null) {
                    bossState = Manager<PhantomFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<PhantomFightManager>.Instance.bossInstance.hittable.CurrentHP.ToString() + "/" + Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                    bossHealth = bossHealth + "  P2: " + Manager<PhantomFightManager>.Instance.bossInstance.moveSequence_2_Threshold * Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                    bossHealth = bossHealth + ", P3: " + Manager<PhantomFightManager>.Instance.bossInstance.moveSequence_3_Threshold * Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                }
            }
            if (currentSceneName == "Level_15_Surf") {
                bossName = "Octo";
                if (Manager<SurfBossManager>.Instance != null) {
                    bossState = Manager<SurfBossManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<SurfBossManager>.Instance.bossInstance.hittable.CurrentHP + "/" + Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                    bossHealth = bossHealth + "  P2: " + Manager<SurfBossManager>.Instance.bossInstance.moveSequence_2_Threshold * Manager<SurfBossManager>.Instance.bossInstance.hittable.maxHP;
                    bossHealth = bossHealth + ", P3: " + Manager<SurfBossManager>.Instance.bossInstance.moveSequence_3_Threshold * Manager<SurfBossManager>.Instance.bossInstance.hittable.maxHP;
                }
            }
            if (currentSceneName == "Level_16_Beach_Build") {
                bossName = "Totem";
                if (Manager<TotemBossFightManager>.Instance != null) {
                    bossState = Manager<TotemBossFightManager>.Instance.bossInstance.StateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    bossHealth = Manager<TotemBossFightManager>.Instance.bossInstance.CurrentHp + "/" + Manager<TotemBossFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_18_Volcano_Chase_Build") {
                bossName = "Unable to debug Punch Out Boss";
            }
            if (bossHealth == "") {
                return "\r\nNo Boss Found";
            }
            return "\r\n" + bossName + " HP: " + bossHealth + "\r\nState: " + bossState;
        }
    }
}
