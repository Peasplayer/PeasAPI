using HarmonyLib;
using System.Collections.Generic;
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
                __instance.AllHats.Add(hat.CreateHat());
            foreach (var visor in CustomHatManager.CustomVisors)
                __instance.AllVisors.Add(visor.CreateVisor());
            
            __instance.AllHats.Sort((Il2CppSystem.Comparison<HatBehaviour>)((h1, h2) => h2.ProductId.CompareTo(h1.ProductId)));
            
            __instance.AllVisors.Sort((Il2CppSystem.Comparison<VisorData>)((h1, h2) => h2.ProductId.CompareTo(h1.ProductId)));
        }
    }
}