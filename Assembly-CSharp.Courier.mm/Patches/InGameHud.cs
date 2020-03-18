#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections;
using Mod.Courier;
using Mod.Courier.GFX;
using Mod.Courier.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class patch_InGameHud : InGameHud {
    public GameObject CourierWatermark_8;
    public GameObject CourierWatermark_16;

    private extern void orig_Start();
    private void Start() {
#pragma warning disable RECS0117 // Local variable has the same name as a member and hides it
        CourierWatermark_8 = new GameObject("CourierWatermark");
        CourierWatermark_8.transform.SetParent(hud_8.transform.Find("Background"));
        CourierWatermark_8.transform.Translate(new Vector3(0, 600));
        Image image = CourierWatermark_8.AddComponent<Image>();
        image.preserveAspect = true;
        Texture2D tex = ResourceHelper.GetEmbeddedTexture("Mod.Courier.UI.courier_watermark_animated.png");
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;
        AnimationFrame[] animationFrames = AnimationHelper.CreateAnimationFramesFromSpriteSheet(tex, 17, SpriteSheetOrientation.Horizontal, .39f, .02f, .02f, .02f, .05f, .05f, .05f, .05f, .02f, .02f, .05f, .02f, .05f, .02f, .05f, .02f, .05f, .01f);
        Animation animation_8 = AnimationHelper.AddAnimation(CourierWatermark_8, WrapMode.Loop, image, animationFrames);

        CourierWatermark_16 = new GameObject("CourierWatermark");
        CourierWatermark_16.transform.SetParent(hud_16.transform.Find("Background"));
        CourierWatermark_16.transform.Translate(new Vector3(0, 600));
        image = CourierWatermark_16.AddComponent<Image>();
        image.preserveAspect = true;
        tex = ResourceHelper.GetEmbeddedTexture("Mod.Courier.UI.courier_watermark_animated.png");
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.filterMode = FilterMode.Point;
        animationFrames = AnimationHelper.CreateAnimationFramesFromSpriteSheet(tex, 17, SpriteSheetOrientation.Horizontal, .39f, .02f, .02f, .02f, .05f, .05f, .05f, .05f, .02f, .02f, .05f, .02f, .05f, .02f, .05f, .02f, .05f, .01f);
        Animation animation_16 = AnimationHelper.AddAnimation(CourierWatermark_16, WrapMode.Loop, image, animationFrames);

        Manager<SaveManager>.Instance.onBeforeSave -= OnBeforeSave;
        Manager<SaveManager>.Instance.onBeforeSave += OnBeforeSave;

        Manager<SaveManager>.Instance.onSaveDone -= OnSaveDone;
        Manager<SaveManager>.Instance.onSaveDone += OnSaveDone;

        orig_Start();
    }

    private void OnBeforeSave() {
        if (!Manager<Level>.Instance.Initialized || !enabled) return;

        Animation animation_8 = CourierWatermark_8?.GetComponent<Animation>();
        Animation animation_16 = CourierWatermark_16?.GetComponent<Animation>();
        if (animation_8 != null) {
            animation_8.wrapMode = WrapMode.Loop;
        }
        if (animation_16 != null) {
            animation_16.wrapMode = WrapMode.Loop;
        }
        animation_8?.Play(this, "animation", false);
        animation_16?.Play(this, "animation", false);
    }

    private void OnSaveDone() {
        if (!Manager<Level>.Instance.Initialized || !enabled) return;

        Animation animation_8 = CourierWatermark_8?.GetComponent<Animation>();
        Animation animation_16 = CourierWatermark_16?.GetComponent<Animation>();
        if(animation_8 != null) {
            animation_8.wrapMode = WrapMode.Once;
        }
        if (animation_16 != null) {
            animation_16.wrapMode = WrapMode.Once;
        }
    }

    public void OnDisable() {
        Manager<SaveManager>.Instance.onSaveDone -= OnSaveDone;
        Manager<SaveManager>.Instance.onBeforeSave -= OnBeforeSave;
    }

    // This allows mods to hook OnGUI()
    public void OnGUI() {

    }
}
