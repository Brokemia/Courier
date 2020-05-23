#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections;

public class patch_LevelManager : LevelManager {
    public extern ELevel orig_GetLevelEnumFromLevelName(string levelName);
    public new ELevel GetLevelEnumFromLevelName(string levelName) {
        ELevel result = orig_GetLevelEnumFromLevelName(levelName);
        // Level_TestSclout will represent modded levels
        if (result == ELevel.NONE && ((patch_SaveManager)Manager<SaveManager>.Instance).GetSaveGameSlotIndex() == 3) {
            return ELevel.Level_TestSclout;
        }
        return result;
    }
}