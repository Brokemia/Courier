#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Crc;
using Ionic.Zip;
using Mod.Courier;

public class patch_LocalizationManager : LocalizationManager {
    public Dictionary<string, string> textByLocID;

    private extern void orig_LoadGeneralLoc(string languageID);
    private void LoadGeneralLoc(string languageID) {
        orig_LoadGeneralLoc(languageID);
        // Create the Mods folder if it doesn't exist
        if (!Directory.Exists(Courier.ModsFolder)) {
            Directory.CreateDirectory(Courier.ModsFolder);
        }

        string[] mods = Directory.GetDirectories(Courier.ModsFolder);

        foreach (string mod in mods) {
            string[] modFiles = Directory.GetFiles(mod);
            // Check files in subfolders
            foreach (string path in modFiles) {
                if (path.EndsWith(".tsv", StringComparison.InvariantCulture) && !Path.GetFileName(path).Contains("Dialog")) {
                    Console.WriteLine("Loading localization file from " + path);
                    LoadGeneralLocFromStream(languageID, File.OpenRead(path));
                }
            }
        }

        IEnumerable<string> zippedMods = Directory.GetFiles(Courier.ModsFolder).Where((s) => s.EndsWith(".zip", StringComparison.InvariantCulture));

        foreach (string mod in zippedMods) {
            using (ZipFile zip = new ZipFile(mod)) {
                foreach (ZipEntry entry in zip) {
                    if (entry.FileName.EndsWith(".tsv", StringComparison.InvariantCulture) && !entry.FileName.Contains("Dialog")) {
                        CrcCalculatorStream stream = entry.OpenReader();
                        Console.WriteLine("Loading zipped localization file from " + Path.Combine(mod, entry.FileName));
                        LoadGeneralLocFromStream(languageID, stream);
                    }
                }
            }
        }

        Courier.AddCourierLocalization(languageID);
    }

    private void LoadGeneralLocFromStream(string languageID, Stream stream) {
        StreamReader streamReader = new StreamReader(stream);
        string[] headings = streamReader.ReadLine().Split('\t');
        int langColumnIndex = -1;
        for (int i = headings.Length - 1; i >= 0; i--) {
            if (headings[i] == languageID) {
                langColumnIndex = i;
                break;
            }
            // Set it to English if a language hasn't been found yet
            if (headings[i] == ELanguage.EN.ToString() && langColumnIndex == -1)
                langColumnIndex = i;
        }
        if (langColumnIndex == -1) {
            return;
        }
        while (!streamReader.EndOfStream) {
            string[] line = streamReader.ReadLine().Split('\t');
            if (line.Length == 0)
                continue;
            string key = line[0];
            string text = line[langColumnIndex];
            text = text.Replace("\\n", "\n");
            textByLocID[key] = text;
        }
        streamReader.Close();
    }
}