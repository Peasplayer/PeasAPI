﻿using System;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;
using UnityEngine;

namespace PeasAPI.Gamemodes
{
    public class Patches
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        class ShipStatusStartPatch
        {
            public static void Prefix(ShipStatus __instance)
            {
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        mode.OnGameStart();
                }
            }
        }
        
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcEndGame))]
        class ShipStatusRpcEndGamePatch
        {
            public static bool Prefix(ShipStatus __instance, [HarmonyArgument(0)] GameOverReason reason)
            {
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        return mode.ShouldGameStop(reason);
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        class HudManagerUpdatePatch
        {
            public static void Prefix(HudManager __instance)
            {
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        mode.OnUpdate();
                }
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        class PlayerControlMurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl victim)
            {
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        return mode.OnKill(__instance, victim);
                }
                    
                return true;
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetInfected))]
        class PlayerControlSetInfectedPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] Il2CppStructArray<byte> _infected)
            {
                var infected = new List<PlayerControl>();
                
                foreach (var id in _infected)
                {
                    infected.Add(id.GetPlayer());
                }
                
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        infected = mode.GetImpostors(infected);
                }
                    
                return true;
            }
        }
        
        [HarmonyPatch(typeof(UseButtonManager), nameof(UseButtonManager.DoClick))]
        public static class UseButtonManagerDoClickPatch
        {
            public static bool Prefix(UseButtonManager __instance)
            {
                if (__instance.isActiveAndEnabled && PeasApi.GameStarted && __instance.currentTarget == null)
                {
                    foreach (var mode in GameModeManager.Modes)
                    {
                        if (mode.Enabled)
                        {
                            HudManager.Instance.ShowMap((Action<MapBehaviour>) (map =>
                            {
                                if (!mode.AllowSabotage())
                                    foreach (MapRoom mapRoom in map.infectedOverlay.rooms.ToArray())
                                        mapRoom.gameObject.SetActive(false);

                                map.ShowInfectedMap();
                            }));

                            return false;
                        }
                    }
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdReportDeadBody))]
        class PlayerControlCmdReportDeadBodyPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] GameData.PlayerInfo target)
            {
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        return mode.OnMeetingCall(__instance, target);
                }
                    
                return true;
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl._CoSetTasks_d__83), nameof(PlayerControl._CoSetTasks_d__83.MoveNext))]
        public static class PlayerControlSetTasks
        {
            public static void Postfix(PlayerControl._CoSetTasks_d__83 __instance)
            {
                if (__instance == null)
                    return;

                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                    {
                        var player = __instance.__4__this;
                        
                        if (!mode.HasToDoTasks)
                            player.ClearTasks();
                        
                        if (mode.GetObjective(player) != null)
                        {
                            var task = new GameObject(mode.Name + "Objective").AddComponent<ImportantTextTask>();
                            task.transform.SetParent(player.transform, false);
                            task.Text = $"</color>{mode.Name}\n</color>{mode.GetObjective(player)}</color>";
                            player.myTasks.Insert(0, task);
                        }
                    }
                }
            }
        }
        
        [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__14), nameof(IntroCutscene._CoBegin_d__14.MoveNext))]
        class IntroCutsceneMoveNextPatch
        {
            public static void Prefix(IntroCutscene._CoBegin_d__14 __instance)
            {
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        __instance.yourTeam = mode.GetIntroTeam();
                }
            }
            
            public static void Postfix(IntroCutscene._CoBegin_d__14 __instance)
            {
                foreach (var mode in GameModeManager.Modes)
                {
                    if (mode.Enabled)
                        mode.OnIntro(__instance);
                }
            }
        }
    }
}