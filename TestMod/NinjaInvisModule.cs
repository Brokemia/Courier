using System;
using System.Reflection;
using Mod.Postman;
using Mod.Postman.Helpers;
using Mod.Postman.Module;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NinjaInvis {
    public class NinjaInvisModule : PostmanModule {

        GameObject invisOption;
        TextMeshProUGUI invisOptionStateText;
        TextMeshProUGUI invisOptionNameText;

        bool playerInvis;
        bool shouldSetInvis;
        OptionScreen optionScreen;

        FieldInfo optionsChangedInfo = typeof(OptionScreen).GetField("optionsChanged", ReflectionHelper.NonPublicInstanceFieldSet);

        public override void Load() {
            Postman.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
            On.OptionScreen.Init += OptionScreen_Init;
            On.OptionScreen.InitOptions += OptionScreen_InitOptions;
            On.OptionScreen.HideUnavailableOptions += OptionScreen_HideUnavailableOptions;
            On.OptionScreen.LateUpdate += OptionScreen_LateUpdate;
        }

        void OptionScreen_Init(On.OptionScreen.orig_Init orig, OptionScreen self, IViewParams screenParams) {
            optionScreen = self;
            invisOption = UnityEngine.Object.Instantiate(self.fullscreenOption, self.backgroundFrame);
            foreach (TextMeshProUGUI text in invisOption.GetComponentsInChildren<TextMeshProUGUI>()) {
                if (text.name.Equals("OptionState"))
                    invisOptionStateText = text;
                if (text.name.Equals("OptionName"))
                    invisOptionNameText = text;
            }
            Button button = invisOption.GetComponentInChildren<Button>();
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(OnInvis);
            orig(self, screenParams);
        }

        void OptionScreen_InitOptions(On.OptionScreen.orig_InitOptions orig, OptionScreen self) {
            orig(self);
            invisOptionStateText.text = ((!playerInvis) ? Manager<LocalizationManager>.Instance.GetText(self.offLocID) : Manager<LocalizationManager>.Instance.GetText(self.onLocID));
        }
        
        void OptionScreen_LateUpdate(On.OptionScreen.orig_LateUpdate orig, OptionScreen self) {
            orig(self);
            invisOptionNameText.text = "Player Invisibility"; // TODO Patch LoadGeneralLoc to load custom language files
            // TODO Find an earlier place to set this. Currently, it flickers briefly before putting itself in the right spot
            invisOption.transform.position = optionScreen.controlsButton.transform.position - new Vector3(20.2f, .9f);
            foreach (GameObject obj in UnityEngine.Object.FindObjectsOfType<GameObject>()) {
                if (obj.activeInHierarchy && obj.name.Equals("Back")) {
                    obj.transform.position = invisOption.transform.position + new Vector3(10.4f, -.9f);
                }
            }
        }


        void OptionScreen_HideUnavailableOptions(On.OptionScreen.orig_HideUnavailableOptions orig, OptionScreen self) {
            orig(self);
            optionScreen.backgroundFrame.sizeDelta = new Vector2(optionScreen.backgroundFrame.sizeDelta.x, optionScreen.backgroundFrame.sizeDelta.y + optionScreen.heightPerButton);
        }


        void OnInvis() {
            playerInvis = !playerInvis;
            shouldSetInvis = true;
            optionsChangedInfo.SetValue(optionScreen, true);
            invisOptionStateText.text = ((!playerInvis) ? Manager<LocalizationManager>.Instance.GetText(optionScreen.offLocID) : Manager<LocalizationManager>.Instance.GetText(optionScreen.onLocID));
            Console.WriteLine("Player Invisible: " + playerInvis);
        }

        void PlayerController_OnUpdate(PlayerController controller) {
            if (shouldSetInvis) {
                if (playerInvis) {
                    controller.Hide();
                } else {
                    controller.Show();
                }
                shouldSetInvis = false;
            }
        }

    }
}
