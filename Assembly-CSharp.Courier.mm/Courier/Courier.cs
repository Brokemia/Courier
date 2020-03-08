using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ionic.Zip;
using Mod.Courier.Helpers;
using Mod.Courier.Module;
using UnityEngine;

namespace Mod.Courier {
    public static partial class Courier {

        public static string CourierAssemblyLocation = typeof(Courier).Assembly.Location;

        public static string ModsFolder = Path.Combine(CourierAssemblyLocation.Substring(0, CourierAssemblyLocation.Length - typeof(patch_BootGame).Assembly.GetName().Name.Length - 4), "../../Mods").Replace('\\', '/');

        public static string CacheFolder = Path.Combine(ModsFolder, "Cache");

        public static bool Loaded { get; private set; }

        public static List<CourierModule> Modules = new List<CourierModule>();

        private static Dictionary<string, Sprite> embeddedSprites;

        private static bool spriteParamsSetup;

        public static Dictionary<string, Sprite> EmbeddedSprites {
            get {
                if (!spriteParamsSetup) SetupCourierSpriteParams();
                return embeddedSprites ?? (embeddedSprites = ResourceHelper.GetSprites());
            }
        }

        public static void DumpHierarchy(Transform transform) {
            Console.WriteLine("Dumping hierarchy for: " + transform.name);
            DumpHierarchy(transform, 0);
        }

        private static void DumpHierarchy(Transform transform, int level) {
            string spaces = string.Empty;
            for(int i = 0; i < level; i++) {
                spaces += "   ";
            }
            Console.WriteLine(spaces + transform.name);
            foreach(Transform t in transform.GetChildren()) {
                DumpHierarchy(t, level + 1);
            }
        }

        public static void Boot() {
            UI.SetupModdedUI();
        }

        public static void LoadAssemblyMods() {
            if (Loaded) return;

            // Create the Mods folder if it doesn't exist
            if (!Directory.Exists(ModsFolder)) {
                Directory.CreateDirectory(ModsFolder);
            }

            // Delete the contents of the Cache folder
            if(Directory.Exists(CacheFolder)) {
                foreach (string file in Directory.GetFiles(CacheFolder)) {
                    File.Delete(file);
                }
            }

            string[] mods = Directory.GetDirectories(ModsFolder);

            foreach (string mod in mods) {
                string[] modFiles = Directory.GetFiles(mod);
                // Check files in subfolders
                foreach (string path in modFiles) {
                    if (path.EndsWith(".dll", StringComparison.InvariantCulture)) {
                        Console.WriteLine("Loading assembly from " + path);
                        LoadDLL(path);
                    }
                }
            }

            IEnumerable<string> zippedMods = Directory.GetFiles(ModsFolder).Where((s) => s.EndsWith(".zip", StringComparison.InvariantCulture));

            foreach (string mod in zippedMods) {
                using(ZipFile zip = new ZipFile(mod)) {
                    foreach (ZipEntry entry in zip) {
                        if(entry.FileName.EndsWith(".dll", StringComparison.InvariantCulture)) {
                            // Create the Cache folder if it doesn't exist
                            if (!Directory.Exists(CacheFolder)) {
                                Directory.CreateDirectory(CacheFolder);
                            }

                            entry.Extract(CacheFolder);
                            Console.WriteLine("Loading zipped assembly from " + Path.Combine(CacheFolder, entry.FileName));
                            LoadDLL(Path.Combine(CacheFolder, entry.FileName));
                        }
                    }
                }
            }

            Loaded = true;
        }

        public static void LoadDLL(string path) {
            Assembly asm = Assembly.LoadFrom(path);

            IEnumerable<Type> modules = FindDerivedTypes(asm, typeof(CourierModule));

            foreach (Type moduleType in modules) {
                object o = asm.CreateInstance(moduleType.FullName, false, BindingFlags.ExactBinding, null, new object[] { }, null, null);
                (o as CourierModule).Load();
                Modules.Add(o as CourierModule);
            }
        }

        internal static void SetupCourierSpriteParams() {
            spriteParamsSetup = true;
            ResourceHelper.SpriteConfig["Mod.Courier.UI.mod_options_frame"] = new SpriteParams { pixelsPerUnit = 20, border = new Vector4(15, 15, 15, 15)};
        }

        internal static void AddCourierLocalization(string languageID) {
            // Defaults to English
            switch (languageID) {
                default:
                    ((patch_LocalizationManager)Manager<LocalizationManager>.Instance).textByLocID[UI.MOD_OPTIONS_BUTTON_LOC_ID] = "Third Party Mod Options";
                    ((patch_LocalizationManager)Manager<LocalizationManager>.Instance).textByLocID[UI.MOD_OPTIONS_MENU_TITLE_LOC_ID] = "Courier Mod Menu - Third Party Content";
                    break;
            }
        }

        public static IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType) {
            List<Type> derived = new List<Type>();
            foreach(Type t in assembly.GetTypes()) {
                if(baseType.IsAssignableFrom(t)) {
                    derived.Add(t);
                }
            }
            return derived;
        }
    }
}
