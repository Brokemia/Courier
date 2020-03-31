using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ionic.Zip;
using Mod.Courier.GFX;
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

        public static List<AssetBundle> AssetBundles = new List<AssetBundle>();

        private static Dictionary<string, Sprite> embeddedSprites;

        private static bool spriteParamsSetup;

        public static LogWriter logWriter;

        public static Dictionary<string, Sprite> EmbeddedSprites {
            get {
                if (!spriteParamsSetup) SetupCourierSpriteParams();
                return embeddedSprites ?? (embeddedSprites = ResourceHelper.GetEmbeddedSprites());
            }
        }

        public static void DumpHierarchy(Transform transform) {
            CourierLogger.Log("DumpHierarchy", "Dumping hierarchy for: " + transform.name);
            DumpHierarchy(transform, 0);
        }

        private static void DumpHierarchy(Transform transform, int level) {
            string spaces = string.Empty;
            for(int i = 0; i < level; i++) {
                spaces += "   ";
            }
            CourierLogger.Log("DumpHierarchy", spaces + transform.name);
            foreach(Transform t in transform.GetChildren()) {
                DumpHierarchy(t, level + 1);
            }
        }

        public static void Boot() {
            if (File.Exists("log.txt"))
                File.Delete("Log.txt");

            Stream fileStream = new FileStream("log.txt", FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
            StreamWriter fileWriter = new StreamWriter(fileStream, Console.OutputEncoding);
            logWriter = new LogWriter {
                STDOUT = Console.Out,
                File = fileWriter
            };
            Debug.unityLogger.logHandler = new UnityLogHandler();
            Console.SetOut(logWriter);
            AppDomain currentDomain = AppDomain.CurrentDomain;
            currentDomain.UnhandledException += UnhandledExceptionHandler;

            UI.SetupModdedUI();
        }

        static void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e) {
            (e?.ExceptionObject as Exception ?? new Exception("Unknown unhandled exception")).LogDetailed("UNHANDLED");
        }


        public static void Quit() {
            Console.SetOut(logWriter.STDOUT);
            logWriter.STDOUT = null;
        }

        public static void LoadMods() {
            if (Loaded) return;

            LoadAssetBundleMods();
            LoadAssemblyMods();

            Loaded = true;
        }

        public static void LoadAssetBundleMods() {
            if (Loaded) return;

            // Create the Mods folder if it doesn't exist
            if (!Directory.Exists(ModsFolder)) {
                Directory.CreateDirectory(ModsFolder);
            }

            string[] mods = Directory.GetDirectories(ModsFolder);

            // Loop through unzipped mods
            foreach (string mod in mods) {
                if (Directory.Exists(Path.Combine(mod, "AssetBundles"))) {
                    string[] assetBundles = Directory.GetFiles(Path.Combine(mod, "AssetBundles"));

                    // Check files in subfolders
                    foreach (string path in assetBundles) {
                        // Assume it's an assetbundle if there's an associated .manifest file
                        if (File.Exists(path + ".manifest")) {
                            CourierLogger.Log("ModLoader", "Loading AssetBundle from " + path);
                            try {
                                LoadAssetBundle(File.OpenRead(path));
                            } catch (Exception e) {
                                CourierLogger.Log(LogType.Error, "ModLoader", "Exception while loading AssetBundle from: " + path);
                                e.LogDetailed();
                            }
                        }
                    }
                }
            }

            IEnumerable<string> zippedMods = Directory.GetFiles(ModsFolder).Where((s) => s.EndsWith(".zip", StringComparison.InvariantCulture));
            foreach (string mod in zippedMods) {
                using (ZipFile zip = new ZipFile(mod)) {
                    ZipEntry assetBundleFolder = zip["AssetBundles"];
                    if (assetBundleFolder != null && assetBundleFolder.IsDirectory) {
                        foreach (ZipEntry entry in zip) {
                            if (entry.FileName.EndsWith(".manifest", StringComparison.InvariantCulture) && entry.FileName.Replace('\\', '/').StartsWith("AssetBundle/", StringComparison.InvariantCulture)) {
                                ZipEntry assetBundle = zip[entry.FileName.Substring(0, entry.FileName.Length - ".manifest".Length)];
                                CourierLogger.Log("ModLoader", "Loading zipped AssetBundle from " + Path.Combine(mod, assetBundle.FileName));
                                try {
                                    LoadAssetBundle(assetBundle.OpenReader());
                                } catch (Exception e) {
                                    CourierLogger.Log(LogType.Error, "ModLoader", "Exception while loading zipped AssetBundle from: " + Path.Combine(mod, assetBundle.FileName));
                                    e.LogDetailed();
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void LoadAssetBundle(Stream stream) {
            AssetBundle ab = AssetBundle.LoadFromStream(stream);
            foreach(string name in ab.GetAllAssetNames()) {
                CourierLogger.Log("AssetBundle", name);
            }
            AssetBundles.Add(ab);
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
                        CourierLogger.Log("ModLoader", "Loading assembly from " + path);
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
                            CourierLogger.Log("ModLoader", "Loading zipped assembly from " + Path.Combine(CacheFolder, entry.FileName));
                            LoadDLL(Path.Combine(CacheFolder, entry.FileName));
                        }
                    }
                }
            }
        }

        public static void LoadDLL(string path) {
            try {
                Assembly asm = Assembly.LoadFrom(path);

                IEnumerable<Type> modules = FindDerivedTypes(asm, typeof(CourierModule));

                foreach (Type moduleType in modules) {
                    object o = asm.CreateInstance(moduleType.FullName, false, BindingFlags.ExactBinding, null, new object[] { }, null, null);
                    (o as CourierModule).Load();
                    Modules.Add(o as CourierModule);
                }
            } catch(Exception e) {
                CourierLogger.Log(LogType.Error, "ModLoader", "Exception while loading assembly from: " + path);
                CourierLogger.LogDetailed(e);
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

        public static T LoadFromAssetBundles<T>(string path) where T : UnityEngine.Object {
            foreach(AssetBundle ab in AssetBundles) {
                T asset = ab.LoadAsset<T>(path);
                if(asset != null) {
                    return asset;
                }
            }
            return null;
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
