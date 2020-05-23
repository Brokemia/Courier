#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.IO;
using Mod.Courier;
using Mod.Courier.Save;
using UnityEngine;

public class patch_SaveLoadStandalone : SaveLoadStandalone {
    public extern void orig_Save(SaveGame saveGame);
    public override void Save(SaveGame saveGame) {
        string json = ModSaveGame.Instance.GetJson();
        PlayerPrefs.SetString("ModSave", json);
        string path = Application.persistentDataPath + "/ModSave.json";
        try {
            using (StreamWriter streamWriter = new StreamWriter(path, false)) {
                streamWriter.WriteLine(json);
                streamWriter.Close();
            }
        } catch (Exception) {
            CourierLogger.Log("Mod Options Save", "An error occured while saving the file, retry.");
            try {
                File.Delete(path);
                using (StreamWriter streamWriter2 = new StreamWriter(path, false)) {
                    streamWriter2.WriteLine(json);
                    streamWriter2.Close();
                }
            } catch (Exception) {
                CourierLogger.Log("Mod Options Save", "Retry Failed, continue without saving.");
            }
        }
        orig_Save(saveGame);
    }

    public extern void orig_Load();
    public override void Load() {
        LoadModOptions();
        orig_Load();
    }

    public static void LoadModOptions() {
        ModSaveGame moddedSave = null;
        string text = string.Empty;
        try {
            text = File.ReadAllText(Application.persistentDataPath + "/ModSave.json");
            if (string.IsNullOrEmpty(text)) {
                throw new Exception("Modded SaveLoadStandalone::Load : Modded save file is empty.");
            }
            text = text.Replace("\u008c\u008b", string.Empty);
            moddedSave = JsonUtility.FromJson<ModSaveGame>(text);
        } catch (Exception) {
            try {
                if (PlayerPrefs.HasKey("ModSave")) {
                    text = PlayerPrefs.GetString("ModSave");
                }
                moddedSave = JsonUtility.FromJson<ModSaveGame>(text);
            } catch (Exception e) {
                CourierLogger.Log(LogType.Exception, "Mod Options Load", "Error while reading modded save from the registry.");
                e.LogDetailed("Mod Options Load");
                moddedSave = null;
            }
        }
        if (moddedSave != null) {
            ModSaveGame.Instance = moddedSave;
            SaveGameSlot slot = new SaveGameSlot();
            slot.Clear(true);
            moddedSave.modSaveSlots.Add(slot);
        }
        ModSaveGame.Instance.LoadOptions();
    }
}