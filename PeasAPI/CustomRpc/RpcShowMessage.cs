using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PeasAPI.Managers;
using Reactor;
using Reactor.Networking;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.ShowMessage)]
    public class RpcShowMessage : PlayerCustomRpc<PeasAPI, RpcShowMessage.Data>
    {
        public RpcShowMessage(PeasAPI plugin, uint id) : base(plugin, id)
        {
        }

        public struct Data
        {
            public readonly string Message;
            public readonly List<byte> Targets;

            public Data(string message, List<byte> targets)
            {
                Message = message;
                Targets = targets;
            }
        }
        
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
        
        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Message);
            writer.WritePacked(data.Targets.Count);
            data.Targets.Do(target => writer.Write(target));
        }

        public override Data Read(MessageReader reader)
        {
            var message = reader.ReadString();
            var count = reader.ReadPackedInt32();
            var targets = new List<byte>();
            for (int i = 1; i <= count; i++)
            {
                targets.Add(reader.ReadByte());
            }
            return new Data(message, targets);
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            if (data.Targets.Contains(PlayerControl.LocalPlayer.PlayerId))
                TextMessageManager.ShowMessage(data.Message);
        }
    }
}