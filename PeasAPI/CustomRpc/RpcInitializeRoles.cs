using PeasAPI.CustomEndReason;
using PeasAPI.Roles;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.InitializeRoles)]
    public class RpcInitializeRoles : PlayerCustomRpc<PeasApi>
    {
        public RpcInitializeRoles(PeasApi plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;

        public override void Handle(PlayerControl innerNetObject)
        {
            if (AmongUsClient.Instance.AmHost)
            {
                PeasAPI.Roles.RoleManager.RpcResetRoles();

                EndReasonManager.Reset();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.IsImpostor)
                        PeasAPI.Roles.RoleManager.Impostors.Add(player.PlayerId);
                    else
                        PeasAPI.Roles.RoleManager.Crewmates.Add(player.PlayerId);
                }

                if (PeasApi.EnableRoles && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                {
                    foreach (var role in PeasAPI.Roles.RoleManager.Roles)
                    {
                        if (role.Team == Team.Impostor)
                        {
                            for (int i = 1; i <= role.Limit && PeasAPI.Roles.RoleManager.Impostors.Count >= 1; i++)
                            {
                                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                                    PlayerControl.LocalPlayer.GetRole() == null)
                                {
                                    PlayerControl.LocalPlayer.RpcSetRole(role);
                                    continue;
                                }
                                
                                var member =
                                    PeasAPI.Roles.RoleManager.Impostors[PeasApi.Random.Next(0, PeasAPI.Roles.RoleManager.Impostors.Count)];
                                member.GetPlayer().RpcSetRole(role);
                                PeasAPI.Roles.RoleManager.Impostors.Remove(member);
                            }
                        }
                        else if (role.Team != Team.Impostor)
                        {
                            for (int i = 1; i <= role.Limit && PeasAPI.Roles.RoleManager.Crewmates.Count >= 1; i++)
                            {
                                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                                    PlayerControl.LocalPlayer.GetRole() == null)
                                {
                                    PlayerControl.LocalPlayer.RpcSetRole(role);
                                    continue;
                                }
                                
                                var member =
                                    PeasAPI.Roles.RoleManager.Crewmates[PeasApi.Random.Next(0, PeasAPI.Roles.RoleManager.Crewmates.Count)];
                                member.GetPlayer().RpcSetRole(role);
                                PeasAPI.Roles.RoleManager.Crewmates.Remove(member);
                            }
                        }
                    }
                }
            }
            else
            {
                PeasAPI.Roles.RoleManager.Crewmates.Clear();
                PeasAPI.Roles.RoleManager.Impostors.Clear();

                foreach (var player in PlayerControl.AllPlayerControls)
                {
                    if (player.Data.Role.IsImpostor)
                    {
                        if (player.GetRole() == null)
                            PeasAPI.Roles.RoleManager.Impostors.Add(player.PlayerId);
                    }
                    else
                    {
                        if (player.GetRole() == null)
                            PeasAPI.Roles.RoleManager.Crewmates.Add(player.PlayerId);
                    }
                }
            }

            if (PeasApi.EnableRoles)
            {
                foreach (var role in PeasAPI.Roles.RoleManager.Roles)
                {
                    role._OnGameStart();
                }
            }
        }
    }
}