using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mod.Postman.Helpers;
using Mod.Postman.Module;

namespace Mod.Postman {
    public static partial class Postman {

        public static string PostmanAssemblyLocation = typeof(Postman).Assembly.Location;

        public static string ModsFolder = Path.Combine(PostmanAssemblyLocation.Substring(0, PostmanAssemblyLocation.Length - typeof(patch_BootGame).Assembly.GetName().Name.Length - 4), "../../Mods").Replace('\\', '/');

        public static bool Loaded { get; private set; }
        
        public static List<PostmanModule> Modules = new List<PostmanModule>();

        public static void Boot() {
        }

        public static void LoadAssemblyMods() {
            if (Loaded) return;

            // Create the Mods folder if it doesn't exist
            if (!Directory.Exists(ModsFolder)) {
                Directory.CreateDirectory(ModsFolder);
            }

            string[] mods = Directory.GetFiles(ModsFolder);

            foreach (string path in mods) {
                if (path.EndsWith(".dll", StringComparison.InvariantCulture)) {
                    Assembly asm = Assembly.LoadFrom(path);

                    IEnumerable<Type> modules = FindDerivedTypes(asm, typeof(PostmanModule));

                    foreach(Type moduleType in modules) {
                        object o = asm.CreateInstance(moduleType.FullName, false, BindingFlags.ExactBinding, null, new Object[] { }, null, null);
                        (o as PostmanModule).Load();
                        Modules.Add(o as PostmanModule);
                    }
                }
            }

            Loaded = true;
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
