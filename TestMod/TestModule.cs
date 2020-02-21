using System;
using Mod.Postman;
using Mod.Postman.Module;

namespace TestMod {
    public class TestModule : PostmanModule {
        public override void Load() {
            Postman.Events.PlayerController.OnUpdate += PlayerController_OnUpdate;
        }


        void PlayerController_OnUpdate(PlayerController controller) {
            controller.GiveAirJump();
        }

    }
}
