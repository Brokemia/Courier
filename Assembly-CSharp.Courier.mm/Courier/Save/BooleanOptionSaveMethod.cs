using System;
namespace Mod.Courier.Save {
    public class BooleanOptionSaveMethod : OptionSaveMethod {
        public Func<bool> GetBooleanValue;
        public Action<bool> SetBooleanValue;

        public BooleanOptionSaveMethod(string optionKey, Func<bool> GetBooleanValue, Action<bool> SetBooleanValue) {
            this.optionKey = optionKey;
            this.GetBooleanValue = GetBooleanValue;
            this.SetBooleanValue = SetBooleanValue;
        }

        public override string Save() {
            return GetBooleanValue?.Invoke().ToString();
        }

        public override void Load(string load) {
            if (bool.TryParse(load, out bool res)) {
                SetBooleanValue?.Invoke(res);
            }
        }
    }
}
