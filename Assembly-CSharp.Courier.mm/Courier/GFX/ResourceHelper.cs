using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Mod.Courier.GFX {
    public static class ResourceHelper {
        public static Dictionary<string, SpriteParams> SpriteConfig = new Dictionary<string, SpriteParams>();
        
        public static Sprite GetSpriteFromFile(string path, SpriteParams spriteParams = null) {
            return GetSpriteFromStream(File.OpenRead(path), spriteParams);
        }

        public static Texture2D GetTextureFromStream(Stream stream) {
            if (stream == null) return null;

            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);

            // Create texture from bytes
            Texture2D tex = new Texture2D(1, 1);
            tex.LoadImage(buffer);
            return tex;
        }

        public static Texture2D GetEmbeddedTexture(string resource, Assembly callingAssembly = null) {
            if (callingAssembly == null)
                callingAssembly = new StackFrame(1, false).GetMethod()?.DeclaringType?.Assembly;

            using (Stream stream = callingAssembly.GetManifestResourceStream(resource)) {
                if (stream == null) {
                    return null;
                }

                return GetTextureFromStream(stream);
            }
        }

        public static Sprite GetSpriteFromStream(Stream stream, SpriteParams spriteParams = null) {
            Texture2D tex = GetTextureFromStream(stream);
            // Create sprite from texture
            if (spriteParams != null) {
                return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), spriteParams.pivot, spriteParams.pixelsPerUnit, spriteParams.extrude, spriteParams.meshType, spriteParams.border);
            }

            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        public static Sprite GetEmbeddedSprite(string resource, SpriteParams spriteParams = null, Assembly callingAssembly = null) {
            if (callingAssembly == null)
                callingAssembly = new StackFrame(1, false).GetMethod()?.DeclaringType?.Assembly;

            using (Stream stream = callingAssembly.GetManifestResourceStream(resource)) {
                if (stream == null) {
                    return null;
                }

                return GetSpriteFromStream(stream, spriteParams);
            }
        }

        // https://github.com/seanpr96/HollowKnight.SeanprCore/blob/master/SeanprCore/ResourceHelper.cs
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Dictionary<string, Sprite> GetEmbeddedSprites(string prefix = null, Assembly callingAssembly = null) {
            if(callingAssembly == null)
                callingAssembly = new StackFrame(1, false).GetMethod()?.DeclaringType?.Assembly;

            Dictionary<string, Sprite> sprites = new Dictionary<string, Sprite>();

            if (callingAssembly == null) {
                return sprites;
            }

            foreach (string resource in callingAssembly.GetManifestResourceNames()
                .Where(name => name.ToLower().EndsWith(".png", StringComparison.InvariantCulture))) {
                try {
                    string resName = Path.GetFileNameWithoutExtension(resource);
                    if (!string.IsNullOrEmpty(prefix)) {
                        resName = resName.Replace(prefix, "");
                    }

                    // Create sprite from texture
                    if (SpriteConfig.ContainsKey(resName)) {
                        SpriteParams spriteParams = SpriteConfig[resName];
                        sprites.Add(resName, GetEmbeddedSprite(resource, spriteParams, callingAssembly));
                    } else {
                        sprites.Add(resName, GetEmbeddedSprite(resource, callingAssembly: callingAssembly));
                    }
                } catch (Exception e) {
                    CourierLogger.LogDetailed(e, "ResourceHelper");
                }
            }

            return sprites;
        }
    }
}
