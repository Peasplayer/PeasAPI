using System;
using System.Linq;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;
using PeasAPI.CustomRpc;
using Reactor.Networking;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace PeasAPI.Roles
{
    [HarmonyPatch]
    public static class Patches
    {
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        [HarmonyPrefix]
        public static void OnGameEndPatch(AmongUsClient __instance)
        {
            foreach (var role in RoleManager.Roles)
            {
                role._OnGameStop();
            }
        }
        
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameEnd))]
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.ExitGame))]
        [HarmonyPrefix]
        public static void ResetRolePatch(AmongUsClient __instance)
        {
            if (PlayerControl.LocalPlayer)
                PlayerControl.LocalPlayer.SetRole(null);
        }

        [HarmonyPatch(typeof(global::RoleManager), nameof(global::RoleManager.SelectRoles))]
        [HarmonyPostfix]
        public static void InitializeRolesPatch()
        {
            Rpc<RpcInitializeRoles>.Instance.Send();
        }

        [HarmonyPatch(typeof(global::RoleManager), nameof(global::RoleManager.AssignRolesFromList))]
        [HarmonyPrefix]
        public static bool ChangeImpostors(global::RoleManager __instance,
            [HarmonyArgument(0)] List<GameData.PlayerInfo> players, [HarmonyArgument(1)] int teamMax,
            [HarmonyArgument(2)] List<RoleTypes> roleList, [HarmonyArgument(3)] ref int rolesAssigned)
        {
            while (roleList.Count > 0 && players.Count > 0 && rolesAssigned < teamMax)
            {
                int index = HashRandom.FastNext(roleList.Count);
                RoleTypes roleType = roleList[index];
                roleList.RemoveAt(index);
                int index2 = global::RoleManager.IsImpostorRole(roleType) && RoleManager.HostMod.IsImpostor
                    ? 0
                    : HashRandom.FastNext(players.Count);
                players[index2].Object.RpcSetRole(roleType);
                players.RemoveAt(index2);
                rolesAssigned++;
            }

            return false;
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.SetUpRoleText))]
        [HarmonyPostfix]
        public static void RoleTextPatch(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.GetRole() != null)
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                var scene = __instance;

                scene.RoleText.text = role.Name;
                scene.RoleBlurbText.text = role.Description;
                scene.RoleText.color = role.Color;
                scene.RoleBlurbText.color = role.Color;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        [HarmonyPostfix]
        public static void TeamTextPatch(IntroCutscene __instance)
        {
            if (PlayerControl.LocalPlayer.GetRole() != null)
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                var scene = __instance;

                scene.TeamTitle.text = role.Name;
                scene.ImpostorText.gameObject.SetActive(true);
                scene.ImpostorText.text = role.Description;
                scene.BackgroundBar.material.color = role.Color;
                scene.TeamTitle.color = role.Color;
            }
        }

        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginImpostor))]
        [HarmonyPatch(typeof(IntroCutscene), nameof(IntroCutscene.BeginCrewmate))]
        [HarmonyPrefix]
        public static void RoleTeamPatch(IntroCutscene __instance,
            [HarmonyArgument(0)] ref List<PlayerControl> yourTeam)
        {
            if (PlayerControl.LocalPlayer.GetRole() != null)
            {
                var role = PlayerControl.LocalPlayer.GetRole();
                if (role.Team == Team.Alone)
                {
                    yourTeam = new List<PlayerControl>();
                    yourTeam.Add(PlayerControl.LocalPlayer);
                }
                else if (role.Team == Team.Role)
                {
                    yourTeam = new List<PlayerControl>();
                    yourTeam.Add(PlayerControl.LocalPlayer);
                    foreach (var player in role.Members)
                    {
                        if (player != PlayerControl.LocalPlayer.PlayerId)
                            yourTeam.Add(player.GetPlayer());
                    }
                }
                else if (role.Team == Team.Impostor)
                {
                    yourTeam = new List<PlayerControl>();
                    yourTeam.Add(PlayerControl.LocalPlayer);
                    foreach (var player in role.Members)
                    {
                        if (player != PlayerControl.LocalPlayer.PlayerId &&
                            player.GetPlayer().Data.Role.IsImpostor)
                            yourTeam.Add(player.GetPlayer());
                    }
                }
            }
        }

        [HarmonyPatch(typeof(TranslationController), nameof(TranslationController.GetString), typeof(StringNames),
            typeof(Il2CppReferenceArray<Object>))]
        public class TranslationControllerPatch
        {
            public static bool Prefix(ref string __result, [HarmonyArgument(0)] StringNames name)
            {
                if (ExileController.Instance != null && ExileController.Instance.exiled != null)
                {
                    if (name == StringNames.ExileTextPN || name == StringNames.ExileTextSN)
                    {
                        foreach (var role in RoleManager.Roles)
                        {
                            if (ExileController.Instance.exiled.Object.IsRole(role))
                            {
                                if (role.Members.Count > 1)
                                {
                                    __result = $"{ExileController.Instance.exiled.PlayerName} was a {role.Name}.";
                                }
                                else
                                {
                                    __result = $"{ExileController.Instance.exiled.PlayerName} was the {role.Name}.";
                                }
                            }
                        }

                        if (__result == null)
                        {
                            if (RoleManager.Impostors.Count == 1)
                            {
                                __result = ExileController.Instance.exiled.PlayerName + " was not The Impostor.";
                            }
                            else
                            {
                                __result = ExileController.Instance.exiled.PlayerName + " was not An Impostor.";
                            }
                        }

                        return false;
                    }
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class HudManagerUpdatePatch
        {
            public static void Prefix(HudManager __instance)
            {
                if (PeasAPI.GameStarted)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        role._OnUpdate();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        public static class MeetingUpdatePatch
        {
            public static void Postfix(MeetingHud __instance)
            {
                foreach (var role in RoleManager.Roles) 
                {
                    role._OnMeetingUpdate(__instance);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class PlayerControlFixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (PeasAPI.GameStarted)
                {
                    var localRole = PlayerControl.LocalPlayer.GetRole();

                    if (localRole != null && __instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        HudManager.Instance.KillButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead &&
                                                                            localRole.CanKill(null));

                        if (localRole.CanKill(null) && __instance.CanMove && !__instance.Data.IsDead)
                        {
                            if (!__instance.Data.Role.IsImpostor)
                                __instance.SetKillTimer(__instance.killTimer - Time.fixedDeltaTime);
                            PlayerControl target = __instance.FindClosestTarget(false);
                            HudManager.Instance.KillButton.SetTarget(target);
                        }
                        else
                        {
                            HudManager.Instance.KillButton.SetTarget(null);
                            HudManager.Instance.KillButton.SetDisabled();
                        }

                        HudManager.Instance.SabotageButton.gameObject.SetActive(
                            !PlayerControl.LocalPlayer.Data.IsDead && localRole.CanSabotage(null));

                        if (localRole.CanSabotage(null) && __instance.CanMove && !__instance.Data.IsDead)
                        {
                            HudManager.Instance.SabotageButton.SetEnabled();
                        }
                        else
                        {
                            HudManager.Instance.SabotageButton.SetDisabled();
                        }

                        HudManager.Instance.ImpostorVentButton.gameObject.SetActive(
                            !PlayerControl.LocalPlayer.Data.IsDead && localRole.CanVent);

                        if (localRole.CanVent && __instance.CanMove && !__instance.Data.IsDead)
                        {
                            HudManager.Instance.ImpostorVentButton.SetEnabled();
                        }
                        else
                        {
                            HudManager.Instance.ImpostorVentButton.SetDisabled();
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CheckMurder))]
        public static class PlayerControlMurderPlayerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (AmongUsClient.Instance.IsGameOver || !AmongUsClient.Instance.AmHost)
                    return false;
                if (!target || __instance.Data == null || __instance.Data.IsDead || !__instance.Data.Role.IsImpostor && __instance.GetRole() == null || !__instance.Data.Role.IsImpostor && !__instance.GetRole().CanKill() || __instance.Data.Disconnected)
                {
                    int num = target ? target.PlayerId : -1;
                    Debug.LogWarning(string.Format("Bad kill from {0} to {1}", __instance.PlayerId, num));
                    return false;
                }
                if (target.Data == null || target.Data.IsDead || target.inVent)
                {
                    Debug.LogWarning("Invalid target data for kill");
                    return false;
                }
                __instance.RpcMurderPlayer(target);
                return false;
            }
        }

        [HarmonyPatch(typeof(KillButton), nameof(KillButton.SetTarget))]
        public static class KillButtonManagerSetTargetPatch
        {
            public static bool Prefix(KillButton __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (!PlayerControl.LocalPlayer || PlayerControl.LocalPlayer.Data == null || !PlayerControl.LocalPlayer.Data.Role)
                    return false;
                RoleTeamTypes teamType = PlayerControl.LocalPlayer.GetRole() == null ? PlayerControl.LocalPlayer.Data.Role.TeamType : PlayerControl.LocalPlayer.GetRole().CanKill() ? RoleTeamTypes.Impostor : RoleTeamTypes.Crewmate;
                if (__instance.currentTarget && __instance.currentTarget != target)
                {
                    __instance.currentTarget.ToggleHighlight(false, teamType);
                }
                __instance.currentTarget = target;
                if (__instance.currentTarget)
                {
                    __instance.currentTarget.ToggleHighlight(true, teamType);
                    __instance.SetEnabled();
                    return false;
                }
                __instance.SetDisabled();
                return false;
            }
        }
        
        [HarmonyPatch(typeof(KillButton), nameof(KillButton.DoClick))]
        public static class KillButtonManagerDoClickPatch
        {
            public static bool Prefix(KillButton __instance)
            {
                if (__instance.isActiveAndEnabled && __instance.currentTarget && !__instance.isCoolingDown && !PlayerControl.LocalPlayer.Data.IsDead && PlayerControl.LocalPlayer.CanMove)
                {
                    PlayerControl.LocalPlayer.CmdCheckMurder(__instance.currentTarget);
                    __instance.SetTarget(null);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
        public static class PlayerControlSetKillTimerPatch
        {
            public static bool Prefix(PlayerControl __instance, [HarmonyArgument(0)] float time)
            {
                if (__instance.GetRole() != null && __instance.GetRole().CanKill() || __instance.Data.Role.CanUseKillButton)
                {
                    if (PlayerControl.GameOptions.KillCooldown <= 0f)
                        return false;
                    __instance.killTimer = Mathf.Clamp(time, 0f, PlayerControl.GameOptions.KillCooldown);
                    DestroyableSingleton<HudManager>.Instance.KillButton.SetCoolDown(__instance.killTimer, PlayerControl.GameOptions.KillCooldown);
                }
                return false;
            }
        }
        
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcMurderPlayer))]
        public static class PlayerControlRpcMurderPlayerPatch
        {
            public static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] PlayerControl target)
            {
                if (__instance.GetRole() != null)
                {
                    __instance.GetRole()._OnKill(target);
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FindClosestTarget))]
        public static class PlayerControlFindClosestTargetPatch
        {
            public static bool Prefix(PlayerControl __instance, out PlayerControl __result,
                [HarmonyArgument(0)] bool protecting)
            {
                if (__instance.GetRole() != null)
                {
                    __result = __instance.GetRole().FindClosestTarget(__instance, protecting);
                    return false;
                }

                __result = null;
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

                var player = __instance.__4__this;
                var role = player.GetRole();

                if (role == null)
                    return;

                if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId)
                    return;

                if (!role.HasToDoTasks)
                    player.ClearTasks();

                if (role.TaskText == null)
                    return;

                var task = new GameObject(role.Name + "Task").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = $"</color>Role: {role.Color.GetTextColor()}{role.Name}\n{role.TaskText}</color>";
                player.myTasks.Insert(0, task);
            }
        }

        [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
        public static class UseButtonManagerDoClickPatch
        {
            public static bool Prefix(SabotageButton __instance)
            {
                if (__instance.isActiveAndEnabled && PeasAPI.GameStarted)
                {
                    var role = PlayerControl.LocalPlayer.GetRole();

                    if (role == null)
                        return true;

                    HudManager.Instance.ShowMap((Action<MapBehaviour>)(map =>
                    {
                        foreach (MapRoom mapRoom in map.infectedOverlay.rooms.ToArray()
                            .Where(room => !role.CanSabotage(room.room)))
                        {
                            mapRoom.gameObject.SetActive(false);
                        }

                        map.ShowSabotageMap();
                    }));

                    return false;
                }

                return true;
            }
        }
        
        [HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.ShowSabotageMap))]
        public static class MapBehaviourShowSabotageMapPatch
        {
            public static bool Prefix(MapBehaviour __instance)
            {
                if (PeasAPI.GameStarted)
                {
                    var role = PlayerControl.LocalPlayer.GetRole();

                    if (role == null)
                        return true;
                    
                    HudManager.Instance.ShowMap((Action<MapBehaviour>) (map =>
                    {
                        foreach (MapRoom mapRoom in map.infectedOverlay.rooms.ToArray())
                        {
                            mapRoom.gameObject.SetActive(role.CanSabotage(mapRoom.room));
                        }
                    }));
                    
                    //return false;
                }

                return true;
            }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        [HarmonyPriority(Priority.First)]
        public static class VentCanUsePatch
        {
            public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool canUse,
                [HarmonyArgument(2)] ref bool couldUse, ref float __result)
            {
                BaseRole role = PlayerControl.LocalPlayer.GetRole();

                if (role == null)
                    return;

                couldUse = canUse = role.CanVent;
                __result = float.MaxValue;

                if (canUse)
                {
                    Vector3 center = PlayerControl.LocalPlayer.Collider.bounds.center;
                    Vector3 position = __instance.transform.position;

                    __result = Vector2.Distance(center, position);
                    canUse &= (__result <= __instance.UsableDistance &&
                               !PhysicsHelpers.AnythingBetween(PlayerControl.LocalPlayer.Collider, center, position,
                                   Constants.ShipOnlyMask, false));
                }
            }
        }
    }
}