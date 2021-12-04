using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using PeasAPI.Options;
using Reactor;

namespace PeasAPI.GameModes
{
    public class GameModeManager
    {
        public static List<GameMode> Modes = new List<GameMode>();
        
        public static byte GetModeId() => (byte) Modes.Count;
        
        public static void RegisterMode(GameMode mode) => Modes.Add(mode);

        public static CustomStringOption GameModeOption;

        public static bool IsGameModeActive(GameMode mode) => GameModeOption.StringValue.Equals(mode.Name);
        
        [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnGameJoined))]
        class AmongUsClientOnGameJoinedPatch
        {
            static void Postfix(AmongUsClient __instance)
            {
                GameModeOption.Values = Modes.ConvertAll(mode => mode.Name).Prepend("None").ToList().ConvertAll(mode => (StringNames) CustomStringName.Register(mode));
                PeasAPI.EnableRoles = GameModeOption.StringValue.Equals("None");
            }
        }
    }
}