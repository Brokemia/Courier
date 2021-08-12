using System;
using System.Collections.Generic;
using Mod.Courier.Save;

namespace Mod.Courier.Module {
    public class CourierLevelSet {
        public CourierLevelSet() {
            // Give a unique ID to each level set
            ID = IDCount;
            IDCount++;
        }

        public SaveGameSlot AddSlot() {
            SlotID = ModSaveGame.Instance.modSaveSlots.Count;
            Slot = new SaveGameSlot();
            Slot.Clear(true);
            ModSaveGame.Instance.modSaveSlots.Add(Slot);
            return Slot;
        }

        public virtual string StartingScene {
            get;
            set;
        }

        public virtual string NameLocID {
            get;
            set;
        }

        public virtual List<int> StartingInventory {
            get;
            set;
        }

        public virtual EBits StartingBits {
            get;
            set;
        }

        public static int IDCount {
            get;
            private set;
        }

        public int SlotID {
            get;
            set;
        }

        public SaveGameSlot Slot {
            get;
            private set;
        }

        public int ID {
            get;
            private set;
        }
    }
}
