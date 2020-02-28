using System;
using MonoMod;

public class patch_BaseColossus : BaseColossus {
    [MonoModPublic]
    private StateMachine stateMachine;
}
