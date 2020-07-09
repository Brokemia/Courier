using System;
using Discord;
using Mod.Courier.UI;
using UnityEngine;

namespace Mod.Courier.RichPresence {
    public class RichPresenceCourier : IRichPresence {
        //private const long DISCORD_CLIENT_ID = 568503633449189407L;
        // Use a different application ID so we can change the name and images. Ask Brokemia if you want access to this
        private const long DISCORD_CLIENT_ID = 730261529680674917L;

        private const float DISCORD_RICH_UPDATE_TIME = 20f;

        private const int DISCORD_RICH_MAX_MESSAGE_PER_TIME = 5;

        private Discord.Discord discord;

        private float messageCountTimer;

        private int messageCount;

        private bool latestActivityWasSent;

        private Activity latestActivity;

        private bool enabled;

        public void Init() {
            try {
                discord = new Discord.Discord(DISCORD_CLIENT_ID, 1uL);
                enabled = true;
            } catch (Exception) {
                CourierLogger.Log(LogType.Warning, "RichPresenceCourier::Init", "Discord not available.");
                enabled = false;
                discord = null;
            }
        }

        public void Update() {
            if (!enabled) {
                return;
            }
            if (messageCountTimer > 0f) {
                messageCountTimer -= Time.unscaledDeltaTime;
                if (messageCountTimer <= 0f && !latestActivityWasSent) {
                    SendRichPresenceUpdate(latestActivity);
                }
            }
            try {
                discord.RunCallbacks();
            } catch (Exception) {
                CourierLogger.Log(LogType.Warning, "RichPresenceCourier::Update", "Discord not available.");
                enabled = false;
                if (discord != null) {
                    discord.Dispose();
                    discord = null;
                }
            }
        }

        public void Destroy() {
            if (discord != null) {
                discord.Dispose();
                discord = null;
            }
        }

        public void SetMainMenuRichPresence() {
            if (enabled) {
                int modsLoaded = Courier.Mods.Count - 1;
                UpdateRichPresence("Title Screen", $"{modsLoaded} Mod" + (modsLoaded == 1 ? "" : "s") + " Installed", "icon", string.Empty);
            }
        }

        public void SetShopRichPresence() {
            if (enabled) {
                string largeImageId = "shop";
                if (Manager<DLCManager>.Instance.IsCurrentDLC(EDLC.PICNIC_PANIC)) {
                    largeImageId = "shop_pp";
                }
                string state = "Shop";
                string dlcAbbrev = GetDLCAbbrev();
                if (!string.IsNullOrEmpty(dlcAbbrev)) {
                    state += " - " + dlcAbbrev;
                }
                // -1 to remove Courier itself
                int modsLoaded = Courier.Mods.Count - 1;
                UpdateRichPresence(state, $"{modsLoaded} Mod" + (modsLoaded == 1 ? "" : "s") + " Installed", largeImageId, GetPortraitId());
            }
        }

        public void SetLevelRichPresence(ELevel level = ELevel.NONE, EBits dimension = EBits.NONE) {
            if (enabled) {
                string state = GetLevelName(level);
                string dlcAbbrev = GetDLCAbbrev();
                if(!string.IsNullOrEmpty(dlcAbbrev)) {
                    state += " - " + dlcAbbrev;
                }
                // -1 to remove Courier itself
                int modsLoaded = Courier.Mods.Count - 1;
                UpdateRichPresence(state, $"{modsLoaded} Mod" + (modsLoaded == 1 ? "" : "s") + " Installed", level.ToString().ToLower(), GetPortraitId(dimension));
            }
        }

        public void UpdateDimensionRichPresence(EBits dimension) {
            if (enabled) {
                latestActivity.Assets.SmallImage = GetPortraitId(dimension);
                SendRichPresenceUpdate(latestActivity);
            }
        }

        private string GetDLCAbbrev() {
            switch (Manager<DLCManager>.Instance.CurrentDLC) {
                case EDLC.PICNIC_PANIC:
                    return "PP";
                case EDLC.NONE:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private string GetDLCName() {
            switch (Manager<DLCManager>.Instance.CurrentDLC) {
                case EDLC.PICNIC_PANIC:
                    return "Picnic Panic";
                case EDLC.NONE:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        private string GetLevelName(ELevel level) {
            switch (level) {
                case ELevel.Level_01_NinjaVillage:
                    return "Ninja Village";
                case ELevel.Level_02_AutumnHills:
                    return "Autumn Hills";
                case ELevel.Level_03_ForlornTemple:
                    return "Forlorn Temple";
                case ELevel.Level_04_B_DarkCave:
                    return "Dark Cave";
                case ELevel.Level_04_C_RiviereTurquoise:
                    return "Rivière Turquoise";
                case ELevel.Level_04_Catacombs:
                    return "Catacombs";
                case ELevel.Level_05_A_HowlingGrotto:
                    return "Howling Grotto";
                case ELevel.Level_05_B_SunkenShrine:
                    return "Sunken Shrine";
                case ELevel.Level_06_A_BambooCreek:
                    return "Bamboo Creek";
                case ELevel.Level_07_QuillshroomMarsh:
                    return "Quillshroom Marsh";
                case ELevel.Level_08_SearingCrags:
                    return "Searing Crags";
                case ELevel.Level_09_A_GlacialPeak:
                    return "Glacial Peak";
                case ELevel.Level_09_B_ElementalSkylands:
                    return "Elemental Skylands";
                case ELevel.Level_10_A_TowerOfTime:
                    return "Tower of Time";
                case ELevel.Level_11_A_CloudRuins:
                    return "Cloud Ruins";
                case ELevel.Level_11_B_MusicBox:
                    return "Music Box";
                case ELevel.Level_12_UnderWorld:
                    return "Underworld";
                case ELevel.Level_13_TowerOfTimeHQ:
                    return "Tower of Time HQ";
                case ELevel.Level_14_CorruptedFuture:
                    return "Corrupted Future";
                case ELevel.Level_15_Surf:
                    return "Open Sea";
                case ELevel.Level_16_Beach:
                    return "Voodkin Shore";
                case ELevel.Level_17_Volcano:
                    return "Fire Mountain";
                case ELevel.Level_18_Volcano_Chase:
                    return "Voodoo Heart";
                case ELevel.Level_TestSclout:
                    string editorSceneName = Manager<LevelManager>.Instance.GetEditorSceneName(((patch_LevelManager) Manager<LevelManager>.Instance).lastLevelLoaded);
                    return Manager<LocalizationManager>.Instance.GetText(ModSaveSlotUI.modLevelLocIDPrefix + editorSceneName);
                default:
                    return string.Empty;
            }
        }

        private string GetPortraitId(EBits dimension = EBits.NONE) {
            ESkin eSkin = (Manager<SkinManager>.Instance != null) ? Manager<SkinManager>.Instance.CurrentSkinID : ESkin.DEFAULT;
            if (dimension == EBits.NONE) {
                dimension = ((!(Manager<DimensionManager>.Instance != null)) ? EBits.BITS_8 : Manager<DimensionManager>.Instance.CurrentDimension);
            }
            switch (eSkin) {
                case ESkin.DEFAULT:
                    return (dimension != EBits.BITS_8) ? "messenger_16" : "messenger_8";
                case ESkin.DARK_MESSENGER:
                    return (dimension != EBits.BITS_8) ? "darkmessenger_16" : "darkmessenger_8";
                case ESkin.GHEESLING_RED:
                    return (dimension != EBits.BITS_8) ? "messenger_red_16" : "messenger_red_8";
                case ESkin.GHEESLING_BLUE:
                    return (dimension != EBits.BITS_8) ? "messenger_blue_16" : "messenger_blue_8";
                default:
                    return (dimension != EBits.BITS_8) ? "messenger_16" : "messenger_8";
            }
        }

        private void UpdateRichPresence(string state = "", string details = "", string largeImageId = "", string smallImageId = "") {
            if (enabled) {
                CourierLogger.Log("RichPresenceCourier::UpdateRichPresence", "state: " + state + ", details: " + details + ", largeImageId: " + largeImageId + ", smallImageId: " + smallImageId);
                Activity activity = default(Activity);
                activity.Type = ActivityType.Playing;
                activity.ApplicationId = DISCORD_CLIENT_ID;
                activity.State = state;
                activity.Details = details;
                activity.Assets.LargeImage = largeImageId;
                activity.Assets.SmallImage = smallImageId;
                DateTime d = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                activity.Timestamps.Start = Convert.ToInt64((DateTime.UtcNow - d).TotalSeconds);
                SendRichPresenceUpdate(activity);
            }
        }

        private void SendRichPresenceUpdate(Activity activity) {
            latestActivity = activity;
            latestActivityWasSent = false;
            if (messageCountTimer <= 0f) {
                messageCountTimer = 20f;
                messageCount = 0;
            }
            messageCount++;
            if (messageCount <= 5) {
                CourierLogger.Log("RichPresenceCourier::SendRichPresenceUpdate", "State: " + activity.State + ", Details: " + activity.Details + ", LargeImage: " + activity.Assets.LargeImage + ", SmallImage: " + activity.Assets.SmallImage);
                latestActivityWasSent = true;
                try {
                    discord.GetActivityManager().UpdateActivity(activity, OnRichPresenceUpdate);
                } catch (Exception) {
                    CourierLogger.Log(LogType.Warning, "RichPresenceCourier::SendRichPresenceUpdate", "Discord not available.");
                    enabled = false;
                    if (discord != null) {
                        discord.Dispose();
                        discord = null;
                    }
                }
            }
        }

        private void OnRichPresenceUpdate(Result result) {
            CourierLogger.Log("RichPresenceCourier::OnRichPresenceUpdate", "" + result);
        }
    }
}
