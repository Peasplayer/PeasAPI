/*
using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using InnerNet;
using PeasAPI.CustomRpc;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Reactor.Localization.Utilities;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using UnityEngine;
using Object = UnityEngine.Object;
using AmongUs.Data.Legacy;

namespace PeasAPI.Managers
{
    public static class CustomColorManager
    {
        private static readonly List<AUColor> CustomColorsList = new();

        public static void RegisterCustomColor(IEnumerable<AUColor> auColor)
        {
            CustomColorsList.AddRange(auColor);
        }
        
        public static void RegisterCustomColor(Color body, string name)
        {
            RegisterCustomColor(body,
                Color.Lerp(body, Color.black, .3f),
                name);
        }

        public static void RegisterCustomColor(Color body, Color shadow, string name)
        {
            CustomColorsList.Add(new AUColor(body, shadow, name));
        }
        
        public static void RegisterCustomColor(AUColor auColor)
        {
            CustomColorsList.Add(auColor);
        }

        private static void Initialize()
        {
            if (!CustomColorsList.Any()) return;
            
            Palette.PlayerColors = Palette.PlayerColors.Concat(CustomColorsList.Select(x =>  x.Body)).ToArray();
            Palette.ShadowColors = Palette.ShadowColors.Concat(CustomColorsList.Select(x => x.Shadow)).ToArray();
            Palette.ColorNames = Palette.ColorNames.Concat(CustomColorsList.Select(x => x.Name)).ToArray();
            
            CustomColorsList.Clear();
        }
        
        public class AUColor
        {
            public Color32 Body { get; set; }
            public Color32 Shadow { get; set; }
            public StringNames Name { get; set; }

            public AUColor(Color body, Color shadow, string name)
            {
                Body = body;
                Shadow = shadow;
                Name = CustomStringName.CreateAndRegister(name);
            }
        }
        
        [HarmonyPatch]
        private static class Patches
        {
            // Initialize
            [HarmonyPostfix]
            [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.Awake))]
            private static void AmongUsClientAwake() => Initialize();

            [HarmonyPatch(typeof(PlayerTab))]
            private static class PlayerTabPatch
            {
                
                // Scroller implementation
                [HarmonyPrefix]
                [HarmonyPatch(nameof(PlayerTab.OnEnable))]
                private static void OnEnablePrefix(PlayerTab __instance)
                {
                    var hatsTab = __instance.transform.parent.parent
                        .GetComponentInChildren<HatsTab>(true);
                    
                    if (__instance.scroller || !hatsTab) return;

                    __instance.scroller = Object.Instantiate(hatsTab.scroller, __instance.ColorTabArea);
                    
                    __instance.scroller.name = "Scroller";
                    __instance.scroller.Hitbox.transform.localPosition = Vector3.zero;

                    var cc = __instance.ColorTabPrefab;
                    cc.GetComponent<SpriteRenderer>().maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                    foreach (var spr in cc.GetComponentsInChildren<SpriteRenderer>())
                        spr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                    foreach (var spr in cc.PlayerEquippedForeground.GetComponentsInChildren<SpriteRenderer>())
                        spr.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                }
                
                // Resize and reposition color chips in playertab
                [HarmonyPostfix]
                [HarmonyPatch(nameof(PlayerTab.OnEnable))]
                private static void OnEnablePostfix(PlayerTab __instance)
                {
                    var chips = __instance.ColorChips.ToArray();
                    
                    for (var i = 0; i < __instance.ColorChips.Count; i++) {
                        var colorChip = chips[i];
                        var x = __instance.XRange.Lerp(i % 5 / 4f);
                        var y = __instance.YStart - (i / 5) * 0.6f;

                        var transform = colorChip.transform;
                        transform.localPosition = new Vector3(x, y, -1f);
                        transform.SetParent(__instance.scroller.Inner.transform);
                        
                        var fg = colorChip.transform.GetChild(0).gameObject;
                        var oldShade = fg.transform.GetChild(0).gameObject;
                        var newShade = fg.transform.GetChild(1).gameObject;

                        var shadeRenderer = newShade.GetComponent<SpriteRenderer>();
                        shadeRenderer.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                        shadeRenderer.color = Color.Lerp(shadeRenderer.color, Color.black, .5f);

                        fg.GetComponent<SpriteMask>().Destroy();
                        oldShade.Destroy();
                    }
                    
                    var rows = Mathf.Max(0, (chips.Count / 5) - 6);
                    __instance.scroller.ContentYBounds.max = (rows * 0.6f) + 0.5f;
                }
                
                // Set hat everytime player selecting color
                [HarmonyPostfix]
                [HarmonyPatch(nameof(PlayerTab.SelectColor))]
                private static void SelectColor(PlayerTab __instance, int colorId)
                {
                    __instance.PlayerPreview.SetHat(LegacySaveManager.LastHat, colorId);
                }
            }

            // Custom RPC check/set to prevent from triggering anti-cheat
            [HarmonyPatch(typeof(PlayerControl))]
            private static class PlayerControlPatch
            {
                /*[HarmonyPrefix]
                [HarmonyPatch(nameof(PlayerControl.CmdCheckColor))]
                private static bool CmdCheckColor(byte bodyColor, PlayerControl __instance)
                {
                    if (AmongUsClient.Instance.AmHost)
                    {
                        __instance.CheckColor(bodyColor);
                        return false;
                    }

                    Rpc<RpcCustomCheckColor>.Instance.Send(bodyColor);
                    return false;
                }

                [HarmonyPrefix]
                [HarmonyPatch(nameof(PlayerControl.RpcSetColor))]
                private static bool RpcSetColor(byte bodyColor, PlayerControl __instance)
                {
                    if (AmongUsClient.Instance.AmClient)
                        __instance.SetColor(bodyColor);
                    
                    Rpc<RpcSetColor>.Instance.Send(new RpcSetColor.Data(__instance, bodyColor));
                    return false;
                }
                
                // It does the same as the original one
                // But needed because it sends vanilla RPC for some reason
                // (even patching PlayerControl#RpcSetColor won't work)
                [HarmonyPrefix]
                [HarmonyPatch(nameof(PlayerControl.CheckColor))]
                private static bool CheckColor(byte bodyColor, PlayerControl __instance)
                {
                    var allPlayers = GameData.Instance.AllPlayers.ToArray();
                    var num = 0;
                    while (num++ < int.MaxValue && (bodyColor >= Palette.PlayerColors.Length || allPlayers.Any(ColorIsOccupied)))
                    {
                        bodyColor = (byte) ((bodyColor + 1) % Palette.PlayerColors.Length);
                    }
                    
                    __instance.RpcSetColor(bodyColor);
                    return false;

                    bool ColorIsOccupied(GameData.PlayerInfo p)
                    {
                        return !p.Disconnected && p.PlayerId != __instance.PlayerId
                                               && p.DefaultOutfit.ColorId == bodyColor;
                    }
                }
            }
            
            // Prevent custom color from being saved inside SaveManager
            [HarmonyPatch(typeof(LegacySaveManager), nameof(LegacySaveManager.BodyColor))]
            private static class LegacySaveManagerPatch
            {
                private const byte MAXColor = 17;
                private static ConfigEntry<byte> Data => PeasAPI.ConfigFile
                    .Bind("CustomSaveManager", "Player Color ID", (byte) LegacySaveManager.colorConfig);
                
                [HarmonyPrefix]
                [HarmonyPatch(MethodType.Getter)]
                private static bool GetterPatch(ref byte __result)
                {
                    __result = Data.Value;
                    return false;
                }
                
                [HarmonyPrefix]
                [HarmonyPatch(MethodType.Setter)]
                private static bool SetterPatch(byte value)
                {
                    Data.Value = value;
                    return value <= MAXColor;
                }
            }
            
            // Can't figure out what making the game sends vanilla RPC
            // (Found it, but gonna keep this anyway)
            // [HarmonyPrefix]
            // [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.StartRpc))]
            // private static bool InnerNetClientStartRpc(byte callId)
            // {
            //     Logger<PeasApi>.Debug("RPC sent: " + callId);
            //     return callId is not (7 or 8);
            // }
        }
    }
}
*/