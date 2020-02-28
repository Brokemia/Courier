#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Courier;
using MonoMod;

public class patch_PlayerController : PlayerController {
    [MonoModIgnore]
    private float runSpeedMultiplier;

    private extern void orig_Update();
    private void Update() {
        orig_Update();
        Courier.Events.PlayerController.Update(this);
    }

    public extern void orig_UpdatePhysics();
    public new void UpdatePhysics() {
        orig_UpdatePhysics();
        Courier.Events.PlayerController.UpdatePhysics(this);
    }

    public void SetRunSpeedMultiplier(float speed) {
        runSpeedMultiplier = speed;
    }
}

