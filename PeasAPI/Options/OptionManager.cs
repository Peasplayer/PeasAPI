using System.Collections.Generic;

namespace PeasAPI.Options
{
    public class OptionManager
    {
        public static List<CustomOption> CustomOptions = new List<CustomOption>();
        
        public static List<CustomRoleOption> CustomRoleOptions = new List<CustomRoleOption>();

        public static List<CustomOption> HudVisibleOptions => CustomOptions.FindAll(option => option.HudVisible);
        
        public static List<CustomOption> MenuVisibleOptions => CustomOptions.FindAll(option => option.MenuVisible && !option.AdvancedRoleOption);

        public static ToggleOption ToggleOptionPrefab;
        public static NumberOption NumberOptionPrefab;
        public static StringOption StringOptionPrefab;
    }
}