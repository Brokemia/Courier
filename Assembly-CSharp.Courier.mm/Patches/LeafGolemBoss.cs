using System;
using MonoMod;

public class patch_LeafGolemBoss : LeafGolemBoss {
    [MonoModPublic]
    private StateMachine stateMachine;
}
