using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Ionic.Zip;
using Mod.Courier.GFX;
using Mod.Courier.Helpers;
using Mod.Courier.Module;
using Mod.Courier.Save;
using Tommy;
using UnityEngine;

namespace Mod.Courier {
    public static partial class Courier {

        public static Version CourierVersion = new Version(0, 6, 10);

        public static string CourierVersionString = "Courier v" + CourierVersion + "-alpha";

        public static string CourierAssemblyLocation = typeof(Courier).Assembly.Location;

        public static string ModsFolder = Path.Combine(CourierAssemblyLocation.Substring(0, CourierAssemblyLocation.Length - typeof(patch_BootGame).Assembly.GetName().Name.Length - 4), "../../Mods").Replace('\\', '/');

        public static string CacheFolder = Path.Combine(ModsFolder, "Cache");

        public static bool Loaded { get; private set; }

        public static List<CourierModuleMetadata> Mods = new List<CourierModuleMetadata>();

        public static List<CourierModuleMetadata> DeferredMods = new List<CourierModuleMetadata>();

        /// <summary>
        /// All the save data that isn't linked to a button.
        /// </summary>
        public static List<OptionSaveMethod> ModOptionSaveData = new List<OptionSaveMethod>();

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

        public static bool DependencyLoaded(CourierModuleMetadata dep) {
            foreach(CourierModuleMetadata modMeta in Mods) {
                if(modMeta.Name.Equals(dep.Name) && VersionSatisfiesDependency(dep.Version, modMeta.Version)) {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks if the given version number is "compatible" with the one required as a dependency.
        /// 
        /// Source: https://github.com/EverestAPI/Everest/blob/master/Celeste.Mod.mm/Mod/Everest/Everest.Loader.cs
        /// </summary>
        /// <param name="requiredVersion">The version required by a mod in their dependencies</param>
        /// <param name="installedVersion">The version to check for</param>
        /// <returns>true if the versions number are compatible, false otherwise.</returns>
        public static bool VersionSatisfiesDependency(Version requiredVersion, Version installedVersion) {
            // Special case: Always true if version == 0.0.*
            if (installedVersion.Major == 0 && installedVersion.Minor == 0)
                return true;

            // Major version, breaking changes, must match.
            if (installedVersion.Major != requiredVersion.Major)
                return false;
            // Minor version, non-breaking changes, installed can't be lower than what we depend on.
            if (installedVersion.Minor < requiredVersion.Minor)
                return false;

            // "Build" is "PATCH" in semver, but we'll also check for it and "Revision".
            if (installedVersion.Minor == requiredVersion.Minor && installedVersion.Build < requiredVersion.Build)
                return false;
            if (installedVersion.Minor == requiredVersion.Minor && installedVersion.Build == requiredVersion.Build && installedVersion.Revision < requiredVersion.Revision)
                return false;

            return true;
        }

        public static void LoadMods() {
            if (Loaded) return;

            // Add a CourierModuleMetadata to represent Courier itself
            Mods.Add(new CourierModuleMetadata {
                Name = "Courier",
                VersionString = CourierVersion.ToString()
            });

            // Create the Mods folder if it doesn't exist
            if (!Directory.Exists(ModsFolder)) {
                Directory.CreateDirectory(ModsFolder);
            }

            // Delete the contents of the Cache folder
            if (Directory.Exists(CacheFolder)) {
                foreach (string file in Directory.GetFiles(CacheFolder)) {
                    File.Delete(file);
                }
            }

            string[] mods = Directory.GetDirectories(ModsFolder);

            // Get mods into the deferred list to start a queue to be loaded

            // Loop through unzipped mods
            foreach (string mod in mods) {
                // Find all valid mods, defer them to get them in the queue
                if (File.Exists(Path.Combine(mod, "courier.toml"))) {
                    try {
                        CourierModuleMetadata modMeta = CourierModuleMetadata.FromTOML(TOML.Parse(new StreamReader(File.OpenRead(Path.Combine(mod, "courier.toml")))));

                        modMeta.ZippedMod = false;
                        modMeta.DirectoryMod = true;
                        modMeta.DirectoryPath = mod;

                        DeferredMods.Add(modMeta);
                    } catch(Exception e) {
                        e.LogDetailed("ModLoader");
                    }
                }
            }

            // Loop through zipped mods
            IEnumerable<string> zippedMods = Directory.GetFiles(ModsFolder).Where((s) => s.EndsWith(".zip", StringComparison.InvariantCulture));
            foreach (string mod in zippedMods) {
                using (ZipFile zip = new ZipFile(mod)) {
                    ZipEntry toml = zip["courier.toml"];
                    if (toml != null && !toml.IsDirectory) {
                        try {
                            CourierModuleMetadata modMeta = CourierModuleMetadata.FromTOML(TOML.Parse(new StreamReader(toml.OpenReader())));

                            modMeta.ZippedMod = true;
                            modMeta.DirectoryMod = false;
                            modMeta.ZipFile = zip;

                            DeferredMods.Add(modMeta);
                        } catch(Exception e) {
                            e.LogDetailed("ModLoader");
                        }
                    }
                }
            }

            CourierLogger.Log("ModLoader", "Located " + DeferredMods.Count + " mods containing a courier.toml");

            // Keep loading until no new mods are loaded on a pass
            // That means no new dependencies are available to be fulfilled
            List<CourierModuleMetadata> loadedMods = new List<CourierModuleMetadata>();
            do {
                loadedMods.Clear();
                foreach(CourierModuleMetadata modMeta in DeferredMods) {
                    bool depsFulfilled = true;
                    foreach (CourierModuleMetadata dep in modMeta.Dependencies) {
                        if(!DependencyLoaded(dep)) {
                            depsFulfilled = false;
                            break;
                        }
                    }
                    if (!depsFulfilled) {
                        continue;
                    }

                    CourierLogger.Log("ModLoader", "All dependencies fulfilled for " + modMeta.Name + " " + modMeta.VersionString);

                    try {
                        // All dependencies are filled, commence loading
                        if (modMeta.ZippedMod) {
                            LoadZippedMod(modMeta);
                        } else if(modMeta.DirectoryMod) {
                            LoadDirectoryMod(modMeta);
                        }

                        Mods.Add(modMeta);
                        CourierLogger.Log("ModLoader", "Successfully loaded " + modMeta.Name + " " + modMeta.VersionString);
                    } catch (Exception e) {
                        CourierLogger.Log("ModLoader", "An exception ocurred while loading " + modMeta.Name + " " + modMeta.VersionString);
                        e.LogDetailed("ModLoader");
                    }
                    loadedMods.Add(modMeta);
                }

                foreach(CourierModuleMetadata loadedMod in loadedMods) {
                    DeferredMods.Remove(loadedMod);
                }
            } while (loadedMods.Count > 0);

            foreach(CourierModuleMetadata failed in DeferredMods) {
                CourierLogger.Log("ModLoader", "Failed to load " + failed.Name + " " + failed.VersionString + " due to missing dependencies");
            }

            // The remaining deferred mods have no need to stick around
            DeferredMods.Clear();

            Loaded = true;
        }

        private static void LoadDirectoryMod(CourierModuleMetadata modMeta) {
            if (Loaded || modMeta.ZippedMod || !modMeta.DirectoryMod || modMeta.DirectoryPath == null) return;
            string[] modFiles = Directory.GetFiles(modMeta.DirectoryPath);

            // Load AssetBundles
            if (Directory.Exists(Path.Combine(modMeta.DirectoryPath, "AssetBundles"))) {
                string[] assetBundles = Directory.GetFiles(Path.Combine(modMeta.DirectoryPath, "AssetBundles"));

                // Check files in subfolders
                foreach (string path in assetBundles) {
                    // Assume it's an assetbundle if there's an associated .manifest file
                    if (File.Exists(path + ".manifest")) {
                        CourierLogger.Log("ModLoader", "Loading AssetBundle from: " + path);
                        try {
                            modMeta.AssetBundles.Add(LoadAssetBundle(File.OpenRead(path)));
                        } catch (Exception e) {
                            CourierLogger.Log(LogType.Error, "ModLoader", "Exception while loading AssetBundle from: " + path);
                            e.LogDetailed();
                        }
                    }
                }
            }

            foreach (string path in modFiles) {
                if (path.EndsWith(".dll", StringComparison.InvariantCulture)) {
                    CourierLogger.Log("ModLoader", "Loading assembly from " + path);
                    List<CourierModule> modulesLoaded = LoadDLL(path);
                    foreach(CourierModule module in modulesLoaded) {
                        modMeta.Modules.Add(module);
                    }
                }
            }
        }

        private static void LoadZippedMod(CourierModuleMetadata modMeta) {
            if (Loaded || !modMeta.ZippedMod || modMeta.DirectoryMod || modMeta.ZipFile == null) return;

            // Load AssetBundles
            foreach (ZipEntry entry in modMeta.ZipFile) {
                // Console.WriteLine(entry.FileName);
                if (entry.FileName.EndsWith(".manifest", StringComparison.InvariantCulture) && entry.FileName.Replace('\\', '/').StartsWith("AssetBundles/", StringComparison.InvariantCulture)) {
                    ZipEntry assetBundle = modMeta.ZipFile[entry.FileName.Substring(0, entry.FileName.Length - ".manifest".Length)];
                    CourierLogger.Log("ModLoader", "Loading zipped AssetBundle from " + Path.Combine(modMeta.ZipFile.Name, assetBundle.FileName));
                    try {
                        MemoryStream assetBundleStream = new MemoryStream();
                        assetBundle.Extract(assetBundleStream);
                        modMeta.AssetBundles.Add(LoadAssetBundle(assetBundleStream));
                    } catch (Exception e) {
                        CourierLogger.Log(LogType.Error, "ModLoader", "Exception while loading zipped AssetBundle from: " + Path.Combine(modMeta.ZipFile.Name, assetBundle.FileName));
                        e.LogDetailed();
                    }
                }
            }

            foreach (ZipEntry entry in modMeta.ZipFile) {
                if (entry.FileName.EndsWith(".dll", StringComparison.InvariantCulture)) {
                    // Create the Cache folder if it doesn't exist
                    if (!Directory.Exists(CacheFolder)) {
                        Directory.CreateDirectory(CacheFolder);
                    }

                    entry.Extract(CacheFolder);
                    CourierLogger.Log("ModLoader", "Loading zipped assembly from " + Path.Combine(CacheFolder, entry.FileName));
                    List<CourierModule> modulesLoaded = LoadDLL(Path.Combine(CacheFolder, entry.FileName));
                    foreach (CourierModule module in modulesLoaded) {
                        modMeta.Modules.Add(module);
                    }
                }
            }
        }

        public static AssetBundle LoadAssetBundle(Stream stream) {
            return AssetBundle.LoadFromStream(stream);
        }

        /// <summary>
        /// Loads the assembly at the path listed and calls the Load() method on any CourierModules in it
        /// </summary>
        /// <returns>A list of modules loaded</returns>
        /// <param name="path">The path to the DLL to load</param>
        public static List<CourierModule> LoadDLL(string path) {
            List<CourierModule> modulesLoaded = new List<CourierModule>();
            try {
                Assembly asm = Assembly.LoadFrom(path);

                IEnumerable<Type> modules = FindDerivedTypes(asm, typeof(CourierModule));

                foreach (Type moduleType in modules) {
                    CourierLogger.Log("ModLoader", "Loading module class " + moduleType);
                    object o = asm.CreateInstance(moduleType.FullName, false, BindingFlags.ExactBinding, null, new object[] { }, null, null);
                    (o as CourierModule).Load();
                    modulesLoaded.Add(o as CourierModule);
                }
            } catch(Exception e) {
                CourierLogger.Log(LogType.Error, "ModLoader", "Exception while loading assembly from: " + path);
                e.LogDetailed("ModLoader");
            }

            return modulesLoaded;
        }

        public static void InitMods() {
            foreach (CourierModuleMetadata modMeta in Mods) {
                foreach (CourierModule module in modMeta.Modules) {
                    module.Initialize();
                }
            }
        }

        internal static void SetupCourierSpriteParams() {
            spriteParamsSetup = true;
            ResourceHelper.SpriteConfig["Mod.Courier.UI.mod_options_frame"] = new SpriteParams { pixelsPerUnit = 20, border = new Vector4(15, 15, 15, 15)};
        }

        internal static void AddCourierLocalization(string languageID) {
            patch_LocalizationManager locManager = ((patch_LocalizationManager)Manager<LocalizationManager>.Instance);

            // Defaults to English
            switch (languageID) {
                default:
                    locManager.textByLocID[UI.MOD_OPTIONS_BUTTON_LOC_ID] = "Third Party Mod Options";
                    locManager.textByLocID[UI.MOD_OPTIONS_MENU_TITLE_LOC_ID] = "Courier Mod Menu - Third Party Content";
                    locManager.textByLocID[SaveLoadJSON.SAVE_TO_JSON_BUTTON_LOC_ID] = "Save as JSON";
                    locManager.textByLocID[SaveLoadJSON.LOAD_FROM_JSON_BUTTON_LOC_ID] = "Load from JSON";
                    locManager.textByLocID[patch_TitleScreen.PLAY_MODS_BUTTON_LOC_ID] = "Play Mod Levels";
                    break;
            }
        }

        public static T LoadFromAssetBundles<T>(string path) where T : UnityEngine.Object {
            foreach (CourierModuleMetadata modMeta in Mods) {
                foreach (AssetBundle ab in modMeta.AssetBundles) {
                    if (!ab.isStreamedSceneAssetBundle) {
                        T asset = ab.LoadAsset<T>(path);
                        if (asset != null) {
                            return asset;
                        }
                    }
                }
            }
            return null;
        }

        public static void AddLevelSetSlots() {
            foreach (CourierModuleMetadata modMeta in Mods) {
                foreach (CourierLevelSet levelSet in modMeta.LevelSets) {
                    levelSet.AddSlot();
                }
            }
        }

        public static CourierLevelSet FindLevelSetWithID(int id) {
            foreach(CourierModuleMetadata modMeta in Mods) {
                foreach(CourierLevelSet levelSet in modMeta.LevelSets) {
                    if (levelSet.ID == id) return levelSet;
                }
            }

            return null;
        }

        public static CourierLevelSet FindLevelSetWithSlotID(int slotId) {
            foreach (CourierModuleMetadata modMeta in Mods) {
                foreach (CourierLevelSet levelSet in modMeta.LevelSets) {
                    if (levelSet.SlotID == slotId) return levelSet;
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
