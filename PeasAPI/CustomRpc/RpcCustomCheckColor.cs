using Hazel;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.CmdCheckColor)]
    public class RpcCustomCheckColor : PlayerCustomRpc<PeasApi, byte>
    {
        public RpcCustomCheckColor(PeasApi plugin, uint id) : base(plugin, id)
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