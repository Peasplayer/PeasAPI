using System.Linq;
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

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.IsImpostor)
                        Roles.RoleManager.Impostors.Add(player.PlayerId);
                    else
                        Roles.RoleManager.Crewmates.Add(player.PlayerId);
                }

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
                                !RoleManager.IsGhostRole(id.GetPlayerInfo().Role.Role)).ToArray();
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
                                Roles.RoleManager.Impostors.Remove(member);
                            }
                        }
                        else if (role.Team != Team.Impostor)
                        {
                            var nonRoleCrewmates = Roles.RoleManager.Crewmates.Where(id =>
                                id.GetPlayer().Data.Role.IsSimpleRole &&
                                !RoleManager.IsGhostRole(id.GetPlayerInfo().Role.Role)).ToArray();
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
                                Roles.RoleManager.Crewmates.Remove(member);
                            }
                        }
                    }
                }
            }
            else
            {
                Roles.RoleManager.Crewmates.Clear();
                Roles.RoleManager.Impostors.Clear();
                
                EndReasonManager.Reset();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.IsImpostor)
                    {
                        if (player.GetRole() == null)
                            Roles.RoleManager.Impostors.Add(player.PlayerId);
                    }
                    else
                    {
                        if (player.GetRole() == null)
                            Roles.RoleManager.Crewmates.Add(player.PlayerId);
                    }
                }
            }
            
            foreach (var role in Roles.RoleManager.Roles)
            {
                role._OnGameStart();
            }
        }
    }
}