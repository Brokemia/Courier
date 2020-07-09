using System;
using Mod.Courier.RichPresence;
using MonoMod;

public class patch_RichPresenceManager : Manager<RichPresenceManager> {
    [MonoModIgnore]
    private IRichPresence richPresence;

    protected override void Awake() {
        base.Awake();
        richPresence = new RichPresenceCourier();
        richPresence.Init();
        richPresence.SetMainMenuRichPresence();
    }
}