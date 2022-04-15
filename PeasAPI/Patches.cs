using HarmonyLib;
using UnityEngine;

namespace PeasAPI
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        [HarmonyPostfix]
        public static void ChangeZOfAccountTabPatch()
        {
            var tab = AccountManager.Instance.accountTab;
            tab.transform.SetZ(1f);
            tab.GetComponent<SlideOpen>().computedClosedPosition = tab.GetComponent<SlideOpen>().computedClosedPosition.SetZ(1);
        }
        
        [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.RandomizeName))]
        [HarmonyPrefix]
        public static bool RemoveRandomNamePatch()
        {
            return false;
        }
    }
}