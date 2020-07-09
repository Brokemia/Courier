using System;
using System.IO;
using System.Reflection;
using Mod.Courier.Helpers;
using Mod.Courier.UI;
using UnityEngine;

namespace Mod.Courier.Save {
    public static class SaveLoadJSON {
        public static string SAVE_TO_JSON_BUTTON_LOC_ID = "COURIER_SAVE_TO_JSON_BUTTON";
        public static string LOAD_FROM_JSON_BUTTON_LOC_ID = "COURIER_LOAD_FROM_JSON_BUTTON";

        public static OptionsButtonInfo SaveToJSONButton;
        public static OptionsButtonInfo LoadFromJSONButton;

        private static MethodInfo OnSaveGameLoadedInfo = typeof(SaveManager).GetMethod("OnSaveGameLoaded", ReflectionHelper.NonPublicInstance);

        public static void RegisterModOptions() {
            SaveToJSONButton = Courier.UI.RegisterSubMenuModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(SAVE_TO_JSON_BUTTON_LOC_ID), SaveToJSON);
            LoadFromJSONButton = Courier.UI.RegisterSubMenuModOptionButton(() => Manager<LocalizationManager>.Instance.GetText(LOAD_FROM_JSON_BUTTON_LOC_ID), LoadFromJSON);
        }

        public static void SaveToJSON() {
            string json = Manager<SaveManager>.Instance.SaveFile.GetJson();
            string path = Application.persistentDataPath + "/SaveGame.json";
            try {
                using (StreamWriter streamWriter = new StreamWriter(path, false)) {
                    streamWriter.WriteLine(json);
                    streamWriter.Close();
                }
            } catch (Exception) {
                CourierLogger.Log("SaveLoadJSON::Save", "An error ocurred while saving the file, retry");
                try {
                    File.Delete(path);
                    using (StreamWriter streamWriter2 = new StreamWriter(path, false)) {
                        streamWriter2.WriteLine(json);
                        streamWriter2.Close();
                    }
                } catch (Exception) {
                    CourierLogger.Log("SaveLoadJSON::Save", "Retry failed, continue without saving");
                }
            }
        }

        public static void LoadFromJSON() {
            SaveGame saveGame = null;
            try {
                string text = File.ReadAllText(Application.persistentDataPath + "/SaveGame.json");
                if (string.IsNullOrEmpty(text)) {
                    throw new Exception("SaveLoadJSON::Load : Save file is empty.");
                }
                // I don't know why this line is here, but I should probably leave it in
                text = text.Replace("\u008c\u008b", string.Empty);
                saveGame = JsonUtility.FromJson<SaveGame>(text);
                if(saveGame != null) {
                    CourierLogger.Log("SaveLoadJSON::Load", "Save game successfully loaded from JSON");
                    OnSaveGameLoadedInfo.Invoke(Manager<SaveManager>.Instance, new object[] { saveGame });
                } else {
                    CourierLogger.Log("SaveLoadJSON::Load", "Save game failed to be loaded from JSON");
                }
            } catch (Exception e) {
                // No need to try and load from the registry
                CourierLogger.Log("SaveLoadJSON::Load", "Error while reading from JSON");
                e.LogDetailed("SaveLoadJSON::Load");
            }
        }
    }
}
