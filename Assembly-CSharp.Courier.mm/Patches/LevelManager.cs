#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections;

public class patch_LevelManager : LevelManager {
    /// <summary>
    /// Exists for the purpose of keeping track of which modded level has been loaded for rich presence.
    /// Probably shouldn't be used for anything important.
    /// </summary>
    public string lastLevelLoaded;

    public extern void orig_LoadLevel(LevelLoadingInfo levelLoadingInfo);
    public new void LoadLevel(LevelLoadingInfo levelLoadingInfo) {
        lastLevelLoaded = levelLoadingInfo.levelName;
        orig_LoadLevel(levelLoadingInfo);
    }

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