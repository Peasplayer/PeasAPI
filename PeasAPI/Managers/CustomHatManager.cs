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

        public readonly struct Hat
        {
            public readonly string Name;
            public readonly string ImagePath;
            public readonly Assembly Assembly;
            public readonly bool InFront;
            public readonly bool NoBounce;
            public readonly Vector2 ChipOffset;
            public readonly Sprite BackImage;
            public readonly Sprite FloorImage;
            
            public Hat(string name, string imagePath, Assembly assembly, bool inFront, bool noBounce, Vector2 chipOffset, Sprite backImage, Sprite floorImage)
            {
                Name = name;
                ImagePath = imagePath;
                Assembly = assembly;
                InFront = inFront;
                NoBounce = noBounce;
                ChipOffset = chipOffset;
                BackImage = backImage;
                FloorImage = floorImage;
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
                    newHat.MainImage = newHat.LeftMainImage = Sprite.Create(
                        tex,
                        new Rect(0, 0, tex.width, tex.height),
                        new Vector2(0.53f, 0.575f),
                        tex.width * 0.375f
                    );
                    
                    newHat.ProductId = $"+{Name}";
                    newHat.Order += 100;
                    newHat.Free = true;
                    newHat.StoreName = Name;
                    newHat.name = Name;
                    
                    newHat.InFront = InFront;
                    newHat.NoBounce = NoBounce;
                    newHat.ChipOffset = ChipOffset;
                    newHat.BackImage = newHat.LeftBackImage = BackImage;
                    newHat.ClimbImage = newHat.LeftClimbImage = BackImage;
                    newHat.FloorImage = newHat.LeftFloorImage = FloorImage;
                    
                    return newHat;
                }
                catch (Exception e)
                {
                    PeasAPI.Logger.LogError($"Error while creating a hat: {e.StackTrace}");
                }

                return null;
            }
        }
        
        public static void RegisterNewHat(string name, string imagePath, Vector2 chipOffset = new Vector2(), bool inFront = true, bool noBounce = true, Sprite backImage = null, Sprite floorImage = null)
        {
            var hat = new Hat(name, imagePath, Assembly.GetCallingAssembly(), inFront, noBounce, chipOffset, backImage, floorImage);
            
            CustomHats.Add(hat);
            
            if (PeasAPI.Logging)
                PeasAPI.Logger.LogInfo($"Registered hat {name} from {Assembly.GetCallingAssembly().GetName().Name}");
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