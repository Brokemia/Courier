#pragma warning disable CS0114 // Member hides inherited member; missing override keyword
#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Ionic.Crc;
using Ionic.Zip;
using Mod.Courier;
using Mod.Courier.Module;

public class patch_CreditScreen : CreditScreen {
    private List<CreditItemData> toAppend;

    protected extern void orig_LoadCredits();
    protected virtual void LoadCredits() {
        orig_LoadCredits();

        toAppend = new List<CreditItemData>();
        
        foreach (CourierModuleMetadata modMeta in Courier.Mods) {
            if (modMeta.DirectoryMod) {
                string[] modFiles = Directory.GetFiles(modMeta.DirectoryPath);
                // Check files in subfolders
                foreach (string path in modFiles) {
                    if (path.EndsWith(".tsv", StringComparison.InvariantCulture) && Path.GetFileName(path).Contains("Credits")) {
                        List<CreditItemData> credits = LoadCreditsFromStream(File.OpenRead(path));
                        if (Path.GetFileNameWithoutExtension(path).Equals(creditFile)) {
                            creditItems.Clear();
                            creditItems.AddRange(credits);
                        } else if (!Path.GetFileName(path).Equals("Credits.tsv") && !Path.GetFileName(path).Equals("Credits_PP.tsv")) {
                            // Put appended credits away for later so they don't get overwritten
                            toAppend.AddRange(credits);
                        }
                        CourierLogger.Log("CreditScreen", "Loading credits file from " + path);
                    }
                }
            } else if(modMeta.ZippedMod) {
                foreach (ZipEntry entry in modMeta.ZipFile) {
                    if (entry.FileName.EndsWith(".tsv", StringComparison.InvariantCulture) && entry.FileName.Contains("Credits")) {
                        CrcCalculatorStream stream = entry.OpenReader();
                        CourierLogger.Log("CreditScreen", "Loading zipped credits file from " + Path.Combine(modMeta.ZipFile.Name, entry.FileName));
                        List<CreditItemData> credits = LoadCreditsFromStream(stream);
                        if (Path.GetFileNameWithoutExtension(entry.FileName).Equals(creditFile)) {
                            creditItems.Clear();
                            creditItems.AddRange(credits);
                        } else if (!entry.FileName.Equals("Credits.tsv") && !entry.FileName.Equals("Credits_PP.tsv")) {
                            // Put appended credits away for later so they don't get overwritten
                            toAppend.AddRange(credits);
                        }
                    }
                }
            }
        }

        creditItems.AddRange(toAppend);
        toAppend = null;
    }

    private List<CreditItemData> LoadCreditsFromStream(Stream stream) {
        List<CreditItemData> credits = new List<CreditItemData>();
        StreamReader streamReader = new StreamReader(stream);
        string text = streamReader.ReadLine();
        string[] array = text.Split('\t');
        int num = 0;
        int num2 = 0;
        for (int num3 = array.Length - 1; num3 >= 0; num3--) {
            if (array[num3] == "CREDIT_ITEM") {
                num = num3;
            } else if (array[num3] == "TYPE") {
                num2 = num3;
            }
        }
        while (!streamReader.EndOfStream) {
            string[] array2 = streamReader.ReadLine().Split('\t');
            CreditItemData item = new CreditItemData(array2[num], array2[num2]);
            credits.Add(item);
        }
        streamReader.Close();
        return credits;
    }
}
