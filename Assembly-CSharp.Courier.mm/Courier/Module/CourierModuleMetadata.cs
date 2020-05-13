using System;
using System.Collections.Generic;
using Ionic.Zip;
using Tommy;
using UnityEngine;

namespace Mod.Courier.Module {
    // Adapted from Everest's EverestModuleMetadata
    public class CourierModuleMetadata {
        protected string _VersionString;

        public virtual string Name {
            get;
            set;
        }

        public virtual Version Version {
            get;
            set;
        } = new Version(1, 0);

        public virtual string VersionString {
            get {
                return _VersionString;
            }
            set {
                _VersionString = value;
                int num = value.IndexOf('-');
                if (num == -1) {
                    Version = new Version(value);
                } else {
                    Version = new Version(value.Substring(0, num));
                }
            }
        }

        public virtual List<CourierModuleMetadata> Dependencies {
            get;
            set;
        } = new List<CourierModuleMetadata>();

        public virtual List<CourierLevelSet> LevelSets {
            get;
            set;
        } = new List<CourierLevelSet>();

        public virtual List<CourierModule> Modules {
            get;
            set;
        } = new List<CourierModule>();

        public virtual List<AssetBundle> AssetBundles {
            get;
            set;
        } = new List<AssetBundle>();

        public virtual bool ZippedMod {
            get;
            set;
        }

        public virtual bool DirectoryMod {
            get;
            set;
        }

        /***
         * null if !DirectoryMod
         */
        public virtual string DirectoryPath {
            get;
            set;
        } = null;

        /***
         * null if !ZippedMod
         */
        public virtual ZipFile ZipFile {
            get;
            set;
        } = null;

        public static CourierModuleMetadata FromTOML(TomlTable table) {
            CourierModuleMetadata result = new CourierModuleMetadata {
                Name = table["name"].AsString.Value,
                VersionString = table["version"].AsString.Value
            };

            if (table.HasKey("dependencies")) {
                TomlArray deps = table["dependencies"].AsArray;
                foreach (TomlNode depNode in deps) {
                    result.Dependencies.Add(new CourierModuleMetadata { Name = depNode["name"].AsString.Value, VersionString = depNode["version"].AsString.Value });
                }
            }

            if (table.HasKey("level-sets")) {
                TomlArray levelSets = table["level-sets"].AsArray;
                foreach (TomlNode levelSet in levelSets) {
                    result.LevelSets.Add(new CourierLevelSet { StartingScene = levelSet["starting-scene"].AsString.Value, NameLocID = levelSet["name-loc-ID"].AsString.Value });
                }
            }
            
            return result;
        }
    }
}
