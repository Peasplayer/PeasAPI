using System.Collections.Generic;
using Hazel;
using PeasAPI.Roles;
using UnityEngine;

namespace PeasAPI.CustomEndReason
{
    public class CustomEndReason
    {
        public byte Id { get; internal set; }

        public virtual Color Color { get; }

        public virtual IEnumerable<PlayerControl> Winners { get; }

        public virtual string Stinger => null;

        public CustomEndReason(Color color, List<byte> _winners, string stinger)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRpc.ResetRole, SendOption.None, -1);
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            writer.Write(stinger);
            writer.Write(_winners.Count);
            writer.Write(_winners.ToArray());
            AmongUsClient.Instance.FinishRpcImmediately(writer);

            var winners = new List<GameData.PlayerInfo>();
            foreach (var winner in _winners)
            {
                winners.Add(winner.GetPlayerInfo());
            }
            
            EndReasonManager.Color = color;
            EndReasonManager.Winners = winners;
            EndReasonManager.Stinger = stinger;

            ShipStatus.RpcEndGame(EndReasonManager.CustomGameOverReason, false);
        }

        public CustomEndReason(PlayerControl player)
        {
            if (player.GetRole() != null)
            {
                var role = player.GetRole();


                MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                    (byte) CustomRpc.CustomEndReason, SendOption.None, -1);

                writer.Write(role.Color.r);
                writer.Write(role.Color.g);
                writer.Write(role.Color.b);

                var _winners = new List<byte>();

                if (role.Team == Team.Crewmate)
                {
                    writer.Write("crew");

                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && !_player.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }
                    
                    writer.Write(_winners.Count);
                    writer.Write(_winners.ToArray());
                }
                else if (role.Team == Team.Impostor)
                {
                    writer.Write("impostor");

                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && _player.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }

                    writer.Write(_winners.Count);
                    writer.Write(_winners.ToArray());
                }
                else if (role.Team == Team.Alone)
                {
                    writer.Write("impostor");

                    _winners.Add(player.PlayerId);
                    writer.Write(_winners.Count);
                    writer.Write(_winners.ToArray());
                } else if (role.Team == Team.Role)
                {
                    writer.Write("impostor");
                    
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && _player.GetRole() == player.GetRole())
                            _winners.Add(_player.PlayerId);
                    }
                    writer.Write(_winners.Count);
                    writer.Write(_winners.ToArray());
                }

                AmongUsClient.Instance.FinishRpcImmediately(writer);

                var winners = new List<GameData.PlayerInfo>();
                foreach (var winner in _winners)
                {
                    winners.Add(winner.GetPlayerInfo());
                }
                
                EndReasonManager.Color = role.Color;
                EndReasonManager.Winners = winners;
                if (role.Team == Team.Crewmate)
                    EndReasonManager.Stinger = "crew";
                else
                    EndReasonManager.Stinger = "impostor";

                ShipStatus.RpcEndGame(EndReasonManager.CustomGameOverReason, false);
            }
        }
    }
}