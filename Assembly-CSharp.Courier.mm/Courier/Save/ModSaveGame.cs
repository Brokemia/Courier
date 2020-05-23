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
            return JsonUtility.ToJson(this);
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
            }
        }

        public void UpdateOptionsData() {
            OptionPair[] allOptions = new OptionPair[Courier.UI.OptionButtons.Count + Courier.UI.ModOptionButtons.Count];
            int numSavableOptions = 0;

            for(int i = 0; i < Courier.UI.OptionButtons.Count; i++) {
                string val = Courier.UI.OptionButtons[i].SaveMethod.Save();

                if (!string.IsNullOrEmpty(Courier.UI.OptionButtons[i].SaveMethod.optionKey) && !string.IsNullOrEmpty(val)) {
                    allOptions[i] = new OptionPair { optionKey = Courier.UI.OptionButtons[i].SaveMethod.optionKey, optionValue = val };
                    numSavableOptions++;
                }
            }

            for (int i = 0; i < Courier.UI.ModOptionButtons.Count; i++) {
                string val = Courier.UI.ModOptionButtons[i].SaveMethod.Save();

                if (!string.IsNullOrEmpty(Courier.UI.ModOptionButtons[i].SaveMethod.optionKey) && !string.IsNullOrEmpty(val)) {
                    allOptions[Courier.UI.OptionButtons.Count + i] = new OptionPair { optionKey = Courier.UI.ModOptionButtons[i].SaveMethod.optionKey, optionValue = val };
                    numSavableOptions++;
                }
            }

            Options = new OptionPair[numSavableOptions];
            for(int i = 0, option = 0; i < allOptions.Length; i++) {
                if(allOptions[i] != null) {
                    Options[option] = allOptions[i];
                    option++;
                }
            }
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
