using MonoMod.Utils;
using System;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public class CourierDimensionPortalShaderAdder : MonoBehaviour {
        public DimensionPortal portal;

        void Awake() {
            portal.distortionExpandingMaterial.shader = Shader.Find("DimensionZone/DistortionExpanding");
        }
    }
}
