using PeasAPI.Roles;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.ResetRoles)]
    public class RpcResetRoles : PlayerCustomRpc<PeasApi>
    {
        public RpcResetRoles(PeasApi plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
        public override void Handle(PlayerControl innerNetObject)
        {
            PeasAPI.Roles.RoleManager.ResetRoles();
        }
    }
}