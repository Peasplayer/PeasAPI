using System.IO;
using System.Reflection;
using Reactor.Extensions;
using UnityEngine;

namespace PeasAPI
{
    public class Utility
    {
        public static Sprite CreateSprite(string image, Assembly assembly)
        {
            Texture2D tex = GUIExtensions.CreateEmptyTexture();
            Stream myStream = assembly.GetManifestResourceStream(image);
            byte[] buttonTexture = Reactor.Extensions.Extensions.ReadFully(myStream);
            ImageConversion.LoadImage(tex, buttonTexture, false);
            return GUIExtensions.CreateSprite(tex);
        }
    }
}