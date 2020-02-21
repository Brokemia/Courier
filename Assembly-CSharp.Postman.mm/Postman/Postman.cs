using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Mod.Postman.Helpers;
using Mod.Postman.Module;

namespace Mod.Postman {
    public class Postman {

        public static string PostmanAssemblyLocation = typeof(Postman).Assembly.Location;

        public static string ModsFolder = Path.Combine(PostmanAssemblyLocation.Substring(0, PostmanAssemblyLocation.Length - typeof(patch_BootGame).Assembly.GetName().Name.Length - 4), "../../Mods").Replace('\\', '/');

        public bool Loaded { get; private set; }

        public static Postman Instance { get; private set; }

        public List<PostmanModule> Modules = new List<PostmanModule>();

        public Postman() {
            Instance = this;
        }

        public static void Boot() {
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
            new Postman();
#pragma warning restore RECS0026 // Possible unassigned object created by 'new'
        }

        public void LoadAssemblyMods() {
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

        public IEnumerable<Type> FindDerivedTypes(Assembly assembly, Type baseType) {
            return assembly.GetTypes().Where(baseType.IsAssignableFrom);
        }
    }
}
