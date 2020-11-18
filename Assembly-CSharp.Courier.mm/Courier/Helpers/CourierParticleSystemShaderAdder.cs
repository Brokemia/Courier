using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mod.Courier.Helpers {
    public class CourierParticleSystemShaderAdder : MonoBehaviour {

        public string desiredShader;

        void Awake() {
            GetComponent<ParticleSystemRenderer>().material.shader = Shader.Find(desiredShader);
        }
    }
}