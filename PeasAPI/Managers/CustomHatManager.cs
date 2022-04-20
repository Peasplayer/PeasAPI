using System;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using static PeasAPI.Data;

namespace PeasAPI.Managers
{
    public class CustomHatManager
    {
        public static List<Hat> CustomHats = new List<Hat>();
        public static List<Visor> CustomVisors = new List<Visor>();

        public static void RegisterNewHat(string name, string imagePath, Vector2 chipOffset = new Vector2(), bool inFront = true, bool noBounce = true, Sprite backImage = null, Sprite floorImage = null)
        {
            var hat = new Hat(name, imagePath, Assembly.GetCallingAssembly(), inFront, noBounce, chipOffset, backImage, floorImage);
            
            CustomHats.Add(hat);
            
            if (PeasAPI.Logging)
                PeasAPI.Logger.LogInfo($"Registered hat {name} from {Assembly.GetCallingAssembly().GetName().Name}");
        }
        
        public static void RegisterNewVisor(string name, string imagePath, Vector2 chipOffset = new Vector2(), Sprite climbImage = null, Sprite floorImage = null)
        {
            var visor = new Visor(name, imagePath, Assembly.GetCallingAssembly(), chipOffset, climbImage, floorImage);
            
            CustomVisors.Add(visor);
            
            if (PeasAPI.Logging)
                PeasAPI.Logger.LogInfo($"Registered visor {name} from {Assembly.GetCallingAssembly().GetName().Name}");
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
                __instance.allHats.Add(hat.CreateHat());
            foreach (var visor in CustomHatManager.CustomVisors)
                __instance.allVisors.Add(visor.CreateVisor());
            
            __instance.allHats.ToArray().ToList().Sort((h1, h2) => String.Compare(h2.ProductId, h1.ProductId, StringComparison.Ordinal));
            
            __instance.allVisors.ToArray().ToList().Sort((h1, h2) => String.Compare(h2.ProductId, h1.ProductId, StringComparison.Ordinal));
        }
    }
}