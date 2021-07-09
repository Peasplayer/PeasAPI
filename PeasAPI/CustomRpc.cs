using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PeasAPI.CustomEndReason;
using PeasAPI.Roles;
using Reactor;
using Reactor.Networking;
using UnityEngine;

namespace PeasAPI
{
    enum CustomRpcCalls : uint
    {
        SetRole,
        ResetRole,
        CustomEndReason
    }
    
    [RegisterCustomRpc((uint) CustomRpcCalls.SetRole)]
    public class SetRoleRpc : PlayerCustomRpc<PeasApi, SetRoleRpc.Data>
    {
        public SetRoleRpc(PeasApi plugin, uint id) : base(plugin, id)
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
            return new Data(reader.ReadByte().GetPlayer(), RoleManager.GetRole(reader.ReadByte()));
        }

        public override void Handle(PlayerControl innerNetObject, Data data)
        {
            data.Player.SetRole(data.Role);
        }
    }
    
    [RegisterCustomRpc((uint) CustomRpcCalls.ResetRole)]
    public class ResetRoleRpc : PlayerCustomRpc<PeasApi>
    {
        public ResetRoleRpc(PeasApi plugin, uint id) : base(plugin, id)
        {
        }

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;
        public override void Handle(PlayerControl innerNetObject)
        {
            RoleManager.ResetRoles();
        }
    }
    
    [RegisterCustomRpc((uint) CustomRpcCalls.CustomEndReason)]
    public class CustomEndReasonRpc : PlayerCustomRpc<PeasApi, CustomEndReasonRpc.Data>
    {
        public CustomEndReasonRpc(PeasApi plugin, uint id) : base(plugin, id)
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

        public override RpcLocalHandling LocalHandling => RpcLocalHandling.None;

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
            EndReasonManager.Color = data.Color;
            EndReasonManager.Winners = data.Winners;
            EndReasonManager.VictoryText = data.VictoryText;
            EndReasonManager.DefeatText = data.DefeatText;
            EndReasonManager.Stinger = data.Stinger;
        }
    }
}