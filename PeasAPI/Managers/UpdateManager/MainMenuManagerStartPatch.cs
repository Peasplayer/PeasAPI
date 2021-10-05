using HarmonyLib;

namespace PeasAPI.Managers.UpdateManager
{
    [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
    public static class MainMenuManagerStartPatch
    {
        private static bool _initialized;

        public static void Postfix(MainMenuManager __instance)
        {
            if (!_initialized) UpdateManager.CheckForUpdates();
            _initialized = true;
        }
    }
}