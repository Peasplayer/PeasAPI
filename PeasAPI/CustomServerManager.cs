using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using HarmonyLib;

namespace PeasAPI
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
        
        [HarmonyPatch(typeof(AuthManager), nameof(AuthManager.CoConnect))]
        class AuthManagerCoConnectPatch
        {
            public static void Prefix(AuthManager __instance, [HarmonyArgument(0)] string targetIp,
                [HarmonyArgument(0)] ushort targetPort)
            {
                targetIp = "172.105.251.170";
            }
        }
        
        [HarmonyPatch(typeof(ServerManager), nameof(ServerManager.LoadServers))]
        class LoadServersPatch
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
        
        [HarmonyPatch(typeof(JoinGameButton), nameof(JoinGameButton.OnClick))]
        public static class JoinGameButtonOnClickPatch
        {
            static void Postfix(JoinGameButton __instance)
            {
                AmongUsClient.Instance.SetEndpoint(DestroyableSingleton<ServerManager>.Instance.OnlineNetAddress, DestroyableSingleton<ServerManager>.Instance.OnlineNetPort);
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