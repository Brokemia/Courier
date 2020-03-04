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
using UnityEngine.SceneManagement;
using static Mod.Courier.UI.TextEntryButtonInfo;

namespace TrainerReborn {
    public class TrainerRebornModule : CourierModule {

        public bool noBounds;

        public bool debugPos;

        public bool debugBoss;

        public bool infJump;

        public bool infHealth;

        public bool infShuriken;

        public float speedMult = 1;

        public Color debugTextColor = Color.white;
        
        private string[] cmdArray;

        private string command = "Trainer";

        private TextMeshProUGUI debugText8;

        private TextMeshProUGUI debugText16;

        private bool IsCapsLockOn => (GetKeyState(20) & 1) > 0;

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

        [DllImport("USER32.dll")]
        public static extern short GetKeyState(int nVirtKey);

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
                //On.RetroCamera.SnapPositionToCameraBounds += RetroCamera_SnapPositionToCameraBounds;
            }

            infHealthButton = Courier.UI.RegisterToggleModOptionButton("Infinite Health", OnInfHealth, (b) => infHealth);
            infShurikenButton = Courier.UI.RegisterToggleModOptionButton("Infinite Shurikens", OnInfShuriken, (b) => infShuriken);
            infJumpButton = Courier.UI.RegisterToggleModOptionButton("Infinite Jumps", OnInfJump, (b) => infJump);
            noBoundsButton = Courier.UI.RegisterToggleModOptionButton("No Camera Bounds", OnNoBounds, (b) => noBounds);
            debugPosButton = Courier.UI.RegisterToggleModOptionButton("Position Debug Display", OnDebugPos, (b) => debugPos);
            debugBossButton = Courier.UI.RegisterToggleModOptionButton("Boss Debug Display", OnDebugBoss, (b) => debugBoss);
            toggleCollisionsButton = Courier.UI.RegisterToggleModOptionButton("Collisions", OnToggleCollisions, (b) => (Manager<PlayerManager>.Instance?.Player?.Controller?.collisionMaskList?.Count ?? 3) >= 2);
            secondQuestButton = Courier.UI.RegisterToggleModOptionButton("Second Quest", OnSecondQuest, (b) => Manager<ProgressionManager>.Instance.secondQuest);
            speedMultButton = Courier.UI.RegisterTextEntryModOptionButton("Speed Multiplier", OnEnterSpeed, 4, null, () => Manager<PlayerManager>.Instance?.Player?.RunSpeedMultiplier.ToString() ?? "" + speedMult, CharsetFlags.Number | CharsetFlags.Dot);
            debugTextColorButton = Courier.UI.RegisterTextEntryModOptionButton("Debug Text Color", OnEnterDebugTextColor, 7, null, () => "", CharsetFlags.Letter);
            reloadButton = Courier.UI.RegisterSubMenuModOptionButton("Reload To Last Checkpoint", OnReloadButton);
            saveButton = Courier.UI.RegisterSubMenuModOptionButton("Instant Save", OnSaveButton);
        }

        void PlayerController_Awake(On.PlayerController.orig_Awake orig, PlayerController self) {
            orig(self);
            self.SetRunSpeedMultiplier(speedMult);
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
            List<LayerMask> collisionMaskList = Manager<PlayerManager>.Instance.Player.Controller.collisionMaskList;
            // Only add collisions if they are turned off
            if (collisionMaskList.Count < 2) {
                collisionMaskList.Add(4608);
                collisionMaskList.Add(25165824);
            } else {
                collisionMaskList.Clear();
            }
            toggleCollisionsButton.UpdateStateText();
            Console.WriteLine("Collisions: " + (collisionMaskList.Count >= 2));
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

        void OnEnterSpeed(string entry) {
            if (float.TryParse(entry, out speedMult)) {
                Manager<PlayerManager>.Instance?.Player?.SetRunSpeedMultiplier(speedMult);
                Console.WriteLine("Speed Multiplier: " + speedMult);
            } else {
                Console.WriteLine("Speed Multiplier set to invalid value");
            }
        }

        void OnEnterDebugTextColor(string entry) {
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
            if (cmdArray == null) {
                cmdArray = new string[]
                {
                "Item",
                "Tp"
                };
            }
            if (Dicts.tpDict == null) {
                Dicts.InitTpDict();
            }
            if (Dicts.itemDict == null) {
                Dicts.InitItemDict();
            }
            if (Dicts.levelDict == null) {
                Dicts.InitLevelDict();
            }
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
            string text3 = debugText8.text = (debugText16.text = "");
            UpdateDebugText();
            if (Event.current.isKey && Event.current.type == EventType.KeyDown && Event.current.keyCode != 0) {
                HandleInput(Event.current.keyCode);
                UpdateCommand(self, command);
            }
        }

        private void UpdateCommand(InGameHud gameHud, string cmd) {
            gameHud.hud_8.playerName.SetText(cmd);
            gameHud.hud_16.playerName.SetText(cmd);
        }

        private void AddToDebug(string debug) {
            debugText8.text += debug;
            debugText16.text += debug;
        }

        private void UpdateDebugText() {
            if (debugPos) {
                Vector2 vector = Manager<PlayerManager>.Instance.Player.transform.position;
                AddToDebug("\r\nPos (" + vector.x.ToString("F1") + ", " + vector.y.ToString("F1") + ")");
            }
            if (noBounds) {
                AddToDebug("\r\nCamera Unlocked");
            }
            if (Manager<PlayerManager>.Instance.Player.Controller.collisionMaskList.Count == 0) {
                AddToDebug("\r\nNo Collisions");
            }
            if (infShuriken) {
                AddToDebug("\r\nInf Shuriken");
            }
            if (infHealth) {
                AddToDebug("\r\nInf Health");
            }
            if (infJump) {
                AddToDebug("\r\nInf Jumps");
            }
            if (Manager<PlayerManager>.Instance.Player.RunSpeedMultiplier > 1f) {
                AddToDebug("\r\nSpeed: " + Manager<PlayerManager>.Instance.Player.RunSpeedMultiplier);
            }
            if (debugBoss) {
                AddToDebug(GetDebugBossString());
            }
        }


        private void HandleInput(KeyCode keyCode) {
            Event.current.keyCode = KeyCode.None;
            if (keyCode == KeyCode.Slash) {
                command = "/";
            } else {
                if (!command.StartsWith("/")) {
                    return;
                }
                switch (keyCode) {
                    case KeyCode.Period:
                        command += ".";
                        return;
                    case KeyCode.Minus:
                        command += "-";
                        return;
                }
                if (keyCode.ToString().StartsWith("Alpha")) {
                    command += keyCode.ToString().Substring(5);
                    return;
                }
                if (keyCode.ToString().StartsWith("Keypad") && keyCode != KeyCode.KeypadEnter) {
                    if (keyCode.ToString().Equals("KeypadMinus")) {
                        command += "-";
                    } else if (keyCode.ToString().Equals("KeypadPeriod")) {
                        command += ".";
                    } else if (keyCode.ToString().Length <= 7) {
                        command += keyCode.ToString().Substring(6);
                    }
                    return;
                }
                switch (keyCode) {
                    case KeyCode.Backspace:
                        command = command.Remove(command.Length - 1);
                        return;
                    case KeyCode.Tab: {
                            string[] array = command.Split('.');
                            if (array.Length == 1) {
                                for (int i = 0; i < cmdArray.Length; i++) {
                                    if (array[0].Substring(1).CompareTo(cmdArray[i]) == -1) {
                                        command = "/" + cmdArray[i];
                                        break;
                                    }
                                    if (i == cmdArray.Length - 1) {
                                        command = "/" + cmdArray[0];
                                    }
                                }
                            }
                            if (array.Length > 1 && array[0].Equals("/Item", StringComparison.InvariantCultureIgnoreCase)) {
                                if (array.Length == 2) {
                                    List<string> list = new List<string>(Dicts.itemDict.Keys);
                                    for (int j = 0; j < list.Count; j++) {
                                        if (array[1].CompareTo(list[j]) == -1) {
                                            command = array[0] + "." + list[j];
                                            break;
                                        }
                                        if (j == list.Count - 1) {
                                            command = array[0] + "." + list[0];
                                        }
                                    }
                                } else if (array.Length >= 3) {
                                    string text;
                                    if (array[1].Equals("TimeShard", StringComparison.InvariantCultureIgnoreCase)) {
                                        if (array[2].Equals("")) {
                                            text = "1";
                                        } else {
                                            int num = int.Parse(array[2]);
                                            int num2 = 1;
                                            if (num >= 100 || num <= -100) {
                                                num2 = 10;
                                            }
                                            if (num >= 1000 || num <= -1000) {
                                                num2 = 100;
                                            }
                                            if (num >= 10000 || num <= -10000) {
                                                num2 = 1000;
                                            }
                                            text = (num + num2).ToString();
                                        }
                                    } else {
                                        text = (array[2].Equals("1") ? "0" : (array[2].Equals("0") ? "1" : "0"));
                                    }
                                    command = string.Concat(new object[5]
                                    {
                            array[0],
                            ".",
                            array[1],
                            ".",
                            text
                                    });
                                }
                            }
                            if (array.Length <= 1 || !array[0].Equals("/Tp", StringComparison.InvariantCultureIgnoreCase)) {
                                break;
                            }
                            if (array.Length == 2) {
                                List<string> list2 = new List<string>(Dicts.tpDict.Keys);
                                for (int k = 0; k < list2.Count; k++) {
                                    if (array[1].CompareTo(list2[k]) == -1) {
                                        command = array[0] + "." + list2[k];
                                        break;
                                    }
                                    if (k == list2.Count - 1) {
                                        command = array[0] + "." + list2[0];
                                    }
                                }
                            } else if (array.Length == 3) {
                                List<int> list3 = new List<int>(Dicts.tpDict[array[1]].Keys);
                                for (int l = 0; l < list3.Count; l++) {
                                    if (array[2].Equals("") || int.Parse(array[2]) < list3[l]) {
                                        command = array[0] + "." + array[1] + "." + list3[l];
                                        break;
                                    }
                                    if (l == list3.Count - 1) {
                                        command = array[0] + "." + array[1] + "." + list3[0];
                                    }
                                }
                            } else if (array.Length >= 4) {
                                string text2 = array[3].Equals("16") ? "8" : (array[3].Equals("8") ? "16" : "8");
                                command = string.Concat(new object[7]
                                {
                        array[0],
                        ".",
                        array[1],
                        ".",
                        array[2],
                        ".",
                        text2
                                });
                            }
                            break;
                        }
                }
                if (keyCode == KeyCode.Return || keyCode == KeyCode.KeypadEnter) {
                    string[] array2 = command.Split('.');
                    if (array2[0].Equals("/Item", StringComparison.InvariantCultureIgnoreCase)) {
                        if (array2[1].Equals("TimeShard", StringComparison.InvariantCultureIgnoreCase)) {
                            if (int.Parse(array2[2]) >= 0) {
                                Manager<InventoryManager>.Instance.CollectTimeShard(int.Parse(array2[2]));
                            } else {
                                Manager<InventoryManager>.Instance.SpendTimeShard(-int.Parse(array2[2]));
                            }
                        } else {
                            string[] array3 = Dicts.itemDict[array2[1]].Split('-');
                            if (array2[2] == "1") {
                                if (array3.Length == 1) {
                                    Manager<InventoryManager>.Instance.AddItem((EItems)int.Parse(Dicts.itemDict[array2[1]]), 1);
                                } else {
                                    for (int m = int.Parse(array3[0]); m <= int.Parse(array3[1]); m++) {
                                        Manager<InventoryManager>.Instance.AddItem((EItems)m, 1);
                                    }
                                }
                            }
                            if (array2[2] == "0") {
                                if (array3.Length == 1) {
                                    Manager<InventoryManager>.Instance.RemoveItem((EItems)int.Parse(Dicts.itemDict[array2[1]]), 1);
                                } else {
                                    for (int n = int.Parse(array3[0]); n <= int.Parse(array3[1]); n++) {
                                        Manager<InventoryManager>.Instance.RemoveItem((EItems)n, 1);
                                    }
                                }
                            }
                            command = array2[0] + ".";
                        }
                    }
                    if (array2[0].Equals("/Tp", StringComparison.InvariantCultureIgnoreCase)) {
                        float[] array4 = Dicts.tpDict[array2[1]][int.Parse(array2[2])];
                        EBits dimension = Manager<DimensionManager>.Instance.currentDimension;
                        if (array2.Length == 4) {
                            if (array2[3].Equals("8")) {
                                dimension = EBits.BITS_8;
                            }
                            if (array2[3].Equals("16")) {
                                dimension = EBits.BITS_16;
                            }
                        }
                        string levelName = array2[1].Equals("Surf", StringComparison.InvariantCultureIgnoreCase) ? Dicts.levelDict[array2[1]] : (Dicts.levelDict[array2[1]] + "_Build");
                        Manager<ProgressionManager>.Instance.checkpointSaveInfo.loadedLevelPlayerPosition = new Vector2(array4[0], array4[1]);
                        LevelLoadingInfo levelLoadingInfo = new LevelLoadingInfo(levelName, false, true, LoadSceneMode.Single, ELevelEntranceID.NONE, dimension);
                        Manager<LevelManager>.Instance.LoadLevel(levelLoadingInfo);
                    }
                }
                string text3 = keyCode.ToString();
                if (text3.Length <= 1) {
                    if (!IsCapsLockOn && !Event.current.shift) {
                        text3 = text3.ToLower();
                    }
                    command += text3;
                }
            }
        }

        private string GetDebugBossString() {
            string text = "";
            string text2 = "";
            string text3 = "";
            string currentSceneName = Manager<LevelManager>.Instance.CurrentSceneName;
            if (currentSceneName == "Level_02_AutumnHills_Build") {
                text = "Leaf Golem";
                if (Manager<LeafGolemFightManager>.Instance != null) {
                    text2 = Manager<LeafGolemFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<LeafGolemFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<LeafGolemFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_03_ForlornTemple_Build") {
                text = "Demon King";
                if (Manager<DemonKingFightManager>.Instance != null) {
                    text2 = Manager<DemonKingFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<DemonKingFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<DemonKingFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_04_Catacombs_Build") {
                text = "Ruxxtin";
                if (Manager<NecromancerFightManager>.Instance != null) {
                    text2 = Manager<NecromancerFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<NecromancerFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<NecromancerFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_05_A_HowlingGrotto_Build") {
                text = "Emerald Golem";
                if (Manager<EmeraldGolemFightManager>.Instance != null) {
                    if (Manager<EmeraldGolemFightManager>.Instance.EssenceComponent == null) {
                        text2 = Manager<EmeraldGolemFightManager>.Instance.bossComponent.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                        text3 = "B: " + Manager<EmeraldGolemFightManager>.Instance.bossComponent.CurrentHP + "/" + Manager<EmeraldGolemFightManager>.Instance.bossComponent.maxHP + " G: " + Manager<EmeraldGolemFightManager>.Instance.bossComponent.gemHP + "/" + Manager<EmeraldGolemFightManager>.Instance.bossComponent.gemMaxHP;
                    } else {
                        text2 = "MovementCoroutine";
                        text3 = "E: " + Manager<EmeraldGolemFightManager>.Instance.EssenceComponent.CurrentHP.ToString() + "/" + Manager<EmeraldGolemFightManager>.Instance.EssenceComponent.maxHP;
                    }
                }
            }
            if (currentSceneName == "Level_07_QuillshroomMarsh_Build") {
                text = "Queen Of Quills";
                if (Manager<QueenOfQuillsFightManager>.Instance != null) {
                    text2 = Manager<QueenOfQuillsFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<QueenOfQuillsFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<QueenOfQuillsFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_08_SearingCrags_Build") {
                text = "Colos & Suses";
                if (Manager<SearingCragsBossFightManager>.Instance != null && Manager<SearingCragsBossFightManager>.Instance.colossusesInstance.stateMachine != null) {
                    string text4 = Manager<SearingCragsBossFightManager>.Instance.colossusesInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    string text5 = Manager<SearingCragsBossFightManager>.Instance.colosInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    string text6 = Manager<SearingCragsBossFightManager>.Instance.susesInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text2 = string.Concat(new object[6]
                    {
                    text2,
                    text4,
                    " C: ",
                    text5,
                    " S: ",
                    text6
                    });
                    if (Manager<SearingCragsBossFightManager>.Instance.colosInstance != null && Manager<SearingCragsBossFightManager>.Instance.susesInstance != null) {
                        text3 = text3 + "C: " + Manager<SearingCragsBossFightManager>.Instance.colosInstance.CurrentHP + "/" + Manager<SearingCragsBossFightManager>.Instance.colosInstance.maxHP + " S: " + Manager<SearingCragsBossFightManager>.Instance.susesInstance.CurrentHP + "/" + Manager<SearingCragsBossFightManager>.Instance.susesInstance.maxHP;
                    }
                }
            }
            if (currentSceneName == "Level_10_A_TowerOfTime_Build") {
                text = "Arcane Golem";
                if (Manager<ArcaneGolemBossFightManager>.Instance != null) {
                    text2 = Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.head.CurrentHP.ToString() + "/" + Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.head.maxHP;
                    text3 = text3 + "  P2: " + Manager<ArcaneGolemBossFightManager>.Instance.bossInstance.secondPhaseStartHP;
                }
            }
            if (currentSceneName == "Level_11_A_CloudRuins_Build") {
                text = "Manfred";
                if (Manager<ManfredBossfightManager>.Instance != null) {
                    text2 = Manager<ManfredBossfightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<ManfredBossfightManager>.Instance.bossInstance.head.hittable.CurrentHP.ToString() + "/" + Manager<ManfredBossfightManager>.Instance.bossInstance.head.hittable.maxHP;
                }
            }
            if (currentSceneName == "Level_12_UnderWorld_Build") {
                text = "Demon General";
                if (Manager<DemonGeneralFightManager>.Instance != null) {
                    text2 = Manager<DemonGeneralFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<DemonGeneralFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<DemonGeneralFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_04_C_RiviereTurquoise_Build") {
                text = "Butterfly Matriarch";
                if (Manager<ButterflyMatriarchFightManager>.Instance != null) {
                    text2 = Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.CurrentHP.ToString() + "/" + Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.maxHP;
                    text3 = text3 + "  P2: " + Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.phase1MaxHP;
                    text3 = text3 + ", P3: " + Manager<ButterflyMatriarchFightManager>.Instance.bossInstance.phase2MaxHP;
                }
            }
            if (currentSceneName == "Level_09_B_ElementalSkylands_Build") {
                text = "Clockwork Concierge";
                if (Manager<ConciergeFightManager>.Instance != null) {
                    string text7 = Manager<ConciergeFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    string text8 = Manager<ConciergeFightManager>.Instance.bossInstance.bodyStateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    string text9 = Manager<ConciergeFightManager>.Instance.bossInstance.headStateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text2 = string.Concat(new object[6]
                    {
                    text2,
                    text7,
                    " B: ",
                    text8,
                    " H: ",
                    text9
                    });
                    text3 = ((!Manager<ConciergeFightManager>.Instance.bossInstance.opened) ? (text3 + "H: " + Manager<ConciergeFightManager>.Instance.bossInstance.head.CurrentHP + " C: " + Manager<ConciergeFightManager>.Instance.bossInstance.bodyCanon_1.CurrentHP + "|" + Manager<ConciergeFightManager>.Instance.bossInstance.bodyCanon_2.CurrentHP + "|" + Manager<ConciergeFightManager>.Instance.bossInstance.bodyCanon_3.CurrentHP + " T: " + Manager<ConciergeFightManager>.Instance.bossInstance.sideTrap.CurrentHP) : ("H: " + Manager<ConciergeFightManager>.Instance.bossInstance.heart.CurrentHP + "/" + Manager<ConciergeFightManager>.Instance.bossInstance.heart.maxHP));
                }
            }
            if (currentSceneName == "Level_11_B_MusicBox_Build") {
                text = "Phantom";
                if (Manager<PhantomFightManager>.Instance != null) {
                    text2 = Manager<PhantomFightManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<PhantomFightManager>.Instance.bossInstance.hittable.CurrentHP.ToString() + "/" + Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                    text3 = text3 + "  P2: " + Manager<PhantomFightManager>.Instance.bossInstance.moveSequence_2_Threshold * (float)Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                    text3 = text3 + ", P3: " + Manager<PhantomFightManager>.Instance.bossInstance.moveSequence_3_Threshold * (float)Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                }
            }
            if (currentSceneName == "Level_15_Surf") {
                text = "Octo";
                if (Manager<SurfBossManager>.Instance != null) {
                    text2 = Manager<SurfBossManager>.Instance.bossInstance.stateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<SurfBossManager>.Instance.bossInstance.hittable.CurrentHP + "/" + Manager<PhantomFightManager>.Instance.bossInstance.hittable.maxHP;
                    text3 = text3 + "  P2: " + Manager<SurfBossManager>.Instance.bossInstance.moveSequence_2_Threshold * (float)Manager<SurfBossManager>.Instance.bossInstance.hittable.maxHP;
                    text3 = text3 + ", P3: " + Manager<SurfBossManager>.Instance.bossInstance.moveSequence_3_Threshold * (float)Manager<SurfBossManager>.Instance.bossInstance.hittable.maxHP;
                }
            }
            if (currentSceneName == "Level_16_Beach_Build") {
                text = "Totem";
                if (Manager<TotemBossFightManager>.Instance != null) {
                    text2 = Manager<TotemBossFightManager>.Instance.bossInstance.StateMachine.CurrentState.ToString().Split(' ', '(')[0];
                    text3 = Manager<TotemBossFightManager>.Instance.bossInstance.CurrentHp + "/" + Manager<TotemBossFightManager>.Instance.bossInstance.maxHP;
                }
            }
            if (currentSceneName == "Level_18_Volcano_Chase_Build") {
                text = "Unable to debug Punch Out Boss";
            }
            if (text3 == "") {
                return "\r\nNo Boss Found";
            }
            return "\r\n" + text + " HP: " + text3 + "\r\nState: " + text2;
        }
    }
}
