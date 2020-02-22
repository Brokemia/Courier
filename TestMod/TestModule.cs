using System;
using Mod.Postman;
using Mod.Postman.Module;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TestMod {
    public class TestModule : PostmanModule {

        GameObject invisOption;
        bool playerInvis;
        OptionScreen optionScreen;

        public override void Load() {
            Postman.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
            On.OptionScreen.Init += OptionScreen_Init;
        }

        void OptionScreen_Init(On.OptionScreen.orig_Init orig, OptionScreen self, IViewParams screenParams) {
            optionScreen = self;
            invisOption = UnityEngine.Object.Instantiate(self.fullscreenOption, self.backgroundFrame);
            //UnityEngine.Object.Destroy(invisOption.GetComponentInChildren<Button>());
            Button button = invisOption.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(OnInvis);
            foreach(Component c in invisOption.GetComponentsInChildren<Component>()) {
                Console.WriteLine("\n\n\n\n\n\n\n" + c.name + " " + c.GetType() + " " + c.gameObject.name + " " + c.gameObject.GetType());
            }
            orig(self, screenParams);
        }

        void OnInvis() {
            playerInvis = !playerInvis;
            //optionScreen.optionsChanged = true;
            Console.WriteLine("Player Invisible: " + playerInvis);
        }

        void PlayerController_OnUpdate(PlayerController controller) {
            if(playerInvis) {
                controller.Hide();
            } else {
                controller.Show();
            }
        }

    }
}
