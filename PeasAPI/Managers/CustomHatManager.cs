using System;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;
using Reactor.Extensions;

namespace PeasAPI.Managers
{
    public class CustomHatManager
    {
        public static List<Hat> CustomHats = new List<Hat>();

        public class Hat
        {
            public string Name;
            public string ImagePath;
            public Assembly Assembly;
            public bool InFront = true;
            public bool NoBounce = true;
            public Vector2 ChipOffset = new Vector2();
            public Sprite BackImage = null;
            public Sprite FloorImage = null;
            
            public Hat(string name, string imagePath, Assembly assembly)
            {
                Name = name;
                ImagePath = imagePath;
                Assembly = assembly;
            }
            
            public HatBehaviour CreateHat()
            {
                try
                {
                    Texture2D tex = new Texture2D(128, 128, TextureFormat.ARGB32, false);
                    Stream myStream = Assembly.GetManifestResourceStream(ImagePath);
                    byte[] data = myStream.ReadFully();
                    ImageConversion.LoadImage(tex, data, false);

                    var newHat = ScriptableObject.CreateInstance<HatBehaviour>();
                    newHat.MainImage = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.53f, 0.575f),
                        tex.width * 0.375f
                    );
                    
                    newHat.ProductId = $"+{Name}";
                    newHat.Order += 100;
                    
                    newHat.InFront = InFront;
                    newHat.NoBounce = NoBounce;
                    newHat.ChipOffset = ChipOffset;
                    newHat.BackImage = BackImage;
                    newHat.FloorImage = FloorImage;
                    
                    return newHat;
                }
                catch (Exception e)
                {
                    PeasApi.Logger.LogError($"Error while creating a hat: {e.StackTrace}");
                }

                return null;
            }
        }
        
        public static void RegisterNewHat(string name, string imagePath, Vector2 chipOffset = new Vector2(), bool inFront = true, bool noBounce = true, Sprite backImage = null, Sprite floorImage = null)
        {
            var hat = new Hat(name, imagePath, Assembly.GetCallingAssembly());
            
            hat.InFront = inFront;
            hat.NoBounce = noBounce;
            hat.ChipOffset = chipOffset;
            hat.BackImage = backImage;
            hat.FloorImage = floorImage;
            
            CustomHats.Add(hat);
        }
    }

    [HarmonyPatch(typeof(HatManager), nameof(HatManager.GetHatById))]
    public static class HatManagerPatch
    {
        private static bool modded = false;

        public static void Prefix(HatManager __instance)
        {
            if (modded)
                return;
            
            modded = true;

            foreach (var hat in CustomHatManager.CustomHats)
                __instance.AllHats.Add(hat.CreateHat());
            
            __instance.AllHats.Sort((Il2CppSystem.Comparison<HatBehaviour>)((h1, h2) => h2.ProductId.CompareTo(h1.ProductId)));
        }
    }
}