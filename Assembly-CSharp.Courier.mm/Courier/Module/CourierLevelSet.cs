using System;
namespace Mod.Courier.Module {
    public class CourierLevelSet {
        public CourierLevelSet() {
            // Give a unique ID to each level set
            SlotID = SlotIDCount;
            SlotIDCount++;
        }

        public virtual string StartingScene {
            get;
            set;
        }

        public virtual string NameLocID {
            get;
            set;
        }

        public static int SlotIDCount {
            get;
            private set;
        }

        public int SlotID {
            get;
            private set;
        }
    }
}
