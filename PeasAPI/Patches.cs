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

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
        [HarmonyPrefix]
        public static bool CheckMurderPatch(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
        {
            if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
            {
                return false;
            }
            if (!target || __instance.Data.IsDead || __instance.Data.Disconnected)
            {
                int num = target ? target.PlayerId : -1;
                Debug.LogWarning(string.Format("Bad kill from {0} to {1}", __instance.PlayerId, num));
                return false;
            }
            GameData.PlayerInfo data = target.Data;
            if (data == null || data.IsDead || target.inVent)
            {
                Debug.LogWarning("Invalid target data for kill");
                return false;
            }
            __instance.RpcMurderPlayer(target);
            return false;
        }
    }
}