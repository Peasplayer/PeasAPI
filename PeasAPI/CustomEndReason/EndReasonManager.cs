using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace PeasAPI.CustomEndReason
{
    public class EndReasonManager
    {
        public static GameOverReason CustomGameOverReason = (GameOverReason) 255;

        public static Color Color;

        public static List<GameData.PlayerInfo> Winners;

        public static string VictoryText;
        
        public static string DefeatText;
        
        public static string Stinger;

        public static bool GameIsEnding;

        public static void Reset()
        {
            Color = Color.clear;
            Winners = null;
            VictoryText = null;
            DefeatText = null;
            Stinger = null;
            GameIsEnding = false;
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.SetEverythingUp))]
        private class EndGameManager_SetEverythingUp
        {
            private static readonly Color GhostColor = new(1f, 1f, 1f, 0.5f);

            public static bool Prefix(EndGameManager __instance)
            {
                if (TempData.EndReason != CustomGameOverReason)
                    return true;

                List<WinningPlayerData> _winners = new List<WinningPlayerData>();
                foreach (var winner in Winners)
                {
                    _winners.Add(new WinningPlayerData(winner));
                }

                __instance.DisconnectStinger = Stinger switch
                {
                    "crew" => __instance.CrewStinger,
                    "impostor" => __instance.ImpostorStinger,
                    _ => __instance.DisconnectStinger
                };

                __instance.WinText.text = DefeatText;
                __instance.WinText.color = Palette.ImpostorRed;
                __instance.BackgroundBar.material.color = Palette.ImpostorRed;
                foreach (var winner in Winners)
                {
                    if (winner.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        __instance.WinText.text = VictoryText;
                        __instance.WinText.color = Palette.CrewmateBlue;
                        __instance.BackgroundBar.material.color = Palette.CrewmateBlue;
                    }
                }

                for (int i = 0; i < _winners.Count; i++)
                {
                    var winner = _winners[i];
                    int oddness = (i + 1) / 2;
                    PoolablePlayer player = Object.Instantiate(__instance.PlayerPrefab, __instance.transform);
                    var transform = player.transform;
                    transform.localPosition = new Vector3(
                        0.8f * (i % 2 == 0 ? -1 : 1) * oddness * 1 - oddness * 0.035f,
                        FloatRange.SpreadToEdges(-1.125f, 0f, oddness, Mathf.CeilToInt(7.5f)),
                        (i == 0 ? -8 : -1) + oddness * 0.01f
                    ) * 1.25f;
                    float scale = 1f - oddness * 0.075f;
                    var scaleVec = new Vector3(scale, scale, scale);
                    transform.localScale = scaleVec;
                    if (winner.IsDead)
                    {
                        player.Body.sprite = __instance.GhostSprite;
                        player.SetDeadFlipX(i % 2 == 1);
                        player.HatSlot.color = GhostColor;
                    }
                    else
                    {
                        player.SetFlipX(i % 2 == 0);
                        DestroyableSingleton<HatManager>.Instance.SetSkin(player.Skin.layer, winner.SkinId);
                    }

                    PlayerControl.SetPlayerMaterialColors(winner.ColorId, player.Body);
                    player.HatSlot.SetHat(winner.HatId, winner.ColorId);
                    PlayerControl.SetPetImage(winner.PetId, winner.ColorId, player.PetSlot);
                    player.NameText.text = winner.PlayerName;
                    player.NameText.transform.SetLocalZ(-15f);
                }
                
                SoundManager.Instance.PlaySound(__instance.DisconnectStinger, false, 1f);
                
                return false;
            }
        }

        [HarmonyPatch(typeof(EndGameManager), nameof(EndGameManager.Start))]
        private class EndGameManagerStartPatch
        {
            public static void Prefix(EndGameManager __instance)
            {
                if (TempData.EndReason != CustomGameOverReason)
                    return;
                
                __instance.DisconnectStinger = Stinger switch
                {
                    "crew" => __instance.CrewStinger,
                    "impostor" => __instance.ImpostorStinger,
                    _ => __instance.DisconnectStinger
                };
                
                __instance.WinText.text = "Defeat";
                __instance.WinText.color = Palette.ImpostorRed;
                
                foreach (var winner in Winners)
                {
                    if (winner.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        __instance.WinText.text = "Victory";
                        __instance.WinText.color = Palette.Blue;
                    }
                }
                
                __instance.BackgroundBar.material.color = Color;
            }

            public static void Postfix(EndGameManager __instance)
            {
                if (TempData.EndReason != CustomGameOverReason)
                    return;

                Reset();
            }
        }
    }
}