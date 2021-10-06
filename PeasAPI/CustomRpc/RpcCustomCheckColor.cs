using Hazel;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.CmdCheckColor)]
    public class RpcCustomCheckColor : PlayerCustomRpc<PeasApi, RpcCustomCheckColor.Data>
    {
        public RpcCustomCheckColor(PeasApi plugin, uint id) : base(plugin, id)
        {
        }

        public readonly struct Data
        {
            public readonly byte ColorId;

            public Data(byte colorId)
            {
                ColorId = colorId;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.ColorId);
        }

        public override Data Read(MessageReader reader)
        {
            return new Data(reader.ReadByte());
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            if (AmongUsClient.Instance.AmHost)
                innerNetObject.CheckColor(data.ColorId);
        }
    }
}