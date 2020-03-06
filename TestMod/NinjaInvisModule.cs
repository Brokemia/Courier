using System;
using System.Reflection;
using Mod.Courier;
using Mod.Courier.Helpers;
using Mod.Courier.Module;
using Mod.Courier.UI;
using UnityEngine;

namespace NinjaInvis {
    public class NinjaInvisModule : CourierModule {
        public const string PLAYER_VISIBILITY_BUTTON_LOC_ID = "NINJA_INVIS_PLAYER_VISIBILITY_BUTTON";

        bool ninjaVisibility = true;

        bool shouldSetInvis;

        ToggleButtonInfo invisButtonInfo;

        FieldInfo optionsChangedInfo = typeof(OptionScreen).GetField("optionsChanged", ReflectionHelper.NonPublicInstanceFieldSet);

        public override void Load() {
            Courier.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
            On.SaveGameSlot.Load += SaveGameSlot_Load;
            invisButtonInfo = Courier.UI.RegisterToggleModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(PLAYER_VISIBILITY_BUTTON_LOC_ID), OnInvis, (b) => ninjaVisibility);
        }

        void OnInvis() {
            ninjaVisibility = !ninjaVisibility;
            shouldSetInvis = true;
            invisButtonInfo.UpdateStateText();
            Console.WriteLine("Player Invisible: " + !ninjaVisibility);
        }

        void SaveGameSlot_Load(On.SaveGameSlot.orig_Load orig, SaveGameSlot self) {
            orig(self);
            shouldSetInvis = true;
        }


        void PlayerController_OnUpdate(PlayerController controller) {
            if (shouldSetInvis) {
                if (ninjaVisibility) {
                    controller.Show();
                } else {
                    controller.Hide();
                }
                shouldSetInvis = false;
            }
        }

    }
}
