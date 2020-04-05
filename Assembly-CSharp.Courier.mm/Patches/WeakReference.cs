#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it
#pragma warning disable CS0649

using System;
using Mod.Courier;
using MonoMod;

public class patch_WeakReference<T> : WeakReference<T> where T : UnityEngine.Object {
    [MonoModIgnore]
    private string path;

    public extern T orig_get_Object();

    // If nothing is found in Resources, try loading from a mod AssetBundle
    public T get_Object() {
        return orig_get_Object() ?? Courier.LoadFromAssetBundles<T>(path + ".asset");
    }
}