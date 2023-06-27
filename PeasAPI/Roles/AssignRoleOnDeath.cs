/*
using AmongUs.GameOptions;
using HarmonyLib;

namespace PeasAPI;

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.AssignRoleOnDeath))]
public class AssignRoleOnDeathPatch
{
    public static bool Prefix(RoleManager __instance, [HarmonyArgument(0)] PlayerControl player)
    {
        return false;
    }


    public static void Postfix(RoleManager __instance, [HarmonyArgument(0)] PlayerControl player, [HarmonyArgument(1)] bool specialRolesAllowed)
    {
        player.RpcSetRole(player.Get().IsImpostor() ? RoleTypes.ImpostorGhost : RoleTypes.CrewmateGhost);
        VentLogger.Debug($"Dead Player {player.name} => {player.Data.Role.Role}");
    }
}
*/