using Hazel;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.SetVanillaRole)]
    public class RpcSetVanillaRole : PlayerCustomRpc<PeasAPI, RpcSetVanillaRole.Data>
    {
        public RpcSetVanillaRole(PeasAPI plugin, uint id) : base(plugin, id)
        {
        }
        
        public struct Data
        {
            public PlayerControl Player;
            public RoleTypes Role;

            public Data(PlayerControl player, RoleTypes role)
            {
                Player = player;
                Role = role;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
        
        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Player.PlayerId);
            writer.WritePacked((ushort) data.Role);
        }

        public override Data Read(MessageReader reader)
        {
            var playerId = reader.ReadByte();
            var role = reader.ReadUInt16();
            return new Data(playerId.GetPlayer(), (RoleTypes) role);
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            data.Player.SetVanillaRole(data.Role);
        }
    }
}