#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Courier;

public class patch_PlayerController : PlayerController{
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
}

