using HarmonyLib;
using UnhollowerBaseLib;
using UnityEngine;
using Object = Il2CppSystem.Object;

namespace PeasAPI.Roles
{
    public class Patches
    {
        [HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.Start))]
        public static class ShipStatusStartPatch
        {
            public static void Prefix(PlayerControl __instance)
            {
                if (PeasApi.EnableRoles)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        role._OnGameStart();
                    }
                }
            }
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetInfected))]
        public static class PlayerControlRpcSetInfectedPatch
        {
            public static void Postfix(PlayerControl __instance)
            {
                RoleManager.ResetRoles();
                RoleManager.RpcResetRoles();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.IsImpostor)
                        RoleManager.Impostors.Add(player.PlayerId);
                    else
                        RoleManager.Crewmates.Add(player.PlayerId);
                }

                if (PeasApi.EnableRoles)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        if (role.Team == Team.Impostor)
                        {
                            foreach (var player in RoleManager.Impostors)
                            {
                                for (int i = 1; i <= role.Limit && RoleManager.Impostors.Count >= 1; i++)
                                {
                                    var member =
                                        RoleManager.Impostors[PeasApi.Random.Next(0, RoleManager.Impostors.Count)];
                                    member.GetPlayer().RpcSetRole(role);
                                    RoleManager.Impostors.Remove(member);
                                }
                            }
                        }
                        else if (role.Team != Team.Impostor)
                        {
                            for (int i = 1; i <= role.Limit && RoleManager.Crewmates.Count >= 1; i++)
                            {
                                var member = RoleManager.Crewmates[PeasApi.Random.Next(0, RoleManager.Crewmates.Count)];
                                member.GetPlayer().RpcSetRole(role);
                                RoleManager.Crewmates.Remove(member);
                            }
                        }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(IntroCutscene._CoBegin_d__14), nameof(IntroCutscene._CoBegin_d__14.MoveNext))]
        public static class IntroCutscenePatch
        {
            public static void Prefix(IntroCutscene._CoBegin_d__14 __instance)
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
                                        player.GetPlayer().Data.IsImpostor)
                                        yourTeam.Add(player.GetPlayer());
                                }

                                __instance.yourTeam = yourTeam;
                            }
                        }
                    }
                }
            }

            public static void Postfix(IntroCutscene._CoBegin_d__14 __instance)
            {
                if (PeasApi.EnableRoles)
                {
                    foreach (var role in RoleManager.Roles)
                    {
                        if (PlayerControl.LocalPlayer.IsRole(role))
                        {
                            var scene = __instance.__4__this;

                            scene.Title.text = role.Name;
                            scene.ImpostorText.gameObject.SetActive(true);
                            scene.ImpostorText.text = role.Description;
                            scene.BackgroundBar.material.color = role.Color;
                            scene.Title.color = role.Color;
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

        [HarmonyPatch(typeof(PlayerControl._CoSetTasks_d__83), nameof(PlayerControl._CoSetTasks_d__83.MoveNext))]
        public static class PlayerControlSetTasks
        {
            public static void Postfix(PlayerControl._CoSetTasks_d__83 __instance)
            {
                if (__instance == null) 
                    return;
                
                var player = __instance.__4__this;
                var role = player.GetRole();

                if (role == null) 
                    return;
                
                if (player.PlayerId != PlayerControl.LocalPlayer.PlayerId) 
                    return;
                
                var task = new GameObject(role.Name + "Task").AddComponent<ImportantTextTask>();
                task.transform.SetParent(player.transform, false);
                task.Text = $"</color>Role: {role.Color.GetTextColor()}{role.Name}\n{role.TaskText}</color>";
                player.myTasks.Insert(0, task);
            }
        }
    }
}