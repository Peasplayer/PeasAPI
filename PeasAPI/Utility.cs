using System.IO;
using System.Reflection;
using Reactor.Extensions;
using UnityEngine;

namespace PeasAPI
{
    public static class Utility
    {
        public static Sprite CreateSprite(string image, float pixelsPerUnit = 128f)
        {
            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            Stream myStream = Assembly.GetCallingAssembly().GetManifestResourceStream(image);
            byte[] data = myStream.ReadFully();
            ImageConversion.LoadImage(tex, data, false);
            var sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f),
                pixelsPerUnit);
            sprite.DontDestroy();
            return sprite;
        }
    }
}