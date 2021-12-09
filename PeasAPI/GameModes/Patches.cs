using System;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using Reactor;
using UnityEngine;

namespace PeasAPI.GameModes
{
    [HarmonyPatch]
    public static class Patches
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
                    if (mode.Enabled && PlayerControl.LocalPlayer && PeasAPI.GameStarted)
                        mode.OnUpdate();
                }
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
        class PlayerControlRpcMurderPlayerPatch
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
        
        [HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
        [HarmonyPriority(Priority.Last)]
        [HarmonyPostfix]
        public static void AssignRolesPatch(RoleManager __instance)
        {
            foreach (var mode in GameModeManager.Modes)
            {
                if (mode.Enabled)
                    mode.AssignRoles();
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRole))]
        [HarmonyPrefix]
        public static void AssignLocalRolePatch(PlayerControl __instance, [HarmonyArgument(0)] ref RoleTypes roleType)
        {
            foreach (var mode in GameModeManager.Modes)
            {
                if (mode.Enabled && mode.AssignLocalRole(__instance).HasValue)
                {
                    roleType = mode.AssignLocalRole(__instance).GetValueOrDefault();
                }
            }
        }
        
        [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
        public static class UseButtonManagerDoClickPatch
        {
            public static bool Prefix(SabotageButton __instance)
            {
                if (__instance.isActiveAndEnabled && PeasAPI.GameStarted)
                {
                    foreach (var mode in GameModeManager.Modes)
                    {
                        if (mode.Enabled)
                        {
                            HudManager.Instance.ShowMap((Action<MapBehaviour>) (map =>
                            {
                                foreach (MapRoom mapRoom in map.infectedOverlay.rooms.ToArray())
                                {
                                    mapRoom.gameObject.SetActive(mode.AllowSabotage(mapRoom.room));
                                }

                                map.ShowSabotageMap();
                            }));

                            return false;
                        }
                    }
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
        public static class MapBehaviourShowSabotageMapPatch
        {
            public static void Prefix(MapBehaviour __instance)
            {
                if (PeasAPI.GameStarted)
                {
                    foreach (var mode in GameModeManager.Modes)
                    {
                        if (mode.Enabled)
                        {
                            HudManager.Instance.ShowMap((Action<MapBehaviour>) (map =>
                            {
                                foreach (MapRoom mapRoom in map.infectedOverlay.rooms.ToArray())
                                {
                                    mapRoom.gameObject.SetActive(mode.AllowSabotage(mapRoom.room));
                                }
                            }));
                        }
                    }
                }
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
        
        [HarmonyPatch(typeof(PlayerControl._CoSetTasks_d__102), nameof(PlayerControl._CoSetTasks_d__102.MoveNext))]
        public static class PlayerControlSetTasks
        {
            public static void Postfix(PlayerControl._CoSetTasks_d__102 __instance)
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
        
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.SetUpRoleText))]
        [HarmonyPostfix]
        public static void RoleTextPatch(IntroCutscene __instance)
        {
            foreach (var mode in GameModeManager.Modes)
            {
                if (mode.Enabled)
                {
                    if (!mode.GetIntroScreen(PlayerControl.LocalPlayer).HasValue)
                        continue;
                    
                    var scene = __instance;
                    var intro = mode.GetIntroScreen(PlayerControl.LocalPlayer).GetValueOrDefault();
                    
                    if (!intro.OverrideRole)
                        continue;

                    scene.RoleText.text = intro.Role;
                    scene.RoleBlurbText.text = intro.RoleDescription;
                    scene.RoleText.color = intro.RoleColor;
                    scene.RoleBlurbText.color = intro.RoleColor;
                }
            }
        }
        
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        [HarmonyPostfix]
        public static void TeamTextPatch(IntroCutscene __instance)
        {
            foreach (var mode in GameModeManager.Modes)
            {
                if (mode.Enabled)
                {
                    if (!mode.GetIntroScreen(PlayerControl.LocalPlayer).HasValue)
                        continue;
                    
                    var scene = __instance;
                    var intro = mode.GetIntroScreen(PlayerControl.LocalPlayer).GetValueOrDefault();

                    if (!intro.OverrideTeam)
                        continue;
                    
                    scene.TeamTitle.text = intro.Team;
                    scene.ImpostorText.gameObject.SetActive(true);
                    scene.ImpostorText.text = intro.TeamDescription;
                    scene.BackgroundBar.material.color = intro.TeamColor;
                    scene.TeamTitle.color = intro.TeamColor;
                }
            }
        }
        
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        [HarmonyPrefix]
        public static void RoleTeamPatch(IntroCutscene __instance, [HarmonyArgument(0)] ref List<PlayerControl> yourTeam)
        {
            foreach (var mode in GameModeManager.Modes)
            {
                if (mode.Enabled)
                {
                    if (!mode.GetIntroScreen(PlayerControl.LocalPlayer).HasValue)
                        continue;
                    if (!mode.GetIntroScreen(PlayerControl.LocalPlayer).GetValueOrDefault().OverrideTeam)
                        continue;
                    
                    var _yourTeam = new List<PlayerControl>();
                    mode.GetIntroScreen(PlayerControl.LocalPlayer).GetValueOrDefault().TeamMembers
                        .Do(member => _yourTeam.Add(member.GetPlayer()));
                    yourTeam = _yourTeam;
                }
            }
        }
        
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        [HarmonyPostfix]
        static void SetupGameModeSetting(AmongUsClient __instance)
        {
            GameModeManager.GameModeOption.Values = GameModeManager.Modes.ConvertAll(mode => mode.Name).Prepend("None").ToList().ConvertAll(mode => (StringNames) CustomStringName.Register(mode));
        }
    }
}