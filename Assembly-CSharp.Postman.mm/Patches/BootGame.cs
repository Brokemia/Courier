#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.IO;
using System.Reflection;
using Mod.Postman;

public class patch_BootGame : BootGame {
    private extern void orig_Start();
    private void Start() {

        Postman.Boot();
        Postman.Instance.LoadAssemblyMods();

        orig_Start();
    }
}