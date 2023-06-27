using Hazel;
using Reactor.Networking.Rpc;
using Reactor.Networking.Attributes;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.CheckColor)]
    public class RpcCustomCheckColor : PlayerCustomRpc<PeasAPI, byte>
    {
        public RpcCustomCheckColor(PeasAPI plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

        public override void Write(MessageWriter writer, byte data)
        {
            writer.Write(data);
        }

        public override byte Read(MessageReader reader)
        {
            return reader.ReadByte();
        }

        public override void Handle(PlayerControl innerNetObject, byte data)
        {
            if (AmongUsClient.Instance.AmHost)
                innerNetObject.CheckColor(data);
        }
    }
}