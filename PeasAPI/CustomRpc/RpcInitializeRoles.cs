using PeasAPI.CustomEndReason;
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

                if (PeasAPI.EnableRoles && AmongUsClient.Instance.GameMode != GameModes.FreePlay)
                {
                    foreach (var role in Roles.RoleManager.Roles)
                    {
                        if (role.Team == Team.Impostor)
                        {
                            for (int i = 1; i <= role.Limit && Roles.RoleManager.Impostors.Count >= 1; i++)
                            {
                                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                                    PlayerControl.LocalPlayer.GetRole() == null)
                                {
                                    PlayerControl.LocalPlayer.RpcSetRole(role);
                                    continue;
                                }
                                
                                var member =
                                    Roles.RoleManager.Impostors[PeasAPI.Random.Next(0, Roles.RoleManager.Impostors.Count)];
                                member.GetPlayer().RpcSetRole(role);
                                Roles.RoleManager.Impostors.Remove(member);
                            }
                        }
                        else if (role.Team != Team.Impostor)
                        {
                            for (int i = 1; i <= role.Limit && Roles.RoleManager.Crewmates.Count >= 1; i++)
                            {
                                if (Roles.RoleManager.HostMod.IsRole.ContainsKey(role) && Roles.RoleManager.HostMod.IsRole[role] &&
                                    PlayerControl.LocalPlayer.GetRole() == null)
                                {
                                    PlayerControl.LocalPlayer.RpcSetRole(role);
                                    continue;
                                }
                                
                                var member =
                                    Roles.RoleManager.Crewmates[PeasAPI.Random.Next(0, Roles.RoleManager.Crewmates.Count)];
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

            if (PeasAPI.EnableRoles)
            {
                foreach (var role in Roles.RoleManager.Roles)
                {
                    role._OnGameStart();
                }
            }
        }
    }
}