using System.Linq;
using HarmonyLib;
using PeasAPI.CustomEndReason;
using PeasAPI.GameModes;
using PeasAPI.Roles;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.InitializeRoles)]
    public class RpcInitializeRoles : PlayerCustomRpc<PeasAPI>
    {
        public RpcInitializeRoles(PeasAPI plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                Roles.RoleManager.RpcResetRoles();

                EndReasonManager.Reset();

                if (AmongUsClient.Instance.GameMode != global::GameModes.FreePlay)
                {
                    foreach (var role in Roles.RoleManager.Roles)
                    {
                        if (GameModeManager.GetCurrentGameMode() != null && !GameModeManager.GetCurrentGameMode().RoleWhitelist.Contains(role.GetType()))
                            continue;
                        if (GameModeManager.GetCurrentGameMode() == null && role.GameModeWhitelist.Length != 0)
                            continue;
                        if (role.GameModeWhitelist.Length != 0 && GameModeManager.GetCurrentGameMode() != null &&
                            !role.GameModeWhitelist.Contains(GameModeManager.GetCurrentGameMode().GetType()))
                            continue;
                        
                        if (role.Team == Team.Impostor)
                        {
                            var nonRoleImpostors = Roles.RoleManager.Impostors.Where(id =>
                                id.GetPlayer().Data.Role.IsSimpleRole &&
                                !RoleManager.IsGhostRole(id.GetPlayerInfo().Role.Role) && id.GetPlayer().GetRole() == null).ToArray();
                            if (nonRoleImpostors.Length == 0)
                                continue;
                            
                            for (int i = 1; i <= role.Limit && nonRoleImpostors.Length >= 1; i++)
                            {
                                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                                    PlayerControl.LocalPlayer.GetRole() == null)
                                {
                                    PlayerControl.LocalPlayer.RpcSetRole(role);
                                    continue;
                                }

                                var member =
                                    nonRoleImpostors[PeasAPI.Random.Next(0, nonRoleImpostors.Length)];
                                member.GetPlayer().RpcSetRole(role);
                            }
                        }
                        else if (role.Team != Team.Impostor)
                        {
                            var nonRoleCrewmates = Roles.RoleManager.Crewmates.Where(id =>
                                id.GetPlayer().Data.Role.IsSimpleRole &&
                                !RoleManager.IsGhostRole(id.GetPlayerInfo().Role.Role) && id.GetPlayer().GetRole() == null).ToArray();
                            if (nonRoleCrewmates.Length == 0)
                                continue;
                            
                            for (int i = 1; i <= role.Limit && nonRoleCrewmates.Length >= 1; i++)
                            {
                                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                                    PlayerControl.LocalPlayer.GetRole() == null)
                                {
                                    PlayerControl.LocalPlayer.RpcSetRole(role);
                                    continue;
                                }
                                
                                var member =
                                    nonRoleCrewmates[PeasAPI.Random.Next(0, nonRoleCrewmates.Length)];
                                member.GetPlayer().RpcSetRole(role);
                            }
                        }
                    }
                }
            }
            else
            {
                EndReasonManager.Reset();
            }
            
            Roles.RoleManager.Roles.Do(r => r.OnGameStart());
        }
    }
}