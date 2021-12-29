using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using HarmonyLib;

namespace PeasAPI.Managers
{
    public class CustomServerManager
    {
        public static List<IRegionInfo> CustomServer = new List<IRegionInfo>();
        
        /// <summary>
        /// Adds a custom region to the game
        /// </summary>
        public static void RegisterServer(string name, string ip, ushort port)
        {
            if (Uri.CheckHostName(ip).ToString() == "Dns")
            {
                try
                {
                    foreach (IPAddress address in Dns.GetHostAddresses(ip))
                        if (address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            ip = address.ToString();
                            break;
                        }
                }
                catch
                {
                }
            }

            CustomServer.Add(new DnsRegionInfo(ip, name, StringNames.NoTranslation, ip, port)
                .Cast<IRegionInfo>());
        }
        
        //Skidded from https://github.com/edqx/Edward.SkipAuth
        [HarmonyPatch(typeof(AuthManager._CoConnect_d__4), nameof(AuthManager._CoConnect_d__4.MoveNext))]
        public static class DoNothingInConnect
        {
            public static bool Prefix(AuthManager __instance)
            {
                return false;
            }
        }

        [HarmonyPatch(typeof(AuthManager._CoWaitForNonce_d__6), nameof(AuthManager._CoWaitForNonce_d__6.MoveNext))]
        public static class DontWaitForNonce
        {
            public static bool Prefix(AuthManager __instance)
            {
                return false;
            }
        }
        
        [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.Awake))]
        class ServerManagerAwakePatch
        {
            public static void Postfix(ServerManager __instance)
            {
                var defaultRegions = new List<IRegionInfo>();
                foreach (var server in CustomServer)
                {
                    defaultRegions.Add(server);
                }
                ServerManager.DefaultRegions = defaultRegions.ToArray();
                __instance.AvailableRegions = defaultRegions.ToArray();
            }
        }
        
        [HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Start))]
        public static class MainMenuManagerStartPatch
        {
            private static bool _initialized;

            public static void Postfix(MainMenuManager __instance)
            {
                if (!_initialized && ServerManager.Instance.CurrentRegion != CustomServer[0]) 
                    ServerManager.Instance.SetRegion(CustomServer[0]);
                _initialized = true;
            }
        }
        
        [HarmonyPatch(typeof(StatsManager), nameof(StatsManager.AmBanned), MethodType.Getter)]
        public static class AmBannedPatch
        {
            public static void Postfix(out bool __result)
            {
                __result = false;
            }
        }
    }
}