using System;

using _PlayerController = PlayerController;

namespace Mod.Courier {
    public partial class Courier {
        public static class Events {
        
            public static class PlayerController {
                public static event Action<_PlayerController> OnUpdate;
                internal static void Update(_PlayerController playerController)
                    => OnUpdate?.Invoke(playerController);

                public static event Action<_PlayerController> OnUpdatePhysics;
                internal static void UpdatePhysics(_PlayerController playerController)
                    => OnUpdatePhysics?.Invoke(playerController);
            }

        }
    }
}
