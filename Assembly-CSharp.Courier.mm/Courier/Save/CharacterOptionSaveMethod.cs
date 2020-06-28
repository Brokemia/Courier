using System;
namespace Mod.Courier.Save {
    public class CharacterOptionSaveMethod : OptionSaveMethod {
        public Func<char> GetCharacterValue;
        public Action<char> SetCharacterValue;

        public CharacterOptionSaveMethod(string optionKey, Func<char> GetCharacterValue, Action<char> SetCharacterValue) {
            this.optionKey = optionKey;
            this.GetCharacterValue = GetCharacterValue;
            this.SetCharacterValue = SetCharacterValue;
        }

        public override string Save() {
            return GetCharacterValue?.Invoke().ToString();
        }

        public override void Load(string load) {
            // Uses the first character in the loaded string
            if (!string.IsNullOrEmpty(load)) {
                SetCharacterValue?.Invoke(load[0]);
            }
        }
    }
}
