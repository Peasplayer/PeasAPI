using PeasAPI.Roles;
using Reactor.Networking.Rpc;
using Reactor.Networking.Attributes;


namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.ResetRoles)]
    public class RpcResetRoles : PlayerCustomRpc<PeasAPI>
    {
        public RpcResetRoles(PeasAPI plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
        public override void Handle(PlayerControl innerNetObject)
        {
            Roles.RoleManager.ResetRoles();
        }
    }
}