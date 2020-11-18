using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public class CourierShaderAdder : MonoBehaviour {

        public Renderer renderer;

        public string desiredShader;

        void Awake() {
            foreach (Material m in renderer.materials) {
                m.shader = Shader.Find(desiredShader);
            }
        }

        void Start() {
            foreach (Material m in renderer.materials) {
                m.shader = Shader.Find(desiredShader);
            }
        }
    }
}