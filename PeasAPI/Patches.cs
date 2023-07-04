using HarmonyLib;
using UnityEngine;

namespace PeasAPI
{
    [HarmonyPatch]
    public static class Patches
    {        
        [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.RandomizeName))]
        [HarmonyPrefix]
        public static bool RemoveRandomNamePatch()
        {
            return false;
        }
    }
}