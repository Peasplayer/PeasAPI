using System.Collections.Generic;

namespace PeasAPI.Options
{
    public class OptionManager
    {
        public static List<CustomOption> CustomOptions = new List<CustomOption>();

        public static List<CustomOption> HudVisibleOptions => CustomOptions.FindAll(option => option.HudVisible);
        
        public static List<CustomOption> MenuVisibleOptions => CustomOptions.FindAll(option => option.MenuVisible);
    }
}