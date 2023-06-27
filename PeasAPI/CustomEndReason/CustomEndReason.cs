using System.Collections.Generic;
using Hazel;
using PeasAPI.CustomRpc;
using PeasAPI.Roles;
using Reactor.Networking.Rpc;
using UnityEngine;

namespace PeasAPI.CustomEndReason
{
    public class CustomEndReason
    {
        /// <summary>
        /// Ends the game with the specified values
        /// </summary>
        internal CustomEndReason(Color color, string victoryText, string defeatText, string stinger, List<GameData.PlayerInfo> winners)
        {
            Rpc<RpcCustomEndReason>.Instance.Send(new RpcCustomEndReason.Data(color, victoryText, defeatText, stinger, winners));
        }

        /// <summary>
        /// Ends the game with the team of the <see cref="player"/> as winners
        /// </summary>
        public CustomEndReason(PlayerControl player)
        {
            var role = player.GetRole();

            if (role == null)
            {
                if (player.Data.Role.IsImpostor)
                {

                    var _winners = new List<byte>();
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && _player.Role.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }

                    var winners = new List<GameData.PlayerInfo>();
                    foreach (var winner in _winners)
                    {
                        winners.Add(winner.GetPlayerInfo());
                    }
                    
                    Rpc<RpcCustomEndReason>.Instance.Send(new RpcCustomEndReason.Data(Palette.ImpostorRed, "Victory", "Defeat", "impostor", winners));

                    EndReasonManager.Color = Palette.ImpostorRed;
                    EndReasonManager.Winners = winners;
                    EndReasonManager.VictoryText = "Victory";
                    EndReasonManager.DefeatText = "Defeat";
                    EndReasonManager.Stinger = "impostor";
                }
                else
                {
                    var _winners = new List<byte>();
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && !_player.Role.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }

                    var winners = new List<GameData.PlayerInfo>();
                    foreach (var winner in _winners)
                    {
                        winners.Add(winner.GetPlayerInfo());
                    }
                    
                    Rpc<RpcCustomEndReason>.Instance.Send(new RpcCustomEndReason.Data(Palette.Blue, "Victory", "Defeat", "crew", winners));

                    EndReasonManager.Color = Palette.Blue;
                    EndReasonManager.Winners = winners;
                    EndReasonManager.VictoryText = "Victory";
                    EndReasonManager.DefeatText = "Defeat";
                    EndReasonManager.Stinger = "crew";
                }

            }
            else
            {
                var _winners = new List<byte>();

                if (role.Team == Team.Crewmate)
                {
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && !_player.Role.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }
                }
                else if (role.Team == Team.Impostor)
                {
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && _player.Role.IsImpostor)
                            _winners.Add(_player.PlayerId);
                    }
                }
                else if (role.Team == Team.Alone)
                {
                    _winners.Add(player.PlayerId);
                }
                else if (role.Team == Team.Role)
                {
                    _winners.Add(player.PlayerId);
                    foreach (var _player in GameData.Instance.AllPlayers)
                    {
                        if (_player.PlayerId != player.PlayerId && _player.GetRole() == player.GetRole())
                            _winners.Add(_player.PlayerId);
                    }
                }

                var winners = new List<GameData.PlayerInfo>();
                foreach (var winner in _winners)
                {
                    winners.Add(winner.GetPlayerInfo());
                }

                var stinger = "";
                if (role.Team == Team.Crewmate)
                    stinger = "crew";
                else
                    stinger = "impostor";
                
                Rpc<RpcCustomEndReason>.Instance.Send(new RpcCustomEndReason.Data(role.Color, $"{role.Name} wins", $"{role.Name} wins", stinger, winners));

                EndReasonManager.Color = role.Color;
                EndReasonManager.Winners = winners;
                EndReasonManager.VictoryText = $"{role.Name} wins";
                EndReasonManager.DefeatText = $"{role.Name} wins";
                EndReasonManager.Stinger = stinger;
            }
        }
    }
}