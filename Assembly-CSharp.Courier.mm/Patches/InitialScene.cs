﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Courier;

public class patch_InitialScene : InitialScene {
    private extern void orig_Start();
    private void Start() {
        Courier.Boot();
        Courier.LoadMods();

        orig_Start();
    }
}