using HarmonyLib;
using UnhollowerRuntimeLib;
using UnityEngine;

namespace PeasAPI
{
    public class Watermark
    {
        /// <summary>
        /// Text that gets added to the version text
        /// </summary>
        public static string VersionText { get; set; } = null;
        
        /// <summary>
        /// How much the version text should be lowered
        /// </summary>
        public static Vector3 VersionTextOffset { get; set; } = new (0f, -0.2f, 0f);

        /// <summary>
        /// Whether the reactor version text should be destroyed or not
        /// </summary>
        public static bool UseReactorVersion { get; set; } = false;

        /// <summary>
        /// Text that gets added to the ping text
        /// </summary>
        public static string PingText { get; set; } = null;

        /// <summary>
        /// How much the ping text should be lowered
        /// </summary>
        public static Vector3 PingTextOffset { get; set; } = new (0f, 0f, 0f);

        [HarmonyPatch(typeof(VersionShower), nameof(VersionShower.Start))]
        public static class VersionShowerStartPatch
        {
            static void Postfix(VersionShower __instance)
            {
                __instance.transform.position += VersionTextOffset;

                if (UseReactorVersion)
                {
                    if (VersionText != null)
                        Reactor.Patches.ReactorVersionShower.TextUpdated += text => text.text = VersionText;
                    
                    Reactor.Patches.ReactorVersionShower.TextUpdated += text => text.text = $"\n<color=#ff0000ff>PeasAPI {PeasApi.Version} <color=#ffffffff> by <color=#ff0000ff>Peasplayer\n<color=#ffffffff>Reactor-Framework";
                }
                else
                {
                    if (VersionText != null)
                        __instance.text.text += VersionText;
                    
                    __instance.text.text += $"\n<color=#ff0000ff>PeasAPI {PeasApi.Version} <color=#ffffffff> by <color=#ff0000ff>Peasplayer\n<color=#ffffffff>Reactor-Framework";
                    
                    foreach (var gameObject in Object.FindObjectsOfTypeAll(Il2CppType.Of<GameObject>()))
                        if (gameObject.name.Contains("ReactorVersion"))
                            Object.Destroy(gameObject);
                }
            }
        }
        
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingTrackerStartPatch
        {
            public static void Postfix(PingTracker __instance)
            {
                __instance.transform.position += PingTextOffset;
            }
        }
        
        [HarmonyPatch(typeof(PingTracker), nameof(PingTracker.Update))]
        public static class PingTrackerUpdatePatch
        {
            public static void Postfix(PingTracker __instance)
            {
                if (PingText != null)
                    __instance.text.text += PingText;

                __instance.text.text +=
                    $"\n<color=#ff0000ff>PeasAPI";
            }
        }
    }
}
