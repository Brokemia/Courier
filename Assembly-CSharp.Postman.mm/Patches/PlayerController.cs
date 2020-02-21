#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using Mod.Postman;

public class patch_PlayerController : PlayerController{
    private extern void orig_Update();
    private void Update() {
        orig_Update();
        Postman.Events.PlayerController.Update(this);
    }

    public extern void orig_UpdatePhysics();
    public new void UpdatePhysics() {
        orig_UpdatePhysics();
        Postman.Events.PlayerController.UpdatePhysics(this);
    }
}

