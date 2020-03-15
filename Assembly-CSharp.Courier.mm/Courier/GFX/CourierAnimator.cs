using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mod.Courier.GFX {
    public class CourierAnimator : MonoBehaviour {
        public float progress;
        public AnimationFrame[] frames;
        public SpriteRenderer renderer;
        public Image image;

        public void Update() {
            if (renderer != null)
                renderer.sprite = frames[(int)progress].sprite;
            if (image != null)
                image.sprite = frames[(int)progress].sprite;
        }
    }
}
