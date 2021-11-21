using Hazel;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.SetColor)]
    public class RpcSetColor : PlayerCustomRpc<PeasAPI, RpcSetColor.Data>
    {
        public RpcSetColor(PeasAPI plugin, uint id) : base(plugin, id)
        {
        }

        public readonly struct Data
        {
            public readonly PlayerControl Player;
            public readonly byte ColorId;

            public Data(PlayerControl player, byte colorId)
            {
                Player = player;
                ColorId = colorId;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Player.PlayerId);
            writer.Write(data.ColorId);
        }

        public override Data Read(MessageReader reader)
        {
            return new Data(reader.ReadByte().GetPlayer(), reader.ReadByte());
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            data.Player.SetColor(data.ColorId);
        }
    }
}