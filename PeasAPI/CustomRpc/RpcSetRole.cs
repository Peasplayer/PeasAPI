using Hazel;
using PeasAPI.Roles;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.SetRole)]
    public class RpcSetRole : PlayerCustomRpc<PeasApi, RpcSetRole.Data>
    {
        public RpcSetRole(PeasApi plugin, uint id) : base(plugin, id)
        {
        }

        public readonly struct Data
        {
            public readonly PlayerControl Player;
            public readonly BaseRole Role;

            public Data(PlayerControl player, BaseRole role)
            {
                Player = player;
                Role = role;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Player.PlayerId);
            writer.Write(data.Role.Id);
        }

        public override Data Read(MessageReader reader)
        {
            return new Data(reader.ReadByte().GetPlayer(), PeasAPI.Roles.RoleManager.GetRole(reader.ReadByte()));
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            data.Player.SetRole(data.Role);
        }
    }
}