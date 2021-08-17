using Mod.Courier.Save;
using System;
namespace Mod.Courier.Module {
    public class CourierModule {

        public virtual Type ModuleSaveType { get; }

        public CourierModSave ModuleSave { set; get; }

        // Called in InitialScene, before anything happens
        public virtual void Load() {

        }

        // Called in BootGame, after Managers are created
        public virtual void Initialize() {

        }

        // Called immediately after loading save data for this module, if any save data exists
        public virtual void SaveLoaded() {

        }

    }
}
