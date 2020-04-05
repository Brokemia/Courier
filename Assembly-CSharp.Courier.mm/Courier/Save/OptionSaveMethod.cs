using System;
namespace Mod.Courier.Save {
    public class OptionSaveMethod {
        public string optionKey;

        public virtual string Save() {
            return null;
        }

        public virtual void Load(string load) {

        }
    }
}
