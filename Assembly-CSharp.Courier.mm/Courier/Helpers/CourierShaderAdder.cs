using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public class CourierShaderAdder : MonoBehaviour {

        public Renderer renderer;

        public string desiredShader;

        void Awake() {
            renderer.material.shader = Shader.Find(desiredShader);
        }
    }
}