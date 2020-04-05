#pragma warning disable CS0169

using System;
using MonoMod;

public class patch_NecromancerBoss : NecromancerBoss {
    [MonoModPublic]
    private StateMachine stateMachine;
}
