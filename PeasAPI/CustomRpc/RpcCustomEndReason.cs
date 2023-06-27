using System.Collections.Generic;
using System.Linq;
using Hazel;
using PeasAPI.CustomEndReason;
using Reactor.Networking.Rpc;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace PeasAPI.CustomRpc
{
    [RegisterCustomRpc((uint) CustomRpcCalls.CustomEndReason)]
    public class RpcCustomEndReason : PlayerCustomRpc<PeasAPI, RpcCustomEndReason.Data>
    {
        public RpcCustomEndReason(PeasAPI plugin, uint id) : base(plugin, id)
        {
        }

        public readonly struct Data
        {
            public readonly Color Color;
            public readonly string VictoryText;
            public readonly string DefeatText;
            public readonly string Stinger;
            public readonly List<GameData.PlayerInfo> Winners;

            public Data(Color color, string victoryText, string defeatText, string stinger, List<GameData.PlayerInfo> winners)
            {
                Color = color;
                VictoryText = victoryText;
                DefeatText = defeatText;
                Stinger = stinger;
                Winners = winners;
            }
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.After;

        public override void Write(MessageWriter writer, Data data)
        {
            writer.Write(data.Color.r);
            writer.Write(data.Color.g);
            writer.Write(data.Color.b);
            
            writer.Write(data.VictoryText);
            writer.Write(data.DefeatText);
            
            writer.Write(data.Stinger);
            
            var _winners = new List<byte>();
            foreach (var winner in data.Winners)
            {
                _winners.Add(winner.PlayerId);
            }
            writer.Write(_winners.Count);
            writer.Write(_winners.ToArray());
        }

        public override Data Read(MessageReader reader)
        {
            var color = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            var victoryText = reader.ReadString();
            var defeatText = reader.ReadString();
            
            var stinger = reader.ReadString();
                    
            var winnerCount = reader.ReadInt32();
            var _winners = reader.ReadBytes(winnerCount).ToList();
            var winners = new List<GameData.PlayerInfo>();
            foreach (var winner in _winners)
            {
                winners.Add(winner.GetPlayerInfo());
            }
            
            return new Data(color, victoryText, defeatText, stinger, winners);
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            if (EndReasonManager.GameIsEnding)
                return;
            EndReasonManager.GameIsEnding = true;
            
            EndReasonManager.Color = data.Color;
            EndReasonManager.Winners = data.Winners;
            EndReasonManager.VictoryText = data.VictoryText;
            EndReasonManager.DefeatText = data.DefeatText;
            EndReasonManager.Stinger = data.Stinger;
            
            if (AmongUsClient.Instance.AmHost)
                GameManager.Instance.RpcEndGame(EndReasonManager.CustomGameOverReason, false);
        }
    }
}