using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace PeasAPI
{
    public static class Utility
    {
        public static Dictionary<string, Sprite> CachedSprites = new();

      public static Sprite LoadSprite(string path, float pixelsPerUnit = 1f)
    {
        try
        {
            if (CachedSprites.TryGetValue(path + pixelsPerUnit, out var sprite)) return sprite;
            Texture2D texture = LoadTextureFromResources(path);
            sprite = Sprite.Create(texture, new(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), pixelsPerUnit);
            sprite.hideFlags |= HideFlags.HideAndDontSave | HideFlags.DontSaveInEditor;
            return CachedSprites[path + pixelsPerUnit] = sprite;
        }
        catch
        {
            PeasAPI.Logger.LogError($"An error was occured when loading the follow path {path}");
        }
        return null;
    }
    public static Texture2D LoadTextureFromResources(string path)
    {
        try
        {
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(path);
            var texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            using MemoryStream ms = new();
            stream.CopyTo(ms);
            ImageConversion.LoadImage(texture, ms.ToArray(), false);
            return texture;
        }
        catch
        {
            PeasAPI.Logger.LogError($"An error was occured when loading the follow path {path}");

        }
        return null;
    }


        public static List<PlayerControl> GetAllPlayers()
        {
            if (PlayerControl.AllPlayerControls != null && PlayerControl.AllPlayerControls.Count > 0)
                return PlayerControl.AllPlayerControls.ToArray().ToList();
            return GameData.Instance.AllPlayers.ToArray().ToList().ConvertAll(p => p.Object);
        }

        public class StringColor
        {
            public const string Reset = "<color=#ffffffff>";
            public const string White = "<color=#ffffffff>";
            public const string Black = "<color=#000000ff>";
            public const string Red = "<color=#ff0000ff>";
            public const string Green = "<color=#169116ff>";
            public const string Blue = "<color=#0400ffff>";
            public const string Yellow = "<color=#f5e90cff>";
            public const string Purple = "<color=#a600ffff>";
            public const string Cyan = "<color=#00fff2ff>";
            public const string Pink = "<color=#e34dd4ff>";
            public const string Orange = "<color=#ff8c00ff>";
            public const string Brown = "<color=#8c5108ff>";
            public const string Lime = "<color=#1eff00ff>";
        }
    }
}