using System;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public class SpriteParams {
        public float pixelsPerUnit = 100;
        public uint extrude = 1;
        public SpriteMeshType meshType = SpriteMeshType.FullRect;
        public Vector4 border = new Vector4();
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
    }
}
