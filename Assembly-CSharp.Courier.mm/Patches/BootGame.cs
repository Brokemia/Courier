﻿#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it


using System;
using Mod.Courier;

public class patch_BootGame : BootGame {
    private extern void orig_Start();
    private void Start() {
        Courier.InitMods();

        orig_Start();
    }

    // This might need to be moved somewhere more global
    void OnApplicationQuit() {
        Courier.Quit();
    }
}