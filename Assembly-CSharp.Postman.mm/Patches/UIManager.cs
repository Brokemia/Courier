using System;
using System.Collections.Generic;
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