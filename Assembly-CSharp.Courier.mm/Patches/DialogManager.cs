#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Crc;
using Ionic.Zip;
using Mod.Courier;
using MonoMod;
using UnityEngine;

public class patch_DialogManager : DialogManager {
    [MonoModIgnore]
    private Dictionary<string, List<DialogInfo>> dialogByLocID;

    [MonoModIgnore]
    private extern void CreateDialogInfo(string currentDialogName, string[] lineItems, int languageIndex, int skippableIndex, int avatarIndex, int boxPositionIndex, int autoCloseIndex, int autoCloseDelayIndex, int locIdIndex, int forcedPortraitOrientationIndex);

    [MonoModIgnore]
    private extern string GetActualDialogID(string locId);

    private extern void orig_LoadTSVDialogs(string languageID);
    private void LoadTSVDialogs(string languageID) {
        orig_LoadTSVDialogs(languageID);
        // Create the Mods folder if it doesn't exist
        if (!Directory.Exists(Courier.ModsFolder)) {
            Directory.CreateDirectory(Courier.ModsFolder);
        }

        string[] mods = Directory.GetDirectories(Courier.ModsFolder);

        foreach (string mod in mods) {
            string[] modFiles = Directory.GetFiles(mod);
            // Check files in subfolders
            foreach (string path in modFiles) {
                if (path.EndsWith(".tsv", StringComparison.InvariantCulture) && Path.GetFileName(path).Contains("Dialog")) {
                    Console.WriteLine("Loading dialog localization file from " + path);
                    LoadTSVDialogsFromStream(languageID, File.OpenRead(path));
                }
            }
        }

        IEnumerable<string> zippedMods = Directory.GetFiles(Courier.ModsFolder).Where((s) => s.EndsWith(".zip", StringComparison.InvariantCulture));

        foreach (string mod in zippedMods) {
            using (ZipFile zip = new ZipFile(mod)) {
                foreach (ZipEntry entry in zip) {
                    if (entry.FileName.EndsWith(".tsv", StringComparison.InvariantCulture) && entry.FileName.Contains("Dialog")) {
                        CrcCalculatorStream stream = entry.OpenReader();
                        Console.WriteLine("Loading zipped dialog localization file from " + Path.Combine(mod, entry.FileName));
                        LoadTSVDialogsFromStream(languageID, stream);
                    }
                }
            }
        }
    }

    private void LoadTSVDialogsFromStream(string languageID, Stream stream) {
        StreamReader streamReader = new StreamReader(stream);
        string[] headings = streamReader.ReadLine().Split('\t');
        // The index of the column the language we're reading is in
        int langColumnIndex = -1;
        int skippableIndex = -1;
        int avatarIndex = -1;
        int boxPositionIndex = -1;
        int autoCloseIndex = -1;
        int autoCloseDelayIndex = -1;
        int forcedPortraitOrientationIndex = -1;
        int locIDIndex = -1;
        for (int i = headings.Length - 1; i >= 0; i--) {
            if (headings[i] == languageID) {
                langColumnIndex = i;
            } else if (headings[i] == "LOC_ID") {
                locIDIndex = i;
            } else if (headings[i] == "SKIPPABLE") {
                skippableIndex = i;
            } else if (headings[i] == "AVATAR") {
                avatarIndex = i;
            } else if (headings[i] == "BOX_POSITION") {
                boxPositionIndex = i;
            } else if (headings[i] == "AUTO_CLOSE") {
                autoCloseIndex = i;
            } else if (headings[i] == "AUTO_CLOSE_DELAY") {
                autoCloseDelayIndex = i;
            } else if (headings[i] == "FORCED_PORTRAIT_ORIENTATION") {
                forcedPortraitOrientationIndex = i;
            }
        }
        // If we couldn't find the specified language
        if (langColumnIndex == -1) {
            return;
        }
        bool duringConversation = false;
        // The loc ID representing the whole conversation
        // Basically the starting loc ID without _BEGIN at the end
        string conversationLocID = string.Empty;
        while (!streamReader.EndOfStream) {
            string[] entry = streamReader.ReadLine().Split('\t');
            string locID = entry[locIDIndex];
            if (locID == string.Empty) {
                continue;
            }
            if (!duringConversation) {
                conversationLocID = locID;
                if (locID.Length > 6) {
                    if (locID.Substring(locID.Length - 6) == "_BEGIN") {
                        duringConversation = true;
                        conversationLocID = locID.Substring(0, locID.Length - 6);
                    }
                }
                if (!dialogByLocID.ContainsKey(conversationLocID) || dialogByLocID[conversationLocID] == null) {
                    dialogByLocID[conversationLocID] = new List<DialogInfo>();
                }
                CreateDialogInfo(conversationLocID, entry, langColumnIndex, skippableIndex, avatarIndex, boxPositionIndex, autoCloseIndex, autoCloseDelayIndex, locIDIndex, forcedPortraitOrientationIndex);
                continue;
            }
            string actualDialogID = GetActualDialogID(locID);
            if (actualDialogID != conversationLocID) {
                if (!dialogByLocID.ContainsKey(locID)) {
                    dialogByLocID[locID] = new List<DialogInfo>();
                }
                CreateDialogInfo(locID, entry, langColumnIndex, skippableIndex, avatarIndex, boxPositionIndex, autoCloseIndex, autoCloseDelayIndex, locIDIndex, forcedPortraitOrientationIndex);
                continue;
            }
            if (locID.Length > 4 && locID.Substring(locID.Length - 4) == "_END") {
                duringConversation = false;
            }
            CreateDialogInfo(conversationLocID, entry, langColumnIndex, skippableIndex, avatarIndex, boxPositionIndex, autoCloseIndex, autoCloseDelayIndex, locIDIndex, forcedPortraitOrientationIndex);
        }
        streamReader.Close();
    }


}