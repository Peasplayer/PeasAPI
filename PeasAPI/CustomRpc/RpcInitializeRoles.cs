using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PeasAPI.CustomEndReason;
using PeasAPI.GameModes;
using PeasAPI.Roles;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint)CustomRpcCalls.InitializeRoles)]
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
                    var rolesForPlayers = new List<BaseRole>();

                    var roles = Roles.RoleManager.Roles.Where(role => role.GetChance() == 100);
                    foreach (var role in roles)
                    {
                        for (int i = 0; i < role.GetCount(); i++)
                        {
                            rolesForPlayers.Add(role);
                        }
                    }

                    roles = from role in Roles.RoleManager.Roles
                        where role.GetCount() > 0 && role.GetChance() > 0 && role.GetChance() < 100
                        select role;
                    foreach (var role in roles)
                    {
                        for (int i = 0; i < role.GetCount(); i++)
                        {
                            rolesForPlayers.Add(role);
                        }
                    }

                    foreach (var role in rolesForPlayers)
                    {
                        AssignRole(role);
                    }
                }
            }
            else
            {
                EndReasonManager.Reset();
            }

            Roles.RoleManager.Roles.Do(r => r.OnGameStart());
        }

        private void AssignRole(BaseRole role)
        {
            if (GameModeManager.GetCurrentGameMode() != null &&
                !GameModeManager.GetCurrentGameMode().RoleWhitelist.Contains(role.GetType()))
                return;
            if (GameModeManager.GetCurrentGameMode() == null && role.GameModeWhitelist.Length != 0)
                return;
            if (role.GameModeWhitelist.Length != 0 && GameModeManager.GetCurrentGameMode() != null &&
                !role.GameModeWhitelist.Contains(GameModeManager.GetCurrentGameMode().GetType()))
                return;

            if (role.Team == Team.Impostor)
            {
                var nonRoleImpostors = Roles.RoleManager.Impostors.Where(id =>
                        id.GetPlayer().Data.Role.IsSimpleRole &&
                        !RoleManager.IsGhostRole(id.GetPlayerInfo().Role.Role) && id.GetPlayer().GetRole() == null)
                    .ToArray();
                
                if (nonRoleImpostors.Length == 0)
                    return;

                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                    PlayerControl.LocalPlayer.GetRole() == null)
                {
                    PlayerControl.LocalPlayer.RpcSetRole(role);
                    return;
                }

                var chance = HashRandom.Next(101);
                if (chance < role.GetChance())
                {
                    var member =
                        nonRoleImpostors[PeasAPI.Random.Next(0, nonRoleImpostors.Length)];
                    member.GetPlayer().RpcSetRole(role);
                }
            }
            else if (role.Team != Team.Impostor)
            {
                var nonRoleCrewmates = Roles.RoleManager.Crewmates.Where(id =>
                        id.GetPlayer().Data.Role.IsSimpleRole &&
                        !RoleManager.IsGhostRole(id.GetPlayerInfo().Role.Role) && id.GetPlayer().GetRole() == null)
                    .ToArray();
                
                if (nonRoleCrewmates.Length == 0)
                    return;
                
                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                    PlayerControl.LocalPlayer.GetRole() == null)
                {
                    PlayerControl.LocalPlayer.RpcSetRole(role);
                    return;
                }
                
                var chance = HashRandom.Next(101);
                if (chance < role.GetChance())
                {
                    var member =
                        nonRoleCrewmates[PeasAPI.Random.Next(0, nonRoleCrewmates.Length)];
                    member.GetPlayer().RpcSetRole(role);
                }
            }
        }
    }
}