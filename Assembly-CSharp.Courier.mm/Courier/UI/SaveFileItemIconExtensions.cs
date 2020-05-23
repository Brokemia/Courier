using System;
using Mod.Courier.Save;

namespace Mod.Courier.UI {
    public static class SaveFileItemIconExtensions {
        public static void SetState(this SaveFileItemIcon self, SaveGameSlot saveSlot) {
            if (saveSlot.Items.ContainsKey(self.item)) {
                self.icon.enabled = saveSlot.Items[self.item] > 0;
                self.icon.texture = Manager<InventoryManager>.Instance.GetItemDefinition(self.item).itemIcon;
            } else {
                self.icon.enabled = false;
            }
            self.lockedFrame.enabled = self.icon.enabled && saveSlot.LockedItems.Contains(self.item);
        }
    }
}
