using System;
using System.Reflection;
using Mod.Postman;
using Mod.Postman.Helpers;
using Mod.Postman.Module;
using Mod.Postman.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaInvis {
    public class NinjaInvisModule : PostmanModule {
        
        bool shouldSetInvis;
        ToggleButtonInfo invisButtonInfo;

        FieldInfo optionsChangedInfo = typeof(OptionScreen).GetField("optionsChanged", ReflectionHelper.NonPublicInstanceFieldSet);

        public override void Load() {
            Postman.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
            On.SaveGameSlot.Load += SaveGameSlot_Load;
            invisButtonInfo = Postman.UI.RegisterToggleModOptionButton("Player Visibility", OnInvis);
            invisButtonInfo.state = true;
        }

        void OnInvis() {
            invisButtonInfo.state = !invisButtonInfo.state;
            shouldSetInvis = true;
            invisButtonInfo.UpdateStateText();
            Console.WriteLine("Player Invisible: " + !invisButtonInfo.state);
        }

        void SaveGameSlot_Load(On.SaveGameSlot.orig_Load orig, SaveGameSlot self) {
            orig(self);
            shouldSetInvis = true;
        }


        void PlayerController_OnUpdate(PlayerController controller) {
            if (shouldSetInvis) {
                if (invisButtonInfo.state) {
                    controller.Show();
                } else {
                    controller.Hide();
                }
                shouldSetInvis = false;
            }
        }

    }
}
