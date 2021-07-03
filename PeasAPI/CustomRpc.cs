using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Hazel;
using PeasAPI.CustomEndReason;
using PeasAPI.Roles;
using UnityEngine;

namespace PeasAPI
{
    enum CustomRpc
    {
        SetRole = 43,
        ResetRole = 44,
        CustomEndReason = 45
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
    class HandleRpcPatch
    {
        static void Prefix(PlayerControl __instance, [HarmonyArgument(0)] byte packetId, [HarmonyArgument(1)] MessageReader reader)
        {
            switch (packetId)
            {
                case (byte)CustomRpc.SetRole:
                    var player = reader.ReadByte().GetPlayer();
                    var role = reader.ReadByte();
                    player.SetRole(RoleManager.GetRole(role));
                    break;
                case (byte)CustomRpc.ResetRole:
                    RoleManager.ResetRoles();
                    break;
                case (byte)CustomRpc.CustomEndReason:
                    var color = new Color(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                    
                    var stinger = reader.ReadString();
                    
                    var winnerCount = reader.ReadInt32();
                    var _winners = reader.ReadBytes(winnerCount).ToList();
                    var winners = new List<GameData.PlayerInfo>();
                    foreach (var winner in _winners)
                    {
                        winners.Add(winner.GetPlayerInfo());
                    }
                    
                    EndReasonManager.Color = color;
                    EndReasonManager.Winners = winners;
                    EndReasonManager.Stinger = stinger;
                    break;
            }
        }
    }
}