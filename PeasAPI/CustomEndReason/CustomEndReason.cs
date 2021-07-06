using System.Collections.Generic;
using Hazel;
using PeasAPI.Roles;
using UnityEngine;

namespace PeasAPI.CustomEndReason
{
    public class CustomEndReason
    {
        /// <summary>
        /// Ends the game with the specified values
        /// </summary>
        public CustomEndReason(Color color, List<byte> _winners, string victoryText, string defeatText, string stinger)
        {
            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRpc.CustomEndReason, SendOption.None, -1);
            
            writer.Write(color.r);
            writer.Write(color.g);
            writer.Write(color.b);
            
            writer.Write(victoryText);
            writer.Write(defeatText);
            
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
            EndReasonManager.VictoryText = victoryText;
            EndReasonManager.DefeatText = defeatText;
            EndReasonManager.Stinger = stinger;

            ShipStatus.RpcEndGame(EndReasonManager.CustomGameOverReason, false);
        }

        /// <summary>
        /// Ends the game with the team of the <see cref="player"/> as winners
        /// </summary>
        public CustomEndReason(PlayerControl player)
        {
            var role = player.GetRole();

            MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(PlayerControl.LocalPlayer.NetId,
                (byte) CustomRpc.CustomEndReason, SendOption.None, -1);

            if (role == null)
            {
                if (player.Data.IsImpostor)
                {
                    writer.Write(Palette.ImpostorRed.r);
                    writer.Write(Palette.ImpostorRed.g);
                    writer.Write(Palette.ImpostorRed.b);

                    var _winners = new List<byte>();
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && _player.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }

                    var winners = new List<GameData.PlayerInfo>();
                    foreach (var winner in _winners)
                    {
                        winners.Add(winner.GetPlayerInfo());
                    }

                    EndReasonManager.Color = Palette.ImpostorRed;
                    EndReasonManager.Winners = winners;
                    EndReasonManager.VictoryText = "Victory";
                    EndReasonManager.DefeatText = "Defeat";
                    EndReasonManager.Stinger = "impostor";
                }
                else
                {
                    writer.Write(Palette.Blue.r);
                    writer.Write(Palette.Blue.g);
                    writer.Write(Palette.Blue.b);

                    var _winners = new List<byte>();
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && !_player.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }

                    var winners = new List<GameData.PlayerInfo>();
                    foreach (var winner in _winners)
                    {
                        winners.Add(winner.GetPlayerInfo());
                    }

                    EndReasonManager.Color = Palette.Blue;
                    EndReasonManager.Winners = winners;
                    EndReasonManager.VictoryText = "Victory";
                    EndReasonManager.DefeatText = "Defeat";
                    EndReasonManager.Stinger = "crew";
                }

                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }
            else
            {
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
                }
                else if (role.Team == Team.Role)
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

                var winners = new List<GameData.PlayerInfo>();
                foreach (var winner in _winners)
                {
                    winners.Add(winner.GetPlayerInfo());
                }

                EndReasonManager.Color = role.Color;
                EndReasonManager.Winners = winners;
                EndReasonManager.VictoryText = $"{role.Name} wins";
                EndReasonManager.DefeatText = $"{role.Name} wins";
                if (role.Team == Team.Crewmate)
                    EndReasonManager.Stinger = "crew";
                else
                    EndReasonManager.Stinger = "impostor";

                AmongUsClient.Instance.FinishRpcImmediately(writer);
            }

            ShipStatus.RpcEndGame(EndReasonManager.CustomGameOverReason, false);
        }
    }
}