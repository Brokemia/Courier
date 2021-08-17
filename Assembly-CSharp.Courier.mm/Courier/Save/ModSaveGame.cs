using AdvancedInspector;
using Mod.Courier.Module;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Mod.Courier.Save {
    [Serializable]
    public class ModSaveGame {
        public static ModSaveGame Instance { set; get; }

        public List<SaveGameSlot> modSaveSlots = new List<SaveGameSlot>();

        // Key is module class full name
        [SerializeField]
        public StringByString ModSaves;

        [NonSerialized]
        public int currentModSaveSlotIndex = -1;

        static ModSaveGame() {
            Instance = new ModSaveGame();
        }

        public SaveGameSlot GetCurrentModSaveSlot() {
            return modSaveSlots[currentModSaveSlotIndex];
        }

        public string GetJson() {
            // Temporarily remove empty mod save slots
            List<SaveGameSlot> allSlots = new List<SaveGameSlot>(modSaveSlots);
            modSaveSlots.RemoveAll((slot) => slot == null || slot.IsEmpty());

            // Update mod save options
            UpdateOptionsData();

            string json = JsonUtility.ToJson(this);
            modSaveSlots = allSlots;
            return json;
        }

        public void LoadOptions() {
            if (ModSaves == null) return;
            foreach(CourierModuleMetadata meta in Courier.Mods) {
                foreach(CourierModule module in meta.Modules) {
                    if (module.ModuleSaveType != null) {
                        if (ModSaves.ContainsKey(module.GetType().FullName) && !string.IsNullOrEmpty(ModSaves[module.GetType().FullName])) {
                            module.ModuleSave = (CourierModSave)JsonUtility.FromJson(ModSaves[module.GetType().FullName], module.ModuleSaveType);
                        } else {
                            module.ModuleSave = (CourierModSave)module.ModuleSaveType.GetConstructor(new Type[] { }).Invoke(null);
                        }
                        module.SaveLoaded();
                    }
                }
            }
        }

        public void UpdateOptionsData() {
            if(ModSaves == null) {
                ModSaves = new StringByString();
            }

            foreach (CourierModuleMetadata meta in Courier.Mods) {
                foreach (CourierModule module in meta.Modules) {
                    if (module.ModuleSaveType != null && module.ModuleSave != null) {
                        ModSaves[module.GetType().FullName] = JsonUtility.ToJson(module.ModuleSave);
                    }
                }
            }
        }
    }
}
