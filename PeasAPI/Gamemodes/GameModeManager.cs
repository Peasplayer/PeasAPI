using System.Collections.Generic;

namespace PeasAPI.Gamemodes
{
    public class GameModeManager
    {
        public static List<GameMode> Modes = new List<GameMode>();
        
        public static byte GetModeId() => (byte) Modes.Count;
        
        public static void RegisterMode(GameMode mode) => Modes.Add(mode);
    }
}