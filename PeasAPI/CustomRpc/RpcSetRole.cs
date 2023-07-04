using Hazel;
using PeasAPI.Roles;
using Reactor.Networking.Rpc;
using Reactor.Networking.Attributes;


namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.SetRole)]
    public class RpcSetRole : PlayerCustomRpc<PeasAPI, RpcSetRole.Data>
    {
        public RpcSetRole(PeasAPI plugin, uint id) : base(plugin, id)
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
            if (data.Role != null)
                writer.Write(data.Role.Id);
            else
                writer.Write(byte.MaxValue);
        }

        public override Data Read(MessageReader reader)
        {
            var playerId = reader.ReadByte();
            var roleId = reader.ReadByte();
            BaseRole role = null;
            if (roleId != byte.MaxValue)
                role = Roles.RoleManager.GetRole(roleId);
            return new Data(playerId.GetPlayer(), role);
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            data.Player.SetRole(data.Role);
        }
    }
}