#pragma warning disable CS0626 // Method, operator, or accessor is marked external and has no attributes on it

using System;
using System.Collections;
using Mod.Courier;
using Mod.Courier.Helpers;
using UnityEngine;
using UnityEngine.UI;

public class patch_InGameHud : InGameHud {

    private extern void orig_Start();
    private void Start() {
#pragma warning disable RECS0117 // Local variable has the same name as a member and hides it
        GameObject watermark8 = new GameObject("CourierWatermark");
        watermark8.transform.SetParent(hud_8.transform.Find("Background"));
        watermark8.transform.Translate(new Vector3(0, 600));
        Image image = watermark8.AddComponent<Image>();
        Sprite sprite = image.sprite = Courier.EmbeddedSprites["Mod.Courier.UI.courier_watermark"];
        sprite.texture.wrapMode = TextureWrapMode.Clamp;
        image.preserveAspect = true;
        sprite.texture.filterMode = FilterMode.Point;

        GameObject watermark16 = new GameObject("CourierWatermark");
        watermark16.transform.SetParent(hud_16.transform.Find("Background"));
        watermark16.transform.Translate(new Vector3(0, 600));
        image = watermark16.AddComponent<Image>();
        sprite = image.sprite = Courier.EmbeddedSprites["Mod.Courier.UI.courier_watermark"];
        sprite.texture.wrapMode = TextureWrapMode.Clamp;
        image.preserveAspect = true;
        sprite.texture.filterMode = FilterMode.Point;

        orig_Start();
    }

    // This allows mods to hook OnGUI()
    public void OnGUI() {

    }
}
