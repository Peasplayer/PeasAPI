using System.Collections.Generic;
using HarmonyLib;
using Hazel;
using PeasAPI.Managers;
using Reactor.Networking.Rpc;
using Reactor.Networking.Attributes;

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
            public readonly float Duration;
            public readonly List<byte> Targets;

            public Data(string message, float duration, List<byte> targets)
            {
                Message = message;
                Duration = duration;
                Targets = targets;
            }
        }
        
        public override RpcLocalHandling LocalHandling => RpcLocalHandling.Before;
        
        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Message);
            writer.Write(data.Duration);
            writer.WritePacked(data.Targets.Count);
            data.Targets.Do(target => writer.Write(target));
        }

        public override Data Read(MessageReader reader)
        {
            var message = reader.ReadString();
            var duration = reader.ReadSingle();
            var count = reader.ReadPackedInt32();
            var targets = new List<byte>();
            for (int i = 1; i <= count; i++)
            {
                targets.Add(reader.ReadByte());
            }
            return new Data(message, duration, targets);
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            if (data.Targets.Contains(PlayerControl.LocalPlayer.PlayerId))
                TextMessageManager.ShowMessage(data.Message, data.Duration);
        }
    }
}