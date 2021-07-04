using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PeasAPI
{
    public class Watermark
    {
        public static string VersionText { get; set; } = null;

        public static string PingText { get; set; } = null;
        
        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionShowerPatch
        {
            static void Postfix(VersionShower __instance)
            {
                if (VersionText != null)
                    __instance.text.text += VersionText;
                __instance.text.text += $"\n<color=#ff0000ff>PeasAPI {PeasApi.Version} <color=#ffffffff> by <color=#ff0000ff>Peasplayer\n<color=#ffffffff>Reactor-Framework";
                __instance.transform.position -= new Vector3(0, 0.5f, 0);
                
                AccountManager.Instance.accountTab.gameObject.SetActive(false);
                
                foreach (var _object in GameObject.FindObjectsOfTypeAll(Il2CppType.Of<GameObject>()))
                    if (_object.name.Contains("ReactorVersion"))
                        GameObject.Destroy(_object);
            }
        }
        
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingTrackerPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                if (PingText != null)
                    __instance.text.text += PingText;

                __instance.transform.position -= new Vector3(0, 0.5f, 0);
                __instance.text.text +=
                    $"\n<color=#ff0000ff>PeasAPI";
            }
        }
    }
}