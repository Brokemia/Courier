#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections.Generic;
using Mod.Courier;
using Mod.Courier.UI;
using MonoMod;
using UnityEngine;

public class patch_UIManager : UIManager {
    [MonoModIgnore]
    private Dictionary<Type, List<GameObject>> preloadedScreens;

    public List<GameObject> GetPreloadedViews<T>() {
        Type typeFromHandle = typeof(T);
        return GetPreloadedViews(typeFromHandle);
    }

    public List<GameObject> GetPreloadedViews(Type t) {
        return preloadedScreens[t];
    }
}