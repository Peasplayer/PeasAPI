using System.Collections.Generic;
using PeasAPI.Options;

namespace PeasAPI.GameModes
{
    public class GameModeManager
    {
        public static List<GameMode> Modes = new List<GameMode>();
        
        public static byte GetModeId() => (byte) Modes.Count;
        
        public static void RegisterMode(GameMode mode) => Modes.Add(mode);

        public static CustomStringOption GameModeOption;

        public static GameMode GetCurrentGameMode()
        {
            foreach (var mode in Modes)
            {
                if (GameModeOption.StringValue.Equals(mode.Name))
                    return mode;
            }

            return null;
        }
        
        public static bool IsGameModeActive(GameMode mode) => GameModeOption.StringValue.Equals(mode.Name);
    }
}