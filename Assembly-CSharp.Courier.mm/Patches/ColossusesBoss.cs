using System;
using MonoMod;

public class patch_ColossusesBoss : ColossusesBoss {
    [MonoModPublic]
    private StateMachine stateMachine;
}
