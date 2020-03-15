using System;
using UnityEngine;
using UnityEngine.UI;

namespace Mod.Courier.GFX {
    public static class AnimationHelper {
        public static Sprite[] CreateSpritesFromSheet(Texture2D tex, int frameCount, SpriteSheetOrientation orientation = SpriteSheetOrientation.Horizontal) {
            Sprite[] sprites = new Sprite[frameCount];
            for(int i = 0; i < frameCount; i++) {
                if(orientation == SpriteSheetOrientation.Horizontal)
                    sprites[i] = Sprite.Create(tex, new Rect(i * (tex.width / frameCount), 0, tex.width / frameCount, tex.height), new Vector2(.5f, .5f));
                else
                    sprites[i] = Sprite.Create(tex, new Rect(0, i * (tex.height / frameCount), tex.width, tex.height / frameCount), new Vector2(.5f, .5f));
            }

            return sprites;
        }

        // If no times are provided, defaults to 30 FPS
        public static AnimationFrame[] CreateAnimationFramesFromSpriteSheet(Texture2D tex, int frameCount, SpriteSheetOrientation orientation = SpriteSheetOrientation.Horizontal, params float[] times) {
            AnimationFrame[] frames = new AnimationFrame[frameCount];
            Sprite[] sprites = CreateSpritesFromSheet(tex, frameCount, orientation);
            for(int i = 0; i < frameCount; i++) {
                frames[i] = new AnimationFrame { sprite = sprites[i], time = i < times.Length ? times[i] : 1 / 30f };
            }

            return frames;
        }

        public static Animation AddAnimation(GameObject g, WrapMode wrapMode, SpriteRenderer renderer, params AnimationFrame[] frames) {
            Animation animation = g.AddComponent<Animation>();
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;
            AnimationCurve curve = new AnimationCurve();
            float totalTime = 0;
            for (int i = 0; i < frames.Length; i++) {
                curve.AddKey(totalTime, i);
                totalTime += frames[i].time;
            }
            CourierAnimator animator = g.AddComponent<CourierAnimator>();
            animator.frames = frames;
            animator.renderer = renderer;
            clip.SetCurve("", typeof(CourierAnimator), "progress", curve);
            animation.clip = clip;
            animation.enabled = true;
            animation.AddClip(clip, "animation");
            animation.wrapMode = wrapMode;

            return animation;
        }

        public static Animation AddAnimation(GameObject g, WrapMode wrapMode, Image image, params AnimationFrame[] frames) {
            Animation animation = g.AddComponent<Animation>();
            AnimationClip clip = new AnimationClip();
            clip.legacy = true;
            AnimationCurve curve = new AnimationCurve();
            float totalTime = 0;
            for (int i = 0; i < frames.Length; i++) {
                curve.AddKey(totalTime, i);
                totalTime += frames[i].time;
            }
            CourierAnimator animator = g.AddComponent<CourierAnimator>();
            animator.frames = frames;
            animator.image = image;
            clip.SetCurve("", typeof(CourierAnimator), "progress", curve);
            animation.clip = clip;
            animation.enabled = true;
            animation.AddClip(clip, "animation");
            animation.wrapMode = wrapMode;

            return animation;
        }

        public static Animation AddAnimation(GameObject g, WrapMode wrapMode = WrapMode.Loop, params AnimationFrame[] frames) {
            return AddAnimation(g, wrapMode, g.AddComponent<SpriteRenderer>(), frames);
        }
    }
}
