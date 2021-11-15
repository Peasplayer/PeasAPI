using System;
using System.Linq;
using HarmonyLib;
using PeasAPI.CustomButtons;
using PeasAPI.CustomEndReason;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace PeasAPI.Roles
{
    public class Patches
    {

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetRole))]
        public static class PlayerControlSetInfectedPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (AmongUsClient.Instance.AmHost)
                {
                    RoleManager.RpcResetRoles();

                    EndReasonManager.Reset();

                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player.Data.Role.IsImpostor)
                            RoleManager.Impostors.Add(player.PlayerId);
                        else
                            RoleManager.Crewmates.Add(player.PlayerId);
                    }

                    if (PeasApi.EnableRoles && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                    {
                        foreach (var role in RoleManager.Roles)
                        {
                            if (role.Team == Team.Impostor)
                            {
                                for (int i = 1; i <= role.Limit && RoleManager.Impostors.Count >= 1; i++)
                                {
                                    var member =
                                        RoleManager.Impostors[PeasApi.Random.Next(0, RoleManager.Impostors.Count)];
                                    member.GetPlayer().RpcSetRole(role);
                                    RoleManager.Impostors.Remove(member);
                                }
                            }
                            else if (role.Team != Team.Impostor)
                            {
                                for (int i = 1; i <= role.Limit && RoleManager.Crewmates.Count >= 1; i++)
                                {
                                    var member =
                                        RoleManager.Crewmates[PeasApi.Random.Next(0, RoleManager.Crewmates.Count)];
                                    member.GetPlayer().RpcSetRole(role);
                                    RoleManager.Crewmates.Remove(member);
                                }
                            }
                        }
                    }
                }
                else
                {
                    RoleManager.Crewmates.Clear();
                    RoleManager.Impostors.Clear();

                    foreach (var player in PlayerControl.AllPlayerControls)
                    {
                        if (player.Data.Role.IsImpostor)
                        {
                            if (player.GetRole() == null)
                                RoleManager.Impostors.Add(player.PlayerId);
                        }
                        else
                        {
                            if (player.GetRole() == null)
                                RoleManager.Crewmates.Add(player.PlayerId);
                        }
                    }
                }
                
                if (PeasApi.EnableRoles)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        role._OnGameStart();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__18), nameof(IntroCutscene._CoBegin_d__18.MoveNext))]
        public static class IntroCutscenePatch
        {
            public static void Prefix(IntroCutscene._CoBegin_d__18 __instance)
            {
                if (PeasApi.EnableRoles)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        if (PlayerControl.LocalPlayer.IsRole(role))
                        {
                            if (role.Team == Team.Alone)
                            {
                                var yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                                yourTeam.Add(PlayerControl.LocalPlayer);
                                __instance.yourTeam = yourTeam;
                            }
                            else if (role.Team == Team.Role)
                            {
                                var yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                                yourTeam.Add(PlayerControl.LocalPlayer);
                                foreach (var player in role.Members)
                                {
                                    if (player != PlayerControl.LocalPlayer.PlayerId)
                                        yourTeam.Add(player.GetPlayer());
                                }

                                __instance.yourTeam = yourTeam;
                            }
                            else if (role.Team == Team.Impostor)
                            {
                                var yourTeam = new Il2CppSystem.Collections.Generic.List<PlayerControl>();
                                yourTeam.Add(PlayerControl.LocalPlayer);
                                foreach (var player in role.Members)
                                {
                                    if (player != PlayerControl.LocalPlayer.PlayerId &&
                                        player.GetPlayer().Data.Role.IsImpostor)
                                        yourTeam.Add(player.GetPlayer());
                                }

                                __instance.yourTeam = yourTeam;
                            }
                        }
                    }
                }
            }

            public static void Postfix(IntroCutscene._CoBegin_d__18 __instance)
            {
                if (PeasApi.EnableRoles)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        if (PlayerControl.LocalPlayer.IsRole(role))
                        {
                            var scene = __instance.__4__this;

                            scene.RoleText.text = role.Name;
                            scene.ImpostorText.gameObject.SetActive(true);
                            scene.ImpostorText.text = role.Description;
                            scene.BackgroundBar.material.color = role.Color;
                            scene.RoleText.color = role.Color;
                        }
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
                if (PeasApi.GameStarted && PeasApi.EnableRoles)
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
                if (PeasApi.EnableRoles)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        role._OnMeetingUpdate(__instance);
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        public static class PlayerControlFixedUpdatePatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                if (PeasApi.GameStarted && PeasApi.EnableRoles)
                {
                    var localRole = PlayerControl.LocalPlayer.GetRole();

                    if (localRole != null && __instance.PlayerId == PlayerControl.LocalPlayer.PlayerId)
                    {
                        HudManager.Instance.KillButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && localRole.CanKill(null));

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
                        }
                        
                        HudManager.Instance.SabotageButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && localRole.CanSabotage(null));

                        if (localRole.CanSabotage(null) && __instance.CanMove && !__instance.Data.IsDead)
                        {
                            HudManager.Instance.SabotageButton.SetEnabled();
                        }
                        else
                        {
                            HudManager.Instance.SabotageButton.SetDisabled();
                        }
                        
                        HudManager.Instance.ImpostorVentButton.gameObject.SetActive(!PlayerControl.LocalPlayer.Data.IsDead && localRole.CanVent);

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

        /*[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
        public static class PlayerControlMurderPlayerPatch
        {
            public static void Prefix(PlayerControl __instance, out bool __state)
            {
                __state = __instance.Data.Role.IsImpostor;

                __instance.Data.Role.IsImpostor = true;
            }

            public static void Postfix(PlayerControl __instance, bool __state)
            {
                __instance.Data.Role.IsImpostor = __state;
            }
        }*/

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FindClosestTarget))]
        public static class PlayerControlFindClosestTargetPatch
        {
            public static bool Prefix(PlayerControl __instance, out PlayerControl __result)
            {
                if (__instance.GetRole() != null)
                {
                    __result = __instance.GetRole().FindClosestTarget(__instance);
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

        /*[HarmonyPatch(typeof(UseButton), nameof(UseButton.SetTarget))]
        public static class UseButtonSetTargetPatch
        {
            public static void Postfix(UseButton __instance, [HarmonyArgument(0)] IUsable target)
            {
                if (PeasApi.GameStarted && PlayerControl.LocalPlayer.GetRole() != null &&
                    PlayerControl.LocalPlayer.GetRole().CanSabotage(null) && PlayerControl.LocalPlayer.CanMove)
                {
                    __instance.Refresh();
                    if (target == null)
                    {
                        __instance.currentButtonShown = __instance.otherButtons[ImageNames.SabotageButton];
                        __instance.currentButtonShown.Show();
                    }
                    else
                    {
#pragma warning disable 184
                        if (target is Vent)
                        {
                            __instance.currentButtonShown = __instance.otherButtons[ImageNames.VentButton];
                        }
                        else if (target is OptionsConsole)
#pragma warning restore 184
                        {
                            __instance.currentButtonShown = __instance.otherButtons[ImageNames.OptionsButton];
                        }
                        else
                        {
                            __instance.currentButtonShown = __instance.otherButtons[target.UseIcon];
                            __instance.currentButtonShown.graphic.color = UseButtonManager.EnabledColor;
                            __instance.currentButtonShown.text.color = UseButtonManager.EnabledColor;
                        }

                        __instance.currentButtonShown.Show(target.PercentCool);
                    }
                }
            }
        }*/

        [HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
        public static class UseButtonManagerDoClickPatch
        {
            public static bool Prefix(SabotageButton __instance)
            {
                if (__instance.isActiveAndEnabled && PeasApi.GameStarted)
                {
                    var role = PlayerControl.LocalPlayer.GetRole();

                    if (role == null)
                        return true;

                    if (!role.CanSabotage(null))
                        return true;

                    HudManager.Instance.ShowMap((Action<MapBehaviour>) (map =>
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

        [HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
        public static class VentCanUsePatch
        {
            public static void Postfix(Vent __instance, [HarmonyArgument(1)] ref bool canUse, [HarmonyArgument(2)] ref bool couldUse, ref float __result) 
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
