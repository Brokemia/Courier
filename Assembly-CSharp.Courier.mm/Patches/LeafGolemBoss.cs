#pragma warning disable CS0169
#pragma warning disable CS0414

using System;
using MonoMod;

public class patch_LeafGolemBoss : LeafGolemBoss {
    [MonoModIgnore]
    public new event Action onDie;

    [MonoModIgnore]
    public new event Action onThrow;

    [MonoModIgnore]
    public new event Action onReceiveHit;

    [MonoModIgnore]
    public new event Action onSpawnInDone;

    [MonoModIgnore]
    public new event Action onIdleAnimStart;

    public void ClearOnDieHandlers() {
        onDie = null;
    }

    public void ClearOnThrowHandlers() {
        onThrow = null;
    }

    public void ClearOnReceiveHitHandlers() {
        onReceiveHit = null;
    }

    public void ClearOnSpawnInDoneHandlers() {
        onSpawnInDone = null;
    }

    public void ClearOnIdleAnimStartHandlers() {
        onIdleAnimStart = null;
    }
}
