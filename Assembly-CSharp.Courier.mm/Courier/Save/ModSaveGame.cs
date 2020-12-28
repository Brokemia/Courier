using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Mod.Courier.Save {
    [Serializable]
    public class ModSaveGame {
        public static ModSaveGame Instance { set; get; }

        public List<SaveGameSlot> modSaveSlots = new List<SaveGameSlot>();

        public OptionPair[] Options;

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
            if (Options == null) return;
            foreach (OptionPair option in Options) {
                for (int i = 0; i < Courier.UI.OptionButtons.Count; i++) {
                    if (!string.IsNullOrEmpty(Courier.UI.OptionButtons[i].SaveMethod.optionKey) && Courier.UI.OptionButtons[i].SaveMethod.optionKey.Equals(option.optionKey)) {
                        Courier.UI.OptionButtons[i].SaveMethod.Load(option.optionValue);
                    }
                }

                for (int i = 0; i < Courier.UI.ModOptionButtons.Count; i++) {
                    if (!string.IsNullOrEmpty(Courier.UI.ModOptionButtons[i].SaveMethod.optionKey) && Courier.UI.ModOptionButtons[i].SaveMethod.optionKey.Equals(option.optionKey)) {
                        Courier.UI.ModOptionButtons[i].SaveMethod.Load(option.optionValue);
                    }
                }

                foreach (OptionSaveMethod saveMethod in Courier.ModOptionSaveData) {
                    if (!string.IsNullOrEmpty(saveMethod.optionKey) && saveMethod.optionKey.Equals(option.optionKey)) {
                        saveMethod.Load(option.optionValue);
                    }
                }
            }
        }

        public void UpdateOptionsData() {
            List<OptionPair> allOptions = new List<OptionPair>();

            for(int i = 0; i < Courier.UI.OptionButtons.Count; i++) {
                string val = Courier.UI.OptionButtons[i].SaveMethod.Save();

                if (!string.IsNullOrEmpty(Courier.UI.OptionButtons[i].SaveMethod.optionKey) && !string.IsNullOrEmpty(val)) {
                    allOptions.Add(new OptionPair { optionKey = Courier.UI.OptionButtons[i].SaveMethod.optionKey, optionValue = val });
                }
            }

            for (int i = 0; i < Courier.UI.ModOptionButtons.Count; i++) {
                string val = Courier.UI.ModOptionButtons[i].SaveMethod.Save();

                if (!string.IsNullOrEmpty(Courier.UI.ModOptionButtons[i].SaveMethod.optionKey) && !string.IsNullOrEmpty(val)) {
                    allOptions.Add(new OptionPair { optionKey = Courier.UI.ModOptionButtons[i].SaveMethod.optionKey, optionValue = val });
                }
            }

            foreach (OptionSaveMethod saveMethod in Courier.ModOptionSaveData) {
                string val = saveMethod.Save();

                if (!string.IsNullOrEmpty(saveMethod.optionKey) && !string.IsNullOrEmpty(val)) {
                    allOptions.Add(new OptionPair { optionKey = saveMethod.optionKey, optionValue = val });
                }
            }

            Options = allOptions.ToArray();
        }
    }

    [Serializable]
    public class OptionPair {
        [SerializeField]
        public string optionKey;

        [SerializeField]
        public string optionValue;
    }
}
