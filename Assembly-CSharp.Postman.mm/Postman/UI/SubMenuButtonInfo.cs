﻿using System;
using UnityEngine.Events;

namespace Mod.Postman.UI {
    public class SubMenuButtonInfo : OptionsButtonInfo {
        public SubMenuButtonInfo(string text, UnityAction onClick) : base(text, onClick) {

        }

        public override string GetStateText() {
            return "";
        }

        public override void UpdateStateText() {}
    }
}
