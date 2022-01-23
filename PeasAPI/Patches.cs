using HarmonyLib;
using UnityEngine;

namespace PeasAPI
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionShowerStartPatch
        {
            static void Postfix(VersionShower __instance)
            {
                AccountManager.Instance.accountTab.transform.position = new Vector3(-8.5333f, 0f, -5f) + PeasAPI.AccountTabOffset;
            }
        }

        [HarmonyPatch(typeof(AccountTab), nameof(AccountTab.Toggle))]
        public static class AccountTabTogglePatch
        {
            public static bool Prefix(AccountTab __instance)
            {
                if (PeasAPI.AccountTabOnlyChangesName)
                {
                    if (__instance.editNameScreen.gameObject.active)
                    {
                        __instance.editNameScreen.gameObject.SetActive(false);
                    }
                    else
                    {
                        __instance.editNameScreen.gameObject.transform.position = new Vector3(0, 0, -15);
                        __instance.editNameScreen.gameObject.SetActive(true);
                    }
                    return false;
                }
                return true;
            }
        }
        
        [HarmonyPatch(typeof(AccountManager), nameof(AccountManager.RandomizeName))]
        [HarmonyPrefix]
        public static bool RemoveRandomNamePatch()
        {
            return false;
        }
    }
}